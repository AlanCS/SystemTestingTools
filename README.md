[<img align="right" src="https://i.imgur.com/DdoC5Il.png" width="100" />](https://www.nuget.org/packages/Polly/)

**SystemTestingTools** (for .net core 3.1+) extends your test capabilities, providing ways to create / return stubs, allowing you to run more comprehensive / deterministic / reliable tests in your local dev machine / build tool and in non-prod environments.

* supports interception of Http (HttpClient or WCF) calls:
    * before they are sent, returning stubs (ideal for automated testing)
    * after they are sent, where you can save the request and response (recording), log appropriately or replace bad responses by stubs (ideal for dev/test environments that are flaky or not ready)
    * asserting outgoing calls (ie: making sure out downstream calls have SessionIds)
* intercept logs and run asserts on them

[Nuget package](https://www.nuget.org/packages/SystemTestingTools/) | [CHANGE LOG](CHANGELOG.md)

Recommended article to understand the philosophy behind T shaped testing, and how to best leverage this tool:  https://www.linkedin.com/pulse/evolving-past-test-pyramid-shaped-testing-alan-sarli

In summary, you are better off using T shaped testing (focusing mainly component testing and using unit tests to complement those + contract testing) instead of the traditional testing pyramid, here is a summary of pros and cons:

| Benefit        | Test pyramid           | T shaped tests  | Remediation |
| ------------- |:-------------:|:-------------:| -----:|
| Execution speed     | :heavy_check_mark:  | :heavy_check_mark: |
| Ease of refactoring     | :x: | :heavy_check_mark: |
| Test all “your” code, like real users: more confidence      | :x: | :heavy_check_mark: |
| Documentation of external dependencies +++      | :x: | :heavy_check_mark: |
| Balance between production code and testing code     | :x: | :heavy_check_mark: |
| Assert quality logs    | :x: | :heavy_check_mark: |
| Assert outgoing requests    | :x: | :heavy_check_mark: |
| Ease to achieve high test coverage    | :x: | :heavy_check_mark: |
| Document requirements for other teams (outsourcing, remote working)     | :x: | :heavy_check_mark: |
| Prepare monolith for later break out      | :x: | :heavy_check_mark: |
| Easier to TDD / BDD      | :x: | :heavy_check_mark: |
| Incentive for small methods / clean code      | :heavy_check_mark: | :x: | PRs reviews, automated checks
| Quickly find bugs      | :heavy_check_mark: | :x: | Small commits
| Handle shared states (cache, circuit breaker)      | :heavy_check_mark: | :x: | Disable or use data so cache doesn’t matter

# Basic capabilities for automated testing

You can use the extension method **HttpClient.AppendHttpCallStub()** to intercept Http calls and return a stub response, then use **HttpClient.GetSessionLogs()** and **HttpClient.GetSessionOutgoingRequests()** to get all the logs and outgoing Http calls relating to your session.

Simple example:

```C#
using SystemTestingTools;
using Shouldly; // nice to have :)
using Xunit;

[Fact]
public async Task When_UserAsksForMovie_Then_ReturnMovieProperly()
{
    // arrange
    var client = Fixture.Server.CreateClient(); // usual creating of HttpClient
    client.CreateSession(); // extension method that adds a session header, should be called first
    var response = ResponseFactory.FromFiddlerLikeResponseFile($"200_Response.txt");
    var uri = new System.Uri("https://www.mydependency.com/api/SomeEndpoint");
    client.AppendHttpCallStub(HttpMethod.Get, uri, response); // will return the stub response when endpoint is called

    // act
    var httpResponse = await client.GetAsync("/api/movie/matrix");

    // assert logs  (make sure the logs were exactly how we expected, no more no less)
    var logs = client.GetSessionLogs();
    logs.Count.ShouldBe(1);
    logs[0].ToString().ShouldBe($"Info: Retrieved movie 'matrix' from downstream because it wasn't cached");

    // assert outgoing (make sure the requests were exactly how we expected)
    var outgoingRequests = client.GetSessionOutgoingRequests();
    outgoingRequests.Count.ShouldBe(1);
    outgoingRequests[0].GetEndpoint().ShouldBe($"GET https://www.mydependency.com/api/SomeEndpoint/matrix");
    outgoingRequests[0].GetHeaderValue("User-Agent").ShouldBe("My app Name");

    // assert return
    httpResponse.ShouldNotBeNull();
    httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

    var movie = await httpResponse.ReadJsonBody<MovieProject.Logic.DTO.Media>();
    movie.ShouldNotBeNull();
    movie.Id.ShouldBe("tt0133093");
    movie.Name.ShouldBe("The Matrix");
    movie.Year.ShouldBe("1999");
    movie.Runtime.ShouldBe("136 min");
}
```
*We need the concept of sessions because many tests with many requests can be happening at the same time, so we need to keep things separated

[Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/GetMovieHappyTests.cs#L53)

# Basic capabilities for stubbing in non prod environments

You can use the extension method **IServiceCollection.InterceptHttpCallsAfterSending()** to intercept Http calls (coming from HttpClient or WCF/SOAP), a lambda method will be called everytime for you to decide what to do; with a few helper methods to facilitate common requirements.

Simple example:

```C#
using SystemTestingTools;

public virtual void ConfigureServices(IServiceCollection services)
{
    services.InterceptHttpCallsAfterSending(async (intercept) => {
        if (intercept.Response?.IsSuccessStatusCode ?? false)
            await intercept.SaveAsRecording("new/unhappy"); // save for later analysis
        else
            await intercept.SaveAsRecording("new/happy"); // save so it can be used to replace an unhappy response later (and analysis)
        
        if (intercept.Response?.IsSuccessStatusCode ?? false) 
            return intercept.KeepResultUnchanged(); // if we got a happy response, just return the original, no need for stubs

        var recording = RecordingCollection.Recordings.FirstOrDefault(
            recording => recording.File.Contains("new/happy")
            && recording.Request.RequestUri.PathAndQuery == intercept.Request.RequestUri.PathAndQuery);  

        if (recording != null) // we found a happy response for the same endpoint, return it instead of original response
            return intercept.ReturnRecording(recording, "unhappy response replaced by a happy one");
        
        return intercept.KeepResultUnchanged(); // return original response
    });
}
```

# Automated testing setup

When creating your WebHostBuilder in your test project, to support HttpClient calls, add

```C#
.InterceptHttpCallsBeforeSending()
.IntercepLogs(minimumLevelToIntercept: LogLevel.Information, 
                namespaceToIncludeStart: new[] { "MovieProject" },
                namespaceToExcludeStart: new[] { "Microsoft" });
```
[Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/TestServerFixture.cs#L42)

Explanation: InterceptHttpCallsBeforeSending() will add a LAST DelegatingHandler to every HttpClient configured with services.AddHttpClient() (as recommended by Microsoft); so we can intercept and return a stub response, configured as above by the method AppendHttpCallStub().

InterceptLogs() allows namespaces to include or exclude in the logs, first the inclusion filter is executed, then the exclusion one. So if you configure namespaceToIncludeStart: new[] { "MovieProject" } namespaceToExcludeStart: new[] { "MovieProject.Proxy" }; then you will get all logs logged from classes whose namespace starts with MovieProject, except if they start with MovieProject.Proxy

# Extra capabilities

## 1 - Multiple ways of creating responses

The HttpClient extension method AppendHttpCallStub() requires you to pass a HttpResponseMessage, which will be returned instead of making a real call downstream. You can create the response in a number of ways:

#### 1.1: You can manually create a response:

```C#
var response = new HttpResponseMessage(HttpStatusCode.OK);
response.Headers.Add("Server", "Kestrel");
response.Content = new StringContent(@"{""value"":""whatever""}", Encoding.UTF8, "application/json");
```
#### 1.2: Use *ResponseFactory.From* method, which loads the body from a string:
```C#
ResponseFactory.From(@"{""Message"":""some json content""}", HttpStatusCode.OK)
```
#### 1.3: Use *ResponseFactory.FromBodyOnlyFile* method, which loads only the content of a file as the response body:
```
ResponseFactory.FromBodyOnlyFile($"response200.txt", HttpStatusCode.OK)
```
```JSON
{"Message":"some json content"}
```

#### 1.4: Use *ResponseFactory.FromFiddlerLikeResponseFile* method, which loads a file with the content just like [Fiddler Web Debugger](https://www.telerik.com/fiddler) displays responses (with the optional added comments on the top):
```C#
ResponseFactory.FromFiddlerLikeResponseFile("response200.txt")
```
Which can load a response just like it's displayed in Fiddler:
```JSON
HTTP/1.1 200 OK
Header1: some value
Header2: some other value

{"Message":"some json content"}
```

#### 1.5: (RECOMMENDED WAY) Use *ResponseFactory.FromRecordedFile* method, which loads a file created by the recording feature bellow:
```C#
ResponseFactory.FromRecordedFile("response200.txt")
```

[Example of recorded file](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/Stubs/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt)

## 2 - Stub exceptions
You can test unhappy paths during Http calls and how well you handle them with the HttpClient.AppendHttpCallStub method exception overload.

```C#
// arrange
var client = Fixture.Server.CreateClient();
client.CreateSession();
var exception = new HttpRequestException("weird network error");
client.AppendHttpCallStub(HttpMethod.Get, new System.Uri(MatrixMovieUrl), exception);

// act
var httpResponse = await client.GetAsync("/api/movie/matrix");

// assert logs
var logs = client.GetSessionLogs();
logs.Count.ShouldBe(1);
logs[0].ToString().ShouldStartWith($"Fatal: GET {MatrixMovieUrl} threw an exception: System.Net.Http.HttpRequestException: weird network error");

// assert outgoing            
var outgoingRequests = client.GetSessionOutgoingRequests();
outgoingRequests.Count.ShouldBe(1);
outgoingRequests[0].ShouldBeEndpoint($"GET {MatrixMovieUrl}");

// assert return
httpResponse.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
var message = await httpResponse.ReadBody();
message.ShouldBe(Constants.DownstreamErrorMessage);
```
[Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/GetMovieUnhappyTests.cs#L30)

## 3 - Recordings and returning stubs under certain conditions
The recorder save the request / response to a file with a bunch of useful metadata, so it can be used for analysis later or stubs for automated testing or manual testing (non prod environments).

Example of generated file:

```JSON
SystemTestingTools_Recording_V2
Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!
Date: 2020-08-31 11:28:21.791 (UTC+10:00) Canberra, Melbourne, Sydney
Recorded from: MovieProject.Web 1.0.0.0 (GET https://localhost:44374/api/movie/matrix)
Local machine: DESKTOP-ODVA6EU
User: DESKTOP-ODVA6EU\AlanPC
Using tool: SystemTestingTools 2.0.0.0 (https://github.com/AlanCS/SystemTestingTools/)
Duration: 170 ms

REQUEST
get http://www.omdbapi.com/someendpoint?search=weird_movie_title
Referer:https://github.com/AlanCS/SystemTestingTools

--!?@Divider: Any text BEFORE this line = comments, AFTER = response in Fiddler like format

HTTP/1.1 200 OK
Header1: some value
Header2: some other value

{"Message":"some json content"}
```

[Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/Stubs/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt)

Lots of class methods and extension methods can help you make your test environment more stable; by for example saving successful responses and returning those if the same endpoint fails; or providing fall back stubs or allowing stubs to be driven by headers.

You can find a full list of capabilities in the changelog, but here is a small example:

```C#
using SystemTestingTools;

public virtual void ConfigureServices(IServiceCollection services)
{
    services.InterceptHttpCallsAfterSending(async (intercept) => {
        if (intercept.Response?.IsSuccessStatusCode ?? false)
            await intercept.SaveAsRecording("new/unhappy");
        else
            await intercept.SaveAsRecording("new/happy");

        var returnStubInstruction = intercept.HttpContextAccessor.GetRequestHeaderValue("SystemTestingTools_ReturnStub");
        if (returnStubInstruction != null) // someone is testing edge cases, we return the requested stub
        {
            var stub = ResponseFactory.FromFiddlerLikeResponseFile(intercept.RootStubsFolder.AppendPath(returnStubInstruction));
            return intercept.ReturnStub(stub, "instructions from header");
        }

        if (intercept.Response?.IsSuccessStatusCode ?? false) return intercept.KeepResultUnchanged();

        var message = intercept.Summarize();

        logger.LogError(intercept.Exception, message);                

        var recording = RecordingCollection.Recordings.FirstOrDefault(
            recording => recording.File.Contains("new/happy")
            && recording.Request.RequestUri.PathAndQuery == intercept.Request.RequestUri.PathAndQuery
            && recording.Request.GetSoapAction() == intercept.Request.GetSoapAction());  

        if (recording != null)
            return intercept.ReturnRecording(recording, message);     

        var fallBackRecording = RecordingCollection.Recordings.FirstOrDefault(
            recording => recording.File.Contains("last_fallback"));

        if (fallBackRecording != null)
            return intercept.ReturnRecording(fallBackRecording, message + " and could not find better match");                                                 

        return intercept.KeepResultUnchanged();
    });
}
```

## 5 - Extension methods for HttpResponseMessage

For easy manipulation and assertions of the responses you get (specially JSON), these extensions might save you time / lines of code:

```JSON
(await httpResponse.ReadBody()).ShouldBe("An error happened, try again later"); // ReadBody will ready the body as a string

var movie = await httpResponse.ReadJsonBody<DTO.Media>(); // ReadJsonBody will parse the json response body as the given class

response.ModifyJsonBody<DTO.User[]>(dto =>
{
    dto[0].Name = "Changed in code";
});
```

ModifyJsonBody() can be useful to make small changes to a complex DTO you just loaded from disk, so you don't need to store lots of small variantions of possible responses, you can make changes to it in code. [Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/GetUserHappyTests.cs#L35)

## 6 - Supporting WFC Http calls

Unfortunately the  HTTP Client used by WCF Http calls is created internally, so intercepting it requires a bit more manual work
```C#
// in your ConfigureServices method, you will add one line to the setup of your WCF interface
services.AddSingleton<ICalculatorSoap, CalculatorSoapClient>(factory => {
    var client = new CalculatorSoapClient(new CalculatorSoapClient.EndpointConfiguration());
    client.Endpoint.Address = new EndpointAddress(_configuration["Url"]);
    client.Endpoint.EnableHttpInterception(); // add this line to  allow interception
    return client;
});
```
[Real life example](/Examples/MovieProject/MovieProject.Web/Startup.cs#L115)

And then you can stub WCF Http calls, example (and you can use the header filters to avoid crossed wires):
```C#
var wcfResponse = ResponseFactory.FromFiddlerLikeResponseFile($"SOAP_Successful_Add.txt");
client.AppendHttpCallStub(HttpMethod.Post, new System.Uri("http://www.dneonline.com/calculator.asmx"), wcfResponse, new Dictionary<string, string>() { { "SOAPAction", @"""http://tempuri.org/Add""" } });
```
[Real life example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/MathHappyTests.cs#L27)

# Recommendations

## 1 - Try to keep stub responses in text files
Try to keep your stub responses in separate text files (as opposite to in your code), for a number of benefits: 
* to keep things tidy
* to allow others to copy just the files if they need to call the same dependency
* as a form of documentation of possible responses received and what your system is prepared to handle
* as separate files that other roles (like testers or BAs) can manage themselves

[Real life Example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/Stubs/OmdbApi/Real_Responses/Happy)

## 2 - Use the recorder to generate stub responses

The recorder function will create a file that contains metadata, the request and the response, all in one human readable text file.

You could use a simpler file format, just containing the response body or just the response in Fiddler like format (as mentioned above), but using the recorder format has some valuable extra benefits:

- The metadata (like time generated, code that requested it, source computer) can be useful to detect if your stub is stale, or helping knowing who generated it so you can ask questions or knowing how to replicate it (if some machines have more permissions than others for example)
- The request data will obviously allow you to replicate the request via tools like Postman more easily, it can be useful months/years later to check if a similar will still be returned
- Having metadata/request/response all in one single file prevents this information from ever being separated / mixed up.
- The human readable format allows anyone in any roles (devs/testers/BAs/penetration testers/performance testers, ...) to participate in using / changing these files for their own uses if necessary. They can also copy them if they create a new project with the same dependency.

## 3 - Asserts in the right order
* assert logs
* assert outgoing requests
* assert returned values

Asserting in this order will make it more likely you will catch the original reason why you are getting unexpected results.

For example: if you have proper error handling, an exception throw while trying to read a downstream response will show up in the logs with great detail, trying to guess why it happened from the returned values could be a challenge.

[Real life Example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/GetMovieHappyTests.cs#L69)

## 4 - Assert non functionals

If you have non functional requirements, such as every request should send the CorrelationId header or when information is not cached it should log that as information; this is the perfect time for you to assert it's programmed as expected.

If this same requirement is applicable for many projects, you could use a shared package to enforce it everywhere.

Remember: if you have a requirement that is not enforced, then it's not a requirement.

## 5 - For each dependency, test at least the 4 most common problems and assert quality logs

It's very common to forget all the ways your requests can go wrong, and you most likely struggle to debug / fix issues if it's not easy to reproduce and you don't proactively log all the details:

* Exception thrown while trying to get response (due to wrong URL or lost network connection for example)
* Unexpected http status returned
* Could not parse the response to DTO (data transfer object)
* DTO contains error

For each of those errors, try to log:

* Full endpoint you were trying to contact (GET http://www.omdbapi.com/someendpoint?search=The_Godfather)
* if an exception was thrown: the full exception
* if (Http Status is unexpected) OR (could not parse response to DTO) OR (DTO contains error): Http Status and full response body

Logging the full response body (if there is no risk of it containing confidential information that you are not allowed to store) will allow you to find out what sort of error happened downstream or why your code was unprepared to handle the response.

Do not trust downstream dependencies to write logs to help you debug, even if you wrote the dependency yourself, you never know what sort of error could cause logs to stop flowing, so make sure your project logs all the details it might need for debugging.

[Real life Example](/Examples/MovieProject/MovieProject.Logic/Proxy/BaseProxy.cs#L65)

# Notes

## Stubs vs Recordings
 A stub (as per industry standard) in this context is a fake pre-defined response you will return instead of a real response from a downstream system. 
 
 A recording is a new concept created by SystemTestingTools, it's a subtype of stub, because it contains a response AND also the request that generated it.
 
 It unlocks quite a few more capabilities, to name just a few: 
 * matching the current request with a recording so you can return a healthy response when your downstream system is momentarily unhealthy
 * documenting how the response was obtained
 * showing how to reproduce response

# Warnings

Unfortunately life is not all rainbows and unicorns :smiley:, here are potential problems you should keep an eye out while using this tool:

## 1 - Risks if you (unintentionally) deploy the recorder function to production

It can store (in the designated folder) user private data or other confidential information (if you deal with that sort of data), in ways that don't comply to your local laws (or company rules) about privacy / security / compliance.

If that's a concern, make sure you only enable this function in non production environments; and leave the option to have a configuration to disable it in an emergency, just in case.

[Example](/Examples/MovieProject/MovieProject.Web/Startup.cs#L64)


## 2 -The recorder generated file format divider

The recorder function generates files which a comment section at the top, with metadata and request information, and the response (in Fiddler like format) at the bottom.

[Example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/Stubs/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt)

The separator can be a problem if it's also found in the request or response, throwing off the reader function.

The separator string (Newline --!?@Divider: Some text Newline) is unique enough to make this very unlikely, but it could still happen. Unfortunately this is the downside of trying to keep everything in one file and human readable, which after a lot of thinking it's a fair trade off between pros/cons.

If you stumble on this problem, maybe you could consider deleting part of the culprit string, or simply revert to store a response only text file.

# Attributions

The http stubbing and mocking part has been inspired tools that have been in the market for years, like https://github.com/nock/nock or https://www.nuget.org/packages/Moksy/ or https://github.com/justeat/httpclient-interception

Icon made by [Smashicons](https://www.flaticon.com/authors/smashicons) from [www.flaticon.com](https://www.flaticon.com/") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)
