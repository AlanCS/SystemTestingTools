using Shouldly;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SystemTestingTools;
using System.Linq;   
using Xunit;

namespace IsolatedTests.ComponentTestings
{
    [Collection("SharedServer collection")]
    [Trait("Project", "Mathr Component Tests (Happy)")]
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

            var addResponse = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/MathWcf/Real_Responses/Happy/200_Add.txt");
            client.AppendHttpCallStub(HttpMethod.Post, new System.Uri(Url), addResponse, new Dictionary<string, string>() { { "SOAPAction", @"""http://tempuri.org/Add""" } });

            var minusResponse = ResponseFactory.FromFiddlerLikeResponseFile($"{Fixture.StubsFolder}/MathWcf/Real_Responses/Happy/200_Minus.txt");
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
                // assert logs
                var logs = client.GetSessionLogs();
                logs.ShouldBeEmpty();

                // assert outgoing
                var outgoingRequests = client.GetSessionOutgoingRequests();
                outgoingRequests.Last().GetEndpoint().ShouldBe($"POST {Url}"); // only the last one matters, as they accumulate over the 2 requests
                outgoingRequests.Last().GetHeaderValue("SOAPAction").ShouldBe(string.Format(@"""http://tempuri.org/{0}""", soapMethodName));

                // assert return
                httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

                var returnedValue = await httpResponse.ReadBody();
                returnedValue.ShouldBe(correctReturnedValue);
            }
        }
    }
}
