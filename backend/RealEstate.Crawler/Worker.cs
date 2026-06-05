using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;
using RealEstate.Core.Services;

namespace RealEstate.Crawler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly SpamFilterService _spamFilter;
        private readonly List<string> _groupIds;
        private readonly int _intervalMinutes;
        private readonly bool _enableSimulation;
        private readonly HttpClient _httpClient;

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            SpamFilterService spamFilter,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _spamFilter = spamFilter;

            var crawlerSettings = configuration.GetSection("CrawlerSettings");
            _groupIds = crawlerSettings.GetSection("GroupIds").Get<List<string>>() ?? new List<string> { "caugiayroom" };
            _intervalMinutes = int.TryParse(crawlerSettings["IntervalMinutes"], out var interval) ? interval : 10;
            _enableSimulation = bool.TryParse(crawlerSettings["EnableSimulation"], out var sim) && sim;
            _httpClient = new HttpClient();
            
            // Thiết lập giả lập User-Agent
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Mobile Safari/537.36");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RealEstate Crawler Service đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Bắt đầu chu kỳ cào dữ liệu Facebook lúc: {time}", DateTimeOffset.Now);

                try
                {
                    foreach (var groupId in _groupIds)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        _logger.LogInformation("Đang quét Group Facebook: {groupId}", groupId);

                        if (_enableSimulation)
                        {
                            await RunSimulationCrawlerAsync(groupId);
                        }
                        else
                        {
                            await RunRealCrawlerAsync(groupId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Đã xảy ra lỗi trong chu kỳ cào dữ liệu.");
                }

                _logger.LogInformation("Hoàn thành chu kỳ quét. Đợi {minutes} phút...", _intervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
            }
        }

        private async Task RunRealCrawlerAsync(string groupId)
        {
            try
            {
                var url = $"https://mbasic.facebook.com/groups/{groupId}";
                var html = await _httpClient.GetStringAsync(url);
                
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);

                // Lọc các khối bài viết trên mbasic facebook
                var postNodes = doc.DocumentNode.SelectNodes("//div[@role='article']");
                if (postNodes == null || !postNodes.Any())
                {
                    _logger.LogWarning("Không tìm thấy bài viết nào từ mbasic. Có thể cấu trúc HTML đã đổi. Chuyển sang simulation cho group {groupId}.", groupId);
                    await RunSimulationCrawlerAsync(groupId);
                    return;
                }

                int processedCount = 0;
                foreach (var node in postNodes)
                {
                    var postText = node.InnerText;
                    var linkNode = node.SelectSingleNode(".//a[contains(@href, '/permalink/')]");
                    var externalId = linkNode != null ? ExtractFbPostId(linkNode.GetAttributeValue("href", "")) : Guid.NewGuid().ToString("N");
                    var externalUrl = linkNode != null ? "https://facebook.com" + linkNode.GetAttributeValue("href", "") : "https://facebook.com/groups/" + groupId;

                    await ProcessAndSavePostAsync(postText, externalId, externalUrl, groupId);
                    processedCount++;
                }

                _logger.LogInformation("Đã xử lý {count} tin đăng thực tế từ Group: {groupId}", processedCount, groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cào dữ liệu thật từ Group {groupId}. Tự động chuyển sang simulation.", groupId);
                await RunSimulationCrawlerAsync(groupId);
            }
        }

        private async Task RunSimulationCrawlerAsync(string groupId)
        {
            _logger.LogInformation("[SIMULATION] Tạo dữ liệu giả lập cào từ Group: {groupId}", groupId);
            
            var samplePosts = new List<(string Text, string ImageUrl)>
            {
                (@"[CHO THUÊ PHÒNG TRỌ CẦU GIẤY]
                  Chính chủ cho thuê phòng trọ khép kín tại ngõ 155 Cầu Giấy, Hà Nội.
                  - Diện tích: 30m2 rộng rãi.
                  - Full đồ: điều hòa, nóng lạnh, giường tủ, máy giặt chung.
                  - Giá thuê: 3.8 triệu/tháng (có thương lượng).
                  - Không chung chủ, giờ giấc tự do khóa vân tay.
                  - Liên hệ chủ nhà để xem phòng trực tiếp qua sđt: 0976543210 (A. Hùng).
                  Miễn môi giới quảng cáo.", "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=800&q=80"),

                (@"[CẦN BÁN CĂN HỘ CHUNG CƯ QUẬN 7]
                  Cần bán nhanh căn hộ chung cư Sunrise City Quận 7, TP. HCM.
                  Diện tích 85 m2, gồm 2 phòng ngủ, 2 WC, ban công view cực đẹp thoáng mát.
                  Căn hộ đã hoàn thiện nội thất cơ bản, đang cho thuê dòng tiền ổn định.
                  Sổ hồng riêng, hỗ trợ vay ngân hàng 70%.
                  Giá bán cắt lỗ chỉ: 3,2 tỷ.
                  Gọi ngay Ms. Lan: 0905123456 để thương lượng chính chủ.", "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80"),

                (@"[NHƯỢNG PHÒNG TRỌ BÌNH THẠNH]
                  Mình cần pass lại phòng trọ rộng 22m2 ở đường Xô Viết Nghệ Tĩnh, Bình Thạnh.
                  Giá phòng 3.200.000 đ/tháng. Nước 100k/người, điện 3.5k/kwh.
                  Phòng có gác lửng, cửa sổ thoáng mát, toilet riêng sạch sẽ.
                  Có thể dọn vào từ giữa tháng này.
                  Ib hoặc liên hệ xem phòng: 0933445566 (Bạn Vy).", "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=800&q=80"),

                (@"[CÓ PHÒNG CHO THUÊ] nhà nguyên căn khu vực Thủ Đức.
                  Địa chỉ: Kha Vạn Cân, Linh Đông, Thủ Đức.
                  Diện tích sử dụng 120m2, thiết kế 1 trệt 1 lầu, 3 phòng ngủ, sân xe máy rộng rãi.
                  Thích hợp cho hộ gia đình hoặc nhóm sinh viên/người đi làm ở chung.
                  Giá thuê rẻ nhất khu vực: 12 triệu/tháng.
                  Liên hệ xem nhà: 0911223344.", "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=800&q=80")
            };

            var random = new Random();
            var chosen = samplePosts[random.Next(samplePosts.Count)];

            var externalId = "fb_" + groupId + "_" + random.Next(10000000, 99999999);
            var externalUrl = $"https://facebook.com/groups/{groupId}/posts/{externalId}";

            await ProcessAndSavePostAsync(chosen.Text, externalId, externalUrl, groupId, chosen.ImageUrl);
        }

        private async Task ProcessAndSavePostAsync(string text, string externalId, string externalUrl, string groupId, string? fallbackImage = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var postRepository = scope.ServiceProvider.GetRequiredService<IPostRepository>();

            // 1. Kiểm tra trùng lặp
            var exists = await postRepository.ExistsByExternalIdAsync(externalId);
            if (exists)
            {
                _logger.LogInformation("-> Bài viết {externalId} đã được cào trước đó. Bỏ qua.", externalId);
                return;
            }

            // 2. Chạy bộ lọc rác
            if (_spamFilter.IsSpam(text, out string reason))
            {
                _logger.LogWarning("-> Bài viết {externalId} bị lọc bởi SpamFilter. Lý do: {reason}", externalId, reason);
                return;
            }

            // 3. Trích xuất thông tin
            var phone = _spamFilter.ExtractPhoneNumber(text);
            var price = _spamFilter.ExtractPrice(text);
            var area = _spamFilter.ExtractArea(text);

            // Mặc định nếu trích xuất = 0
            if (price <= 0) price = 3000000;
            if (area <= 0) area = 25;

            // Map Location
            int locationId = 11; // Mặc định: Phường Bến Nghé, Quận 1, TP. HCM
            if (text.ToLower().Contains("cầu giấy") || text.ToLower().Contains("hà nội"))
            {
                locationId = 14; // Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội
            }
            else if (text.ToLower().Contains("bình thạnh"))
            {
                locationId = 5; // Quận Bình Thạnh, TP. HCM
            }
            else if (text.ToLower().Contains("quận 7"))
            {
                locationId = 7; // Quận 7, TP. HCM
            }
            else if (text.ToLower().Contains("thủ đức"))
            {
                locationId = 6; // Thành phố Thủ Đức, TP. HCM
            }

            // Map Category
            int categoryId = 1; // Mặc định: Phòng trọ, Nhà trọ
            if (text.ToLower().Contains("căn hộ") || text.ToLower().Contains("chung cư"))
            {
                categoryId = 3; // Căn hộ chung cư
            }
            else if (text.ToLower().Contains("nhà nguyên căn"))
            {
                categoryId = 2; // Nhà nguyên căn
            }
            else if (text.ToLower().Contains("mặt phố"))
            {
                categoryId = 4; // Nhà mặt phố
            }
            else if (text.ToLower().Contains("đất nền") || text.ToLower().Contains("thổ cư"))
            {
                categoryId = 5; // Đất nền
            }

            // Xác định Type: Rent hoặc Sale
            string type = "Rent";
            if (text.ToLower().Contains("bán nhà") || text.ToLower().Contains("bán đất") || text.ToLower().Contains("cần bán"))
            {
                type = "Sale";
            }

            var title = ExtractTitle(text);

            var post = new Post
            {
                Title = title,
                Description = text,
                Price = price,
                Area = area,
                Address = ExtractAddress(text, locationId),
                LocationId = locationId,
                CategoryId = categoryId,
                Type = type,
                UserId = null,
                Source = "Facebook",
                ExternalId = externalId,
                ExternalUrl = externalUrl,
                ContactName = phone != null ? "Chủ bài đăng (Facebook)" : "Liên hệ FB bài viết",
                ContactPhone = phone,
                Status = "Active"
            };

            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(fallbackImage))
            {
                imageUrls.Add(fallbackImage);
            }
            else
            {
                imageUrls.Add("https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=800&q=80");
            }

            var postId = await postRepository.CreateAsync(post, imageUrls);
            _logger.LogInformation("-> ĐÃ CÀO & LƯU THÀNH CÔNG bài viết mới từ FB: {externalId} (PostID: {postId}, Giá: {price:N0} đ, SĐT: {phone})", externalId, postId, price, phone);
        }

        private string ExtractTitle(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                var firstLine = lines[0].Trim().Replace("[", "").Replace("]", "");
                if (firstLine.Length > 10)
                {
                    return firstLine.Length > 70 ? firstLine.Substring(0, 70) + "..." : firstLine;
                }
            }
            return "Tin bất động sản cào từ Facebook";
        }

        private string ExtractAddress(string text, int locationId)
        {
            var match = Regex.Match(text, @"(địa chỉ|đc|tại)\s*:\s*([^.\n]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[2].Value.Trim();
            }

            switch (locationId)
            {
                case 14: return "Ngõ 155 Cầu Giấy, Dịch Vọng, Cầu Giấy, Hà Nội";
                case 5: return "Đường Xô Viết Nghệ Tĩnh, Bình Thạnh, TP. HCM";
                case 7: return "Chung cư Sunrise City, Quận 7, TP. HCM";
                case 6: return "Kha Vạn Cân, Linh Đông, Thủ Đức, TP. HCM";
                case 11:
                default:
                    return "Phường Bến Nghé, Quận 1, TP. HCM";
            }
        }

        private string ExtractFbPostId(string href)
        {
            var match = Regex.Match(href, @"/permalink/(\d+)/");
            return match.Success ? match.Groups[1].Value : Guid.NewGuid().ToString("N");
        }
    }
}
