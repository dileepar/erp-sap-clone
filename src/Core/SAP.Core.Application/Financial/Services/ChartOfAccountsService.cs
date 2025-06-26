using Microsoft.Extensions.Logging;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Financial.Repositories;
using SAP.Core.Domain.Financial.Services;

namespace SAP.Core.Application.Financial.Services;

/// <summary>
/// Implementation of the chart of accounts domain service.
/// Provides account validation and hierarchy management logic.
/// </summary>
public class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<ChartOfAccountsService> _logger;

    public ChartOfAccountsService(
        IAccountRepository accountRepository,
        ILogger<ChartOfAccountsService> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return false;

        var exists = await _accountRepository.ExistsWithAccountNumberAsync(accountNumber, cancellationToken);
        return !exists;
    }

    public async Task<bool> CanAccountBeUsedInTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken);
        return account?.CanBeUsedInTransactions() ?? false;
    }

    public async Task<List<Account>> GetAccountHierarchyAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetAccountHierarchyAsync(accountId, cancellationToken);
    }

    public async Task<bool> IsValidParentChildRelationshipAsync(Guid parentAccountId, Guid childAccountId, CancellationToken cancellationToken = default)
    {
        var parentAccount = await _accountRepository.GetByIdAsync(parentAccountId, cancellationToken);
        var childAccount = await _accountRepository.GetByIdAsync(childAccountId, cancellationToken);

        if (parentAccount == null || childAccount == null)
            return false;

        if (!parentAccount.CanHaveChildren())
            return false;

        if (parentAccount.Currency != childAccount.Currency)
            return false;

        var hierarchy = await GetAccountHierarchyAsync(parentAccountId, cancellationToken);
        if (hierarchy.Any(a => a.Id == childAccountId))
        {
            _logger.LogWarning("Circular reference detected: child account {ChildId} is already a parent of {ParentId}", 
                childAccountId, parentAccountId);
            return false;
        }

        return true;
    }

    public async Task<List<Account>> GetAccountsByTypeAsync(AccountType accountType, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        return await _accountRepository.GetByTypeAsync(accountType, activeOnly, cancellationToken);
    }

    public async Task<ValidationResult> ValidateAccountStructureAsync(Account account, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (!IsValidAccountNumberFormat(account.AccountNumber))
        {
            errors.Add($"Account number '{account.AccountNumber}' does not follow the required format");
        }

        if (!IsValidCurrency(account.Currency))
        {
            errors.Add($"Currency '{account.Currency}' is not valid");
        }

        if (account.ParentAccountId.HasValue)
        {
            var isValidRelationship = await IsValidParentChildRelationshipAsync(
                account.ParentAccountId.Value, account.Id, cancellationToken);
            
            if (!isValidRelationship)
            {
                errors.Add($"Invalid parent-child relationship with parent account ID '{account.ParentAccountId}'");
            }
        }

        return errors.Any() 
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    private static bool IsValidAccountNumberFormat(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return false;

        if (accountNumber.Length < 4 || accountNumber.Length > 10)
            return false;

        return accountNumber.All(char.IsLetterOrDigit);
    }

    private static bool IsValidCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        if (currency.Length != 3)
            return false;

        return currency.All(char.IsLetter);
    }
} 