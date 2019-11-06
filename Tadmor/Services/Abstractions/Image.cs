using System.Threading.Tasks;

namespace Tadmor.Services.Abstractions
{
    public abstract class Image
    {
        protected Image(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public abstract Task<byte[]> GetDataAsync();
    }
}