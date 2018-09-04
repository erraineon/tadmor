using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Hangfire;
using Humanizer;
using Humanizer.Localisation;
using Tadmor.Extensions;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;
using Tadmor.Services.Hangfire;

namespace Tadmor.Modules
{
    public class CronModule : ModuleBase<ICommandContext>
    {
        private readonly HangfireService _hangfire;

        public CronModule(HangfireService hangfire)
        {
            _hangfire = hangfire;
        }

        [Command("remind")]
        public async Task Remind(TimeSpan delay, [Remainder] string reminder)
        {
            //to avoid permission issues
            var ownerId = (await Context.Client.GetApplicationInfoAsync()).Owner.Id;
            _hangfire.Once<CommandJob, CommandJobOptions>(delay, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = $"say {Context.User.Mention}: {reminder}",
                OwnerId = ownerId
            });
            await ReplyAsync($"will remind in {delay.Humanize(maxUnit: TimeUnit.Year)}");
        }

        [Command("in")]
        public async Task Once(TimeSpan delay, [Remainder] string command)
        {
            _hangfire.Once<CommandJob, CommandJobOptions>(delay, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id
            });
            await ReplyAsync($"will execute in {delay.Humanize(maxUnit: TimeUnit.Year)}");
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Command("every")]
        public async Task Every(string cron, [Remainder] string command)
        {
            var description = cron.ToCronDescription();
            await ReplyAsync($"will execute '{command}' {description}");
            _hangfire.Every<CommandJob, CommandJobOptions>(cron, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id
            });
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("sched")]
        public class RecurringModule : ModuleBase<SocketCommandContext>
        {
            private readonly HangfireService _hangfire;

            public RecurringModule(HangfireService hangfire)
            {
                _hangfire = hangfire;
            }

            [Command("e621")]
            public async Task RecurringE621Search([Remainder] string tags)
            {
                _hangfire.Every<E621SearchJob, E621SearchJobOptions>(Cron.HourInterval(6), new E621SearchJobOptions
                {
                    ChannelId = Context.Channel.Id,
                    Tags = tags
                });
            }

            [Command("ls")]
            public async Task ViewJobs()
            {
                var jobStrings = _hangfire.GetRecurringJobInfos(Context.Guild);
                var jobInfo = jobStrings.Any()
                    ? string.Join(Environment.NewLine, jobStrings)
                    : throw new Exception("no jobs on this guild");
                await ReplyAsync(jobInfo);
            }

            [Command("rm")]
            public async Task RemoveJob(string jobId)
            {
                _hangfire.RemoveRecurringJob(jobId);
                await ReplyAsync("ok");
            }
        }
    }
}