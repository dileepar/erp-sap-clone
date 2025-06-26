using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Domain.Financial.Repositories;

/// <summary>
/// Repository contract for Account aggregate root.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets an account by its unique identifier.
    /// </summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account by its account number.
    /// </summary>
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all accounts with optional filtering.
    /// </summary>
    Task<List<Account>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets accounts by type.
    /// </summary>
    Task<List<Account>> GetByTypeAsync(AccountType accountType, bool activeOnly = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets child accounts of a parent account.
    /// </summary>
    Task<List<Account>> GetChildAccountsAsync(Guid parentAccountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the parent account hierarchy for an account.
    /// </summary>
    Task<List<Account>> GetAccountHierarchyAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an account number already exists.
    /// </summary>
    Task<bool> ExistsWithAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new account.
    /// </summary>
    Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets accounts that can be used in transactions (active and not control accounts).
    /// </summary>
    Task<List<Account>> GetTransactionAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets control accounts (accounts that can have children but cannot be used in transactions).
    /// </summary>
    Task<List<Account>> GetControlAccountsAsync(CancellationToken cancellationToken = default);
} 