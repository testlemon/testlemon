using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class WhoisDomainExpirationDateAfterValidator : WhoisDomainExpirationDateValidator, IValidator
    {
        public string Name => "domain-expiration-after";

        protected override bool CompareDates(DateTime expectedDate, DateTime domainExpirationDate)
        {
            return domainExpirationDate > expectedDate;
        }
    }
}