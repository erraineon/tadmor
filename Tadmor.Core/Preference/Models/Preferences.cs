using System.Collections.Generic;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Preference.Models
{
    public class Preferences
    {
        public string? CommandPrefix { get; set; }

        // todo: figure out a way to make this some type of dictionary in which
        // adding a new element replaces the previous one. maybe keep it as a list
        // and use a helper?
        public List<CommandPermission> CommandPermissions { get; set; } = new();
        public List<RuleBase> Rules { get; set; } = new();
    }
}