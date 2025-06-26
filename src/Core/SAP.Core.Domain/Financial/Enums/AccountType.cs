namespace SAP.Core.Domain.Financial.Enums;

/// <summary>
/// Defines the fundamental types of accounts in the chart of accounts.
/// Based on standard accounting principles and SAP account categories.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Assets - Resources owned by the company (Debit normal balance)
    /// Examples: Cash, Inventory, Equipment, Accounts Receivable
    /// </summary>
    Asset = 1,

    /// <summary>
    /// Liabilities - Obligations owed to others (Credit normal balance)
    /// Examples: Accounts Payable, Loans, Accrued Expenses
    /// </summary>
    Liability = 2,

    /// <summary>
    /// Equity - Owner's interest in the company (Credit normal balance)
    /// Examples: Capital Stock, Retained Earnings
    /// </summary>
    Equity = 3,

    /// <summary>
    /// Revenue - Income from business operations (Credit normal balance)
    /// Examples: Sales Revenue, Service Revenue, Interest Income
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// Expense - Costs of doing business (Debit normal balance)
    /// Examples: Salaries, Rent, Utilities, Cost of Goods Sold
    /// </summary>
    Expense = 5
} 