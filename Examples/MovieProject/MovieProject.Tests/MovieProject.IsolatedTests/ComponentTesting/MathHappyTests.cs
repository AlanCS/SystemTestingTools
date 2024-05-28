using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools;
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "Math Component Tests (Happy)")]
    public class MathHappyTests
    {
        private readonly TestServerFixture Fixture;

        private static string Url = "http://www.dneonline.com/calculator.asmx";

        public MathHappyTests(TestServerFixture fixture)
        {        
            Fixture = fixture;
        }

        [Fact]
        public async Task When_CallingManyWcfMethods_Then_CanPerformMath_Successfully()
        {
            // arrange
            var client = Fixture.Server.CreateClient();
            client.CreateSession();
            
            var addResponse = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/MathWCF/Real_Responses/Happy/200_Add.txt");
            client.AppendHttpCallStub(HttpMethod.Post, new System.Uri(Url), addResponse, new Dictionary<string, string>() { { "SOAPAction", @"""http://tempuri.org/Add""" } });

            var minusResponse = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/MathWCF/Real_Responses/Happy/200_Minus.txt");
            client.AppendHttpCallStub(HttpMethod.Post, new System.Uri(Url), minusResponse, new Dictionary<string, string>() { { "SOAPAction", @"""http://tempuri.org/Subtract""" } });

            // act
            var httpResponse = await client.GetAsync("/api/math/minus?firstNumber=7");
            // asserts
            await Asserts("Subtract", "4");

            // act
            httpResponse = await client.GetAsync("/api/math/add?firstNumber=7");
            // asserts
            await Asserts("Add", "10");

            async Task Asserts(string soapMethodName, string correctReturnedValue)
            {
                using (new AssertionScope())
                {
                    // assert logs
                    var logs = client.GetSessionLogs();
                    logs.Should().BeEmpty();

                    // assert outgoing
                    var outgoingRequests = client.GetSessionOutgoingRequests();
                    outgoingRequests.Last().GetEndpoint().Should().Be($"POST {Url}"); // only the last one matters, as they accumulate over the 2 requests
                    outgoingRequests.Last().GetHeaderValue("SOAPAction").Should().Be(string.Format(@"""http://tempuri.org/{0}""", soapMethodName));

                    // assert return
                    httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var returnedValue = await httpResponse.ReadBody();
                    returnedValue.Should().Be(correctReturnedValue);
                }
            }
        }
    }
}
