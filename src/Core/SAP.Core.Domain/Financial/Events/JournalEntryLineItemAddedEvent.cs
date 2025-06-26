using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Shared;

namespace SAP.Core.Domain.Financial.Events;

/// <summary>
/// Domain event raised when a line item is added to a journal entry.
/// </summary>
public record JournalEntryLineItemAddedEvent(
    Guid JournalEntryId,
    Guid LineItemId,
    Guid AccountId,
    string AccountNumber,
    DebitCreditIndicator DebitCreditIndicator,
    Money Amount,
    string Description); 