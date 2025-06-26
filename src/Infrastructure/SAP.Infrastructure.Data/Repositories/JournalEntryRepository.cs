using Marten;
using Marten.Events;
using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Repositories;

namespace SAP.Infrastructure.Data.Repositories;

/// <summary>
/// Marten-based implementation of the JournalEntry repository with event sourcing.
/// Uses event streams to persist and reconstruct journal entry aggregates.
/// </summary>
public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly IDocumentSession _session;
    private readonly ILogger<JournalEntryRepository> _logger;

    public JournalEntryRepository(IDocumentSession session, ILogger<JournalEntryRepository> logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving journal entry with ID: {JournalEntryId}", id);

        try
        {
            // Load the aggregate from event stream
            var journalEntry = await _session.Events
                .AggregateStreamAsync<JournalEntry>(id, token: cancellationToken);

            if (journalEntry != null)
            {
                _logger.LogDebug("Found journal entry: {JournalEntryNumber} with {LineItemCount} line items", 
                    journalEntry.JournalEntryNumber, journalEntry.LineItems.Count);
            }
            else
            {
                _logger.LogDebug("Journal entry not found with ID: {JournalEntryId}", id);
            }

            return journalEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry with ID: {JournalEntryId}", id);
            throw;
        }
    }

    public async Task<JournalEntry?> GetByJournalEntryNumberAsync(string journalEntryNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journalEntryNumber))
            throw new ArgumentException("Journal entry number cannot be null or empty", nameof(journalEntryNumber));

        _logger.LogDebug("Retrieving journal entry with number: {JournalEntryNumber}", journalEntryNumber);

        try
        {
            // Query the read model for the journal entry number, then load from event stream
            var journalEntry = await _session
                .Query<JournalEntry>()
                .FirstOrDefaultAsync(je => je.JournalEntryNumber == journalEntryNumber, cancellationToken);

            if (journalEntry != null)
            {
                // Reload from event stream to ensure we have the latest state
                journalEntry = await GetByIdAsync(journalEntry.Id, cancellationToken);
                
                _logger.LogDebug("Found journal entry: {JournalEntryNumber} - {Description}", 
                    journalEntry?.JournalEntryNumber, journalEntry?.Description);
            }

            return journalEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry with number: {JournalEntryNumber}", journalEntryNumber);
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetByPostingDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving journal entries for date range {FromDate} to {ToDate}", fromDate, toDate);

        try
        {
            var journalEntries = await _session
                .Query<JournalEntry>()
                .Where(je => je.PostingDate >= fromDate && je.PostingDate <= toDate)
                .OrderByDescending(je => je.PostingDate)
                .ThenByDescending(je => je.CreatedAt)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {JournalEntryCount} journal entries for date range", journalEntries.Count);
            return journalEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries for date range {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetPostedByPeriodAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving posted journal entries for period {FromDate} to {ToDate}", fromDate, toDate);

        try
        {
            var journalEntries = await _session
                .Query<JournalEntry>()
                .Where(je => je.IsPosted && je.PostingDate >= fromDate && je.PostingDate <= toDate)
                .OrderByDescending(je => je.PostingDate)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {JournalEntryCount} posted journal entries for period", journalEntries.Count);
            return journalEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posted journal entries for period {FromDate} to {ToDate}", fromDate, toDate);
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be null or empty", nameof(createdBy));

        _logger.LogDebug("Retrieving journal entries created by: {CreatedBy}", createdBy);

        try
        {
            var journalEntries = await _session
                .Query<JournalEntry>()
                .Where(je => je.CreatedBy == createdBy)
                .OrderByDescending(je => je.CreatedAt)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {JournalEntryCount} journal entries created by {CreatedBy}", 
                journalEntries.Count, createdBy);
            return journalEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries created by {CreatedBy}", createdBy);
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference cannot be null or empty", nameof(reference));

        _logger.LogDebug("Retrieving journal entries with reference: {Reference}", reference);

        try
        {
            var journalEntries = await _session
                .Query<JournalEntry>()
                .Where(je => je.Reference == reference)
                .OrderByDescending(je => je.PostingDate)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {JournalEntryCount} journal entries with reference {Reference}", 
                journalEntries.Count, reference);
            return journalEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries with reference {Reference}", reference);
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetUnpostedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving unposted journal entries");

        try
        {
            var unpostedEntries = await _session
                .Query<JournalEntry>()
                .Where(je => !je.IsPosted)
                .OrderBy(je => je.CreatedAt)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {UnpostedEntryCount} unposted journal entries", unpostedEntries.Count);
            return unpostedEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unposted journal entries");
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving journal entries for account: {AccountId}", accountId);

        try
        {
            var journalEntries = await _session
                .Query<JournalEntry>()
                .Where(je => je.LineItems.Any(li => li.AccountId == accountId))
                .OrderByDescending(je => je.PostingDate)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {JournalEntryCount} journal entries for account {AccountId}", 
                journalEntries.Count, accountId);
            return journalEntries.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries for account {AccountId}", accountId);
            throw;
        }
    }



    public async Task<bool> ExistsWithJournalEntryNumberAsync(string journalEntryNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(journalEntryNumber))
            throw new ArgumentException("Journal entry number cannot be null or empty", nameof(journalEntryNumber));

        _logger.LogDebug("Checking if journal entry number exists: {JournalEntryNumber}", journalEntryNumber);

        try
        {
            var exists = await _session
                .Query<JournalEntry>()
                .AnyAsync(je => je.JournalEntryNumber == journalEntryNumber, cancellationToken);

            _logger.LogDebug("Journal entry number exists check for {JournalEntryNumber}: {Exists}", 
                journalEntryNumber, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if journal entry number exists: {JournalEntryNumber}", journalEntryNumber);
            throw;
        }
    }

    public async Task<string> GetNextJournalEntryNumberAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating next journal entry number");

        try
        {
            // Get the latest journal entry number and increment
            var latestEntry = await _session
                .Query<JournalEntry>()
                .OrderByDescending(je => je.JournalEntryNumber)
                .FirstOrDefaultAsync(cancellationToken);

            string nextNumber;
            if (latestEntry != null)
            {
                // Extract numeric part and increment (assuming format like JE-000001)
                var numberPart = latestEntry.JournalEntryNumber.Replace("JE-", "");
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = $"JE-{(currentNumber + 1):D6}";
                }
                else
                {
                    nextNumber = "JE-000001";
                }
            }
            else
            {
                nextNumber = "JE-000001";
            }

            _logger.LogDebug("Generated next journal entry number: {NextNumber}", nextNumber);
            return nextNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next journal entry number");
            throw;
        }
    }

    public async Task<JournalEntry> AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogInformation("Adding new journal entry: {JournalEntryNumber} - {Description}", 
            journalEntry.JournalEntryNumber, journalEntry.Description);

        try
        {
            // Store the events in the event stream
            _session.Events.StartStream<JournalEntry>(journalEntry.Id, journalEntry.DomainEvents.ToArray());
            
            // Also store as a document for easier querying
            _session.Store(journalEntry);
            
            await _session.SaveChangesAsync(cancellationToken);

            // Clear domain events after persisting
            journalEntry.ClearDomainEvents();

            _logger.LogInformation("Successfully added journal entry: {JournalEntryId} - {JournalEntryNumber}", 
                journalEntry.Id, journalEntry.JournalEntryNumber);
            
            return journalEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding journal entry: {JournalEntryNumber}", journalEntry.JournalEntryNumber);
            throw;
        }
    }

    public async Task<JournalEntry> UpdateAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
    {
        if (journalEntry == null)
            throw new ArgumentNullException(nameof(journalEntry));

        _logger.LogInformation("Updating journal entry: {JournalEntryId} - {JournalEntryNumber}", 
            journalEntry.Id, journalEntry.JournalEntryNumber);

        try
        {
            // Append new events to the stream
            if (journalEntry.DomainEvents.Any())
            {
                _session.Events.Append(journalEntry.Id, journalEntry.DomainEvents.ToArray());
            }

            // Update the document store for querying
            _session.Store(journalEntry);
            
            await _session.SaveChangesAsync(cancellationToken);

            // Clear domain events after persisting
            journalEntry.ClearDomainEvents();

            _logger.LogInformation("Successfully updated journal entry: {JournalEntryId} - {JournalEntryNumber}", 
                journalEntry.Id, journalEntry.JournalEntryNumber);
            
            return journalEntry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating journal entry: {JournalEntryNumber}", journalEntry.JournalEntryNumber);
            throw;
        }
    }
} 