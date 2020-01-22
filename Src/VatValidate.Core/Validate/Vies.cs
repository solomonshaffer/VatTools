using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VatTools.Validate
{
    public class Vies : IValidator
    {
        private const string ServiceUrl = "http://ec.europa.eu/taxation_customs/vies/services/checkVatService";

        public Task<Response> ValidateAsync(Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return GetServiceResponseAsync(request.VatNumber, request.CountryCode);
        }

        public Task<bool> IsValidAsync(string vatNumber, string countryCode, string accessKey = null)
        {
            var response = GetServiceResponseAsync(vatNumber, countryCode);
            return Task.FromResult(response.Result.IsValid);
        }

        private async Task<Response> GetServiceResponseAsync(string vatNumber, string countryCode)
        {
            if (string.IsNullOrEmpty(vatNumber))
                throw new ArgumentNullException(nameof(vatNumber));

            if (string.IsNullOrEmpty(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            var requestXml = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tns1=\"urn:ec.europa.eu:taxud:vies:services:checkVat:types\" xmlns:impl=\"urn:ec.europa.eu:taxud:vies:services:checkVat\">" +
                             "<soap:Header></soap:Header>" +
                             "<soap:Body>" +
                             "<tns1:checkVat xmlns:tns1=\"urn:ec.europa.eu:taxud:vies:services:checkVat:types\" xmlns=\"urn:ec.europa.eu:taxud:vies:services:checkVat:types\">" +
                             "<tns1:countryCode>" + countryCode + "</tns1:countryCode>" +
                             "<tns1:vatNumber>" + vatNumber + "</tns1:vatNumber>" +
                             "</tns1:checkVat>" +
                             "</soap:Body>" +
                             "</soap:Envelope>";

            using (var client = new HttpClient())
            {
                var content = new StringContent(requestXml, Encoding.UTF8, "text/xml");

                using (var response = await client.PostAsync(ServiceUrl, content))
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return BuildResponse(responseString);
                }
            }
        }

        private static Response BuildResponse(string responseContent)
        {
            var xmlDocument = XDocument.Parse(responseContent);
            XNamespace xmlNamespace = "urn:ec.europa.eu:taxud:vies:services:checkVat:types";
            var xmlElement = xmlDocument.Descendants(xmlNamespace + "checkVatResponse").FirstOrDefault();

            var response = new Response {Request = new Request()};

            if (xmlElement != null)
            {
                response.Request.CountryCode = string.IsNullOrEmpty((string) xmlElement.Element(xmlNamespace + "countryCode")) ? null : (string) xmlElement.Element(xmlNamespace + "countryCode");
                response.Request.VatNumber = string.IsNullOrEmpty((string) xmlElement.Element(xmlNamespace + "vatNumber")) ? null : (string) xmlElement.Element(xmlNamespace + "vatNumber");
                response.IsValid = Convert.ToBoolean((string) xmlElement.Element(xmlNamespace + "valid"));
                response.Name = string.IsNullOrEmpty((string) xmlElement.Element(xmlNamespace + "name")) || (string) xmlElement.Element(xmlNamespace + "name") == "---" ? null : (string) xmlElement.Element(xmlNamespace + "name");
                response.Address = string.IsNullOrEmpty((string) xmlElement.Element(xmlNamespace + "address")) || (string) xmlElement.Element(xmlNamespace + "address") == "---" ? null : (string) xmlElement.Element(xmlNamespace + "address");
            }

            return response;
        }
    }
}