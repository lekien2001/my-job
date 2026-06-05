using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Core.Dtos.Post;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostsController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] PostQueryDto query)
        {
            var (items, totalCount) = await _postRepository.GetPagedAsync(query);
            return Ok(new
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { Message = "Không tìm thấy tin đăng này." });
            }
            return Ok(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { Message = "Bạn cần đăng nhập để đăng tin." });
            }

            var post = new Post
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Area = dto.Area,
                Address = dto.Address,
                LocationId = dto.LocationId,
                CategoryId = dto.CategoryId,
                Type = dto.Type,
                UserId = userId,
                Source = "Web",
                ContactName = dto.ContactName,
                ContactPhone = dto.ContactPhone,
                Status = "Active" // Mặc định hiển thị ngay
            };

            try
            {
                var postId = await _postRepository.CreateAsync(post, dto.ImageUrls);
                return CreatedAtAction(nameof(GetById), new { id = postId }, new { Id = postId, Message = "Đăng tin thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi tạo tin đăng.", Detail = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            if (status != "Pending" && status != "Active" && status != "Rejected" && status != "Hidden")
            {
                return BadRequest(new { Message = "Trạng thái cập nhật không hợp lệ." });
            }

            var success = await _postRepository.UpdateStatusAsync(id, status);
            if (!success)
            {
                return NotFound(new { Message = "Không tìm thấy tin đăng để cập nhật." });
            }

            return Ok(new { Message = "Cập nhật trạng thái tin đăng thành công." });
        }
    }
}
