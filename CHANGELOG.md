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