using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Domain.Financial.Services;

/// <summary>
/// Domain service for managing the chart of accounts structure and validation.
/// </summary>
public interface IChartOfAccountsService
{
    /// <summary>
    /// Validates that an account number is unique within the chart of accounts.
    /// </summary>
    Task<bool> IsAccountNumberUniqueAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that an account can be used in transactions.
    /// </summary>
    Task<bool> CanAccountBeUsedInTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the account hierarchy path for reporting purposes.
    /// </summary>
    Task<List<Account>> GetAccountHierarchyAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a parent-child account relationship is valid.
    /// </summary>
    Task<bool> IsValidParentChildRelationshipAsync(Guid parentAccountId, Guid childAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts of a specific type for reporting and validation.
    /// </summary>
    Task<List<Account>> GetAccountsByTypeAsync(AccountType accountType, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that an account structure follows business rules.
    /// </summary>
    Task<ValidationResult> ValidateAccountStructureAsync(Account account, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of account structure validation.
/// </summary>
public record ValidationResult(bool IsValid, List<string> Errors)
{
    public static ValidationResult Success() => new(true, new List<string>());
    public static ValidationResult Failure(params string[] errors) => new(false, errors.ToList());
} 