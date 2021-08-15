using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Tadmor.Core.Commands.Attributes;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Preference.Interfaces;
using ParameterInfo = Discord.Commands.ParameterInfo;

namespace Tadmor.Core.Commands.Modules
{
    [HideInHelp]
    public class HelpModule : ModuleBase<ICommandContext>
    {
        private readonly IContextualPreferencesProvider _contextualPreferencesProvider;
        private readonly ICommandsMetadataProvider _commandsMetadataProvider;

        public HelpModule(
            IContextualPreferencesProvider contextualPreferencesProvider,
            ICommandsMetadataProvider commandsMetadataProvider)
        {
            _contextualPreferencesProvider = contextualPreferencesProvider;
            _commandsMetadataProvider = commandsMetadataProvider;
        }

        [Command("help")]
        public async Task Help()
        {
            var embedBuilder = await CreateHelpEmbedAsync();
            await ReplyAsync(string.Empty, embed: embedBuilder.Build());
        }

        private async Task<EmbedBuilder> CreateHelpEmbedAsync()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("source code")
                .WithUrl("https://github.com/erraineon/tadmor");

            var prefix = await GetPrefix();

            bool ShouldShow(IReadOnlyCollection<Attribute> attributes) =>
                !attributes.OfType<HideInHelpAttribute>().Any();

            var commandsByRoot = await _commandsMetadataProvider.GetCommandsByModuleAsync(Context);
            var sb = new StringBuilder();
            foreach (var commands in commandsByRoot.Where(g => ShouldShow(g.Key.Attributes)))
            {
                foreach (var cmd in commands.Where(c => ShouldShow(c.Attributes)))
                {
                    AppendCommandInfo(sb, prefix, cmd);
                }

                if (sb.Length > 0)
                    embedBuilder.AddField(field => field
                        .WithName(commands.Key.Summary ?? commands.Key.Name)
                        .WithValue(sb.ToString()));
                sb.Clear();
            }

            return embedBuilder;
        }

        private static void AppendCommandInfo(StringBuilder stringBuilder, string prefix, CommandInfo command)
        {
            stringBuilder.Append($"**{prefix}{command.Aliases.First()}");
            foreach (var parameter in command.Parameters)
            {
                stringBuilder.Append(' ');
                var showAsOptional = parameter.IsOptional || parameter.IsMultiple;
                if (showAsOptional) stringBuilder.Append("_[");
                if (Attribute.IsDefined(parameter.Type, typeof(NamedArgumentTypeAttribute)))
                {
                    var namedArgumentValues = GetNamedArgumentValues(parameter);
                    stringBuilder.Append(namedArgumentValues.Humanize(" "));
                }
                else stringBuilder.Append(parameter.Name.Humanize().Kebaberize().ToLower());
                if (showAsOptional) stringBuilder.Append("]_");
            }

            stringBuilder.Append("**");
            if (command.Summary != null) stringBuilder.Append($": {command.Summary}");
            stringBuilder.AppendLine();
        }

        private static IEnumerable<string> GetNamedArgumentValues(ParameterInfo parameter)
        {
            var argumentsValues = parameter.Type.GetProperties()
                .Select(property =>
                {
                    var name = property.Name.Camelize();
                    var value = property.GetCustomAttribute<DefaultValueAttribute>() is
                        {Value: string defaultValue}
                        ? defaultValue
                        : "?";
                    return $"{name}:{value}";
                });
            return argumentsValues;
        }

        private async Task<string> GetPrefix()
        {
            return (await _contextualPreferencesProvider
                    .GetContextualPreferencesAsync((IGuildChannel) Context.Channel, (IGuildUser) Context.User))
                .CommandPrefix;
        }
    }
}