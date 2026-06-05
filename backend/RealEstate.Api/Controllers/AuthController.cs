using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RealEstate.Core.Dtos.Auth;
using RealEstate.Core.Entities;
using RealEstate.Core.Interfaces;

namespace RealEstate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMemoryCache _memoryCache;

        public AuthController(IUserRepository userRepository, ITokenService tokenService, IMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _memoryCache = memoryCache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) && string.IsNullOrEmpty(dto.PhoneNumber))
            {
                return BadRequest(new { Message = "Bạn phải cung cấp Email hoặc Số điện thoại để đăng ký." });
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { Message = "Email này đã được sử dụng." });
                }
            }

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                var existingUser = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
                if (existingUser != null)
                {
                    return BadRequest(new { Message = "Số điện thoại này đã được sử dụng." });
                }
            }

            var passwordHash = !string.IsNullOrEmpty(dto.Password) 
                ? BCrypt.Net.BCrypt.HashPassword(dto.Password) 
                : null;

            var user = new User
            {
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = passwordHash,
                FullName = dto.FullName,
                Role = "User",
                Status = "Active"
            };

            var userId = await _userRepository.CreateAsync(user);
            user.Id = userId;

            var token = _tokenService.GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                Token = token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return BadRequest(new { Message = "Email hoặc mật khẩu không chính xác." });
            }

            if (user.Status != "Active")
            {
                return BadRequest(new { Message = "Tài khoản của bạn đã bị khóa hoặc chưa kích hoạt." });
            }

            var token = _tokenService.GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                Token = token
            });
        }

        [HttpPost("send-otp")]
        public IActionResult SendOtp([FromBody] PhoneLoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.PhoneNumber))
            {
                return BadRequest(new { Message = "Số điện thoại không được để trống." });
            }

            // Giả lập mã OTP: 123456
            var otpCode = "123456";
            
            // Lưu OTP vào cache trong 5 phút
            var cacheKey = $"OTP_{dto.PhoneNumber}";
            _memoryCache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

            // In log cho môi trường dev
            Console.WriteLine($"[SMS GATEWAY SIMULATOR] Gửi OTP '{otpCode}' tới số điện thoại: {dto.PhoneNumber}");

            return Ok(new { Message = "Mã OTP đã được gửi thành công (Mã mặc định: 123456)." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var cacheKey = $"OTP_{dto.PhoneNumber}";
            if (!_memoryCache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != dto.OtpCode)
            {
                // Cho phép pass nếu nhập đúng mã mặc định "123456" cho dev
                if (dto.OtpCode != "123456")
                {
                    return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn." });
                }
            }

            // Xóa OTP khỏi cache sau khi verify thành công
            _memoryCache.Remove(cacheKey);

            var user = await _userRepository.GetByPhoneNumberAsync(dto.PhoneNumber);
            if (user == null)
            {
                // Đăng ký tự động nếu số điện thoại này chưa có tài khoản
                user = new User
                {
                    PhoneNumber = dto.PhoneNumber,
                    FullName = $"Người dùng {dto.PhoneNumber}",
                    Role = "User",
                    Status = "Active"
                };
                var userId = await _userRepository.CreateAsync(user);
                user.Id = userId;
            }

            if (user.Status != "Active")
            {
                return BadRequest(new { Message = "Tài khoản của bạn đã bị khóa." });
            }

            var token = _tokenService.GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                Token = token
            });
        }
    }
}
