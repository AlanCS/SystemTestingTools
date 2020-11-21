#if !NET46
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
#else
using System.Web;
#endif
using System.Collections.Generic;
using System.Linq;


namespace SystemTestingTools.Internal
{
    internal static class HttpContextProxy
    {
#if !NET46
        public static IHttpContextAccessor httpContextAccessor = null;
#endif

        private static HttpContext GetHttpContext()
        {
#if !NET46
            return httpContextAccessor.HttpContext;
#else
            return HttpContext.Current;
#endif
        }

        public static string GetHeaderValue(string key)
        {
            if (GetHttpContext()?.Request?.Headers == null) return null;
            return GetHttpContext().Request.Headers[key];
        }

        public static IDictionary<string, IReadOnlyList<string>> GetAllHeaders()
        {
            if (GetHttpContext()?.Request?.Headers == null) return null;
            return GetHttpContext().Request.Headers;
        }

        public static void SetResponseHeaderValue(string key, string value)
        {
            // not in a seession, could be happening during app start up or scheduled process
            if (GetHttpContext()?.Response?.Headers == null) return;
            GetHttpContext()?.Response.Headers.Append(key, value);
        }

        public static string GetUrl()
        {
            // not in a seession, could be happening during app start up or scheduled process
            var request = GetHttpContext()?.Request;
            if (request == null) return "No httpcontext available";            

            return $"{request.Method} {request.GetDisplayUrl()}";
        }
    }
}
