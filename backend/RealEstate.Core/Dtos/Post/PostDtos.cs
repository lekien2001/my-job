using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Dtos.Post
{
    public class PostQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public int? LocationId { get; set; }
        public int? CategoryId { get; set; }
        public string? Type { get; set; } // Rent, Sale
        public string? Source { get; set; } // Web, Facebook
        public string SortBy { get; set; } = "newest"; // newest, price_asc, price_desc, area_asc, area_desc
    }

    public class PostListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public string Address { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Type { get; set; } = "Rent";
        public string Source { get; set; } = "Web";
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PostDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Area { get; set; }
        public string Address { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Type { get; set; } = "Rent";
        public int? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserAvatarUrl { get; set; }
        public string Source { get; set; } = "Web";
        public string? ExternalId { get; set; }
        public string? ExternalUrl { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string Status { get; set; } = "Active";
        public List<string> ImageUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PostCreateDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả chi tiết là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 9999999999999.99, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Diện tích không hợp lệ")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Địa chỉ chi tiết là bắt buộc")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khu vực là bắt buộc")]
        public int LocationId { get; set; }

        [Required(ErrorMessage = "Loại hình bất động sản là bắt buộc")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Hình thức Rent hoặc Sale là bắt buộc")]
        public string Type { get; set; } = "Rent"; // Rent, Sale

        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
