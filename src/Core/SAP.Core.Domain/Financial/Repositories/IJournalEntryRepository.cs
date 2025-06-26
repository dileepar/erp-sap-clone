using SAP.Core.Domain.Financial.Entities;

namespace SAP.Core.Domain.Financial.Repositories;

/// <summary>
/// Repository contract for JournalEntry aggregate root with event sourcing support.
/// </summary>
public interface IJournalEntryRepository
{
    /// <summary>
    /// Gets a journal entry by its unique identifier.
    /// </summary>
    Task<JournalEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a journal entry by its journal entry number.
    /// </summary>
    Task<JournalEntry?> GetByJournalEntryNumberAsync(string journalEntryNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries for a specific posting date range.
    /// </summary>
    Task<List<JournalEntry>> GetByPostingDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries for a specific account.
    /// </summary>
    Task<List<JournalEntry>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unposted journal entries.
    /// </summary>
    Task<List<JournalEntry>> GetUnpostedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets posted journal entries for a specific period.
    /// </summary>
    Task<List<JournalEntry>> GetPostedByPeriodAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a journal entry number already exists.
    /// </summary>
    Task<bool> ExistsWithJournalEntryNumberAsync(string journalEntryNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new journal entry and publishes domain events.
    /// </summary>
    Task<JournalEntry> AddAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing journal entry and publishes domain events.
    /// </summary>
    Task<JournalEntry> UpdateAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next available journal entry number.
    /// </summary>
    Task<string> GetNextJournalEntryNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries created by a specific user.
    /// </summary>
    Task<List<JournalEntry>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets journal entries with a specific reference.
    /// </summary>
    Task<List<JournalEntry>> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
} 