#pragma warning disable CS8603

using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Events;

namespace SAP.Infrastructure.Data.Configuration;

/// <summary>
/// Marten configuration for the Financial Management domain.
/// Sets up document store and basic event sourcing.
/// </summary>
public static class MartenConfiguration
{
    /// <summary>
    /// Configure Marten for the SAP Clone Financial module.
    /// </summary>
    public static IServiceCollection AddMartenFinancialStore(
        this IServiceCollection services, 
        string connectionString,
        ILogger? logger = null)
    {
        services.AddMarten(options =>
        {
            // Database connection
            options.Connection(connectionString);
            
            // Document configurations
            ConfigureDocuments(options);
            
            // Event sourcing configurations
            ConfigureEventSourcing(options);
        })
        .UseLightweightSessions(); // Use lightweight sessions for better performance

        return services;
    }

    /// <summary>
    /// Configure document mappings for entities.
    /// </summary>
    private static void ConfigureDocuments(StoreOptions options)
    {
        // Account document configuration
        var accountMapping = options.Schema.For<Account>()
            .Identity(x => x.Id);
        
        accountMapping.Index(x => x.AccountNumber, x => x.IsUnique = true);
        accountMapping.Index(x => x.AccountType);
        accountMapping.Index(x => x.IsActive);
        accountMapping.Index(x => x.ParentAccountId);
        accountMapping.Index(x => x.Currency);

        // JournalEntry document configuration
        var journalEntryMapping = options.Schema.For<JournalEntry>()
            .Identity(x => x.Id);
            
        journalEntryMapping.Index(x => x.JournalEntryNumber, x => x.IsUnique = true);
        journalEntryMapping.Index(x => x.PostingDate);
        journalEntryMapping.Index(x => x.IsPosted);
        journalEntryMapping.Index(x => x.Currency);
        journalEntryMapping.Index(x => x.CreatedAt);
    }

    /// <summary>
    /// Configure event sourcing for aggregates.
    /// </summary>
    private static void ConfigureEventSourcing(StoreOptions options)
    {
        // Register domain events
        options.Events.AddEventType<JournalEntryCreatedEvent>();
        options.Events.AddEventType<JournalEntryLineItemAddedEvent>();
        options.Events.AddEventType<JournalEntryPostedEvent>();
    }
} 