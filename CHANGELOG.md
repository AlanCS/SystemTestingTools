## 1.2.0
- Breaking change
    - Migrated from .net core 2.2 to 3.1 (LTS), therefore removing dependency on Newtonsoft.Json
- Feature
    - Created new extension methods for HttpResponseMessage: ReadBody, ReadJsonBody<T> and ModifyJsonBody<T>

## 1.1.0
Improvements to how interception of HTTP calls happen, to be easier to configure and less intrusive, inspired by https://github.com/justeat/httpclient-interception.

## 1.0.0
After extensive real life testing, and with minor improvements to documentation and other non breaking changes, I decided the library was stable enough to become version 1

## 0.3.0
- Bug fix:
    - Fixed bug that only allowed one downstream dependency, otherwise code threw exception "DelegatingHandler instances provided to HttpMessageHandlerBuilder must not be reused or cached"
- Feature:
    -  When 'RequestResponseRecorder' creates a file, now the first line of metadata will be "Observations: !! EXPLAIN WHY THIS EXAMPLE IS IMPORTANT HERE !!" to encourage better documentation

## 0.2.0
- Feature:
    - Removed dependency on NLog, to enable it to work with any logging framework

## 0.1.0
- Feature:
    - First version