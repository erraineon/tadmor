using System.Threading.Tasks;

namespace Tadmor.Services.Cron
{
    public interface ICronJob<in TOptions>
    {
        Task Do(TOptions options);
    }
}