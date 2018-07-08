using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Hangfire;
using Hangfire.Storage;

namespace Tadmor.Services.Cron
{
    public class CronService
    {
        public void Once<TJob, TOptions>(TimeSpan delay, TOptions options) where TJob : ICronJob<TOptions>
        {
            BackgroundJob.Schedule<TJob>(job => job.Do(options), delay);
        }

        public void Every<TJob, TOptions>(TimeSpan interval, TOptions options) where TJob : ICronJob<TOptions>
        {
            BackgroundJob.Enqueue<TJob>(job => job.Do(options));
            var jobId = Guid.NewGuid().ToString();
            var cron = Hangfire.Cron.MinuteInterval((int) Math.Round(interval.TotalMinutes));
            RecurringJob.AddOrUpdate<TJob>(jobId, j => j.Do(options), cron);
        }

        public List<string> GetRecurringJobInfos(SocketGuild guild)
        {
            var recurringJobDtos = JobStorage.Current.GetConnection().GetRecurringJobs();
            var jobStrings = (from dto in recurringJobDtos
                    let options = (CronJobOptions) dto.Job.Args.Single()
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
    }
}