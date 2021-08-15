using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Formatting.Interfaces;

namespace Tadmor.Core.Commands.Formatters
{
    public class CommandPermissionFormatter : IStringFormatter<CommandPermission>
    {
        public Task<string> ToStringAsync(CommandPermission value)
        {
            var sb = new StringBuilder("permissions for ");
            sb.Append(value.CommandName == "*" ? "all commands" : $"command `{value.CommandName}`");
            sb.Append($" are set to {value.CommandPermissionType.ToString().ToLower()}");
            return Task.FromResult(sb.ToString());
        }
    }
}