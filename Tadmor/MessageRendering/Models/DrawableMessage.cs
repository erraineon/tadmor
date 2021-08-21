namespace Tadmor.MessageRendering.Models
{
    public record DrawableMessage(string AuthorName, string? Content, byte[]? Avatar, byte[]? Image);
}