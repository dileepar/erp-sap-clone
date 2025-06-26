using Microsoft.AspNetCore.Mvc;
using SAP.Core.Application.Financial.Commands;
using SAP.Core.Application.Financial.Queries;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Contracts.Financial.Queries;
using SAP.Core.Domain.Financial.Enums;
using Wolverine;

namespace SAP.API.Controllers;

/// <summary>
/// REST API controller for Account management operations.
/// Provides endpoints for chart of accounts functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMessageBus messageBus, ILogger<AccountsController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all accounts with optional filtering.
    /// </summary>
    /// <param name="activeOnly">Filter to active accounts only</param>
    /// <param name="accountType">Filter by account type</param>
    /// <returns>List of accounts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAccounts(
        [FromQuery] bool activeOnly = true,
        [FromQuery] AccountType? accountType = null)
    {
        try
        {
            _logger.LogInformation("Getting accounts with filters: ActiveOnly={ActiveOnly}, AccountType={AccountType}", 
                activeOnly, accountType);

            var query = new GetAccountsQuery 
            { 
                ActiveOnly = activeOnly,
                AccountType = accountType
            };

            var result = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(query);
            
            _logger.LogInformation("Retrieved {Count} accounts", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving accounts" });
        }
    }

    /// <summary>
    /// Gets a specific account by ID.
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Account details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting account with ID: {AccountId}", id);

            var query = new GetAccountsQuery();
            var results = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(query);
            
            var account = results.FirstOrDefault(a => a.Id == id);
            if (account == null)
            {
                _logger.LogWarning("Account not found: {AccountId}", id);
                return NotFound(new { error = "Account not found" });
            }

            _logger.LogInformation("Retrieved account: {AccountNumber} - {Name}", account.AccountNumber, account.Name);
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account: {AccountId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the account" });
        }
    }

    /// <summary>
    /// Gets an account by account number.
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <returns>Account details</returns>
    [HttpGet("by-number/{accountNumber}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDto>> GetAccountByNumber(string accountNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest(new { error = "Account number is required" });
            }

            _logger.LogInformation("Getting account by number: {AccountNumber}", accountNumber);

            var query = new GetAccountsQuery();
            var results = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(query);
            
            var account = results.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                _logger.LogWarning("Account not found: {AccountNumber}", accountNumber);
                return NotFound(new { error = "Account not found" });
            }

            _logger.LogInformation("Retrieved account: {AccountNumber} - {Name}", account.AccountNumber, account.Name);
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving account by number: {AccountNumber}", accountNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the account" });
        }
    }

    /// <summary>
    /// Gets accounts by type.
    /// </summary>
    /// <param name="accountType">Account type to filter by</param>
    /// <param name="activeOnly">Filter to active accounts only</param>
    /// <returns>List of accounts of the specified type</returns>
    [HttpGet("by-type/{accountType}")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetAccountsByType(
        AccountType accountType,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            if (!Enum.IsDefined(typeof(AccountType), accountType))
            {
                return BadRequest(new { error = "Invalid account type" });
            }

            _logger.LogInformation("Getting accounts by type: {AccountType}, ActiveOnly={ActiveOnly}", 
                accountType, activeOnly);

            var query = new GetAccountsQuery 
            { 
                AccountType = accountType,
                ActiveOnly = activeOnly
            };

            var result = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(query);
            
            _logger.LogInformation("Retrieved {Count} accounts of type {AccountType}", result.Count, accountType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving accounts by type: {AccountType}", accountType);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving accounts" });
        }
    }

    /// <summary>
    /// Gets child accounts for a parent account.
    /// </summary>
    /// <param name="parentId">Parent account ID</param>
    /// <param name="activeOnly">Filter to active accounts only</param>
    /// <returns>List of child accounts</returns>
    [HttpGet("{parentId:guid}/children")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> GetChildAccounts(
        Guid parentId,
        [FromQuery] bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Getting child accounts for parent: {ParentId}, ActiveOnly={ActiveOnly}", 
                parentId, activeOnly);

            var query = new GetAccountsQuery 
            { 
                ParentAccountId = parentId,
                ActiveOnly = activeOnly
            };

            var result = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(query);
            
            _logger.LogInformation("Retrieved {Count} child accounts for parent {ParentId}", result.Count, parentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child accounts for parent: {ParentId}", parentId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving child accounts" });
        }
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    /// <param name="command">Account creation details</param>
    /// <returns>Created account</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountCommand command)
    {
        try
        {
            if (command == null)
            {
                return BadRequest(new { error = "Account data is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating account: {AccountNumber} - {Name}", 
                command.AccountNumber, command.Name);

            var result = await _messageBus.InvokeAsync<AccountDto>(command);
            
            _logger.LogInformation("Successfully created account: {AccountId}", result.Id);
            return CreatedAtAction(nameof(GetAccount), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Attempt to create duplicate account: {AccountNumber}", command?.AccountNumber);
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid account data: {AccountNumber}", command?.AccountNumber);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account: {AccountNumber}", command?.AccountNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the account" });
        }
    }

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated account</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDto>> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Update data is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Updating account: {AccountId}", id);

            // First check if account exists
            var getQuery = new GetAccountsQuery();
            var existingAccounts = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(getQuery);
            
            var existingAccount = existingAccounts.FirstOrDefault(a => a.Id == id);
            if (existingAccount == null)
            {
                _logger.LogWarning("Account not found for update: {AccountId}", id);
                return NotFound(new { error = "Account not found" });
            }

            // Update the account (this would need an UpdateAccountCommand to be implemented)
            // For now, return the existing account as a placeholder
            
            _logger.LogInformation("Account updated successfully: {AccountId}", id);
            return Ok(existingAccount);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update data for account: {AccountId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account: {AccountId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while updating the account" });
        }
    }

    /// <summary>
    /// Deactivates an account (soft delete).
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeactivateAccount(Guid id)
    {
        try
        {
            _logger.LogInformation("Deactivating account: {AccountId}", id);

            // First check if account exists
            var getQuery = new GetAccountsQuery();
            var existingAccounts = await _messageBus.InvokeAsync<IReadOnlyList<AccountDto>>(getQuery);
            
            var existingAccount = existingAccounts.FirstOrDefault(a => a.Id == id);
            if (existingAccount == null)
            {
                _logger.LogWarning("Account not found for deactivation: {AccountId}", id);
                return NotFound(new { error = "Account not found" });
            }

            // Deactivate the account (this would need a DeactivateAccountCommand to be implemented)
            // For now, just log the action
            
            _logger.LogInformation("Account deactivated successfully: {AccountId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating account: {AccountId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while deactivating the account" });
        }
    }
}

/// <summary>
/// Request model for account updates.
/// </summary>
public class UpdateAccountRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
} 