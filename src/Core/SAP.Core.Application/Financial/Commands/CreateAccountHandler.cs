using Microsoft.Extensions.Logging;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Repositories;
using SAP.Core.Domain.Financial.Services;
using Wolverine;

namespace SAP.Core.Application.Financial.Commands;

/// <summary>
/// Command handler for creating new accounts using WolverineFx.
/// Implements CQRS pattern with domain validation.
/// </summary>
public class CreateAccountHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly ILogger<CreateAccountHandler> _logger;

    public CreateAccountHandler(
        IAccountRepository accountRepository,
        IChartOfAccountsService chartOfAccountsService,
        ILogger<CreateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _chartOfAccountsService = chartOfAccountsService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CreateAccountCommand using Wolverine's message handling.
    /// </summary>
    public async Task<AccountDto> Handle(
        CreateAccountCommand command, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating account with number {AccountNumber}", command.AccountNumber);

        // Validate account number uniqueness
        var isUnique = await _chartOfAccountsService.IsAccountNumberUniqueAsync(
            command.AccountNumber, cancellationToken);
        
        if (!isUnique)
        {
            throw new InvalidOperationException($"Account number '{command.AccountNumber}' already exists");
        }

        // Validate parent account if specified
        if (command.ParentAccountId.HasValue)
        {
            var parentAccount = await _accountRepository.GetByIdAsync(
                command.ParentAccountId.Value, cancellationToken);
            
            if (parentAccount == null)
            {
                throw new InvalidOperationException($"Parent account '{command.ParentAccountId}' not found");
            }

            if (!parentAccount.CanHaveChildren())
            {
                throw new InvalidOperationException($"Account '{parentAccount.AccountNumber}' cannot have child accounts");
            }
        }

        // Create the account domain entity
        var account = new Account(
            command.AccountNumber,
            command.Name,
            command.Description,
            command.AccountType,
            command.Currency,
            command.ParentAccountId,
            command.IsControlAccount);

        // Validate account structure
        var validationResult = await _chartOfAccountsService.ValidateAccountStructureAsync(
            account, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Account validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Persist the account
        var createdAccount = await _accountRepository.AddAsync(account, cancellationToken);

        _logger.LogInformation("Successfully created account {AccountId} with number {AccountNumber}", 
            createdAccount.Id, createdAccount.AccountNumber);

        // Return DTO
        return MapToDto(createdAccount);
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
            account.ChildAccounts.Select(MapToDto).ToList());
    }
} 