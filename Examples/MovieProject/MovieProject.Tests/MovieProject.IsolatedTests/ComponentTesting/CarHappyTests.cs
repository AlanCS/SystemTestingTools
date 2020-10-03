using FluentAssertions;
using FluentAssertions.Execution;
using MovieProject.Logic.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            // arrange
            var newCar = new Car() { Make = "Hyundai", Model = "i30" };

            // act
            var httpResponse = await client.PutAsJsonAsync("/api/cars", newCar);

            // asserts
            using (new AssertionScope())
            {
                // assert logs
                client.GetSessionLogs().Should().BeEmpty();
                var events = client.GetSessionEvents();
                events.Should().HaveCount(1);
                events[0].Should().Be("new car added: i30");

                // assert outgoing
                client.GetSessionOutgoingRequests().Should().BeEmpty();

                // assert return
                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            // act
            var carsAfterAdding = await GetCars();

            // asserts
            carsAfterAdding.Should().HaveCount(1);
            carsAfterAdding.First().Make.Should().Be("Hyundai");


            async Task<List<Car>> GetCars()
            {
                // act
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
