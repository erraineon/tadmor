using System.Collections.Generic;
using Tadmor.MessageRendering.Models;

namespace Tadmor.MessageRendering.Interfaces
{
    public interface IMessageRenderer
    {
        byte[] DrawConversation(IList<DrawableMessage> messages);
    }
}