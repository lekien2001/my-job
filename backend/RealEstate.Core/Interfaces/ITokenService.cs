using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
