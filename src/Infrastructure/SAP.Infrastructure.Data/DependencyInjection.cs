using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Repositories;
using SAP.Infrastructure.Data.Configuration;
using SAP.Infrastructure.Data.Repositories;

namespace SAP.Infrastructure.Data;

/// <summary>
/// Dependency injection configuration for the Infrastructure.Data layer.
/// Registers Marten, repositories, and related services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure.Data services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">PostgreSQL connection string</param>
    /// <param name="logger">Optional logger for Marten configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        string connectionString,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Add Marten with Financial domain configuration
        services.AddMartenFinancialStore(connectionString, logger);

        // Register repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

        return services;
    }

    /// <summary>
    /// Add Infrastructure.Data services with IConfiguration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance to read connection string from</param>
    /// <param name="logger">Optional logger for Marten configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger? logger = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=sapclone;Username=sapuser;Password=sappassword";
        
        return AddInfrastructureData(services, connectionString, logger);
    }

    /// <summary>
    /// Add Infrastructure.Data services with environment-specific configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">PostgreSQL connection string</param>
    /// <param name="isDevelopment">Whether running in development environment</param>
    /// <param name="logger">Optional logger for Marten configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureData(
        this IServiceCollection services,
        string connectionString,
        bool isDevelopment,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        // Configure Marten based on environment
        if (isDevelopment)
        {
            // Development: Enable detailed logging and auto-schema creation
            services.AddMartenFinancialStore(connectionString, logger);
        }
        else
        {
            // Production: Minimal logging, manual schema management
            services.AddMartenFinancialStore(connectionString);
        }

        // Register repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();

        return services;
    }
} 