using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstate.Core.Dtos.Post;
using RealEstate.Core.Entities;

namespace RealEstate.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<PostDetailDto?> GetByIdAsync(int id);
        Task<(IEnumerable<PostListDto> Items, int TotalCount)> GetPagedAsync(PostQueryDto query);
        Task<int> CreateAsync(Post post, List<string> imageUrls);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> ExistsByExternalIdAsync(string externalId);
    }
}
