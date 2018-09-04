using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hangfire;
using Hangfire.SQLite;
using Hangfire.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Tadmor.Services.Hangfire
{
    public class HangfireService : IHostedService
    {
        private BackgroundJobServer _server;

        public HangfireService(IServiceProvider services, IConfiguration configuration) 
            : this(new InjectedJobActivator(services), configuration.GetConnectionString("Hangfire"))
        {
        }

        private HangfireService(JobActivator activator, string connectionString)
        {
            GlobalConfiguration.Configuration
                .UseActivator(activator)
                .UseSQLiteStorage(connectionString)
                .UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
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

        public List<string> GetRecurringJobInfos(SocketGuild guild)
        {
            var recurringJobDtos = JobStorage.Current.GetConnection().GetRecurringJobs();
            var jobStrings = (from dto in recurringJobDtos
                    let options = (HangfireJobOptions) dto.Job.Args.Single()
                    let channel = guild.GetTextChannel(options.ChannelId)
                    where channel != null
                    select options.ToString(dto, channel))
                .ToList();
            return jobStrings;
        }

        public void RemoveRecurringJob(string jobId)
        {
            var jobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            var jobToRemove = jobs.SingleOrDefault(job => job.Id == jobId) ?? throw new Exception("job not found");
            RecurringJob.RemoveIfExists(jobToRemove.Id);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //WorkerCount must be one when using sqlite or jobs will fire multiple times
            _server = new BackgroundJobServer(new BackgroundJobServerOptions { WorkerCount = 1 });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Dispose();
            return Task.CompletedTask;
        }
    }
}