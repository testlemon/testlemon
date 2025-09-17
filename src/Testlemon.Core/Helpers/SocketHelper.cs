using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Testlemon.Core.Helpers
{
    public class SocketHelper
    {
        public static async Task<TimeSpan> CalculateConnectionTime(string host, int port)
        {
            // Resolve the IP address of the host
            var addresses = await Dns.GetHostAddressesAsync(host);
            var endPoint = new IPEndPoint(addresses[0], port);

            using Socket socket = new(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Initialize stopwatch
            var stopwatch = new Stopwatch();

            // Start the stopwatch before attempting to connect
            stopwatch.Start();

            // Connect to the server
            await Task.Factory.FromAsync(
                socket.BeginConnect,
                socket.EndConnect,
                endPoint,
                null);

            // Stop the stopwatch after the connection is established
            stopwatch.Stop();

            // Get the elapsed time in milliseconds
            var connectionTime = stopwatch.Elapsed;

            return connectionTime;
        }
    }
}