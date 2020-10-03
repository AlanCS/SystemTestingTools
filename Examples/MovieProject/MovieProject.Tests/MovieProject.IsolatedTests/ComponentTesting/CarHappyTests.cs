using FluentAssertions;
using FluentAssertions.Execution;
using MovieProject.Logic.DTO;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "Car Component Tests (Happy)")]
    public class CarHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string Url = "http://www.dneonline.com/calculator.asmx";

        public CarHappyTests(TestServerFixture fixture)
        {        
            Fixture = fixture;
        }

        [Fact]
        public async Task When_Listing_Adding_ListingAgain_Success()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();

            // act
            var cars = await GetCars();

            // asserts
            cars.Should().BeEmpty();

            async Task<List<Car>> GetCars()
            {
                var httpResponse = await client.GetAsync("/api/cars");

                using (new AssertionScope())
                {
                    // assert logs
                    client.GetSessionLogs().Should().BeEmpty();

                    // assert outgoing
                    client.GetSessionOutgoingRequests().Should().BeEmpty();

                    // assert return
                    httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var cars = await httpResponse.ReadJsonBody<List<Car>>();
                    cars.Should().NotBeNull();

                    return cars;
                }                
            }
        }
    }
}
