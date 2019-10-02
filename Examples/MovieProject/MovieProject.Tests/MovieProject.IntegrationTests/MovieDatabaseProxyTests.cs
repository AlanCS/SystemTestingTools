using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieProject.Logic.Option;
using MovieProject.Logic.Proxy;
using NSubstitute;
using Shouldly;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MovieProject.ContractTests
{
    public class MovieDatabaseProxyTests
    {
        private ILogger<MovieDatabaseProxy> logger;
        private IOptions<Omdb> option;

        public MovieDatabaseProxyTests()
        {
            // load the options from appsettings.json just like the website will
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
            var serviceCollection = new ServiceCollection();
            serviceCollection.Configure<Omdb>(configuration.GetSection("Omdb"));
            option = serviceCollection.BuildServiceProvider().GetService<IOptions<Omdb>>();

            logger = Substitute.For<ILogger<MovieDatabaseProxy>>();
        }

        [Fact]
        public async Task CanRetrieveMovie_TheMatrix()
        {
            var proxy = new MovieDatabaseProxy(new HttpClient(), logger, option);

            var result = await proxy.GetMovieOrTvSeries("movie", "the matrix");

            result.ShouldNotBeNull();
            result.Id.ShouldBe("tt0133093");
            result.Name.ShouldBe("The Matrix");
            result.Year.ShouldBe("1999");

            logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Critical, "", null);
        }

        [Fact]
        public async Task CanRetrieveTvSeries_TheBigBangTheory()
        {
            var proxy = new MovieDatabaseProxy(new HttpClient(), logger, option);

            var result = await proxy.GetMovieOrTvSeries("series", "the big bang theory");

            result.ShouldNotBeNull();
            result.Id.ShouldBe("tt0898266");
            result.Name.ShouldBe("The Big Bang Theory");
            result.Year.ShouldBe("2007–2019");

            logger.DidNotReceiveWithAnyArgs().Log(LogLevel.Critical, "", null);
        }
    }
}
