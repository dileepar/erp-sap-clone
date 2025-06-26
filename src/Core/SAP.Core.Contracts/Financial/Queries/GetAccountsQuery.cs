using SAP.Core.Domain.Financial.Enums;

namespace SAP.Core.Contracts.Financial.Queries;

/// <summary>
/// Query to get accounts with optional filtering.
/// </summary>
public record GetAccountsQuery(
    AccountType? AccountType = null,
    bool ActiveOnly = true,
    Guid? ParentAccountId = null,
    bool IncludeChildren = false); 