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