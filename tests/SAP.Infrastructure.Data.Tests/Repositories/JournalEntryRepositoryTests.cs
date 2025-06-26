using Microsoft.Extensions.Logging;
using Moq;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Infrastructure.Data.Repositories;
using SAP.Infrastructure.Data.Tests.TestBase;

namespace SAP.Infrastructure.Data.Tests.Repositories;

/// <summary>
/// Unit tests for JournalEntryRepository implementation.
/// Tests repository interface compliance and validation logic.
/// </summary>
public class JournalEntryRepositoryTests : MartenTestBase
{
    private readonly JournalEntryRepository _repository;
    private readonly Mock<ILogger<JournalEntryRepository>> _mockLogger;

    public JournalEntryRepositoryTests()
    {
        _mockLogger = new Mock<ILogger<JournalEntryRepository>>();
        _repository = new JournalEntryRepository(Session, _mockLogger.Object);
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
            new JournalEntryRepository(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new JournalEntryRepository(Session, null!));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task AddAsync_WithNullJournalEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithNullJournalEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.UpdateAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByJournalEntryNumberAsync_WithInvalidJournalEntryNumber_ShouldThrowArgumentException(string invalidNumber)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.GetByJournalEntryNumberAsync(invalidNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExistsWithJournalEntryNumberAsync_WithInvalidJournalEntryNumber_ShouldThrowArgumentException(string invalidNumber)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.ExistsWithJournalEntryNumberAsync(invalidNumber));
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void Repository_ShouldImplementIJournalEntryRepository()
    {
        // Act & Assert
        _repository.Should().BeAssignableTo<SAP.Core.Domain.Financial.Repositories.IJournalEntryRepository>();
    }

    #endregion

    #region Helper Methods

    private static JournalEntry CreateTestJournalEntry(
        string journalEntryNumber, 
        DateTime postingDate, 
        string currency = "USD")
    {
        return new JournalEntry(
            journalEntryNumber,
            postingDate,
            DateTime.Now,
            currency,
            "Test Journal Entry",
            "Test Reference",
            "Test User");
    }

    #endregion
} 