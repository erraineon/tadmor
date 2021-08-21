using System.Threading.Tasks;

namespace Tadmor.Core.Rules.Interfaces
{
    public interface IRuleCommandParser
    {
        Task<string> GetCommandAsync(IRuleTriggerContext ruleTriggerContext);
    }
}