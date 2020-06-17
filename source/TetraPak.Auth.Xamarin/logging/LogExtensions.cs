using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace TetraPak.Auth.Xamarin.logging
{
    public static class LogExtensions
    {
        public static void DebugWebRequest(this ILog log, HttpWebRequest request, string body)
        {
            if (log is null)
                return;
            
            var sb = new StringBuilder();
            sb.Append(request.Method.ToUpper());
            sb.Append(' ');
            sb.AppendLine(request.RequestUri.ToString());
            addHeaders(sb, request.Headers);
            if (body is null)
            {
                log.Debug(sb.ToString());
                return;
            }
            sb.AppendLine();
            sb.Append(body);
            log.Debug(sb.ToString());
        }

        public static void DebugWebResponse(this ILog log, HttpWebResponse response, string body)
        {
            if (log is null || response is null)
                return;
            
            var sb = new StringBuilder();
            sb.Append((int) response.StatusCode);
            sb.Append(' ');
            sb.AppendLine(response.StatusCode.ToString());
            addHeaders(sb, response.Headers);
            if (body is null)
            {
                log.Debug(sb.ToString());
                return;
            }

            sb.AppendLine();
            sb.Append(body);
            log.Debug(sb.ToString());
        }

        static void addHeaders(StringBuilder sb, NameValueCollection headers)
        {
            foreach (string header in headers)
            {
                var value = headers[header];
                if (value is null)
                {
                    sb.AppendLine(header);
                    continue;
                }
                
                sb.Append(header);
                sb.Append('=');
                sb.AppendLine(value);
            }
        }
    }
}