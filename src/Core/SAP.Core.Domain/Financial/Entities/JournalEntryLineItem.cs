using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Shared;

namespace SAP.Core.Domain.Financial.Entities;

/// <summary>
/// Represents an individual line item within a journal entry.
/// Each line item affects one account with either a debit or credit.
/// </summary>
public class JournalEntryLineItem
{
    public Guid Id { get; private set; }
    public Guid JournalEntryId { get; private set; }
    public Guid AccountId { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public DebitCreditIndicator DebitCreditIndicator { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public JournalEntry JournalEntry { get; private set; } = null!;
    public Account Account { get; private set; } = null!;

    private JournalEntryLineItem() { } // For EF Core

    public JournalEntryLineItem(
        Guid id,
        Guid journalEntryId,
        Guid accountId,
        string accountNumber,
        DebitCreditIndicator debitCreditIndicator,
        Money amount,
        string description)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        
        if (journalEntryId == Guid.Empty)
            throw new ArgumentException("Journal entry ID cannot be empty", nameof(journalEntryId));
        
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty", nameof(accountId));
        
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be null or empty", nameof(accountNumber));
        
        if (amount.IsZero)
            throw new ArgumentException("Amount cannot be zero", nameof(amount));

        Id = id;
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        AccountNumber = accountNumber;
        DebitCreditIndicator = debitCreditIndicator;
        Amount = amount;
        Description = description ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the signed amount based on debit/credit indicator.
    /// Positive for debits, negative for credits (for balance calculation).
    /// </summary>
    public Money GetSignedAmount()
    {
        return DebitCreditIndicator == DebitCreditIndicator.Debit 
            ? Amount 
            : Amount.Negate();
    }

    public override string ToString()
        => $"{AccountNumber} - {DebitCreditIndicator} {Amount} - {Description}";
} 