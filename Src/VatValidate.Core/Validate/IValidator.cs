using System.Threading.Tasks;

namespace VatTools.Validate
{
    internal interface IValidator
    {
        Task<Response> ValidateAsync(Request request);
        Task<bool> IsValidAsync(string vatNumber, string countryCode, string accessKey = null);
    }
}