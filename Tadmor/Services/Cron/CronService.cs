using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hangfire;
using Hangfire.Storage;
using Tadmor.Extensions;
using Tadmor.Services.E621;

namespace Tadmor.Services.Cron
{
    public class CronService
    {
        private readonly DiscordSocketClient _discord;
        private readonly E621Service _e621;

        public CronService(DiscordSocketClient discord, E621Service e621)
        {
            _discord = discord;
            _e621 = e621;
        }

        [UpdateArguments]
        [CancelRecurrenceUponFailure]
        public async Task Search(E621CronJobOptions options)
        {
            if (_discord.GetChannel(options.ChannelId) is SocketTextChannel destChannel)
            {
                var (newPosts, newAfterId) = await _e621.SearchAfter(options.Tags, options.AfterId);
                if (newPosts.Any())
                {
                    options.AfterId = newAfterId;
                    foreach (var e621Post in newPosts)
                        await destChannel.SendMessageAsync("new submission", embed: e621Post.ToEmbed());
                }
            }
            else
            {
                throw new Exception("channel gone, delete schedule");
            }
        }

        public Task PostReminder(string reminder, ulong channelId, string userMention)
        {
            var channel = (SocketTextChannel) _discord.GetChannel(channelId);
            return channel.SendMessageAsync($"{userMention}: {reminder}");
        }

        public void Remind(TimeSpan delay, string reminder, ulong channelId, string userMention)
        {
            BackgroundJob.Schedule<CronService>(
                discord => discord.PostReminder(reminder, channelId, userMention), delay);
        }

        public async Task RecurringSearch(E621CronJobOptions options, TimeSpan minutesInterval)
        {
            await Search(options);
            var jobId = Guid.NewGuid().ToString();
            var cron = Hangfire.Cron.MinuteInterval((int) Math.Round(minutesInterval.TotalMinutes));
            RecurringJob.AddOrUpdate<CronService>(jobId, job => job.Search(options), cron);
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