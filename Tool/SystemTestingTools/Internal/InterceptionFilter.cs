using System;
using System.Net.Http;
using Microsoft.Extensions.Http;

namespace SystemTestingTools.Internal
{
    /// <summary>
    /// Will add a LAST handler to all HttpClients
    /// inspired by https://github.com/justeat/httpclient-interception
    /// </summary>
    internal class InterceptionFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly Func<DelegatingHandler> handlerCreator;

        internal InterceptionFilter(Func<DelegatingHandler> handler)
        {
            this.handlerCreator = handler;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                // Run any actions the application has configured for itself
                next(builder);

                // Add the interceptor as the last message handler
                builder.AdditionalHandlers.Add(handlerCreator());
            };
        }
    }
}
