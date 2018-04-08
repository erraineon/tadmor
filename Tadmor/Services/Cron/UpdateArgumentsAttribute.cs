using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Tadmor.Services.Cron
{
    public class UpdateArgumentsAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext filterContext)
        {
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            //if it's a recurring job, save arguments as they are after the job executed
            if (filterContext.Exception != null) return;
            var job = filterContext.BackgroundJob;
            var recurringJob = filterContext.Connection.GetRecurringJobs()
                .SingleOrDefault(dto => dto.LastJobId == job.Id);
            if (recurringJob != null)
                new RecurringJobManager().AddOrUpdate(recurringJob.Id, job.Job, recurringJob.Cron);
        }
    }
}