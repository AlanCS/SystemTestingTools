using Shouldly;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;

namespace SystemTestingTools.UnitTests
{
    public static class Helpers
    {
        [DebuggerHidden]
        public static void ShouldContainHeader(this HttpHeaders header, string key, string correctValue)
        {
            Assert.True(header.Contains(key), $"Header '{key}' not found");
            var headerValue = header.GetValues(key).FirstOrDefault()?.Trim();
            Assert.True(headerValue == correctValue, $"Header '{key}' has value '{headerValue}' instead of {correctValue}");
        }

        public static async Task<string> GetResponseString(this HttpResponseMessage httpResponse)
        {
            httpResponse.Content.ShouldNotBeNull();

            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            responseBody.ShouldNotBeNull();

            return responseBody;
        }
    }
}
