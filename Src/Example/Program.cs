using System;
using System.Threading.Tasks;
using VatTools.Validate;

namespace VatTools.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var viesValidator = new VatTools.Validate.Vies();
            var viesValidatorResponse = await viesValidator.ValidateAsync(new Request()
            {
                VatNumber = "263752452",
                CountryCode = "LU"
            });

            Console.Write(viesValidatorResponse.IsValid);

            var vatLayerValidator = new VatTools.Validate.VatLayer();
            var vatLayerValidatorResponse = await vatLayerValidator.ValidateAsync(new Request()
            {
                VatNumber = "26375245",
                CountryCode = "LU",
                AccessKey = "4df8f81fffb10fad1782155f222dd3f1"
            });

            Console.Write(vatLayerValidatorResponse.IsValid);
        }
    }
}
