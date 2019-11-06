using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Tadmor.Services.Abstractions;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;
using Tadmor.Services.Hangfire;
using Tadmor.Utils;

namespace Tadmor.Modules
{
    [Summary("scheduling")]
    public class CronModule : ModuleBase<ICommandContext>
    {
        private readonly HangfireService _hangfire;
        private readonly ChatService _chatService;

        public CronModule(HangfireService hangfire, ChatService chatService)
        {
            _hangfire = hangfire;
            _chatService = chatService;
        }

        [Summary("reminds you the message after the specified amount of time")]
        [Command("remind")]
        public async Task Remind(TimeSpan delay, [Remainder] string reminder)
        {
            //to avoid permission issues
            var ownerId = (await Context.Client.GetApplicationInfoAsync()).Owner.Id;
            _hangfire.Once<CommandJob, CommandJobOptions>(delay, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = $"say {Context.User.Mention}: {reminder}",
                OwnerId = ownerId,
                ContextType = _chatService.GetClientType(Context.Client)
            });
            await ReplyAsync($"will remind in {delay.Humanize(maxUnit: TimeUnit.Year)}");
        }

        [Summary("executes the command after the specified amount of time")]
        [Command("in")]
        public async Task Once(TimeSpan delay, [Remainder] string command)
        {
            _hangfire.Once<CommandJob, CommandJobOptions>(delay, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id,
                ContextType = _chatService.GetClientType(Context.Client)
            });
            await ReplyAsync($"will execute in {delay.Humanize(maxUnit: TimeUnit.Year)}");
        }

        [Summary("executes the command based on the specified cron schedule")]
        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Command("every")]
        public async Task Every(string cron, [Remainder] string command)
        {
            _hangfire.Every<CommandJob, CommandJobOptions>(cron, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id,
                ContextType = _chatService.GetClientType(Context.Client)
            });
            var description = StringUtils.ToCronDescription(cron);
            await ReplyAsync($"will execute '{command}' {description}");
        }

        [RequireOwner(Group = "admin")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("sched")]
        public class RecurringModule : ModuleBase<ICommandContext>
        {
            private readonly HangfireService _hangfire;

            public RecurringModule(HangfireService hangfire)
            {
                _hangfire = hangfire;
            }

            [Summary("schedules a recurring e621 search to stay up to date with the specified tags")]
            [Command("e621")]
            public async Task RecurringE621Search([Remainder] string tags)
            {
                const string cron = "0 */6 * * *";
                _hangfire.Every<E621SearchJob, E621SearchJobOptions>(cron, new E621SearchJobOptions
                {
                    ChannelId = Context.Channel.Id,
                    Tags = tags
                });
                var description = StringUtils.ToCronDescription(cron);
                await ReplyAsync($"will search '{tags}' {description}");
            }

            [Summary("lists pending jobs")]
            [Command("ls")]
            public async Task ViewJobs()
            {
                var jobStrings = await _hangfire.GetJobInfos(Context.Guild);
                var jobInfo = jobStrings.Any()
                    ? string.Join(Environment.NewLine, jobStrings)
                    : throw new Exception("no jobs on this guild");
                await ReplyAsync(jobInfo);
            }

            [Summary("removes the job with the specified id")]
            [Command("rm")]
            public async Task RemoveJob(string jobId)
            {
                _hangfire.RemoveRecurringJob(jobId);
                await ReplyAsync("ok");
            }
        }
    }
}