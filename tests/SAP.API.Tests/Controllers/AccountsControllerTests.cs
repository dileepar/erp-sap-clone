using SAP.API.Tests.TestBase;
using SAP.Core.Contracts.Financial.Commands;
using SAP.Core.Contracts.Financial.DTOs;
using SAP.Core.Domain.Financial.Enums;
using System.Net;
using System.Net.Http.Json;

namespace SAP.API.Tests.Controllers;

/// <summary>
/// Integration tests for AccountsController.
/// Tests the full HTTP pipeline including validation, business logic, and responses.
/// </summary>
public class AccountsControllerTests : ApiTestBase
{
    private const string ApiBase = "/api/accounts";

    public async Task<bool> TestGetAccounts_ShouldReturnOkResult_WhenAccountsExist()
    {
        try
        {
            // Arrange - First create an account
            var createCommand = new CreateAccountCommand(
                AccountNumber: "1000",
                Name: "Test Account",
                AccountType: AccountType.Asset,
                Currency: "USD",
                Description: "Test Description",
                ParentAccountId: null,
                IsControlAccount: false
            );

            // Act - Create account first
            var createResponse = await Client.PostAsJsonAsync(ApiBase, createCommand);
            if (createResponse.StatusCode != HttpStatusCode.Created)
                return false;

            // Act - Get all accounts
            var response = await Client.GetAsync(ApiBase);

            // Assert
            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            var accounts = await FromJsonAsync<List<AccountDto>>(response);
            return accounts != null && accounts.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TestCreateAccount_ShouldReturnCreated_WhenValidAccount()
    {
        try
        {
            // Arrange
            var command = new CreateAccountCommand(
                AccountNumber: "1400",
                Name: "Test Create Account",
                AccountType: AccountType.Asset,
                Currency: "USD",
                Description: "Create test",
                ParentAccountId: null,
                IsControlAccount: false
            );

            // Act
            var response = await Client.PostAsJsonAsync(ApiBase, command);

            // Assert
            if (response.StatusCode != HttpStatusCode.Created)
                return false;

            if (response.Headers.Location == null)
                return false;

            var account = await FromJsonAsync<AccountDto>(response);
            return account != null && 
                   account.AccountNumber == "1400" &&
                   account.Name == "Test Create Account" &&
                   account.AccountType == AccountType.Asset &&
                   account.Currency == "USD";
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TestCreateAccount_ShouldReturnBadRequest_WhenUnbalanced()
    {
        try
        {
            // Arrange
            var command = new CreateAccountCommand(
                AccountNumber: "",
                Name: "Test Account",
                AccountType: AccountType.Asset,
                Currency: "USD",
                Description: "Invalid number test",
                ParentAccountId: null,
                IsControlAccount: false
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

    public async Task<bool> RunAllTests()
    {
        var test1 = await TestGetAccounts_ShouldReturnOkResult_WhenAccountsExist();
        var test2 = await TestCreateAccount_ShouldReturnCreated_WhenValidAccount();
        var test3 = await TestCreateAccount_ShouldReturnBadRequest_WhenUnbalanced();

        return test1 && test2 && test3;
    }
} 