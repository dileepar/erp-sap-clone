using Marten;
using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Financial.Repositories;

namespace SAP.Infrastructure.Data.Repositories;

/// <summary>
/// Marten-based implementation of the Account repository.
/// Provides persistence and retrieval for Chart of Accounts data.
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly IDocumentSession _session;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(IDocumentSession session, ILogger<AccountRepository> logger)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving account with ID: {AccountId}", id);
        
        var account = await _session
            .Query<Account>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (account != null)
        {
            _logger.LogDebug("Found account: {AccountNumber} - {AccountName}", 
                account.AccountNumber, account.Name);
        }
        else
        {
            _logger.LogDebug("Account not found with ID: {AccountId}", id);
        }

        return account;
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be null or empty", nameof(accountNumber));

        _logger.LogDebug("Retrieving account with number: {AccountNumber}", accountNumber);

        var account = await _session
            .Query<Account>()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

        if (account != null)
        {
            _logger.LogDebug("Found account: {AccountNumber} - {AccountName}", 
                account.AccountNumber, account.Name);
        }

        return account;
    }

    public async Task<List<Account>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all accounts, activeOnly: {ActiveOnly}", activeOnly);

        var query = _session.Query<Account>();
        
        if (activeOnly)
        {
            query = (Marten.Linq.IMartenQueryable<Account>)query.Where(a => a.IsActive);
        }

        var accountsList = await query
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {AccountCount} accounts", accountsList.Count);
        return accountsList.ToList();
    }

    public async Task<List<Account>> GetByTypeAsync(AccountType accountType, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving accounts of type: {AccountType}, ActiveOnly: {ActiveOnly}", 
            accountType, activeOnly);

        var query = _session.Query<Account>()
            .Where(a => a.AccountType == accountType);

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var accounts = await query
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {AccountCount} accounts of type {AccountType}", 
            accounts.Count, accountType);
        return accounts.ToList();
    }

    public async Task<List<Account>> GetChildAccountsAsync(Guid parentAccountId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving child accounts for parent: {ParentAccountId}", parentAccountId);

        var childAccounts = await _session
            .Query<Account>()
            .Where(a => a.ParentAccountId == parentAccountId)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {ChildAccountCount} child accounts for parent {ParentAccountId}", 
            childAccounts.Count, parentAccountId);
        return childAccounts.ToList();
    }

    public async Task<List<Account>> GetAccountHierarchyAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving account hierarchy for: {AccountId}", accountId);

        var hierarchy = new List<Account>();
        var currentAccount = await GetByIdAsync(accountId, cancellationToken);

        while (currentAccount != null)
        {
            hierarchy.Insert(0, currentAccount); // Insert at beginning to maintain hierarchy order
            
            if (currentAccount.ParentAccountId.HasValue)
            {
                currentAccount = await GetByIdAsync(currentAccount.ParentAccountId.Value, cancellationToken);
            }
            else
            {
                break;
            }
        }

        _logger.LogDebug("Retrieved hierarchy with {HierarchyLevels} levels for account {AccountId}", 
            hierarchy.Count, accountId);
        return hierarchy;
    }

    public async Task<List<Account>> GetTransactionAccountsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving accounts available for transactions");

        var accounts = await _session
            .Query<Account>()
            .Where(a => a.IsActive && !a.IsControlAccount)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {TransactionAccountCount} accounts available for transactions", 
            accounts.Count);
        return accounts.ToList();
    }

    public async Task<List<Account>> GetControlAccountsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving control accounts");

        var accounts = await _session
            .Query<Account>()
            .Where(a => a.IsActive && a.IsControlAccount)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Retrieved {ControlAccountCount} control accounts", accounts.Count);
        return accounts.ToList();
    }

    public async Task<bool> ExistsWithAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be null or empty", nameof(accountNumber));

        _logger.LogDebug("Checking if account number exists: {AccountNumber}", accountNumber);

        var exists = await _session
            .Query<Account>()
            .AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);

        _logger.LogDebug("Account number exists check for {AccountNumber}: {Exists}", 
            accountNumber, exists);
        return exists;
    }

    public async Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogInformation("Adding new account: {AccountNumber} - {AccountName}", 
            account.AccountNumber, account.Name);

        _session.Store(account);
        await _session.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added account: {AccountId} - {AccountNumber}", 
            account.Id, account.AccountNumber);
        
        return account;
    }

    public async Task<Account> UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        _logger.LogInformation("Updating account: {AccountId} - {AccountNumber}", 
            account.Id, account.AccountNumber);

        _session.Store(account);
        await _session.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated account: {AccountId} - {AccountNumber}", 
            account.Id, account.AccountNumber);
        
        return account;
    }
} 