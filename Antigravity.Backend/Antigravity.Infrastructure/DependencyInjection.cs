using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Antigravity.Application.Common.Interfaces;
using Antigravity.Infrastructure.Persistence;
using Antigravity.Infrastructure.Persistence.Repositories;
using Antigravity.Infrastructure.Services;

namespace Antigravity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=antigravity_db;User=root;Password=;";

        // Configure EF Core DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register Dapper Context
        services.AddSingleton<IDapperContext, DapperContext>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register JWT Service
        services.AddScoped<IJwtService, JwtService>();

        // Register Ollama AI Service
        services.AddHttpClient<IOllamaService, OllamaService>(client =>
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
