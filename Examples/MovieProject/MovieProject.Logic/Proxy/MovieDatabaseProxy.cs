using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Exceptions;
using MovieProject.Logic.Option;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieProject.Logic.Proxy
{
    public interface IMovieDatabaseProxy
    {
        Task<Proxy.DTO.Media> GetMovieOrTvSeries(string type, string movieName);
    }

    public class MovieDatabaseProxy  : IMovieDatabaseProxy
    {
        private string _apiKey;
        private HttpClient _client;
        private ILogger<MovieDatabaseProxy> _logger;

        public MovieDatabaseProxy(HttpClient client,
                                  ILogger<MovieDatabaseProxy> logger,
                                  IOptions<Omdb> omdbOption)
        {
            _apiKey = omdbOption.Value.ApiKey ?? throw new ArgumentNullException(nameof(omdbOption));

            _client = client;
            _client.BaseAddress = new Uri(omdbOption.Value.Url);
            _client.DefaultRequestHeaders.Add("Referer", Constants.Website);
            _client.Timeout = TimeSpan.FromMilliseconds(1500); // Overall timeout across all tries

            _logger = logger;
        }        

        public async Task<Proxy.DTO.Media> GetMovieOrTvSeries(string type, string name)
        {
            string route = $"?apikey={_apiKey}&type={type}&t={name}";

            // try to account for all ways it could go wrong, as mentioned in 
            // https://github.com/AlanCS/SystemTestingTools/

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

            var result = await response.Content.ReadAsAsync<DTO.Media>();

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                // movies and tv series not found get incorrectly returned as "errors", so we filter it out here
                if (result.Error.EndsWith("not found!", System.StringComparison.CurrentCultureIgnoreCase)) return null;

                await ThrowDownstreamErrorWithResponseInfo();
            }

            // could not parse the (hopefully json) response properly
            if (string.IsNullOrWhiteSpace(result.Title)) await ThrowDownstreamErrorWithResponseInfo();

            return result;

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
