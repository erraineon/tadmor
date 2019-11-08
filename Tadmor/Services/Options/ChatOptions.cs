using System.Collections.Generic;
using Tadmor.Services.Commands;

namespace Tadmor.Services.Options
{
    [Options]
    public class ChatOptions
    {
        public List<GuildOptions> GuildOptions { get; set; } = new List<GuildOptions>();
        public List<CommandUsagePermission> CommandUsagePermissions { get; set; } = new List<CommandUsagePermission>();
    }
}