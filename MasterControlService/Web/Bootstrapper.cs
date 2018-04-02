using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses.Negotiation;

namespace MasterControlService.Web
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override NancyInternalConfiguration InternalConfiguration
        {
            get
            {
                return NancyInternalConfiguration.WithOverrides(x =>
                {
                    // Otherwise '.xml' and '.json' will get stripped off request paths
                    x.ResponseProcessors = new List<Type>
                    {
                        typeof(ResponseProcessor),
                        typeof(ViewProcessor)
                    };
                });
            }
        }
    }
}
