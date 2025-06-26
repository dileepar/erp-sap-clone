namespace SAP.Core.Contracts.Financial.Queries;

/// <summary>
/// Query to get journal entries with optional filtering.
/// </summary>
public record GetJournalEntriesQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? AccountId = null,
    bool? IsPosted = null,
    string? Reference = null,
    string? CreatedBy = null,
    int PageNumber = 1,
    int PageSize = 50); 