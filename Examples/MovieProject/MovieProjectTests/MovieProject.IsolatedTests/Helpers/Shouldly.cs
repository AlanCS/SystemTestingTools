using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shouldly;
using System.Linq;
using SystemTestingTools;

namespace IsolatedTests.Helpers
{
    public static class HttpResponseMessageExtensions
    {
        public static void ShouldNotBeNullAndHaveStatus(this HttpResponseMessage httpResponse, HttpStatusCode httpStatus)
        {
            httpResponse.ShouldNotBeNull();
            httpResponse.StatusCode.ShouldBe(httpStatus);
        }

        public static void ShouldContainHeader(this HttpRequestMessageWrapper httpRequest, string key, string value)
        {
            httpRequest.Request.Headers.ShouldContain(c => c.Key == key && c.Value.FirstOrDefault() == value);
        }

        public static void ShouldBeEndpoint(this HttpRequestMessageWrapper httpRequest, string correctValue)
        {
            var endpoint = $"{httpRequest.Request.Method} {httpRequest.Request.RequestUri}";
            endpoint.ShouldBe(correctValue);
        }

        public static async Task<T> ParseResponse<T>(this HttpResponseMessage httpResponse)
        {
            var dto = JsonConvert.DeserializeObject<T>(await httpResponse.GetResponseString());
            dto.ShouldNotBeNull();

            return dto;
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
