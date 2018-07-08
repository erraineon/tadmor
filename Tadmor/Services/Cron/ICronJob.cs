using System.Threading.Tasks;

namespace Tadmor.Services.Cron
{
    public interface ICronJob<in TOptions>
    {
        [UpdateArguments]
        [CancelRecurrenceUponFailure]
        Task Do(TOptions options);
    }
}