/*
*	<copyright file="HttpContextHelper">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 2/23/2026 9:48:56 PM</date>
*	<description></description>
**/

using static System.Net.WebRequestMethods;

namespace Backend_sec_dev.Shared.Helpers
{
    public class HttpContextHelper
    {
        public static string GetClientIpAddress(HttpContext httpContext)
        {
            if (httpContext == null)
                return string.Empty;
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
        public static Guid GetClientSessionId(HttpContext httpContext)
        {
            Guid guidToReturn = Guid.Empty;
            if (httpContext != null && httpContext.Request != null && httpContext.Request.Cookies != null && httpContext.Request.Cookies.Count > 0 && httpContext.Request.Cookies["Session-Id"] != null)
            {
                guidToReturn = new Guid(httpContext.Request.Cookies["Session-Id"] ?? "");
            }
            return guidToReturn;
        }

        public static string GetUserAgent(HttpContext httpContext)
        {
            if (httpContext == null)
                return string.Empty;
            return httpContext.Request.Headers.UserAgent.ToString();
        }
    }
}