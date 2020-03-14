using MovieProject.Logic.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MovieProject.Logic.Proxy
{
    public class BaseProxy
    {
        private HttpClient _client;

        public BaseProxy(HttpClient client)
        {
            _client = client;
        }

        protected async Task<FinalClass> Send<IntermediateClass, FinalClass>(HttpMethod method, string route, Func<IntermediateClass, HttpStatusCode, FinalClass> processResponse, object body = null)
        {
            // may seem excessive, but it's necessary for high quality error logs
            // this has saved me from dozens of hours of debugging many times over
            // we want (for every error) to have (if available): full url, http status, response body, exception            

            var request = new HttpRequestMessage(method, _client.BaseAddress + route);

            if (body != null) request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {

                response = await _client.SendAsync(request);
            }
            catch (System.Exception ex)
            {
                throw new DownstreamException($"{GetEndpoint()} threw exception [{ex.Message}]", ex);
            }

            IntermediateClass result = default;
            try
            {
                result = await response.Content.ReadAsAsync<IntermediateClass>();
            }
            catch (System.Exception ex)
            {
                await ThrowDownstreamErrorWithResponseInfo(ex, "parsing response");
            }

            try
            {
                return processResponse(result, response.StatusCode);
            }
            catch (System.Exception ex)
            {
                await ThrowDownstreamErrorWithResponseInfo(ex, "processing response");
            }

            return default; // should never get here, but just to make compiler happy

            async Task ThrowDownstreamErrorWithResponseInfo(System.Exception ex, string action)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var message = $"{GetEndpoint()} had exception [{ex.Message}] while [{action}], response HttpStatus={(int)response.StatusCode} and body=[{responseBody}]";

                throw new DownstreamException(message, ex);
            }

            string GetEndpoint()
            {
                return $"{request.Method} {request.RequestUri}";
            }
        }
    }
}
