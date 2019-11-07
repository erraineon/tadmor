using System.Collections.Generic;
using Tadmor.Services.Commands;

namespace Tadmor.Services.Options
{
    public class ChatOptions
    {
        public List<GuildOptions>? GuildOptions { get; set; }
        public List<CommandUsagePermission>? CommandUsagePermissions { get; set; }
    }
}