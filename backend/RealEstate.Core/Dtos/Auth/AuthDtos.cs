using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Dtos.Auth
{
    public class RegisterDto
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; } = string.Empty;
    }

    public class PhoneLoginDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class VerifyOtpDto
    {
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Role { get; set; } = "User";
        public string Token { get; set; } = string.Empty;
    }
}
