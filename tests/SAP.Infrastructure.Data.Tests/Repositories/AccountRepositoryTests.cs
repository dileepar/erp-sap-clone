using Microsoft.Extensions.Logging;
using Moq;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Infrastructure.Data.Repositories;
using SAP.Infrastructure.Data.Tests.TestBase;

namespace SAP.Infrastructure.Data.Tests.Repositories;

/// <summary>
/// Unit tests for AccountRepository implementation.
/// Tests repository interface compliance and validation logic.
/// </summary>
public class AccountRepositoryTests : MartenTestBase
{
    private readonly AccountRepository _repository;
    private readonly Mock<ILogger<AccountRepository>> _mockLogger;

    public AccountRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<AccountRepository>>();
        _repository = new AccountRepository(Session, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateRepository()
    {
        // Act & Assert
        _repository.Should().NotBeNull();
        _mockLogger.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullSession_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AccountRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AccountRepository(Session, null!));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task AddAsync_WithNullAccount_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullAccount_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.UpdateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExistsWithAccountNumberAsync_WithInvalidAccountNumber_ShouldThrowArgumentException(string invalidAccountNumber)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.ExistsWithAccountNumberAsync(invalidAccountNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByAccountNumberAsync_WithInvalidAccountNumber_ShouldThrowArgumentException(string invalidAccountNumber)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.GetByAccountNumberAsync(invalidAccountNumber));
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Repository_ShouldImplementIAccountRepository()
    {
        // Act & Assert
        _repository.Should().BeAssignableTo<SAP.Core.Domain.Financial.Repositories.IAccountRepository>();
    }

    #endregion

    #region Helper Methods

    private static Account CreateTestAccount(
        string accountNumber, 
        string name, 
        AccountType accountType, 
        Guid? parentAccountId = null,
        bool isControlAccount = false)
    {
        return new Account(
            accountNumber,
            name,
            "Test Description",
            accountType,
            "USD",
            parentAccountId,
            isControlAccount);
    }

    #endregion
} 