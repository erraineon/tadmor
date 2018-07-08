using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using Tadmor.Services.Cron;
using Tadmor.Services.Discord;
using Tadmor.Services.E621;

namespace Tadmor.Modules
{
    public class CronModule : ModuleBase<ICommandContext>
    {
        private readonly CronService _cron;

        public CronModule(CronService cron)
        {
            _cron = cron;
        }

        [Command("in")]
        public async Task Once(TimeSpan delay, [Remainder] string command)
        {
            _cron.Once<CommandJob, CommandJobOptions>(delay, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id
            });
            await ReplyAsync($"will execute in {delay.Humanize(maxUnit: TimeUnit.Year)}");
        }

        [RequireOwner(Group = "admin"), RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Command("every")]
        public async Task Every(TimeSpan interval, [Remainder] string command)
        {
            _cron.Every<CommandJob, CommandJobOptions>(interval, new CommandJobOptions
            {
                ChannelId = Context.Channel.Id,
                Command = command,
                OwnerId = Context.User.Id
            });
        }

        [RequireOwner(Group = "admin"), RequireUserPermission(GuildPermission.Administrator, Group = "admin")]
        [Group("sched")]
        public class RecurringModule : ModuleBase<SocketCommandContext>
        {
            private readonly CronService _cron;

            public RecurringModule(CronService cron)
            {
                _cron = cron;
            }
            
            [Command("e621")]
            public async Task RecurringE621Search(TimeSpan interval, [Remainder] string tags)
            {
                _cron.Every<E621SearchJob, E621SearchJobOptions>(interval, new E621SearchJobOptions
                {
                    ChannelId = Context.Channel.Id,
                    Tags = tags
                });
            }
            
            [Command("ls")]
            public async Task ViewJobs()
            {
                var jobStrings = _cron.GetRecurringJobInfos(Context.Guild);
                var jobInfo = jobStrings.Any()
                    ? string.Join(Environment.NewLine, jobStrings)
                    : throw new Exception("no jobs on this guild");
                await ReplyAsync(jobInfo);
            }
            
            [Command("rm")]
            public async Task RemoveJob(string jobId)
            {
                _cron.RemoveRecurringJob(jobId);
                await ReplyAsync("ok");
            }
        }
    }
}