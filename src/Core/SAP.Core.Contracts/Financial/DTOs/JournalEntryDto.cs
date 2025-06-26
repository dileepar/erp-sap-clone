using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Contracts.Financial.DTOs;

/// <summary>
/// Data transfer object for JournalEntry entity.
/// </summary>
public record JournalEntryDto(
    Guid Id,
    string JournalEntryNumber,
    DateTime PostingDate,
    DateTime DocumentDate,
    string Reference,
    string Description,
    string Currency,
    bool IsPosted,
    DateTime CreatedAt,
    DateTime? PostedAt,
    string CreatedBy,
    string? PostedBy,
    decimal TotalDebitAmount,
    decimal TotalCreditAmount,
    bool IsBalanced,
    List<JournalEntryLineItemDto> LineItems);

/// <summary>
/// Data transfer object for JournalEntryLineItem entity.
/// </summary>
public record JournalEntryLineItemDto(
    Guid Id,
    Guid JournalEntryId,
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    DebitCreditIndicator DebitCreditIndicator,
    decimal Amount,
    string Currency,
    string Description,
    DateTime CreatedAt);

/// <summary>
/// Simplified journal entry DTO for lists and summaries.
/// </summary>
public record JournalEntrySummaryDto(
    Guid Id,
    string JournalEntryNumber,
    DateTime PostingDate,
    string Reference,
    string Description,
    decimal TotalAmount,
    string Currency,
    bool IsPosted,
    string CreatedBy,
    DateTime CreatedAt); 