namespace SAP.Core.Contracts.Financial.Commands;

/// <summary>
/// Command to post a journal entry, making it permanent and affecting account balances.
/// </summary>
public record PostJournalEntryCommand(
    Guid JournalEntryId); 