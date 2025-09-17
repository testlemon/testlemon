using System.Security.Cryptography.X509Certificates;
using Testlemon.Core.Validators.Helpers;

namespace Testlemon.Core.Validators
{
    public abstract class SSLValidator
    {
        public static async Task<X509Certificate2?> GetSSLCertificate(string url)
        {
            return await SSL.GetSSLCertificateAsync(url);
        }
    }
}