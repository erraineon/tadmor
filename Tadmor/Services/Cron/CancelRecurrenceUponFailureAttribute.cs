using System;
using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Tadmor.Services.Cron
{
    public class CancelRecurrenceUponFailureAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext filterContext)
        {
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            //if an exception of type Exception is thrown, cancel the schedule
            if (filterContext.Exception?.InnerException?.GetType() != typeof(Exception)) return;
            var recurringJob = filterContext.Connection.GetRecurringJobs()
                .SingleOrDefault(dto => dto.LastJobId == filterContext.BackgroundJob.Id);
            if (recurringJob != null) RecurringJob.RemoveIfExists(recurringJob.Id);
        }
    }
}