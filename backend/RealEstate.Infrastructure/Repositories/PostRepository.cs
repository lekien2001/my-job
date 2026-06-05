using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using RealEstate.Core.Dtos.Post;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PostRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<PostDetailDto?> GetByIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            
            // Query thông tin chính của Post kèm thông tin Location, Category, User
            const string postSql = @"
                SELECT p.*, 
                       l.name AS LocationName, 
                       c.name AS CategoryName, 
                       u.full_name AS UserFullName, 
                       u.avatar_url AS UserAvatarUrl
                FROM posts p
                INNER JOIN locations l ON p.location_id = l.id
                INNER JOIN categories c ON p.category_id = c.id
                LEFT JOIN users u ON p.user_id = u.id
                WHERE p.id = @Id LIMIT 1";

            var post = await connection.QueryFirstOrDefaultAsync<PostDetailDto>(postSql, new { Id = id });
            if (post == null) return null;

            // Query thêm danh sách ảnh của Post đó
            const string imageSql = "SELECT image_url FROM post_images WHERE post_id = @PostId";
            var images = await connection.QueryAsync<string>(imageSql, new { PostId = id });
            
            post.ImageUrls = images.ToList();
            return post;
        }

        public async Task<(IEnumerable<PostListDto> Items, int TotalCount)> GetPagedAsync(PostQueryDto query)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
                FROM posts p
                INNER JOIN locations l ON p.location_id = l.id
                INNER JOIN categories c ON p.category_id = c.id
                WHERE p.status = 'Active'");

            // 1. Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                sqlBuilder.Append(" AND (p.title LIKE @Keyword OR p.description LIKE @Keyword)");
                parameters.Add("Keyword", $"%{query.Keyword}%");
            }
            if (query.MinPrice.HasValue)
            {
                sqlBuilder.Append(" AND p.price >= @MinPrice");
                parameters.Add("MinPrice", query.MinPrice.Value);
            }
            if (query.MaxPrice.HasValue)
            {
                sqlBuilder.Append(" AND p.price <= @MaxPrice");
                parameters.Add("MaxPrice", query.MaxPrice.Value);
            }
            if (query.MinArea.HasValue)
            {
                sqlBuilder.Append(" AND p.area >= @MinArea");
                parameters.Add("MinArea", query.MinArea.Value);
            }
            if (query.MaxArea.HasValue)
            {
                sqlBuilder.Append(" AND p.area <= @MaxArea");
                parameters.Add("MaxArea", query.MaxArea.Value);
            }
            if (query.LocationId.HasValue)
            {
                // Lọc theo LocationId hoặc các con của nó (Ví dụ: chọn Quận 1 thì lấy cả các Phường thuộc Quận 1)
                sqlBuilder.Append(" AND (p.location_id = @LocationId OR p.location_id IN (SELECT id FROM locations WHERE parent_id = @LocationId))");
                parameters.Add("LocationId", query.LocationId.Value);
            }
            if (query.CategoryId.HasValue)
            {
                sqlBuilder.Append(" AND p.category_id = @CategoryId");
                parameters.Add("CategoryId", query.CategoryId.Value);
            }
            if (!string.IsNullOrEmpty(query.Type))
            {
                sqlBuilder.Append(" AND p.type = @Type");
                parameters.Add("Type", query.Type);
            }
            if (!string.IsNullOrEmpty(query.Source))
            {
                sqlBuilder.Append(" AND p.source = @Source");
                parameters.Add("Source", query.Source);
            }

            // 2. Đếm tổng số lượng record
            var countSql = "SELECT COUNT(*) " + sqlBuilder.ToString();
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
            {
                return (Enumerable.Empty<PostListDto>(), 0);
            }

            // 3. Sắp xếp
            sqlBuilder.Append(" ORDER BY ");
            switch (query.SortBy.ToLower())
            {
                case "price_asc":
                    sqlBuilder.Append("p.price ASC");
                    break;
                case "price_desc":
                    sqlBuilder.Append("p.price DESC");
                    break;
                case "area_asc":
                    sqlBuilder.Append("p.area ASC");
                    break;
                case "area_desc":
                    sqlBuilder.Append("p.area DESC");
                    break;
                case "newest":
                default:
                    sqlBuilder.Append("p.created_at DESC");
                    break;
            }

            // 4. Phân trang
            sqlBuilder.Append(" LIMIT @Offset, @Limit");
            parameters.Add("Offset", (query.Page - 1) * query.PageSize);
            parameters.Add("Limit", query.PageSize);

            // 5. Query lấy dữ liệu chính kèm thumbnail (ảnh đầu tiên)
            var selectSql = @"
                SELECT p.id, p.title, p.price, p.area, p.address, p.type, p.source, p.created_at,
                       l.name AS LocationName, 
                       c.name AS CategoryName,
                       (SELECT image_url FROM post_images WHERE post_id = p.id LIMIT 1) AS ThumbnailUrl " 
                + sqlBuilder.ToString();

            var items = await connection.QueryAsync<PostListDto>(selectSql, parameters);
            return (items, totalCount);
        }

        public async Task<int> CreateAsync(Post post, List<string> imageUrls)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            try
            {
                const string postSql = @"
                    INSERT INTO posts (title, description, price, area, address, location_id, category_id, type, user_id, source, external_id, external_url, contact_name, contact_phone, status)
                    VALUES (@Title, @Description, @Price, @Area, @Address, @LocationId, @CategoryId, @Type, @UserId, @Source, @ExternalId, @ExternalUrl, @ContactName, @ContactPhone, @Status);
                    SELECT LAST_INSERT_ID();";

                var postId = await connection.ExecuteScalarAsync<int>(postSql, post, transaction);

                if (imageUrls != null && imageUrls.Any())
                {
                    const string imageSql = "INSERT INTO post_images (post_id, image_url) VALUES (@PostId, @ImageUrl)";
                    var imageParams = imageUrls.Select(url => new { PostId = postId, ImageUrl = url }).ToList();
                    await connection.ExecuteAsync(imageSql, imageParams, transaction);
                }

                transaction.Commit();
                return postId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "UPDATE posts SET status = @Status WHERE id = @Id";
            var rows = await connection.ExecuteAsync(sql, new { Id = id, Status = status });
            return rows > 0;
        }

        public async Task<bool> ExistsByExternalIdAsync(string externalId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = "SELECT COUNT(*) FROM posts WHERE external_id = @ExternalId";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { ExternalId = externalId });
            return count > 0;
        }
    }
}
