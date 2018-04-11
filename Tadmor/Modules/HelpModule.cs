using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Services.Discord;

namespace Tadmor.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly DiscordService _discordService;

        public HelpModule(CommandService commands, DiscordService discordService)
        {
            _commands = commands;
            _discordService = discordService;
        }

        [Command("help")]
        public async Task Help()
        {
            var prefix = _discordService.GetCommandsPrefix(Context.Guild);
            var builder = new EmbedBuilder();

            var commandsByRoot = _commands.Modules
                .SelectMany(m => m.Commands)
                .GroupBy(c => c.Module.Parent ?? c.Module)
                .ToList();
            foreach (var commands in commandsByRoot)
            {
                var module = commands.Key;
                var sb = new StringBuilder();
                foreach (var cmd in commands)
                {
                    var result = await Task.WhenAll(cmd.Preconditions
                        .Where(p => !(p is RequireNsfwAttribute))
                        .Select(p => p.CheckPermissionsAsync(Context, cmd, default)));
                    if (result.All(r => r.IsSuccess))
                    {
                        var joinedParameters = string.Join(" ", cmd.Parameters.Select(parameter => parameter.Name));
                        sb.Append($"{prefix}{cmd.Aliases.First()} {joinedParameters}");
                        if (!string.IsNullOrEmpty(cmd.Summary)) sb.Append($": {cmd.Summary}");
                        sb.AppendLine();
                    }
                }

                if (sb.Length > 0)
                    builder.AddField(field => field
                        .WithName(module.Name.Replace("Module", string.Empty).Humanize(LetterCasing.LowerCase))
                        .WithValue(sb.ToString())
                        .WithIsInline(false));
            }

            await ReplyAsync(string.Empty, embed: builder.Build());
        }
    }
}