using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Contracts.Financial.Commands;

/// <summary>
/// Command to create a new account in the chart of accounts.
/// </summary>
public record CreateAccountCommand(
    string AccountNumber,
    string Name,
    string Description,
    AccountType AccountType,
    string Currency,
    Guid? ParentAccountId = null,
    bool IsControlAccount = false); 