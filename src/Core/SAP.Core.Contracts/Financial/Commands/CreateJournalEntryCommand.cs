using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Contracts.Financial.Commands;

/// <summary>
/// Command to create a new journal entry.
/// </summary>
public record CreateJournalEntryCommand(
    DateTime PostingDate,
    DateTime DocumentDate,
    string Reference,
    string Description,
    string Currency,
    List<CreateJournalEntryLineItemCommand> LineItems);

/// <summary>
/// Command to create a journal entry line item.
/// </summary>
public record CreateJournalEntryLineItemCommand(
    Guid AccountId,
    DebitCreditIndicator DebitCreditIndicator,
    decimal Amount,
    string Description); 