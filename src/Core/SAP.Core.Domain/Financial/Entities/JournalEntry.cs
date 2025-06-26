using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Financial.Events;
using SAP.Core.Domain.Shared;

namespace SAP.Core.Domain.Financial.Entities;

/// <summary>
/// Represents a journal entry in double-entry bookkeeping.
/// Aggregate root for financial transaction recording.
/// </summary>
public class JournalEntry
{
    public Guid Id { get; private set; }
    public string JournalEntryNumber { get; private set; } = string.Empty;
    public DateTime PostingDate { get; private set; }
    public DateTime DocumentDate { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Currency { get; private set; } = string.Empty;
    public bool IsPosted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PostedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string? PostedBy { get; private set; }

    private readonly List<JournalEntryLineItem> _lineItems = new();
    public IReadOnlyList<JournalEntryLineItem> LineItems => _lineItems.AsReadOnly();

    // Domain events for event sourcing
    private readonly List<object> _domainEvents = new();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private JournalEntry() { } // For EF Core

    public JournalEntry(
        string journalEntryNumber,
        DateTime postingDate,
        DateTime documentDate,
        string reference,
        string description,
        string currency,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(journalEntryNumber))
            throw new ArgumentException("Journal entry number cannot be null or empty", nameof(journalEntryNumber));
        
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference cannot be null or empty", nameof(reference));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be null or empty", nameof(createdBy));

        Id = Guid.NewGuid();
        JournalEntryNumber = journalEntryNumber;
        PostingDate = postingDate.Date;
        DocumentDate = documentDate.Date;
        Reference = reference;
        Description = description ?? string.Empty;
        Currency = currency.ToUpperInvariant();
        CreatedBy = createdBy;
        IsPosted = false;
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new JournalEntryCreatedEvent(
            Id, JournalEntryNumber, PostingDate, DocumentDate, Reference, Description, Currency, CreatedBy, CreatedAt));
    }

    /// <summary>
    /// Adds a line item to the journal entry.
    /// </summary>
    public void AddLineItem(
        Guid accountId,
        string accountNumber,
        DebitCreditIndicator debitCreditIndicator,
        Money amount,
        string description)
    {
        if (IsPosted)
            throw new InvalidOperationException("Cannot modify posted journal entry");

        if (amount.Currency != Currency)
            throw new InvalidOperationException($"Line item currency {amount.Currency} does not match journal entry currency {Currency}");

        var lineItem = new JournalEntryLineItem(
            Guid.NewGuid(),
            Id,
            accountId,
            accountNumber,
            debitCreditIndicator,
            amount,
            description);

        _lineItems.Add(lineItem);

        // Raise domain event
        AddDomainEvent(new JournalEntryLineItemAddedEvent(
            Id, lineItem.Id, accountId, accountNumber, debitCreditIndicator, amount, description));
    }

    /// <summary>
    /// Posts the journal entry after validation.
    /// </summary>
    public void Post(string postedBy)
    {
        if (IsPosted)
            throw new InvalidOperationException("Journal entry is already posted");

        if (string.IsNullOrWhiteSpace(postedBy))
            throw new ArgumentException("Posted by cannot be null or empty", nameof(postedBy));

        ValidateForPosting();

        IsPosted = true;
        PostedAt = DateTime.UtcNow;
        PostedBy = postedBy;

        // Raise domain event
        AddDomainEvent(new JournalEntryPostedEvent(Id, JournalEntryNumber, PostedAt.Value, PostedBy, GetTotalDebitAmount()));
    }

    /// <summary>
    /// Validates that the journal entry is ready for posting.
    /// </summary>
    private void ValidateForPosting()
    {
        if (!_lineItems.Any())
            throw new InvalidOperationException("Cannot post journal entry without line items");

        if (_lineItems.Count < 2)
            throw new InvalidOperationException("Journal entry must have at least 2 line items");

        var totalDebits = GetTotalDebitAmount();
        var totalCredits = GetTotalCreditAmount();

        if (totalDebits != totalCredits)
            throw new InvalidOperationException($"Journal entry is not balanced. Debits: {totalDebits}, Credits: {totalCredits}");

        // Ensure all line items have valid account references
        if (_lineItems.Any(li => string.IsNullOrWhiteSpace(li.AccountNumber)))
            throw new InvalidOperationException("All line items must have valid account numbers");
    }

    /// <summary>
    /// Gets the total debit amount for the journal entry.
    /// </summary>
    public Money GetTotalDebitAmount()
    {
        var total = Money.Zero(Currency);
        return _lineItems
            .Where(li => li.DebitCreditIndicator == DebitCreditIndicator.Debit)
            .Aggregate(total, (sum, li) => sum + li.Amount);
    }

    /// <summary>
    /// Gets the total credit amount for the journal entry.
    /// </summary>
    public Money GetTotalCreditAmount()
    {
        var total = Money.Zero(Currency);
        return _lineItems
            .Where(li => li.DebitCreditIndicator == DebitCreditIndicator.Credit)
            .Aggregate(total, (sum, li) => sum + li.Amount);
    }

    /// <summary>
    /// Checks if the journal entry is balanced (debits = credits).
    /// </summary>
    public bool IsBalanced()
    {
        return GetTotalDebitAmount() == GetTotalCreditAmount();
    }

    /// <summary>
    /// Adds a domain event to be published.
    /// </summary>
    private void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears domain events after they have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override string ToString()
        => $"{JournalEntryNumber} - {Description} ({PostingDate:yyyy-MM-dd})";
} 