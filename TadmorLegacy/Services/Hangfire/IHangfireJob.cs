using System.Threading.Tasks;

namespace Tadmor.Services.Hangfire
{
    public interface IHangfireJob<in TOptions>
    {
        Task Do(TOptions options);
    }
}