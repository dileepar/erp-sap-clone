using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Contracts.Financial.DTOs;

/// <summary>
/// Data transfer object for Account entity.
/// </summary>
public record AccountDto(
    Guid Id,
    string AccountNumber,
    string Name,
    string Description,
    AccountType AccountType,
    string Currency,
    bool IsActive,
    bool IsControlAccount,
    Guid? ParentAccountId,
    string? ParentAccountNumber,
    string? ParentAccountName,
    decimal CurrentBalanceAmount,
    string CurrentBalanceCurrency,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AccountDto> ChildAccounts);

/// <summary>
/// Simplified account DTO for lookups and selections.
/// </summary>
public record AccountLookupDto(
    Guid Id,
    string AccountNumber,
    string Name,
    AccountType AccountType,
    bool IsActive,
    bool CanBeUsedInTransactions); 