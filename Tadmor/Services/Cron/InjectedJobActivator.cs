using System;
using Hangfire;

namespace Tadmor.Services.Cron
{
    public class InjectedJobActivator : JobActivator
    {
        private readonly IServiceProvider _services;

        public InjectedJobActivator(IServiceProvider services) => _services = services;

        public override object ActivateJob(Type type) => _services.GetService(type);
    }
}