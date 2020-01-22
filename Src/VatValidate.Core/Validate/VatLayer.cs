using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using VatTools.Common;

namespace VatTools.Validate
{
    public class VatLayer : IValidator
    {
        private const string ServiceUrl = "http://apilayer.net/api/validate";

        public Task<Response> ValidateAsync(Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return GetServiceResponseAsync(request.VatNumber, request.CountryCode, request.AccessKey);
        }

        public Task<bool> IsValidAsync(string vatNumber, string countryCode, string accessKey = null)
        {
            var response = GetServiceResponseAsync(vatNumber, countryCode, accessKey);
            return Task.FromResult(response.Result.IsValid);
        }

        private async Task<Response> GetServiceResponseAsync(string vatNumber, string countryCode, string accessKey)
        {
            if (string.IsNullOrEmpty(vatNumber))
                throw new ArgumentNullException(nameof(vatNumber));

            if (string.IsNullOrEmpty(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            if (string.IsNullOrEmpty(accessKey))
                throw new ArgumentNullException(nameof(accessKey));

            vatNumber = VatNumberFormatter.GetWithCountryCode(vatNumber, countryCode);

            using (var client = new HttpClient())
            {
                var responseString = await client.GetStringAsync($"{ServiceUrl}?access_key={accessKey}&vat_number={vatNumber}&format=1");

                var response = BuildResponse(responseString);
                response.Request.AccessKey = accessKey;

                return response;
            }
        }

        private static Response BuildResponse(string responseContent)
        {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(responseContent)))
            {
                var serializer = new DataContractJsonSerializer(typeof(VatLayerResponse));
                var vatLayerResponse = (VatLayerResponse) serializer.ReadObject(stream);

                //cannot use vatLayerResponse.Success because it returns false even on a successful call
                if (vatLayerResponse.Error != null)
                    throw new Exception(vatLayerResponse.Error.Info);

                var response = new Response {Request = new Request()};

                response.Request.CountryCode = string.IsNullOrEmpty(vatLayerResponse.CountryCode) ? null : vatLayerResponse.CountryCode;
                response.Request.VatNumber = string.IsNullOrEmpty(vatLayerResponse.VatNumber) ? null : vatLayerResponse.VatNumber;
                response.IsValid = vatLayerResponse.Valid;
                response.Name = string.IsNullOrEmpty(vatLayerResponse.CompanyName) ? null : vatLayerResponse.CompanyName;
                response.Address = string.IsNullOrEmpty(vatLayerResponse.CompanyAddress) ? null : vatLayerResponse.CompanyAddress;

                return response;
            }
        }

        [DataContract]
        protected class VatLayerResponse
        {
            [DataMember(Name = "valid")]
            public bool Valid { get; set; }

            [DataMember(Name = "country_code")]
            public string CountryCode { get; set; }

            [DataMember(Name = "vat_number")]
            public string VatNumber { get; set; }

            [DataMember(Name = "company_name")]
            public string CompanyName { get; set; }

            [DataMember(Name = "company_address")]
            public string CompanyAddress { get; set; }

            [DataMember(Name = "error")]
            public VatLayerResponseError Error { get; set; }
        }

        [DataContract]
        protected class VatLayerResponseError
        {
            [DataMember(Name = "code")]
            public string Code { get; set; }

            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "info")]
            public string Info { get; set; }
        }
    }
}