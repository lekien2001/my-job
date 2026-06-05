using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
    }
}
