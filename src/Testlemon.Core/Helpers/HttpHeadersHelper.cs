namespace Testlemon.Core.Helpers
{
    public static class HttpHeadersHelper
    {
        public static bool IsRequestHeader(string headerName)
        {
            // List of known request headers
            var requestHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Accept", "Accept-Charset", "Accept-Encoding", "Accept-Language", "Authorization",
                "Cache-Control", "Connection", "Cookie", "Date", "Expect", "From", "Host",
                "If-Match", "If-Modified-Since", "If-None-Match", "If-Range", "If-Unmodified-Since",
                "Max-Forwards", "Pragma", "Proxy-Authorization", "Range", "Referer", "TE",
                "User-Agent", "Upgrade", "Via", "Warning"
            };

            return requestHeaders.Contains(headerName);
        }

        public static bool IsContentHeader(string headerName)
        {
            // List of known content headers
            var contentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Allow", "Content-Disposition", "Content-Encoding", "Content-Language",
                "Content-Length", "Content-Location", "Content-MD5", "Content-Range",
                "Content-Type", "Expires", "Last-Modified", "Content-Security-Policy"
            };

            return contentHeaders.Contains(headerName);
        }
    }
}