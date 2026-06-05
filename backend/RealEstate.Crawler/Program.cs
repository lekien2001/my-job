using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RealEstate.Core.Interfaces;
using RealEstate.Core.Services;
using RealEstate.Infrastructure.DbContext;
using RealEstate.Infrastructure.Repositories;
using RealEstate.Crawler;

var builder = Host.CreateApplicationBuilder(args);

// Đăng ký DB & Repositories
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddSingleton<SpamFilterService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
