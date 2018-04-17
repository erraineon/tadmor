using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DiscordService _discord;
        private readonly IServiceProvider _services;

        public HelpModule(CommandService commands, DiscordService discord, IServiceProvider services)
        {
            _commands = commands;
            _discord = discord;
            _services = services;
        }

        [Command("help")]
        public async Task Help()
        {
            ModuleInfo Root(ModuleInfo module) => module.Parent == null ? module : Root(module.Parent);

            var prefix = _discord.GetCommandsPrefix(Context.Guild);
            var embedBuilder = new EmbedBuilder();
            var commandsByRoot = _commands.Commands.GroupBy(command => Root(command.Module).Name);
            foreach (var commands in commandsByRoot)
            {
                var sb = new StringBuilder();
                foreach (var cmd in commands)
                {
                    var resultGroups = await Task.WhenAll(cmd.Preconditions
                        .Concat(cmd.Module.Preconditions)
                        .GroupBy(p => p.Group, p => p.CheckPermissionsAsync(Context, cmd, _services))
                        .Select(Task.WhenAll));
                    var preconditionsOk = resultGroups.All(resultGroup => resultGroup.Any(result => result.IsSuccess));
                    if (preconditionsOk)
                    {
                        var parameters = string.Join(", ", cmd.Parameters.Select(p => p.Name));
                        sb.Append($"{prefix}{cmd.Aliases.First()} {parameters}");
                        sb.AppendLine(cmd.Summary == default ? string.Empty : $": {cmd.Summary}");
                    }
                }

                if (sb.Length > 0)
                    embedBuilder.AddField(field => field
                        .WithName(commands.Key.Replace("Module", string.Empty))
                        .WithValue(sb.ToString()));
            }

            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }
    }
}