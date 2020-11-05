using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Option;
using MovieProject.Logic.Proxy;
using NSubstitute;

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.ContractTests
{
    public class MovieDatabaseProxyTests
    {
        private MovieDatabaseProxy proxy;
        private ILogger<MovieDatabaseProxy> logger;

        public MovieDatabaseProxyTests()
        {
            // load the options from appsettings.json just like the website will
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<Omdb>(configuration.GetSection("Omdb"));
            var option = serviceCollection.BuildServiceProvider().GetService<IOptions<Omdb>>();

            logger = Substitute.For<ILogger<MovieDatabaseProxy>>();
            proxy = new MovieDatabaseProxy(new HttpClient() { BaseAddress = new System.Uri(option.Value.Url) }, logger);
            Logic.Constants.OmdbApiKey = option.Value.ApiKey;
        }

        [Fact]
        public async Task CanRetrieveMovie_TheMatrix()
        {
            logger.ClearReceivedCalls();

            var result = await proxy.GetMovieOrTvSeries("movie", "the matrix");

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be("tt0133093");
                result.Name.Should().Be("The Matrix");
                result.Year.Should().Be("1999");

                logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Critical, "", null);
            }
        }

        [Fact]
        public async Task CanRetrieveTvSeries_TheBigBangTheory()
        {
            logger.ClearReceivedCalls();

            var result = await proxy.GetMovieOrTvSeries("series", "the big bang theory");

            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Id.Should().Be("tt0898266");
                result.Name.Should().Be("The Big Bang Theory");
                result.Year.Should().Be("2007 to 2019");

                logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Critical, "", null);
            }
        }
    }
}
