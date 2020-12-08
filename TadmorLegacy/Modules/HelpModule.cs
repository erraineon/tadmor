using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Commands;
using Tadmor.Services.Options;
using Tadmor.Utils;

namespace Tadmor.Modules
{
    public class HelpModule : ModuleBase<ICommandContext>
    {
        private readonly ChatOptionsService _chatOptions;
        private readonly CommandsService _commands;

        public HelpModule(CommandsService commands, ChatOptionsService chatOptions)
        {
            _commands = commands;
            _chatOptions = chatOptions;
        }

        [Browsable(false)]
        [Command("help")]
        public async Task Help()
        {
            var prefix = _chatOptions.GetCommandsPrefix(Context.Guild);
            var embedBuilder = new EmbedBuilder()
                .WithTitle("source code")
                .WithUrl("https://github.com/erraineon/tadmor");
            var commandsByRoot = _commands.GetAvailableCommandsAsync(Context);
            var sb = new StringBuilder();
            await foreach (var commands in commandsByRoot)
            {
                await foreach (var cmd in commands)
                {
                    sb.Append($"**{prefix}{cmd.Aliases.First()}");
                    foreach (var parameter in cmd.Parameters)
                    {
                        sb.Append(' ');
                        var showAsOptional = parameter.IsOptional ||
                                             parameter.IsMultiple ||
                                             parameter.Attributes.OfType<ShowAsOptionalAttribute>().Any();
                        if (showAsOptional) sb.Append("(optional) ");
                        sb.Append(parameter.Name);
                    }

                    sb.Append("**");
                    if (cmd.Summary != null) sb.Append($": {cmd.Summary}");
                    sb.AppendLine();
                }

                if (sb.Length > 0)
                    embedBuilder.AddField(field => field
                        .WithName(commands.Key.Summary ?? commands.Key.Name)
                        .WithValue(sb.ToString()));
                sb.Clear();
            }

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }
    }
}