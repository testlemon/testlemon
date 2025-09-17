using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class SSLExpirationDateAfterValidator : SSLExpirationDateValidator, IValidator
    {
        public string Name => "ssl-expiration-after";

        protected override bool CompareDates(DateTime expectedDate, DateTime cetificateDate)
        {
            return cetificateDate > expectedDate;
        }
    }
}