using System;
using System.Text.RegularExpressions;

namespace RealEstate.Core.Services
{
    public class SpamFilterService
    {
        private static readonly string[] RentSaleKeywords = new[]
        {
            "cho thuê", "cho thue", "cần thuê", "can thue", "phòng trọ", "phong tro",
            "căn hộ", "can ho", "nhà nguyên căn", "nha nguyen can", "chung cư", "chung cu",
            "ở ghép", "o ghep", "nhượng phòng", "nhuong phong", "pass phòng", "pass phong",
            "bán nhà", "ban nha", "bán đất", "ban dat", "chính chủ", "chinh chu", "m2", "triệu/tháng"
        };

        private static readonly string[] SpamKeywords = new[]
        {
            "tuyển dụng", "tuyen dung", "việc làm", "viec lam", "lương cao", "luong cao",
            "cho vay", "vay tiền", "vay tien", "hỗ trợ nợ xấu", "tài chính", "tai chinh",
            "mỹ phẩm", "my pham", "trị mụn", "tri mun", "cờ bạc", "kubet", "nhà cái",
            "crypto", "bitcoin", "forex", "tìm bạn đời", "tâm sự", "hẹn hò", "hen ho",
            "se khít", "giảm cân", "giam can", "đánh đề", "lo de"
        };

        public bool IsSpam(string text, out string reason)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                reason = "Nội dung trống";
                return true;
            }

            var lowerText = text.ToLower();

            // 1. Kiểm tra từ khóa rác
            foreach (var spamKeyword in SpamKeywords)
            {
                if (lowerText.Contains(spamKeyword))
                {
                    reason = $"Chứa từ khóa rác: '{spamKeyword}'";
                    return true;
                }
            }

            // 2. Kiểm tra xem có chứa ít nhất một từ khóa bất động sản không
            bool hasRealEstateKeyword = false;
            foreach (var keyword in RentSaleKeywords)
            {
                if (lowerText.Contains(keyword))
                {
                    hasRealEstateKeyword = true;
                    break;
                }
            }

            if (!hasRealEstateKeyword)
            {
                reason = "Không chứa từ khóa liên quan đến bất động sản";
                return true;
            }

            // 3. Phân tích thêm để lọc tin quá ngắn
            if (text.Length < 30)
            {
                reason = "Bài viết quá ngắn (dưới 30 ký tự)";
                return true;
            }

            reason = "Hợp lệ";
            return false;
        }

        // Helper để trích xuất số điện thoại từ nội dung bài đăng bằng Regex
        public string? ExtractPhoneNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            // Regex tìm số điện thoại Việt Nam dạng: 0987654321, 098.765.4321, 0987 654 321...
            var regex = new Regex(@"(0[3|5|7|8|9][0-9]{8})|(\+84[3|5|7|8|9][0-9]{8})|(0[3|5|7|8|9][0-9]{2}[\s\.\-][0-9]{3}[\s\.\-][0-9]{3})");
            var cleanText = text.Replace(" ", "");
            var match = regex.Match(cleanText);
            
            if (match.Success)
            {
                var phone = match.Value;
                if (phone.StartsWith("+84"))
                {
                    phone = "0" + phone.Substring(3);
                }
                return phone;
            }

            // Regex dự phòng tìm số viết cách nhau
            var matchFallback = Regex.Match(text, @"\b\d{4}[.\s-]?\d{3}[.\s-]?\d{3}\b");
            return matchFallback.Success ? matchFallback.Value.Replace(".", "").Replace(" ", "").Replace("-", "") : null;
        }

        // Helper để trích xuất giá (ví dụ: 3.5tr, 3 triệu, 3.500.000)
        public decimal ExtractPrice(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            var lowerText = text.ToLower();

            // Tìm mẫu dạng "3.5 triệu", "3.5tr", "3tr", "3 triệu", "3,5tr"
            var patternMillion = new Regex(@"(\d+[\.,]\d+|\d+)\s*(tr|triệu|trieu)");
            var matchMillion = patternMillion.Match(lowerText);
            if (matchMillion.Success)
            {
                var valStr = matchMillion.Groups[1].Value.Replace(",", ".");
                if (decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
                {
                    return val * 1000000;
                }
            }

            // Tìm mẫu dạng số tiền đầy đủ "3.500.000", "3500000"
            var patternFull = new Regex(@"\b(\d{1,3}([\.,]\d{3})+|\d{6,9})\b");
            var matchFull = patternFull.Match(lowerText);
            if (matchFull.Success)
            {
                var valStr = matchFull.Groups[1].Value.Replace(".", "").Replace(",", "");
                if (decimal.TryParse(valStr, out var val))
                {
                    return val;
                }
            }

            return 0; // Không tìm thấy hoặc mặc định
        }

        // Helper để trích xuất diện tích (ví dụ: 30m2, 30 m2, 30 mét vuông)
        public decimal ExtractArea(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            var lowerText = text.ToLower();

            var patternArea = new Regex(@"(\d+[\.,]\d+|\d+)\s*(m2|m²|mét vuông|met vuong)");
            var matchArea = patternArea.Match(lowerText);
            if (matchArea.Success)
            {
                var valStr = matchArea.Groups[1].Value.Replace(",", ".");
                if (decimal.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
                {
                    return val;
                }
            }
            return 0;
        }
    }
}
