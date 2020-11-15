## 2.0.29
- New feature
    - New class and method EnvironmentHelper.GetProjectFolder() to get folder where you will find the stubs

## 2.0.22
- New feature
    - Added 'events' storage, so you can log string describing what happened (ie 'new item added', 'cache cleared'); useful when mocking non http dependencies

## 2.0.9
- New feature
    - Recording.ReSendRequest() and Recording.ReSendRequestAndUpdateFile() uses by default an HttpClient with automatic follow redirect and automatic decompression; this fits most scenarios, but if doesn't fit yours, you can provide your own HttpClient
    - Minor refactorings that don't affect funcionality

## 2.0.8
- Bugfix
    - Recording.ReSendRequest() and Recording.ReSendRequestAndUpdateFile() had bugs at times when the content-length header was smaller then actual content; now we rely on automatic configuration from .net
- Breaking change
    - Recording.File used to contain folder dividers mixed between foward slash and backwards slash; now it's standardized to forward slash

## 2.0.7
- Bugfix
    - fixing bug in RecordingCollection.LoadFrom() where it wasn't public

## 2.0.6
- Bugfix
    - fixing bug in IEnumerable\<Recording>.ReSendRequestAndUpdateFile() where it wouldn't work outside a running website with interception configured

## 2.0.5
- New features
    - Extension method Recording.ReSendRequest() will resend the request and return the response, but it will not update the recording itself
    - Extension method IEnumerable\<Recording>.ReSendRequestAndUpdateFile() will resend the requests and update the recordings themselves (in memory and file system)

Use cases: when you need to update the recordings because you suspect the response might change and that's relevant for your business; or the latest recording format (with extra fields) adds more value to you.

Example:

```c#
RecordingCollection.LoadFrom(folder);
await RecordingCollection.Recordings
    .Where(c => c.DateTime >= DateTime.Now.AddYears(-1)) // put any filters here (or none)
    .ReSendRequestAndUpdateFile();
```


## 2.0.4

Major new version to support a major new feature: Enable http interception after the request is sent, so you can return healthy stubs instead of a bad response, log, record the request/response or perform any action you wish.

In the spirit of 'everything as code', you provide a lambda to process HttpRequestMessage and HttpResponseMessage; and with the help of a lot of our new helper methods, you can return stubs, log detailed messages or record the request/response to the file system.

- New feature set
  - IServiceCollection.InterceptHttpCallsAfterSending() will receive a lambda with the input parameter InterceptedHttpCall, which contains fields HttpRequestMessage, HttpResponseMessage or Exception, Duration
    - Optional parameter InterceptionConfiguration, with the parameters:
        - RootStubsFolder (default = /App_Data/SystemTestingTools) where stubs / recording will be read / written from
        - ExposeStubsAs (default = Stubs), the URL where your stubs will be browsable (if you call ExposeStubsForDirectoryBrowsing() )
        - ForwardHeadersPrefix (default = SystemTestingTools), will forward any header that starts with this prefix to downstream calls; useful in conjunction with InterceptHttpCallsBeforeSending(), so you could drive stubs in downstream systems
  - IApplicationBuilder.ExposeStubsForDirectoryBrowsing() will expose the stubs folder for navigation
  - InterceptedHttpCall (besides the properties mentioned above) also contains these methods and :
    - SaveAsRecording(): will save the request and response (if no exception ocurrred) to a text file in the RootStubsFolder. Optional params:
        - relativeFolder: sub folder inside RootStubsFolder where to save, if not provided it will save to root
        - fileName: the name of the file (without extension), if not provided the HttpStatus of the response will be used (Ok, Accepted, NotFound, ...)
        - howManyFilesToKeep: if zero (the default value), it will keep 'infinite' files, if one obviously only one will be kept, and any other number will be the maximum number. If it's configured to keep more than one, it will add a number at the end of the file name, example: Ok_00001.txt, NotFound_00001.txt, NotFound_00002.txt
    - KeepResultUnchanged(): return the same result, without changing anything, same response, or if an exceptino occurred, it will be re-thrown
    - Summarize(): Returns the most important metadata of the interception: the full endpoint and the response http status OR exception message; useful if you want to log it or quickly see what happened. Examples:
        - POST http://www.dneonline.com/calculator.asmx received httpStatus [OK]
        - GET http://www.omdbapia.com?type=movie&t=matrix received exception [No such host is known.]    
    - ReturnRecording(): Return the recording response, you can obtain the recording by searching for it in RecordingCollection
    - ReturnStub(): Return a stub HttpResponse, it can obtained via one of the ResponseFactory methods
    - ReturnHandCraftedResponse(): Return a hand crafted HttpResponse created by you
    - Note: All the 'Return' methods require a string reason field, for you to explain why you are not returning the original result. This will be put in a header in the response (SystemTestingToolsStub), so consumers will know they are not receiving a live/real response. You can use the method Summarize() to help you create a reason if you wish. Example: "Recording [omdb/new/happy/matrix] reason GET http://www.omdbapia.com?type=movie&t=matrix received httpStatus [BadGateway]"
    - RootStubsFolder: exposes the root folder where all the stubs are found, can be useful to create a  full path for a stub
    - HttpContextAccessor: can be useful to check details about the current request, like headers and parameters
  - RecordingCollection contains a list of recordings in base folder. You can use this to find a recording of interest (like a succesfull response for the same endpoint you are hitting now) and return that response instead of the one you currently have    

- New features in existing capabilities
  - More extension methods for HttpRequestMessage, to make it's usage easier: GetHeaderValue(), 
  GetSoapAction(), ReadBody() and ReadBody\<T>(), GetQueryValue()
  - ServiceEndpoint.EnableHttpInterception() will allow WCF (SOAP) calls to be intercepted by both IServiceCollection.InterceptHttpCallsBeforeSending() and IWebHostBuilder.InterceptHttpCallsAfterSending()
  - IWebHostBuilder.InterceptHttpCallsBeforeSending() (formerly known as 'ConfigureInterceptionOfHttpClientCalls') has a new parameter keepListOfOutgoingRequests (optional, default=true); turn it off if you are doing performance tests, as the keeping track of calls might throw stats off or look like a memory leak.
  - HttpClient.AppendHttpCallStub() has a new parameter 'counter' (optional, default=1); to represent how many times that response stub will be returned, if more than the limit, an exception will be thrown. Set 0 for infinite, which is very useful for performance testing
  - UnsessionedData.AppendGlobalHttpCallStub will append a response (or an exception) to be returned by a when a matching request is intercepted. This is similar to HttpClient.AppendHttpCallStub, which adds stubs to a session; but the global method will add a stub to all sessions; the interceptor will look for a match in the session, and if not found it will look in the global configuration. This is useful as a 'fall back', so you don't have to configure the same response in many methods; can also be useful when doing performance testing, as the 'counter' of this global response is infinite, meaning no matter how many requests are intercepted, the same response will be returned

- Breaking changes
  - IWebHostBuilder.ConfigureInterceptionOfHttpClientCalls() has been renamed to IWebHostBuilder.InterceptHttpCallsBeforeSending(), to better show it's intent.
  - ContextRepo has been renamed to UnsessionedData, to better show it's intent.    
  - Class WcfHttpInterceptor and it's methods have been decommissioned:
    - CreateRequestResponseRecorder() is no longer necessary, ServiceEndpoint.EnableHttpInterception() enables IServiceCollection.InterceptHttpCallsBeforeSending() to detect http WCF calls
    - CreateInterceptor() is no longer necessary, ServiceEndpoint.EnableHttpInterception() enables IWebHostBuilder.InterceptHttpCallsAfterSending() to detect http WCF calls
  - Class HttpRequestMessageWrapper has been decommissioned, it was only useful to contain the date the request was sent, this can now be achieved with the extension method GetDatesSent(). Everywhere that used HttpRequestMessageWrapper now uses HttpRequestMessage
  - When using extension method GetHeaderValue() or recording requests and responses; the divider between many values in a header has been changed from comma to || (pipe + pipe), as this is a less likely divider to match an existing valid value

- Notes
    - Stub vs Recording: a stub (as per industry standard) is a fake response you will return instead of a real response from a downstream system. A recording is a subtype of stub, because it contains a response and also the request that generated it; it's a new concept created by SystemTestingTools, to enable more scenarios: matching the current request with a recording so you can return a healthy response when your downstream system is momentarily unhealthy, documenting how the response was obtained and how to reproduce it, ...


## 1.3.10
- New features (backwards compatible)
    - Recorder now generates files with duration of request and a identifier header (SystemTestingTools_Recording.V2), which will enable future features. It can only be read by the new method ResponseFactory.FromRecordedFile()    
- Bugfix
    - When recording a response without body, it would generate a file that could not be read by ResponseFactory due to misformatting

## 1.3.9
- Bugfix
    - MinimumLogLevel in IntercepLogs() was not being respected at times

## 1.3.8
- New features
    - Added new methods ReadBody() and ReadJsonBody() in HttpRequestMessageWrapper, to make it easier to verify outgoing requests

## 1.3.7
- Bugfixes
    - Recorder when receiving 204 (no content), would produce a file that could not be ready by function FromFiddlerLikeResponseFile    
    - FromBodyOnlyFile() would set content type header as text/plain, which caused problems when trying to parse to common types (json and xml), now the ending of the file (the extension) is used to guess the content type

## 1.3.6
- New feature
    - New Added HttpClient extension GetSessionId(), can be useful to interact with other tools

## 1.3.5
- Breaking change
    - Renamed mentions of "mock" to "stub", because it's a more precise word to define what is being done
    - Renamed MockInstrumentation to ContextRepo [example](/Examples/MovieProject/MovieProject.Tests/MovieProject.IsolatedTests/ComponentTesting/TestServerFixture.cs#L67)
- Bug fix
    - Interpret content type "text/json" as JSON (not only "application/json"), so recorder will try to format it nicely

## 1.3.4
- Bugfixes
    - ReadJsonBody<T> now parses enums as strings
- Change
    - Recorded file now shows assembly name and version and url of initial request like MovieProject.Web 1.0.0.0 (GET https://localhost:44374/api/movie/matrix)

## 1.3.3
- New feature
    - New HttpRequestMessageWrapper methods: GetEndpoint() will return "HttpMethod FullUrl", GetHeaderValue("headerName") will return header values separated by comma

## 1.3.2
- Bug fix
    - ReadJsonBody<T> is now case insensitive
    - XML responses saved by Recorder are now properly indented for easy visualization

## 1.3.1
- Bug fix
    - Fixed bug where generated recorded files, the displayed source file was incorrect

## 1.3.0
- New feature
    - Introduced support for interception WCF HTTP calls

## 1.2.0
- Breaking change
    - Migrated from .net core 2.2 to 3.1 (LTS), therefore removing dependency on Newtonsoft.Json
- New feature
    - Created new extension methods for HttpResponseMessage: ReadBody, ReadJsonBody<T> and ModifyJsonBody<T>

## 1.1.0
- Breaking change
    - Improvements to how interception of HTTP calls happen, to be easier to configure and less intrusive, inspired by https://github.com/justeat/httpclient-interception.

## 1.0.0
- Change
    - After extensive real life testing, and with minor improvements to documentation and other non breaking changes, I decided the library was stable enough to become version 1

## 0.3.0
- Bug fix
    - Fixed bug that only allowed one downstream dependency, otherwise code threw exception "DelegatingHandler instances provided to HttpMessageHandlerBuilder must not be reused or cached"
- New feature
    -  When 'RequestResponseRecorder' creates a file, now the first line of metadata will be "Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!" to encourage better documentation

## 0.2.0
- New feature
    - Removed dependency on NLog, to enable it to work with any logging framework

## 0.1.0
- New feature
    - First version