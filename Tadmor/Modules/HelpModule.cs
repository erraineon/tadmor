using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Commands;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    public class HelpModule : ModuleBase<ICommandContext>
    {
        private readonly ChatCommandsService _commands;
        private readonly DiscordService _discord;
        private readonly IServiceProvider _services;

        public HelpModule(ChatCommandsService commands, DiscordService discord, IServiceProvider services)
        {
            _commands = commands;
            _discord = discord;
            _services = services;
        }

        [Browsable(false)]
        [Command("help")]
        public async Task Help()
        {

            var prefix = _discord.GetCommandsPrefix(Context.Guild);
            var embedBuilder = new EmbedBuilder()
                .WithTitle("source code")
                .WithUrl("https://github.com/erraineon/tadmor");
            var commandsByRoot = _commands.GetCommandsByRoot();
            var sb = new StringBuilder();
            foreach (var commands in commandsByRoot)
            {
                foreach (var cmd in commands)
                {
                    var preconditionsResult = await cmd.CheckPreconditionsAsync(Context, _services);
                    if (preconditionsResult.IsSuccess)
                    {
                        sb.Append($"**{prefix}{cmd.Aliases.First()}");
                        foreach (var parameter in cmd.Parameters)
                        {
                            sb.Append(' ');
                            if (parameter.IsOptional || parameter.IsMultiple) sb.Append("(optional) ");
                            sb.Append(parameter.Name);
                        }

                        sb.Append("**");
                        if (cmd.Summary != null) sb.Append($": {cmd.Summary}");
                        sb.AppendLine();
                    }
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