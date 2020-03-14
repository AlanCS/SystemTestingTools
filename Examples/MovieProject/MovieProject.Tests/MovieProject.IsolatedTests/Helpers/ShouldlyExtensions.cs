using Shouldly;
using System.Linq;
using SystemTestingTools;

namespace IsolatedTests.Helpers
{
    public static class ShouldlyExtensions
    {
        public static void ShouldContainHeader(this HttpRequestMessageWrapper httpRequest, string key, string value)
        {
            httpRequest.Request.Headers.ShouldContain(c => c.Key == key && c.Value.FirstOrDefault() == value);
        }

        public static void ShouldBeEndpoint(this HttpRequestMessageWrapper httpRequest, string correctValue)
        {
            var endpoint = $"{httpRequest.Request.Method} {httpRequest.Request.RequestUri}";
            endpoint.ShouldBe(correctValue);
        }
    }
}
