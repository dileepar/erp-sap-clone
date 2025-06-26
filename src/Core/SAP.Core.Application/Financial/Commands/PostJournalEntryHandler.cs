using Microsoft.Extensions.Logging;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Domain.Financial.Repositories;
using Wolverine;

namespace SAP.Core.Application.Financial.Commands;

/// <summary>
/// Command handler for posting journal entries.
/// Once posted, journal entries become immutable and affect account balances.
/// </summary>
public class PostJournalEntryHandler
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly ILogger<PostJournalEntryHandler> _logger;

    public PostJournalEntryHandler(
        IJournalEntryRepository journalEntryRepository,
        ILogger<PostJournalEntryHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the PostJournalEntryCommand using Wolverine's message handling.
    /// </summary>
    public async Task<JournalEntryDto> Handle(
        PostJournalEntryCommand command,
        IMessageContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Posting journal entry {JournalEntryId}", command.JournalEntryId);

        // Get the journal entry
        var journalEntry = await _journalEntryRepository.GetByIdAsync(command.JournalEntryId, cancellationToken);
        if (journalEntry == null)
        {
            throw new InvalidOperationException($"Journal entry with ID '{command.JournalEntryId}' not found");
        }

        if (journalEntry.IsPosted)
        {
            throw new InvalidOperationException($"Journal entry '{journalEntry.JournalEntryNumber}' is already posted");
        }

        // Validate that the journal entry is balanced before posting
        if (!journalEntry.IsBalanced())
        {
            var totalDebits = journalEntry.GetTotalDebitAmount();
            var totalCredits = journalEntry.GetTotalCreditAmount();
            throw new InvalidOperationException($"Cannot post unbalanced journal entry. Debits: {totalDebits}, Credits: {totalCredits}");
        }

        // Post the journal entry
        var postedBy = context.TenantId ?? "system"; // Use Wolverine's context for user tracking
        journalEntry.Post(postedBy);

        // Update the journal entry (this will publish JournalEntryPostedEvent)
        var updatedJournalEntry = await _journalEntryRepository.UpdateAsync(journalEntry, cancellationToken);

        _logger.LogInformation("Successfully posted journal entry {JournalEntryId} with number {JournalEntryNumber}", 
            updatedJournalEntry.Id, updatedJournalEntry.JournalEntryNumber);

        // Return DTO
        return MapToDto(updatedJournalEntry);
    }

    private static JournalEntryDto MapToDto(Domain.Financial.Entities.JournalEntry journalEntry)
    {
        var lineItems = journalEntry.LineItems.Select(li => new JournalEntryLineItemDto(
            li.Id,
            li.JournalEntryId,
            li.AccountId,
            li.AccountNumber,
            li.Account?.Name ?? "Unknown",
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