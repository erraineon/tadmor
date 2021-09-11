namespace Tadmor.TextGeneration.Models
{
    public record Gpt3TadmorMindOptions(string ApiKey, string ModelName, int? BufferSize, bool Enabled);
}