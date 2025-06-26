using SAP.API.Tests.TestBase;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Domain.Financial.Enums;
using System.Net;
using System.Net.Http.Json;

namespace SAP.API.Tests.Controllers;

/// <summary>
/// Integration tests for JournalEntriesController.
/// Tests the full HTTP pipeline including double-entry bookkeeping validation and business logic.
/// </summary>
public class JournalEntriesControllerTests : ApiTestBase
{
    private const string ApiBase = "/api/journal-entries";
    private const string AccountsApiBase = "/api/accounts";

    private async Task<AccountDto> CreateTestAccountAsync(string accountNumber, string name, AccountType accountType, string currency = "USD")
    {
        var command = new CreateAccountCommand(
            AccountNumber: accountNumber,
            Name: name,
            AccountType: accountType,
            Currency: currency,
            Description: $"Test {accountType} account",
            ParentAccountId: null,
            IsControlAccount: false
        );

        var response = await Client.PostAsJsonAsync(AccountsApiBase, command);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to create test account: {response.StatusCode}");
        
        return (await FromJsonAsync<AccountDto>(response))!;
    }

    public async Task<bool> TestCreateJournalEntry_ShouldReturnCreated_WhenValidBalancedEntry()
    {
        try
        {
            // Arrange - Create test accounts
            var cashAccount = await CreateTestAccountAsync("1400", "Cash", AccountType.Asset);
            var salesAccount = await CreateTestAccountAsync("4400", "Sales", AccountType.Revenue);

            var command = new CreateJournalEntryCommand(
                PostingDate: DateTime.UtcNow.Date,
                DocumentDate: DateTime.UtcNow.Date,
                Reference: "JE004",
                Description: "Test balanced entry",
                Currency: "USD",
                LineItems: new List<CreateJournalEntryLineItemCommand>
                {
                    new CreateJournalEntryLineItemCommand(
                        AccountId: cashAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Debit,
                        Amount: 1200.00m,
                        Description: "Cash received from sale"
                    ),
                    new CreateJournalEntryLineItemCommand(
                        AccountId: salesAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Credit,
                        Amount: 1200.00m,
                        Description: "Sales revenue earned"
                    )
                }
            );

            // Act
            var response = await Client.PostAsJsonAsync(ApiBase, command);

            // Assert
            if (response.StatusCode != HttpStatusCode.Created)
                return false;

            if (response.Headers.Location == null)
                return false;

            var entry = await FromJsonAsync<JournalEntryDto>(response);
            return entry != null &&
                   entry.Reference == "JE004" &&
                   entry.Description == "Test balanced entry" &&
                   entry.LineItems.Count == 2 &&
                   entry.TotalDebitAmount == 1200.00m &&
                   entry.TotalCreditAmount == 1200.00m;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TestCreateJournalEntry_ShouldReturnBadRequest_WhenUnbalanced()
    {
        try
        {
            // Arrange - Create test accounts
            var cashAccount = await CreateTestAccountAsync("1500", "Cash", AccountType.Asset);
            var salesAccount = await CreateTestAccountAsync("4500", "Sales", AccountType.Revenue);

            var command = new CreateJournalEntryCommand(
                PostingDate: DateTime.UtcNow.Date,
                DocumentDate: DateTime.UtcNow.Date,
                Reference: "JE005",
                Description: "Test unbalanced entry",
                Currency: "USD",
                LineItems: new List<CreateJournalEntryLineItemCommand>
                {
                    new CreateJournalEntryLineItemCommand(
                        AccountId: cashAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Debit,
                        Amount: 1000.00m, // Debit $1000
                        Description: "Cash received"
                    ),
                    new CreateJournalEntryLineItemCommand(
                        AccountId: salesAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Credit,
                        Amount: 800.00m, // Credit $800 - UNBALANCED!
                        Description: "Sales revenue"
                    )
                }
            );

            // Act
            var response = await Client.PostAsJsonAsync(ApiBase, command);

            // Assert
            return response.StatusCode == HttpStatusCode.BadRequest;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TestGetJournalEntries_ShouldReturnOkResult_WhenEntriesExist()
    {
        try
        {
            // Arrange - Create test accounts
            var cashAccount = await CreateTestAccountAsync("1000", "Cash", AccountType.Asset);
            var salesAccount = await CreateTestAccountAsync("4000", "Sales", AccountType.Revenue);

            // Create a journal entry
            var createCommand = new CreateJournalEntryCommand(
                PostingDate: DateTime.UtcNow.Date,
                DocumentDate: DateTime.UtcNow.Date,
                Reference: "JE001",
                Description: "Test sales transaction",
                Currency: "USD",
                LineItems: new List<CreateJournalEntryLineItemCommand>
                {
                    new CreateJournalEntryLineItemCommand(
                        AccountId: cashAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Debit,
                        Amount: 1000.00m,
                        Description: "Cash received"
                    ),
                    new CreateJournalEntryLineItemCommand(
                        AccountId: salesAccount.Id,
                        DebitCreditIndicator: DebitCreditIndicator.Credit,
                        Amount: 1000.00m,
                        Description: "Sales revenue"
                    )
                }
            );

            // Act - Create journal entry first
            var createResponse = await Client.PostAsJsonAsync(ApiBase, createCommand);
            if (createResponse.StatusCode != HttpStatusCode.Created)
                return false;

            // Act - Get all journal entries
            var response = await Client.GetAsync(ApiBase);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var entries = await FromJsonAsync<List<JournalEntrySummaryDto>>(response);
            return entries != null && entries.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RunAllTests()
    {
        var test1 = await TestGetJournalEntries_ShouldReturnOkResult_WhenEntriesExist();
        var test2 = await TestCreateJournalEntry_ShouldReturnCreated_WhenValidBalancedEntry();
        var test3 = await TestCreateJournalEntry_ShouldReturnBadRequest_WhenUnbalanced();

        return test1 && test2 && test3;
    }
} 