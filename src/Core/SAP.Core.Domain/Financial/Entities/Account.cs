using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Shared;

namespace SAP.Core.Domain.Financial.Entities;

/// <summary>
/// Represents an account in the chart of accounts.
/// Central entity for financial transactions and reporting.
/// </summary>
public class Account
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsControlAccount { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Account? ParentAccount { get; private set; }
    public List<Account> ChildAccounts { get; private set; } = new();

    // Balance tracking
    public Money CurrentBalance { get; private set; }

    private Account() { } // For EF Core

    public Account(
        string accountNumber,
        string name,
        string description,
        AccountType accountType,
        string currency,
        Guid? parentAccountId = null,
        bool isControlAccount = false)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be null or empty", nameof(accountNumber));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

        Id = Guid.NewGuid();
        AccountNumber = accountNumber;
        Name = name;
        Description = description ?? string.Empty;
        AccountType = accountType;
        Currency = currency.ToUpperInvariant();
        ParentAccountId = parentAccountId;
        IsControlAccount = isControlAccount;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CurrentBalance = Money.Zero(Currency);
    }

    /// <summary>
    /// Updates the account balance. Should only be called through domain services.
    /// </summary>
    public void UpdateBalance(Money newBalance)
    {
        if (newBalance.Currency != Currency)
            throw new InvalidOperationException($"Balance currency {newBalance.Currency} does not match account currency {Currency}");

        CurrentBalance = newBalance;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the account. Inactive accounts cannot be used in new transactions.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates the account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates account information.
    /// </summary>
    public void UpdateInfo(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be null or empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the normal balance side for this account type.
    /// </summary>
    public DebitCreditIndicator GetNormalBalanceSide()
    {
        return AccountType switch
        {
            AccountType.Asset => DebitCreditIndicator.Debit,
            AccountType.Expense => DebitCreditIndicator.Debit,
            AccountType.Liability => DebitCreditIndicator.Credit,
            AccountType.Equity => DebitCreditIndicator.Credit,
            AccountType.Revenue => DebitCreditIndicator.Credit,
            _ => throw new InvalidOperationException($"Unknown account type: {AccountType}")
        };
    }

    /// <summary>
    /// Determines if this account can have child accounts.
    /// </summary>
    public bool CanHaveChildren()
    {
        return IsControlAccount;
    }

    /// <summary>
    /// Determines if this account can be used in transactions.
    /// Control accounts typically cannot be used directly.
    /// </summary>
    public bool CanBeUsedInTransactions()
    {
        return IsActive && !IsControlAccount;
    }

    public override string ToString()
        => $"{AccountNumber} - {Name} ({AccountType})";
} 