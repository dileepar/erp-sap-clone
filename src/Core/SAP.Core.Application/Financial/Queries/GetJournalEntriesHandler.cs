using Microsoft.Extensions.Logging;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Contracts.Financial.Queries;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Repositories;

namespace SAP.Core.Application.Financial.Queries;

/// <summary>
/// Query handler for retrieving journal entries with filtering and pagination.
/// Uses Wolverine's query handling pattern.
/// </summary>
public class GetJournalEntriesHandler
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly ILogger<GetJournalEntriesHandler> _logger;

    public GetJournalEntriesHandler(
        IJournalEntryRepository journalEntryRepository,
        ILogger<GetJournalEntriesHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetJournalEntriesQuery using Wolverine's message handling.
    /// </summary>
    public async Task<PagedResult<JournalEntrySummaryDto>> Handle(
        GetJournalEntriesQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving journal entries with filters: FromDate={FromDate}, ToDate={ToDate}, AccountId={AccountId}, IsPosted={IsPosted}", 
            query.FromDate, query.ToDate, query.AccountId, query.IsPosted);

        List<JournalEntry> journalEntries;

        // Apply filtering based on query parameters
        if (query.AccountId.HasValue)
        {
            // Get journal entries for specific account
            journalEntries = await _journalEntryRepository.GetByAccountIdAsync(query.AccountId.Value, cancellationToken);
        }
        else if (query.FromDate.HasValue && query.ToDate.HasValue)
        {
            // Get journal entries by date range
            if (query.IsPosted.HasValue && query.IsPosted.Value)
            {
                journalEntries = await _journalEntryRepository.GetPostedByPeriodAsync(query.FromDate.Value, query.ToDate.Value, cancellationToken);
            }
            else
            {
                journalEntries = await _journalEntryRepository.GetByPostingDateRangeAsync(query.FromDate.Value, query.ToDate.Value, cancellationToken);
            }
        }
        else if (query.IsPosted.HasValue && !query.IsPosted.Value)
        {
            // Get unposted journal entries
            journalEntries = await _journalEntryRepository.GetUnpostedAsync(cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(query.Reference))
        {
            // Get journal entries by reference
            journalEntries = await _journalEntryRepository.GetByReferenceAsync(query.Reference, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(query.CreatedBy))
        {
            // Get journal entries by creator
            journalEntries = await _journalEntryRepository.GetByCreatedByAsync(query.CreatedBy, cancellationToken);
        }
        else
        {
            // This would require a GetAllAsync method in the repository
            // For now, we'll get unposted entries as a default
            journalEntries = await _journalEntryRepository.GetUnpostedAsync(cancellationToken);
        }

        // Apply additional filtering if needed
        if (query.IsPosted.HasValue)
        {
            journalEntries = journalEntries.Where(je => je.IsPosted == query.IsPosted.Value).ToList();
        }

        // Apply pagination
        var totalCount = journalEntries.Count;
        var skip = (query.PageNumber - 1) * query.PageSize;
        var pagedEntries = journalEntries
            .OrderByDescending(je => je.CreatedAt) // Most recent first
            .Skip(skip)
            .Take(query.PageSize)
            .ToList();

        var result = pagedEntries.Select(MapToSummaryDto).ToList();

        _logger.LogInformation("Retrieved {Count} journal entries (Page {PageNumber} of {TotalPages})", 
            result.Count, query.PageNumber, Math.Ceiling((double)totalCount / query.PageSize));

        return new PagedResult<JournalEntrySummaryDto>(
            result,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }

    private static JournalEntrySummaryDto MapToSummaryDto(JournalEntry journalEntry)
    {
        return new JournalEntrySummaryDto(
            journalEntry.Id,
            journalEntry.JournalEntryNumber,
            journalEntry.PostingDate,
            journalEntry.Reference,
            journalEntry.Description,
            journalEntry.GetTotalDebitAmount().Amount,
            journalEntry.Currency,
            journalEntry.IsPosted,
            journalEntry.CreatedBy,
            journalEntry.CreatedAt);
    }
}

/// <summary>
/// Represents a paged result for queries.
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
} 