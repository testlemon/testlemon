using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Testlemon.Core.Validators.Helpers
{
    public class SSL
    {
        public static async Task<X509Certificate2?> GetSSLCertificateAsync(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            var host = new Uri(url).Host; // get host part from URL

            try
            {
                // Establish a TCP connection to the server (port 443 for HTTPS)
                using var tcpClient = new TcpClient(host, 443);
                using var sslStream = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate));

                // Perform SSL handshake
                await sslStream.AuthenticateAsClientAsync(host);

                // Get the server's certificate and return the expiration date
                var cert = sslStream.RemoteCertificate as X509Certificate2;
                return cert;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching certificate for {host}: {ex.Message}");
                return null;
            }
        }

        // Custom certificate validation callback (accept all certificates)
        private static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Always accept the certificate
        }
    }
}