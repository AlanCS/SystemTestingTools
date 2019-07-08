using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Exceptions;
using MovieProject.Logic.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieProject.Logic.Proxy
{
    public interface IUserProxy
    {
        Task<DTO.User[]> GetUsers();
    }

    public class UserProxy  : IUserProxy
    {
        private HttpClient _client;
        private ILogger<UserProxy> _logger;

        public UserProxy(HttpClient client,
                                  ILogger<UserProxy> logger,
                                  IOptions<User> userOption)
        {
            _client = client;
            _client.BaseAddress = new Uri(userOption.Value.Url);
            _client.DefaultRequestHeaders.Add("Referer", Constants.Website);
            _client.Timeout = TimeSpan.FromMilliseconds(1500); // Overall timeout across all tries

            _logger = logger;
        }        

        public async Task<DTO.User[]> GetUsers()
        {
            string route = $"/users";

            HttpResponseMessage response;
            try
            {
                response = await _client.GetAsync(route);
            }
            catch (System.Exception ex)
            {
                throw new DownstreamException($"{GetEndpoint()} threw an exception: {ex}");
            }

            if (!response.IsSuccessStatusCode)
                await ThrowDownstreamErrorWithResponseInfo();

            var users = await response.Content.ReadAsAsync<DTO.User[]>();

            return users;

            async Task ThrowDownstreamErrorWithResponseInfo()
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new DownstreamException($"{GetEndpoint()} returned invalid response: http status={(int)response.StatusCode} and body=[{responseBody}]");
            }

            string GetEndpoint()
            {
                return $"GET {_client.BaseAddress}{route}";
            }
        }
    }
}
