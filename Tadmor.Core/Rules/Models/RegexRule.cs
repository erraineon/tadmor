namespace Tadmor.Core.Rules.Models
{
    public record RegexRule(string Trigger, string Reaction) : RuleBase(Reaction);
}