using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VatTools.Validate;

namespace VatTools.Tests
{
    [TestClass]
    public class ViesValidatorTest
    {
        [TestMethod]
        public async Task CanValidateVatNumber(string vatNumber, string countryCode)
        {
            var viesValidator = new Vies();
            var response = await viesValidator.ValidateAsync(new Request()
            {
                VatNumber = "2637524",
                CountryCode = "LU"
            });

            Assert.IsTrue(response.IsValid);
        }
    }
}
