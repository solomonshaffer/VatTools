using System;

namespace VatTools.Common
{
    public static class VatNumberFormatter
    {
        public static string GetWithCountryCode(string vatNumber, string countryCode)
        {
            if (string.IsNullOrEmpty(vatNumber))
                throw new ArgumentNullException(nameof(vatNumber));

            if (string.IsNullOrEmpty(countryCode))
                throw new ArgumentNullException(nameof(countryCode));

            return vatNumber.ToLower().StartsWith(countryCode.ToLower()) ? vatNumber : $"{countryCode.ToUpper()}{vatNumber}";
        }
    }
}