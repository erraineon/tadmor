using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Tadmor.Core.Commands.Interfaces;
using Tadmor.Core.Commands.Models;
using Tadmor.Core.Rules.Interfaces;
using Tadmor.Core.Rules.Models;

namespace Tadmor.Core.Rules.Services
{
    public class RecursiveRuleCommandParser : IRuleCommandParser
    {
        private readonly ICommandContextFactory _commandContextFactory;
        private readonly ICommandExecutor _commandExecutor;

        public RecursiveRuleCommandParser(
            ICommandContextFactory commandContextFactory,
            ICommandExecutor commandExecutor)
        {
            _commandContextFactory = commandContextFactory;
            _commandExecutor = commandExecutor;
        }

        public async Task<string> GetCommandAsync(IRuleTriggerContext ruleTriggerContext)
        {
            var sr = new StringReader(ruleTriggerContext.Rule.Reaction);

            async Task<string> GetCommandRecursive()
            {
                var sb = new StringBuilder();
                while (sr.Peek() != -1)
                {
                    var character = (char) sr.Read();
                    if (character == '{')
                    {
                        var parsed = await GetCommandRecursive();
                        var evaluated = parsed switch
                        {
                            "user" => ruleTriggerContext.ExecuteAs.Mention,
                            var groupName
                                when GetGroupValueOrNull(ruleTriggerContext, groupName) is { } groupValue => groupValue,
                            var subCommand
                                when await ExecuteSubCommand(subCommand, ruleTriggerContext) is RuntimeResult
                                    runtimeResult => runtimeResult.Reason,
                            _ => parsed
                        };
                        sb.Append(evaluated);
                    }
                    else if (character == '}')
                    {
                        break;
                    }
                    else
                    {
                        sb.Append(character);
                    }
                }

                return sb.ToString();
            }

            return await GetCommandRecursive();
        }

        private async Task<IResult> ExecuteSubCommand(string subCommand, IRuleTriggerContext ruleTriggerContext)
        {
            var commandContext = _commandContextFactory.Create(
                subCommand,
                ruleTriggerContext.ExecuteIn,
                ruleTriggerContext.ExecuteAs,
                ruleTriggerContext.ChatClient,
                ruleTriggerContext.ReferencedMessage);
            var result = await _commandExecutor.ExecuteAsync(
                new ExecuteCommandRequest(commandContext, subCommand),
                CancellationToken.None);
            return result;
        }

        private static string? GetGroupValueOrNull(IRuleTriggerContext ruleTriggerContext, string groupName)
        {
            var groupValue = ruleTriggerContext is RegexRuleTriggerContext regexRuleTriggerContext &&
                regexRuleTriggerContext.GetMatch() is {Success: true} m &&
                m.Groups[groupName].Value is var v &&
                !string.IsNullOrEmpty(v)
                    ? v
                    : default;
            return groupValue;
        }
    }
}