using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class SSLExpirationDateBeforeValidator : SSLExpirationDateValidator, IValidator
    {
        public string Name => "ssl-expiration-before";

        protected override bool CompareDates(DateTime expectedDate, DateTime cetificateDate)
        {
            return cetificateDate < expectedDate;
        }
    }
}