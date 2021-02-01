using System;
using System.Threading.Tasks;
using Discord.Commands;
using NCrontab;

namespace Tadmor.Commands.TypeReaders
{
    public class CrontabScheduleTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            var schedule = CrontabSchedule.TryParse(input);
            var result = schedule != null
                ? TypeReaderResult.FromSuccess(schedule)
                : TypeReaderResult.FromError(CommandError.ParseFailed,
                    "input is not a cron expression");
            return Task.FromResult(result);
        }
    }
}