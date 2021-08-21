using System.Collections.Generic;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Interfaces
{
    public interface IMessageRenderer
    {
        byte[] RenderConversation(IList<DrawableMessage> messages);
    }
}