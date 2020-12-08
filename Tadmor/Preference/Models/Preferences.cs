using System.Collections.Generic;
using Tadmor.Commands.Models;

namespace Tadmor.Preference.Models
{
    public class Preferences
    {
        public string CommandPrefix { get; set; } = ".";
        public List<CommandPermission> CommandPermissions { get; set; } = new();
        public List<AutoCommand> AutoCommands { get; set; } = new();
    }
}