using FluentAssertions;
using FluentAssertions.Execution;
using System.Net;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace MovieProject.InterceptionTests
{
    [Collection("SharedServer collection")]
    [Trait("Project", "MovieProject.InterceptionHappyTests")]
    public class GetMovieTests
    {
        private readonly TestServerFixture Fixture;

        public GetMovieTests(TestServerFixture fixture)
        {
            Fixture = fixture;
        }

        /// <summary>
        /// This test relies on downstream system returning a good response; this was the only way to test fully the recording function
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task When_UserAsksForMovie_WithNoStubs_Then_RetrieveFromRealServer()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();

            // act
            var httpResponse = await client.GetAsync("/api/movie/inception");

            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                httpResponse.GetHeaderValue("SystemTestingToolsStub").Should().BeNull();

                var movie = await httpResponse.ReadJsonBody<Logic.DTO.Media>();
                movie.Id.Should().Be("tt1375666");
                movie.Name.Should().Be("Inception");
            }
        }
    }
}
