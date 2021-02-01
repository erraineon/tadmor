using System.Threading.Tasks;

namespace Tadmor.Rules.Interfaces
{
    public interface IRuleCommandParser
    {
        Task<string> GetCommandAsync(IRuleTriggerContext ruleTriggerContext);
    }
}