using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Events;
using SAP.Core.Domain.Financial.Repositories;
using SAP.Core.Domain.Shared;
using Wolverine;

namespace SAP.Core.Application.Financial.EventHandlers;

/// <summary>
/// Event handlers for journal entry domain events.
/// Handles account balance updates and cross-cutting concerns.
/// </summary>
public class JournalEntryEventHandlers
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<JournalEntryEventHandlers> _logger;

    public JournalEntryEventHandlers(
        IAccountRepository accountRepository,
        ILogger<JournalEntryEventHandlers> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles when a journal entry is created.
    /// Logs the creation for audit purposes.
    /// </summary>
    public async Task Handle(JournalEntryCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Journal entry created: {JournalEntryNumber} with ID {JournalEntryId} by {CreatedBy}",
            @event.JournalEntryNumber, @event.JournalEntryId, @event.CreatedBy);

        // Additional business logic can be added here
        // For example: sending notifications, updating dashboards, etc.
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles when a line item is added to a journal entry.
    /// Logs the line item addition for audit purposes.
    /// </summary>
    public async Task Handle(JournalEntryLineItemAddedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Line item added to journal entry {JournalEntryId}: Account {AccountNumber} - {DebitCreditIndicator} {Amount}",
            @event.JournalEntryId, @event.AccountNumber, @event.DebitCreditIndicator, @event.Amount);

        // Additional business logic can be added here
        await Task.CompletedTask;
    }

    /// <summary>
    /// Handles when a journal entry is posted.
    /// Updates account balances to reflect the posted transactions.
    /// </summary>
    public async Task Handle(JournalEntryPostedEvent @event, IMessageBus messageBus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Journal entry posted: {JournalEntryNumber} with ID {JournalEntryId} by {PostedBy}",
            @event.JournalEntryNumber, @event.JournalEntryId, @event.PostedBy);

        try
        {
            // Get the full journal entry to process line items
            var journalEntryRepository = _accountRepository as IJournalEntryRepository;
            if (journalEntryRepository != null)
            {
                var journalEntry = await journalEntryRepository.GetByIdAsync(@event.JournalEntryId, cancellationToken);
                
                if (journalEntry != null && journalEntry.IsPosted)
                {
                    // Update account balances for each line item
                    foreach (var lineItem in journalEntry.LineItems)
                    {
                        await UpdateAccountBalance(lineItem.AccountId, lineItem.GetSignedAmount(), cancellationToken);
                    }

                    _logger.LogInformation("Account balances updated for journal entry {JournalEntryId}", @event.JournalEntryId);

                    // Publish account balance updated events
                    foreach (var lineItem in journalEntry.LineItems)
                    {
                        await messageBus.PublishAsync(new AccountBalanceUpdatedEvent(
                            lineItem.AccountId,
                            lineItem.AccountNumber,
                            @event.JournalEntryId,
                            @event.JournalEntryNumber,
                            lineItem.GetSignedAmount(),
                            @event.PostedAt));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account balances for journal entry {JournalEntryId}", @event.JournalEntryId);
            throw; // Re-throw to ensure the event processing fails and can be retried
        }
    }

    /// <summary>
    /// Updates an account's balance by applying the transaction amount.
    /// </summary>
    private async Task UpdateAccountBalance(Guid accountId, Money amount, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account {AccountId} not found when updating balance", accountId);
            return;
        }

        // Calculate new balance
        var newBalance = account.CurrentBalance + amount;
        
        // Update the account balance
        account.UpdateBalance(newBalance);
        
        // Save the updated account
        await _accountRepository.UpdateAsync(account, cancellationToken);

        _logger.LogDebug("Updated balance for account {AccountNumber}: {OldBalance} -> {NewBalance}",
            account.AccountNumber, account.CurrentBalance - amount, account.CurrentBalance);
    }
}

/// <summary>
/// Event published when an account balance is updated.
/// </summary>
public record AccountBalanceUpdatedEvent(
    Guid AccountId,
    string AccountNumber,
    Guid JournalEntryId,
    string JournalEntryNumber,
    Money BalanceChange,
    DateTime UpdatedAt); 