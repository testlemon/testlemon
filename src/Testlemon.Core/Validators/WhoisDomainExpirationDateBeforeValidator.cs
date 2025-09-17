using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class WhoisDomainExpirationDateBeforeValidator : WhoisDomainExpirationDateValidator, IValidator
    {
        public string Name => "domain-expiration-before";

        protected override bool CompareDates(DateTime expectedDate, DateTime domainExpirationDate)
        {
            return domainExpirationDate < expectedDate;
        }
    }
}