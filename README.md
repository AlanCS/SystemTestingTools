[<img align="right" src="https://i.imgur.com/DdoC5Il.png" width="100" />](https://www.nuget.org/packages/Polly/)

**SystemTestingTools** (for .net core 2.2+) allows you to extend the capabilities of Microsoft.AspNetCore.TestHost.TestServer, allowing you to run more comprehensive + deterministic tests by:

* supporting Http calls:
    * intercepting of outgoing calls, returning mock responses
    * asserting outgoing calls
    * recording live outgoing calls (requests and responses)
* asserting logs

[Nuget package](https://www.nuget.org/packages/SystemTestingTools)

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


# Basic capabilities

You can use the extension method **HttpClient.AppendMockHttpCall()** to intercept Http calls and return a mock response, then use **HttpClient.GetSessionLogs()** and **HttpClient.GetSessionOutgoingRequests()** to get all the logs and outgoing Http calls relating to your session.

Simple example:

```C#
using 
TestingTools;
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
    client.AppendMockHttpCall(HttpMethod.Get, uri, response); // will return the mock response when endpoint is called

    // act
    var httpResponse = await client.GetAsync("/api/movie/matrix");

    // assert logs  (make sure the logs were exactly how we expected, no more no less)
    var logs = client.GetSessionLogs();
    logs.Count.ShouldBe(1);
    logs[0].ToString().ShouldBe($"Info: Retrieved movie 'matrix' from downstream because it wasn't cached");

    // assert outgoing (make sure the requests were exactly how we expected)
    var outgoingRequests = client.GetSessionOutgoingRequests();
    outgoingRequests.Count.ShouldBe(1);
    outgoingRequests[0].ShouldBeEndpoint($"GET https://www.mydependency.com/api/SomeEndpoint/matrix");
    outgoingRequests[0].ShouldContainHeader("User-Agent", "My app Name");

    // assert return
    httpResponse.ShouldNotBeNull();
    httpResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

    var movie = JsonConvert.DeserializeObject<DTO.Movie>(await httpResponse.GetResponseString());
    movie.ShouldNotBeNull();
    movie.Id.ShouldBe("tt0133093");
    movie.Name.ShouldBe("The Matrix");
    movie.Year.ShouldBe("1999");
    movie.Runtime.ShouldBe("136 min");
}
```
*We need the concept of sessions because many tests with many requests can be happening at the same time, so we need to keep things separated

[Real life example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/GetMovieHappyTests.cs#L49)

# Setup

## 1- Startup.cs file

Setup a static DelegatingHandler:

```C#
public static Func<DelegatingHandler> GlobalLastHandlerFactory = null;
```

then for every HttpClient you configure, add the above Handler if not null as the last handler (if you have others):

```C#
services.AddHttpClient("movieApi", c =>
{
    c.BaseAddress = new Uri("https://www.mydependency.com");
    c.DefaultRequestHeaders.Add("User-Agent", "My app Name");
})
.ConfigureHttpMessageHandlerBuilder((c) => {
    if (GlobalLastHandlerFactory != null) c.AdditionalHandlers.Add(GlobalLastHandlerFactory());
});
```

[Real life example](/Examples/MovieProject/MovieProject.Web/Startup.cs#L70)

## 2- Creating a TestServer

Add the line

```C#
Startup.GlobalLastHandlerFactory = () => new HttpCallsInterceptorHandler();
```

And when creating your WebHostBuilder, also add

```C#
.ConfigureInterceptionOfHttpCalls()
.IntercepLogs(minimumLevelToIntercept: LogLevel.Information, 
                namespaceToIncludeStart: new[] { "MovieProject" },
                namespaceToExcludeStart: new[] { "Microsoft" })
```
[Real life example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/TestServerFixture.cs#L32)

# Extra capabilities

## 1 - Multiple ways of creating responses

The HttpClient extension method AppendMockHttpCall() requires you to pass a HttpResponseMessage, which will be returned instead of making a real call downstream. You can create the response in a number of ways:

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

#### 1.4: (RECOMMENDED WAY) Use *ResponseFactory.FromFiddlerLikeResponseFile* method, which loads a file with the content just like [Fiddler Web Debugger](https://www.telerik.com/fiddler) displays responses (with the optional added comments on the top):
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
Or a file generated by the recording feature, as mentioned bellow.

## 2 - Mock exceptions
You can test unhappy paths during Http calls and how well you handle them with the HttpClient.AppendMockHttpCall method exception overload.

```C#
// arrange
var client = Fixture.Server.CreateClient();
client.CreateSession();
var exception = new HttpRequestException("weird network error");
client.AppendMockHttpCall(HttpMethod.Get, new System.Uri(MatrixMovieUrl), exception);

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
httpResponse.ShouldNotBeNullAndHaveStatus(HttpStatusCode.InternalServerError);
var message = await httpResponse.GetResponseString();
message.ShouldBe(Constants.DownstreamErrorMessage);
```
[Real life example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/GetMovieUnhappyTests.cs#L30)

## 3 - Recorder
If you have an existing solution, you might find useful to use a recorder to store the existing requests and responses in a text file, so you can use them for mocking later!

Once you are setup your  Startup.cs file (as per instructions above), just setup the GlobalHandler to record like this:

```C#
public static Func<DelegatingHandler> GlobalLastHandlerFactory = () => new SystemTestingTools.RequestResponseRecorder("C:\\temp");
```

It's recommended you don't commit this code to production :) use it only for quickly creating mock responses for loading later using the ResponseFactory.FromFiddlerLikeResponseFile() method

Example of generated file:

```JSON
METADATA
Date: 2019-03-20 18:51:47.189 (UTC+10:00) Canberra, Melbourne, Sydney
Requested by code: C:\SystemTestingTools\Examples\MovieProject\MovieProject.Web\Startup.cs
Local machine: DESKTOP-OGVA1EU
User: DESKTOP-OGVA1EU\AlanPC
Using tool: SystemTestingTools 0.1.0.0 (https://github.com/AlanCS/SystemTestingTools/)

REQUEST
get http://www.omdbapi.com/someendpoint?search=weird_movie_title
Referer:https://github.com/AlanCS/SystemTestingTools

--!?@Divider: Any text BEFORE this line = comments, AFTER = response in Fiddler like format

HTTP/1.1 200 OK
Header1: some value
Header2: some other value

{"Message":"some json content"}
```

It's useful to keep some metadata like the date the mock was generated and how the request looked like, so if in the future people decide to leverage the same mocks, they know if it's out of date or how they can easily generate new ones.

[Real life example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/Mocks/OmdbApi/Real_Responses/Happy/200_FewFields_OldMovie.txt)


# Recommendations

## 1 - Try to keep mock responses in text files
Try to keep your mock responses in separate text files (as opposite to in your code), for a number of benefits: 
* to keep things tidy
* to allow others to copy just the files if they need to call the same dependency
* as a form of documentation of possible responses received and what your system is prepared to handle
* as separate files that other roles (like testers or BAs) can manage themselves

[Real life Example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/Mocks/OmdbApi/Real_Responses/Happy)

## 2 - Use the recorder to generate mock responses

The recorder function will create a file that contains metadata, the request and the response, all in one human readable text file.

You could use a simpler file format, just containing the response body or just the response in Fiddler like format (as mentioned above), but using the recorder format has some valuable extra benefits:

- The metadata (like time generated, code that requested it, source computer) can be useful to detect if your mock is stale, or helping knowing who generated it so you can ask questions or knowing how to replicate it (if some machines have more permissions than others for example)
- The request data will obviously allow you to replicate the request via tools like Postman more easily, it can be useful months/years later to check if a similar will still be returned
- Having metadata/request/response all in one single file prevents this information from ever being separated / mixed up.
- The human readable format allows anyone in any roles (devs/testers/BAs/penetration testers/performance testers, ...) to participate in using / changing these files for their own uses if necessary. They can also copy them if they create a new project with the same dependency.

## 3 - Asserts in the right order
* assert logs
* assert outgoing requests
* assert returned values

Asserting in this order will make it more likely you will catch the original reason why you are getting unexpected results.

For example: if you have proper error handling, an exception throw while trying to read a downstream response will show up in the logs with great detail, trying to guess why it happened from the returned values could be a challenge.

[Real life Example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/GetMovieHappyTests.cs#L58)

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

[Real life Example](/Examples/MovieProject/MovieProject.Logic/Proxy/MovieDatabaseProxy.cs#L36)

# Warnings

Unfortunately life is not all rainbows and unicorns :smiley:, here are potential problems you should keep an eye out while using this tool:

## 1 - Risks if you (unintentionally) deploy the recorder function to production

It can store (in the designated folder) user private data or other confidential information (if you deal with that sort of data), in ways that don't comply to your local laws (or company rules) about privacy / security / compliance.

If that's a concern, make sure you just use this function in local testing or non-production environments.

Obviously you don't need to worry about using the other functions of this tool, as they only require you to add this package to your test projects, the web project that goes to production remains untouched.

## 2 -The recorder generated file format divider

The recorder function generates files which a comment section at the top, with metadata and request information, and the response (in Fiddler like format) at the bottom.

[Example](/Examples/MovieProject/MovieProjectTests/MovieProject.IsolatedTests/SystemTesting/Mocks/OmdbApi/Real_Responses/Happy/200_ContainsMostFields_TheMatrix.txt)

The separator can be a problem if it's also found in the request or response, throwing off the reader function.

The separator string (Newline --!?@Divider: Some text Newline) is unique enough to make this very unlikely, but it could still happen. Unfortunately this is the downside of trying to keep everything in one file and human readable, which after a lot of thinking it's a fair trade off between pros/cons.

If you stumble on this problem, maybe you could consider deleting part of the culprit string, or simply revert to store a response only text file.

# Attributions

The http mocking part has been inspired tools that have been in the market for years, like https://github.com/nock/nock or https://www.nuget.org/packages/Moksy/ 

Icon made by [Smashicons](https://www.flaticon.com/authors/smashicons) from [www.flaticon.com](https://www.flaticon.com/") is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/)
