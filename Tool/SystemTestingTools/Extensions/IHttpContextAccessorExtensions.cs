using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SystemTestingTools
{
    /// <summary>
    /// HttpRequestMessage extension methods
    /// </summary>
    public static class IHttpContextAccessorExtensions
    {
        /// <summary>
        /// Get the values of a header (joined by 'separator' if more than one), null if not present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="separator">the separator string to join multiple values</param>
        /// <returns>returns null if header is not there, otherwise the value is returned</returns>
        public static string GetRequestHeaderValue(this IHttpContextAccessor accessor, string key, string separator = Constants.DefaultSeparator)
        {
            var result = accessor.HttpContext?.Request?.Headers[key];            

            if (!result.HasValue) return null;
            if (!result.Value.Any()) return string.Empty;

            return string.Join(separator, result.Value);
        }
    }
}
