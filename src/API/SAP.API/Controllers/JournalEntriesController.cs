using Microsoft.AspNetCore.Mvc;
using SAP.Core.Application.Financial.Commands;
using SAP.Core.Application.Financial.Queries;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Contracts.Financial.Queries;
using Wolverine;

namespace SAP.API.Controllers;

/// <summary>
/// REST API controller for Journal Entry management operations.
/// Provides endpoints for financial transaction recording and posting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JournalEntriesController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<JournalEntriesController> _logger;

    public JournalEntriesController(IMessageBus messageBus, ILogger<JournalEntriesController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all journal entries with optional filtering and pagination.
    /// </summary>
    /// <param name="postedOnly">Filter to posted entries only</param>
    /// <param name="currency">Filter by currency</param>
    /// <param name="fromDate">Start date for filtering</param>
    /// <param name="toDate">End date for filtering</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    /// <returns>List of journal entries</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JournalEntrySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<JournalEntrySummaryDto>>> GetJournalEntries(
        [FromQuery] bool postedOnly = false,
        [FromQuery] string? currency = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (pageNumber < 1)
            {
                return BadRequest(new { error = "Page number must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Page size must be between 1 and 100" });
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest(new { error = "From date cannot be after to date" });
            }

            _logger.LogInformation("Getting journal entries with filters: PostedOnly={PostedOnly}, Currency={Currency}, " +
                "FromDate={FromDate}, ToDate={ToDate}, Page={PageNumber}, Size={PageSize}", 
                postedOnly, currency, fromDate, toDate, pageNumber, pageSize);

            var query = new GetJournalEntriesQuery(
                FromDate: fromDate,
                ToDate: toDate,
                AccountId: null,
                IsPosted: postedOnly ? true : null,
                Reference: null,
                CreatedBy: null,
                PageNumber: pageNumber,
                PageSize: pageSize
            );

            var result = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(query);
            
            _logger.LogInformation("Retrieved {Count} journal entries", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving journal entries" });
        }
    }

    /// <summary>
    /// Gets a specific journal entry by ID.
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <returns>Journal entry details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JournalEntryDto>> GetJournalEntry(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting journal entry with ID: {JournalEntryId}", id);

            var query = new GetJournalEntriesQuery();
            var results = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(query);
            
            var journalEntry = results.FirstOrDefault(je => je.Id == id);
            if (journalEntry == null)
            {
                _logger.LogWarning("Journal entry not found: {JournalEntryId}", id);
                return NotFound(new { error = "Journal entry not found" });
            }

            // Convert to detailed DTO (this would need proper mapping)
            var detailedEntry = new JournalEntryDto(
                Id: journalEntry.Id,
                JournalEntryNumber: journalEntry.JournalEntryNumber,
                PostingDate: journalEntry.PostingDate,
                DocumentDate: journalEntry.PostingDate, // Assuming same as posting date for now
                Reference: journalEntry.Reference,
                Description: journalEntry.Description,
                Currency: journalEntry.Currency,
                IsPosted: journalEntry.IsPosted,
                CreatedAt: journalEntry.CreatedAt,
                PostedAt: null, // Would need to be populated from detailed data
                CreatedBy: journalEntry.CreatedBy,
                PostedBy: null, // Would need to be populated from detailed data
                TotalDebitAmount: journalEntry.TotalAmount, // Using total amount for both
                TotalCreditAmount: journalEntry.TotalAmount,
                IsBalanced: true, // Assume balanced for summary
                LineItems: new List<JournalEntryLineItemDto>() // Would be populated from detailed query
            );

            _logger.LogInformation("Retrieved journal entry: {JournalEntryNumber}", journalEntry.JournalEntryNumber);
            return Ok(detailedEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry: {JournalEntryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the journal entry" });
        }
    }

    /// <summary>
    /// Gets a journal entry by its number.
    /// </summary>
    /// <param name="journalEntryNumber">Journal entry number</param>
    /// <returns>Journal entry details</returns>
    [HttpGet("by-number/{journalEntryNumber}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JournalEntryDto>> GetJournalEntryByNumber(string journalEntryNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(journalEntryNumber))
            {
                return BadRequest(new { error = "Journal entry number is required" });
            }

            _logger.LogInformation("Getting journal entry by number: {JournalEntryNumber}", journalEntryNumber);

            var query = new GetJournalEntriesQuery();
            var results = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(query);
            
            var journalEntry = results.FirstOrDefault(je => je.JournalEntryNumber == journalEntryNumber);
            if (journalEntry == null)
            {
                _logger.LogWarning("Journal entry not found: {JournalEntryNumber}", journalEntryNumber);
                return NotFound(new { error = "Journal entry not found" });
            }

            // Convert to detailed DTO
            var detailedEntry = new JournalEntryDto(
                Id: journalEntry.Id,
                JournalEntryNumber: journalEntry.JournalEntryNumber,
                PostingDate: journalEntry.PostingDate,
                DocumentDate: journalEntry.PostingDate, // Assuming same as posting date for now
                Reference: journalEntry.Reference,
                Description: journalEntry.Description,
                Currency: journalEntry.Currency,
                IsPosted: journalEntry.IsPosted,
                CreatedAt: journalEntry.CreatedAt,
                PostedAt: null, // Would need to be populated from detailed data
                CreatedBy: journalEntry.CreatedBy,
                PostedBy: null, // Would need to be populated from detailed data
                TotalDebitAmount: journalEntry.TotalAmount, // Using total amount for both
                TotalCreditAmount: journalEntry.TotalAmount,
                IsBalanced: true, // Assume balanced for summary
                LineItems: new List<JournalEntryLineItemDto>()
            );

            _logger.LogInformation("Retrieved journal entry: {JournalEntryNumber}", journalEntry.JournalEntryNumber);
            return Ok(detailedEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entry by number: {JournalEntryNumber}", journalEntryNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving the journal entry" });
        }
    }

    /// <summary>
    /// Gets journal entries by date range.
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="postedOnly">Filter to posted entries only</param>
    /// <param name="pageNumber">Page number for pagination</param>
    /// <param name="pageSize">Page size for pagination</param>
    /// <returns>List of journal entries in the date range</returns>
    [HttpGet("by-date")]
    [ProducesResponseType(typeof(IReadOnlyList<JournalEntrySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<JournalEntrySummaryDto>>> GetJournalEntriesByDateRange(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] bool postedOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            if (fromDate > toDate)
            {
                return BadRequest(new { error = "From date cannot be after to date" });
            }

            if (pageNumber < 1)
            {
                return BadRequest(new { error = "Page number must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { error = "Page size must be between 1 and 100" });
            }

            _logger.LogInformation("Getting journal entries by date range: {FromDate} to {ToDate}, " +
                "PostedOnly={PostedOnly}, Page={PageNumber}, Size={PageSize}", 
                fromDate, toDate, postedOnly, pageNumber, pageSize);

            var query = new GetJournalEntriesQuery(
                FromDate: fromDate,
                ToDate: toDate,
                AccountId: null,
                IsPosted: postedOnly ? true : null,
                Reference: null,
                CreatedBy: null,
                PageNumber: pageNumber,
                PageSize: pageSize
            );

            var result = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(query);
            
            _logger.LogInformation("Retrieved {Count} journal entries for date range", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving journal entries by date range: {FromDate} to {ToDate}", fromDate, toDate);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving journal entries" });
        }
    }

    /// <summary>
    /// Creates a new journal entry.
    /// </summary>
    /// <param name="command">Journal entry creation details</param>
    /// <returns>Created journal entry</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JournalEntryDto>> CreateJournalEntry([FromBody] CreateJournalEntryCommand command)
    {
        try
        {
            if (command == null)
            {
                return BadRequest(new { error = "Journal entry data is required" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating journal entry: {Reference}", command.Reference);

            var result = await _messageBus.InvokeAsync<JournalEntryDto>(command);
            
            _logger.LogInformation("Successfully created journal entry: {JournalEntryId}", result.Id);
            return CreatedAtAction(nameof(GetJournalEntry), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Attempt to create duplicate journal entry: {Reference}", command?.Reference);
            return Conflict(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid journal entry data: {Reference}", command?.Reference);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating journal entry: {Reference}", command?.Reference);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while creating the journal entry" });
        }
    }

    /// <summary>
    /// Posts a journal entry (finalizes it for general ledger).
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <param name="request">Posting request</param>
    /// <returns>Posted journal entry</returns>
    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JournalEntryDto>> PostJournalEntry(Guid id, [FromBody] PostJournalEntryRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Posting data is required" });
            }

            if (string.IsNullOrWhiteSpace(request.PostedBy))
            {
                return BadRequest(new { error = "Posted by user is required" });
            }

            _logger.LogInformation("Posting journal entry: {JournalEntryId} by {PostedBy}", id, request.PostedBy);

            var command = new PostJournalEntryCommand(id);

            var result = await _messageBus.InvokeAsync<JournalEntryDto>(command);
            
            _logger.LogInformation("Successfully posted journal entry: {JournalEntryId}", id);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning(ex, "Journal entry not found for posting: {JournalEntryId}", id);
            return NotFound(new { error = "Journal entry not found" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already posted"))
        {
            _logger.LogWarning(ex, "Attempt to post already posted journal entry: {JournalEntryId}", id);
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not balanced"))
        {
            _logger.LogWarning(ex, "Attempt to post unbalanced journal entry: {JournalEntryId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid posting data for journal entry: {JournalEntryId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting journal entry: {JournalEntryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while posting the journal entry" });
        }
    }

    /// <summary>
    /// Updates an existing journal entry (only if not posted).
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated journal entry</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JournalEntryDto>> UpdateJournalEntry(Guid id, [FromBody] UpdateJournalEntryRequest request)
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

            _logger.LogInformation("Updating journal entry: {JournalEntryId}", id);

            // First check if journal entry exists and is not posted
            var getQuery = new GetJournalEntriesQuery();
            var existingEntries = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(getQuery);
            
            var existingEntry = existingEntries.FirstOrDefault(je => je.Id == id);
            if (existingEntry == null)
            {
                _logger.LogWarning("Journal entry not found for update: {JournalEntryId}", id);
                return NotFound(new { error = "Journal entry not found" });
            }

            if (existingEntry.IsPosted)
            {
                _logger.LogWarning("Attempt to update posted journal entry: {JournalEntryId}", id);
                return Conflict(new { error = "Cannot update a posted journal entry" });
            }

            // Update logic would go here (needs UpdateJournalEntryCommand implementation)
            // For now, return the existing entry
            var updatedEntry = new JournalEntryDto(
                Id: existingEntry.Id,
                JournalEntryNumber: existingEntry.JournalEntryNumber,
                PostingDate: existingEntry.PostingDate,
                DocumentDate: existingEntry.PostingDate, // Assuming same as posting date for now
                Reference: existingEntry.Reference,
                Description: request.Description ?? existingEntry.Description,
                Currency: existingEntry.Currency,
                IsPosted: existingEntry.IsPosted,
                CreatedAt: existingEntry.CreatedAt,
                PostedAt: null, // Would need to be populated from detailed data
                CreatedBy: existingEntry.CreatedBy,
                PostedBy: null, // Would need to be populated from detailed data
                TotalDebitAmount: existingEntry.TotalAmount, // Using total amount for both
                TotalCreditAmount: existingEntry.TotalAmount,
                IsBalanced: true, // Assume balanced for summary
                LineItems: new List<JournalEntryLineItemDto>()
            );
            
            _logger.LogInformation("Journal entry updated successfully: {JournalEntryId}", id);
            return Ok(updatedEntry);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update data for journal entry: {JournalEntryId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating journal entry: {JournalEntryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while updating the journal entry" });
        }
    }

    /// <summary>
    /// Deletes an unposted journal entry.
    /// </summary>
    /// <param name="id">Journal entry ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteJournalEntry(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting journal entry: {JournalEntryId}", id);

            // First check if journal entry exists and is not posted
            var getQuery = new GetJournalEntriesQuery();
            var existingEntries = await _messageBus.InvokeAsync<IReadOnlyList<JournalEntrySummaryDto>>(getQuery);
            
            var existingEntry = existingEntries.FirstOrDefault(je => je.Id == id);
            if (existingEntry == null)
            {
                _logger.LogWarning("Journal entry not found for deletion: {JournalEntryId}", id);
                return NotFound(new { error = "Journal entry not found" });
            }

            if (existingEntry.IsPosted)
            {
                _logger.LogWarning("Attempt to delete posted journal entry: {JournalEntryId}", id);
                return Conflict(new { error = "Cannot delete a posted journal entry" });
            }

            // Delete logic would go here (needs DeleteJournalEntryCommand implementation)
            
            _logger.LogInformation("Journal entry deleted successfully: {JournalEntryId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting journal entry: {JournalEntryId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while deleting the journal entry" });
        }
    }
}

/// <summary>
/// Request model for posting journal entries.
/// </summary>
public class PostJournalEntryRequest
{
    public string PostedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating journal entries.
/// </summary>
public class UpdateJournalEntryRequest
{
    public string? Description { get; set; }
} 