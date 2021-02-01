using System;
using System.Linq;
using System.Threading.Tasks;
using Tadmor.Rules.Models;

namespace Tadmor.Rules.Interfaces
{
    public interface ITimeRuleProvider
    {
        Task<ILookup<ulong, TimeRule>> GetRulesByChannelId(ulong guildId, DateTime dueDate);
    }
}