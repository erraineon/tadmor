using Microsoft.EntityFrameworkCore;

namespace Tadmor.Core.Data.Interfaces
{
    public interface IModelExtension
    {
        void Extend(ModelBuilder modelBuilder);
    }
}