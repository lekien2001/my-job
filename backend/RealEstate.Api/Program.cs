using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Core.Interfaces;
using RealEstate.Infrastructure.DbContext;
using RealEstate.Infrastructure.Repositories;
using RealEstate.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Connection Factory (Dapper + MySQL) làm Singleton
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

// Đăng ký Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddMemoryCache();

// 2. Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] 
    ?? throw new InvalidOperationException("JWT Secret key must be configured in appsettings.json");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Tắt HTTPS yêu cầu cho môi trường dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 3. Cấu hình CORS để cho phép Frontend Vue 3 truy cập
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173") // Port mặc định của Vite
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 4. Đăng ký Controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 5. Cài đặt Middlewares thứ tự đúng
app.UseCors("AllowVueClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
