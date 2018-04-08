using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Services.Cron;

namespace Tadmor.Modules
{
    public class CronModule : ModuleBase<SocketCommandContext>
    {
        private readonly CronService _cron;

        public CronModule(CronService cron)
        {
            _cron = cron;
        }

        [Command("in")]
        public async Task Remind(TimeSpan timeSpan, [Remainder] string reminder)
        {
            _cron.Remind(timeSpan, reminder, Context.Channel.Id, Context.User.Mention);
            await ReplyAsync($"will remind in {timeSpan.Humanize()}");
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
            public Task RecurringE621Search(TimeSpan interval, [Remainder] string tags)
            {
                return _cron.RecurringSearch(new E621CronJobOptions
                {
                    ChannelId = Context.Channel.Id,
                    Tags = tags
                }, interval);
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