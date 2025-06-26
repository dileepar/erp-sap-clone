namespace SAP.Core.Domain.Financial.Events;

/// <summary>
/// Domain event raised when a journal entry is created.
/// </summary>
public record JournalEntryCreatedEvent(
    Guid JournalEntryId,
    string JournalEntryNumber,
    DateTime PostingDate,
    DateTime DocumentDate,
    string Reference,
    string Description,
    string Currency,
    string CreatedBy,
    DateTime CreatedAt); 