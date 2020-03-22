using System;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SystemTestingTools
{
    /// <summary>
    /// copied from https://medium.com/trueengineering/realization-of-the-connections-pool-with-wcf-for-net-core-with-usage-of-httpclientfactory-c2cb2676423e
    /// </summary>
    internal class CustomEndpointBehaviour : IEndpointBehavior
    {
        private readonly Func<DelegatingHandler> delegatingHander;

        public CustomEndpointBehaviour(Func<DelegatingHandler> delegatingHander)
        {
            this.delegatingHander = delegatingHander;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            bindingParameters.Add(new Func<HttpClientHandler, HttpMessageHandler>(hander => delegatingHander()));
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            
        }
    }
}
