using System;
using Hangfire;

namespace Tadmor
{
    //in order to use the app's ioc container for jobs with hangfire
    public class IocHangfireJobActivator : JobActivator
    {
        private readonly IServiceProvider _services;

        public IocHangfireJobActivator(IServiceProvider services)
        {
            _services = services;
        }

        public override object ActivateJob(Type type)
        {
            return _services.GetService(type);
        }
    }
}