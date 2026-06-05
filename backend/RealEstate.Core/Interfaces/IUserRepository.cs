using System.Threading.Tasks;
using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
