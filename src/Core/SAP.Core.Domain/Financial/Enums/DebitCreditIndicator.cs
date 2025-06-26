namespace SAP.Core.Domain.Financial.Enums;

/// <summary>
/// Indicates whether a journal entry line item is a debit or credit.
/// Fundamental to double-entry bookkeeping.
/// </summary>
public enum DebitCreditIndicator
{
    /// <summary>
    /// Debit entry - increases assets and expenses, decreases liabilities, equity, and revenue
    /// </summary>
    Debit = 1,

    /// <summary>
    /// Credit entry - increases liabilities, equity, and revenue, decreases assets and expenses
    /// </summary>
    Credit = 2
} 