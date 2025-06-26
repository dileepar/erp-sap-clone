using SAP.Core.Domain.Shared;

namespace SAP.Core.Domain.Financial.Events;

/// <summary>
/// Domain event raised when a journal entry is posted.
/// This is the most critical event as it makes the transaction permanent.
/// </summary>
public record JournalEntryPostedEvent(
    Guid JournalEntryId,
    string JournalEntryNumber,
    DateTime PostedAt,
    string PostedBy,
    Money TotalAmount); 