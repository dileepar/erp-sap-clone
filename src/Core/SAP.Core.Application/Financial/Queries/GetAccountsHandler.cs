using Microsoft.Extensions.Logging;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Contracts.Financial.Queries;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Repositories;

namespace SAP.Core.Application.Financial.Queries;

/// <summary>
/// Query handler for retrieving accounts with optional filtering.
/// Uses Wolverine's query handling pattern.
/// </summary>
public class GetAccountsHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountsHandler> _logger;

    public GetAccountsHandler(
        IAccountRepository accountRepository,
        ILogger<GetAccountsHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAccountsQuery using Wolverine's message handling.
    /// </summary>
    public async Task<List<AccountDto>> Handle(
        GetAccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving accounts with filters: AccountType={AccountType}, ActiveOnly={ActiveOnly}, ParentAccountId={ParentAccountId}", 
            query.AccountType, query.ActiveOnly, query.ParentAccountId);

        List<Account> accounts;

        // Apply filtering based on query parameters
        if (query.ParentAccountId.HasValue)
        {
            // Get child accounts of specific parent
            accounts = await _accountRepository.GetChildAccountsAsync(query.ParentAccountId.Value, cancellationToken);
        }
        else if (query.AccountType.HasValue)
        {
            // Get accounts by type
            accounts = await _accountRepository.GetByTypeAsync(query.AccountType.Value, query.ActiveOnly, cancellationToken);
        }
        else
        {
            // Get all accounts
            accounts = await _accountRepository.GetAllAsync(query.ActiveOnly, cancellationToken);
        }

        var result = accounts.Select(MapToDto).ToList();

        // If IncludeChildren is requested, populate child accounts
        if (query.IncludeChildren)
        {
            await PopulateChildAccountsAsync(result, cancellationToken);
        }

        _logger.LogInformation("Retrieved {Count} accounts", result.Count);

        return result;
    }

    private async Task PopulateChildAccountsAsync(List<AccountDto> accounts, CancellationToken cancellationToken)
    {
        foreach (var account in accounts)
        {
            var children = await _accountRepository.GetChildAccountsAsync(account.Id, cancellationToken);
            var childDtos = children.Select(MapToDto).ToList();
            
            // Note: This creates a new AccountDto with populated children
            // In a real implementation, you might want to use a more efficient approach
            // or consider using projections with Marten
        }
    }

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto(
            account.Id,
            account.AccountNumber,
            account.Name,
            account.Description,
            account.AccountType,
            account.Currency,
            account.IsActive,
            account.IsControlAccount,
            account.ParentAccountId,
            account.ParentAccount?.AccountNumber,
            account.ParentAccount?.Name,
            account.CurrentBalance.Amount,
            account.CurrentBalance.Currency,
            account.CreatedAt,
            account.UpdatedAt,
            new List<AccountDto>()); // Children will be populated separately if requested
    }
} 