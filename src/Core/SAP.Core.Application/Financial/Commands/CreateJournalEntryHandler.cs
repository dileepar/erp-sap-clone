using Microsoft.Extensions.Logging;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Repositories;
using SAP.Core.Domain.Shared;
using Wolverine;

namespace SAP.Core.Application.Financial.Commands;

/// <summary>
/// Command handler for creating journal entries with event sourcing support.
/// Ensures proper double-entry bookkeeping validation.
/// </summary>
public class CreateJournalEntryHandler
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateJournalEntryHandler> _logger;

    public CreateJournalEntryHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountRepository accountRepository,
        ILogger<CreateJournalEntryHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateJournalEntryCommand using Wolverine's message handling.
    /// </summary>
    public async Task<JournalEntryDto> Handle(
        CreateJournalEntryCommand command,
        IMessageContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating journal entry with reference {Reference}", command.Reference);

        // Validate line items
        if (!command.LineItems.Any())
        {
            throw new InvalidOperationException("Journal entry must have at least one line item");
        }

        if (command.LineItems.Count < 2)
        {
            throw new InvalidOperationException("Journal entry must have at least two line items for double-entry bookkeeping");
        }

        // Get next journal entry number
        var journalEntryNumber = await _journalEntryRepository.GetNextJournalEntryNumberAsync(cancellationToken);

        // Create the journal entry
        var journalEntry = new JournalEntry(
            journalEntryNumber,
            command.PostingDate,
            command.DocumentDate,
            command.Reference,
            command.Description,
            command.Currency,
            context.TenantId ?? "system"); // Use Wolverine's context for user tracking

        // Validate and add line items
        foreach (var lineItemCommand in command.LineItems)
        {
            // Validate account exists and can be used in transactions
            var account = await _accountRepository.GetByIdAsync(lineItemCommand.AccountId, cancellationToken);
            if (account == null)
            {
                throw new InvalidOperationException($"Account with ID '{lineItemCommand.AccountId}' not found");
            }

            if (!account.CanBeUsedInTransactions())
            {
                throw new InvalidOperationException($"Account '{account.AccountNumber}' cannot be used in transactions");
            }

            // Validate currency consistency
            if (account.Currency != command.Currency)
            {
                throw new InvalidOperationException($"Account currency '{account.Currency}' does not match journal entry currency '{command.Currency}'");
            }

            // Create Money value object
            var amount = new Money(lineItemCommand.Amount, command.Currency);

            // Add line item to journal entry
            journalEntry.AddLineItem(
                account.Id,
                account.AccountNumber,
                lineItemCommand.DebitCreditIndicator,
                amount,
                lineItemCommand.Description);
        }

        // Validate that the journal entry is balanced
        if (!journalEntry.IsBalanced())
        {
            var totalDebits = journalEntry.GetTotalDebitAmount();
            var totalCredits = journalEntry.GetTotalCreditAmount();
            throw new InvalidOperationException($"Journal entry is not balanced. Debits: {totalDebits}, Credits: {totalCredits}");
        }

        // Persist the journal entry (this will also publish domain events)
        var createdJournalEntry = await _journalEntryRepository.AddAsync(journalEntry, cancellationToken);

        _logger.LogInformation("Successfully created journal entry {JournalEntryId} with number {JournalEntryNumber}", 
            createdJournalEntry.Id, createdJournalEntry.JournalEntryNumber);

        // Return DTO
        return MapToDto(createdJournalEntry);
    }

    private static JournalEntryDto MapToDto(JournalEntry journalEntry)
    {
        var lineItems = journalEntry.LineItems.Select(li => new JournalEntryLineItemDto(
            li.Id,
            li.JournalEntryId,
            li.AccountId,
            li.AccountNumber,
            li.Account?.Name ?? "Unknown", // This will be populated properly with projections
            li.DebitCreditIndicator,
            li.Amount.Amount,
            li.Amount.Currency,
            li.Description,
            li.CreatedAt)).ToList();

        return new JournalEntryDto(
            journalEntry.Id,
            journalEntry.JournalEntryNumber,
            journalEntry.PostingDate,
            journalEntry.DocumentDate,
            journalEntry.Reference,
            journalEntry.Description,
            journalEntry.Currency,
            journalEntry.IsPosted,
            journalEntry.CreatedAt,
            journalEntry.PostedAt,
            journalEntry.CreatedBy,
            journalEntry.PostedBy,
            journalEntry.GetTotalDebitAmount().Amount,
            journalEntry.GetTotalCreditAmount().Amount,
            journalEntry.IsBalanced(),
            lineItems);
    }
} 