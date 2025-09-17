using System.Security.Authentication;
using Testlemon.Core.Models;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Validators
{
    public class TLSVersionNotEqualsValidator : TLSVersionValidator, IValidator
    {
        public string Name => "tls-version-not-equals";

        protected override bool CompareVersions(SslProtocols expected, SslProtocols actual)
        {
            return expected != actual;
        }

        protected override ValidationResult HandleAuthenticationException(Exception exception)
        {
            return new ValidationResult
            {
                IsSuccessful = true,
                Message = $"AuthenticationException exception occured. Message: {exception.Message}"
            };
        }
    }
}