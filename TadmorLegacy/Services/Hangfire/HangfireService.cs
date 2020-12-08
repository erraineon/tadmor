using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Hangfire;
using Hangfire.SQLite;
using Hangfire.Storage;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Tadmor.Utils;

namespace Tadmor.Services.Hangfire
{
    [SingletonService]
    public class HangfireService : IHostedService
    {
        private BackgroundJobServer? _server;

        public HangfireService(IServiceProvider services, IConfiguration configuration)
            : this(new InjectedJobActivator(services), configuration.GetConnectionString("Hangfire"))
        {
        }

        private HangfireService(JobActivator activator, string connectionString)
        {
            GlobalConfiguration.Configuration
                .UseActivator(activator)
                .UseSQLiteStorage(connectionString)
                .UseFilter(new AutomaticRetryAttribute {Attempts = 0});
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //WorkerCount must be one when using sqlite or jobs will fire multiple times
            _server = new BackgroundJobServer(new BackgroundJobServerOptions {WorkerCount = 1});
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.Dispose();
            return Task.CompletedTask;
        }

        public void Once<TJob, TOptions>(TimeSpan delay, TOptions options) where TJob : IHangfireJob<TOptions>
        {
            BackgroundJob.Schedule<TJob>(job => job.Do(options), delay);
        }

        public void Every<TJob, TOptions>(string cron, TOptions options) where TJob : IHangfireJob<TOptions>
        {
            var jobId = Guid.NewGuid().ToString();
            RecurringJob.AddOrUpdate<TJob>(jobId, j => j.Do(options), cron);
        }

        public async Task<List<string>> GetJobInfos(IGuild guild)
        {
            var channels = await guild.GetTextChannelsAsync();
            var storageConnection = JobStorage.Current.GetConnection();
            var recurringJobDtos = storageConnection.GetRecurringJobs();
            var oneTimeJobs = JobStorage.Current.GetMonitoringApi().ScheduledJobs(0, int.MaxValue);
            var jobStrings = (from dto in recurringJobDtos
                    let options = (HangfireJobOptions) dto.Job.Args.Single()
                    let channel = channels.SingleOrDefault(c => c.Id == options.ChannelId)
                    where channel != null
                    select options.ToString(dto.Id, StringUtils.ToCronDescription(dto.Cron), channel))
                .Concat(from job in oneTimeJobs
                    let options = (HangfireJobOptions) job.Value.Job.Args.Single()
                    let channel = channels.SingleOrDefault(c => c.Id == options.ChannelId)
                    where channel != null
                    select options.ToString(job.Key, job.Value.EnqueueAt.Humanize(), channel))
                .ToList();
            return jobStrings;
        }

        public void RemoveRecurringJob(string jobId)
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            if (recurringJobs.SingleOrDefault(job => job.Id == jobId) is { } jobToRemove)
                RecurringJob.RemoveIfExists(jobToRemove.Id);
            else if (!BackgroundJob.Delete(jobId))
                throw new Exception("job not found");
        }
    }
}