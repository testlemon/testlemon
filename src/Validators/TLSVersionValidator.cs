using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using Testlemon.Core.Models;

namespace Testlemon.Core.Validators
{
    public abstract class TLSVersionValidator : DomainValidator
    {
        const int DEFAULT_HTTPS_PORT = 443;

        public async Task<IValidationResult> ValidateAsync(Test test, Response? response, Validator validator)
        {
            var domain = GetRootDomain(test.Url);
            if (string.IsNullOrWhiteSpace(domain))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to get domain from URL: {test.Url}"
                };
            }

            var expectedVersion = validator.Value;
            if (string.IsNullOrWhiteSpace(expectedVersion))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = "Expected TLS version is not set."
                };
            }

            if (!Enum.TryParse(expectedVersion, out SslProtocols expectedProtocol))
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Unable to paese TLS version. Value: '{expectedVersion}'"
                };
            }

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13
                                                       | SecurityProtocolType.Tls12
                                                       | SecurityProtocolType.Tls11
                                                       | SecurityProtocolType.Tls;

                using var client = new TcpClient(domain, DEFAULT_HTTPS_PORT);
                using var sslStream = new SslStream(client.GetStream(), false,
                    (sender, certificate, chain, sslPolicyErrors) => true); // Accept any certificate
                                                                            // Initiate the TLS handshake

                await sslStream.AuthenticateAsClientAsync(domain, null, expectedProtocol, false);

                // Retrieve the negotiated TLS version
                var negotiatedProtocol = sslStream.SslProtocol;

                var result = CompareVersions(expectedProtocol, negotiatedProtocol);

                return new ValidationResult
                {
                    IsSuccessful = result,
                    Value = negotiatedProtocol.ToString()
                };
            }
            catch (AuthenticationException ae)
            {
                return HandleAuthenticationException(ae);
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsSuccessful = false,
                    Message = $"Exception occured. Message: {ex.Message}"
                };
            }
        }

        protected abstract bool CompareVersions(SslProtocols expected, SslProtocols actual);

        protected abstract ValidationResult HandleAuthenticationException(Exception exception);
    }
}