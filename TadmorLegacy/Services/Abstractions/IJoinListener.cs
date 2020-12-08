using System.Threading.Tasks;
using Discord;

namespace Tadmor.Services.Abstractions
{
    public interface IJoinListener
    {
        Task OnUserJoinedAsync(IDiscordClient client, IGuildUser user);
    }
}