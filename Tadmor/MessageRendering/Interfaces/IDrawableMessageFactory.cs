using System.Threading.Tasks;
using Discord;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Interfaces
{
    public interface IDrawableMessageFactory
    {
        Task<DrawableMessage> CreateAsync(IMessage message);
    }
}