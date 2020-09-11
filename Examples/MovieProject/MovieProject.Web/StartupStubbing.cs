using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using SystemTestingTools;

namespace MovieProject.Web
{
    public static class StartupStubbing
    {
        public static IServiceCollection AddInterceptionAndStubs(this IServiceCollection serviceCollection, ILogger logger)
        {
            // the bellow call won't be used if IWebHostBuilder.InterceptHttpCallsBeforeSending() is used (which happens in test projects)
            return serviceCollection.InterceptHttpCallsAfterSending(async (intercept) =>
            {
                bool IsHappyPath = false;

                // we check content length because downstream returns a 200 for not found, we can tell by the size it's likely a bad response
                if (intercept.Response?.IsSuccessStatusCode ?? false && intercept.Response.Content.Headers.ContentLength > 100)
                {
                    IsHappyPath = true;
                    if (intercept.Request.RequestUri.ToString().Contains("omdb"))
                    {
                        var movieName = intercept.Request.GetQueryValue("t");
                        await intercept.SaveAsRecording("omdb/new/happy", movieName.Replace(" ", "_"), 1);
                    }
                    else
                    {
                        var action = intercept.Request.GetSoapAction();
                        action = action.Split("/").LastOrDefault();
                        await intercept.SaveAsRecording("math/new/happy", action, howManyFilesToKeep: 50);
                    }
                }

                var returnStubInstruction = intercept.HttpContextAccessor.GetRequestHeaderValue("SystemTestingTools_ReturnStub");
                if (!string.IsNullOrEmpty(returnStubInstruction)) // someone is testing edge cases, we return the requested stub
                {
                    var stub = ResponseFactory.FromFiddlerLikeResponseFile(intercept.RootStubsFolder.AppendPath(returnStubInstruction));
                    return intercept.ReturnStub(stub, "instructions from header");
                }

                if (IsHappyPath)
                    return intercept.KeepResultUnchanged();

                await intercept.SaveAsRecording("new/unhappy");

                var message = intercept.Summarize();

                logger.LogError(intercept.Exception, message);

                // get the most recent recording (stub), so we can be sure to be testing against the latest if possible
                var recentRecording = RecordingCollection.Recordings.FirstOrDefault(
                    recording => recording.File.Contains(@"new/happy")
                    && recording.Request.RequestUri.PathAndQuery == intercept.Request.RequestUri.PathAndQuery
                    && recording.Request.GetSoapAction() == intercept.Request.GetSoapAction());

                if (recentRecording != null)
                    return intercept.ReturnRecording(recentRecording, message);

                // fall back #1, return a recording from the pre-approved folder, stored in github and vouched by a developer; might not be the latest
                // but returns a good response to unblock developers
                var oldRecording = RecordingCollection.Recordings.FirstOrDefault(
                    recording => recording.File.Contains(@"pre-approved/happy")
                    && recording.Request.RequestUri.PathAndQuery == intercept.Request.RequestUri.PathAndQuery
                    && recording.Request.GetSoapAction() == intercept.Request.GetSoapAction());

                if (oldRecording != null)
                    return intercept.ReturnRecording(oldRecording, message);

                // fall back #2, we return a dummy response
                var fallBackRecording = RecordingCollection.Recordings.FirstOrDefault(
                    recording => recording.File.Contains("last_fallback"));

                if (fallBackRecording != null)
                    return intercept.ReturnRecording(fallBackRecording, message + " and could not find better match");

                return intercept.KeepResultUnchanged();
            });
        }
    }
}
