using FluentAssertions;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Financial.Events;
using SAP.Core.Domain.Shared;
using Xunit;

namespace SAP.Core.Domain.Tests.Financial.Entities;

/// <summary>
/// Unit tests for the JournalEntry entity.
/// Tests double-entry bookkeeping rules, validation, and event sourcing.
/// </summary>
public class JournalEntryTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateJournalEntry()
    {
        // Arrange & Act
        var journalEntry = new JournalEntry(
            "JE-001",
            DateTime.Today,
            DateTime.Today.AddDays(-1),
            "INV-001",
            "Sales transaction",
            "USD",
            "testuser");

        // Assert
        journalEntry.JournalEntryNumber.Should().Be("JE-001");
        journalEntry.PostingDate.Should().Be(DateTime.Today);
        journalEntry.DocumentDate.Should().Be(DateTime.Today.AddDays(-1));
        journalEntry.Reference.Should().Be("INV-001");
        journalEntry.Description.Should().Be("Sales transaction");
        journalEntry.Currency.Should().Be("USD");
        journalEntry.CreatedBy.Should().Be("testuser");
        journalEntry.IsPosted.Should().BeFalse();
        journalEntry.PostedAt.Should().BeNull();
        journalEntry.PostedBy.Should().BeNull();
        journalEntry.LineItems.Should().BeEmpty();
        journalEntry.Id.Should().NotBe(Guid.Empty);
        journalEntry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidJournalEntryNumber_ShouldThrowArgumentException(string? journalEntryNumber)
    {
        // Act & Assert
        Action act = () => new JournalEntry(journalEntryNumber, DateTime.Today, DateTime.Today, "REF", "", "USD", "user");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Journal entry number cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidReference_ShouldThrowArgumentException(string? reference)
    {
        // Act & Assert
        Action act = () => new JournalEntry("JE-001", DateTime.Today, DateTime.Today, reference, "", "USD", "user");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Reference cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Act & Assert
        Action act = () => new JournalEntry("JE-001", DateTime.Today, DateTime.Today, "REF", "", currency, "user");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidCreatedBy_ShouldThrowArgumentException(string? createdBy)
    {
        // Act & Assert
        Action act = () => new JournalEntry("JE-001", DateTime.Today, DateTime.Today, "REF", "", "USD", createdBy);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Created by cannot be null or empty*");
    }

    [Fact]
    public void Constructor_LowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange & Act
        var journalEntry = new JournalEntry("JE-001", DateTime.Today, DateTime.Today, "REF", "", "usd", "user");

        // Assert
        journalEntry.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldPublishJournalEntryCreatedEvent()
    {
        // Arrange & Act
        var journalEntry = new JournalEntry("JE-001", DateTime.Today, DateTime.Today, "REF", "Description", "USD", "user");

        // Assert
        journalEntry.DomainEvents.Should().HaveCount(1);
        var createdEvent = journalEntry.DomainEvents.First() as JournalEntryCreatedEvent;
        createdEvent.Should().NotBeNull();
        createdEvent!.JournalEntryId.Should().Be(journalEntry.Id);
        createdEvent.JournalEntryNumber.Should().Be("JE-001");
        createdEvent.CreatedBy.Should().Be("user");
    }

    [Fact]
    public void AddLineItem_ValidData_ShouldAddLineItem()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        var accountId = Guid.NewGuid();
        var amount = new Money(1000m, "USD");

        // Act
        journalEntry.AddLineItem(accountId, "1000", DebitCreditIndicator.Debit, amount, "Cash receipt");

        // Assert
        journalEntry.LineItems.Should().HaveCount(1);
        var lineItem = journalEntry.LineItems.First();
        lineItem.AccountId.Should().Be(accountId);
        lineItem.AccountNumber.Should().Be("1000");
        lineItem.DebitCreditIndicator.Should().Be(DebitCreditIndicator.Debit);
        lineItem.Amount.Should().Be(amount);
        lineItem.Description.Should().Be("Cash receipt");
    }

    [Fact]
    public void AddLineItem_ShouldPublishLineItemAddedEvent()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        var accountId = Guid.NewGuid();
        var amount = new Money(1000m, "USD");

        // Act
        journalEntry.AddLineItem(accountId, "1000", DebitCreditIndicator.Debit, amount, "Cash receipt");

        // Assert
        journalEntry.DomainEvents.Should().HaveCount(2); // Created + LineItemAdded
        var lineItemAddedEvent = journalEntry.DomainEvents.OfType<JournalEntryLineItemAddedEvent>().First();
        lineItemAddedEvent.JournalEntryId.Should().Be(journalEntry.Id);
        lineItemAddedEvent.AccountId.Should().Be(accountId);
        lineItemAddedEvent.Amount.Should().Be(amount);
    }

    [Fact]
    public void AddLineItem_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        var amount = new Money(1000m, "EUR"); // Different currency

        // Act & Assert
        Action act = () => journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, amount, "Test");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Line item currency EUR does not match journal entry currency USD");
    }

    [Fact]
    public void AddLineItem_ToPostedEntry_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Post("testuser");

        // Act & Assert
        Action act = () => journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(100m, "USD"), "Test");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot modify posted journal entry");
    }

    [Fact]
    public void GetTotalDebitAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "1001", DebitCreditIndicator.Debit, new Money(500m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "4000", DebitCreditIndicator.Credit, new Money(1500m, "USD"), "");

        // Act
        var totalDebits = journalEntry.GetTotalDebitAmount();

        // Assert
        totalDebits.Should().Be(new Money(1500m, "USD"));
    }

    [Fact]
    public void GetTotalCreditAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "4000", DebitCreditIndicator.Credit, new Money(500m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "4001", DebitCreditIndicator.Credit, new Money(500m, "USD"), "");

        // Act
        var totalCredits = journalEntry.GetTotalCreditAmount();

        // Assert
        totalCredits.Should().Be(new Money(1000m, "USD"));
    }

    [Fact]
    public void IsBalanced_BalancedEntry_ShouldReturnTrue()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();

        // Act
        var isBalanced = journalEntry.IsBalanced();

        // Assert
        isBalanced.Should().BeTrue();
    }

    [Fact]
    public void IsBalanced_UnbalancedEntry_ShouldReturnFalse()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "4000", DebitCreditIndicator.Credit, new Money(500m, "USD"), ""); // Unbalanced

        // Act
        var isBalanced = journalEntry.IsBalanced();

        // Assert
        isBalanced.Should().BeFalse();
    }

    [Fact]
    public void Post_BalancedEntry_ShouldPostSuccessfully()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();

        // Act
        journalEntry.Post("postuser");

        // Assert
        journalEntry.IsPosted.Should().BeTrue();
        journalEntry.PostedBy.Should().Be("postuser");
        journalEntry.PostedAt.Should().NotBeNull();
        journalEntry.PostedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Post_ShouldPublishJournalEntryPostedEvent()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();

        // Act
        journalEntry.Post("postuser");

        // Assert
        var postedEvent = journalEntry.DomainEvents.OfType<JournalEntryPostedEvent>().First();
        postedEvent.JournalEntryId.Should().Be(journalEntry.Id);
        postedEvent.PostedBy.Should().Be("postuser");
        postedEvent.TotalAmount.Should().Be(new Money(1000m, "USD"));
    }

    [Fact]
    public void Post_AlreadyPosted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Post("firstuser");

        // Act & Assert
        Action act = () => journalEntry.Post("seconduser");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Journal entry is already posted");
    }

    [Fact]
    public void Post_UnbalancedEntry_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "");
        journalEntry.AddLineItem(Guid.NewGuid(), "4000", DebitCreditIndicator.Credit, new Money(500m, "USD"), "");

        // Act & Assert
        Action act = () => journalEntry.Post("postuser");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Journal entry is not balanced*");
    }

    [Fact]
    public void Post_NoLineItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();

        // Act & Assert
        Action act = () => journalEntry.Post("postuser");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot post journal entry without line items");
    }

    [Fact]
    public void Post_SingleLineItem_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "");

        // Act & Assert
        Action act = () => journalEntry.Post("postuser");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Journal entry must have at least 2 line items");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Post_InvalidPostedBy_ShouldThrowArgumentException(string? postedBy)
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();

        // Act & Assert
        Action act = () => journalEntry.Post(postedBy);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Posted by cannot be null or empty*");
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var journalEntry = CreateBalancedJournalEntry();
        journalEntry.Post("postuser");
        journalEntry.DomainEvents.Should().NotBeEmpty();

        // Act
        journalEntry.ClearDomainEvents();

        // Assert
        journalEntry.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var journalEntry = CreateTestJournalEntry();

        // Act
        var result = journalEntry.ToString();

        // Assert
        result.Should().Be($"JE-001 - Test transaction ({DateTime.Today:yyyy-MM-dd})");
    }

    private static JournalEntry CreateTestJournalEntry()
    {
        return new JournalEntry(
            "JE-001",
            DateTime.Today,
            DateTime.Today,
            "REF-001",
            "Test transaction",
            "USD",
            "testuser");
    }

    private static JournalEntry CreateBalancedJournalEntry()
    {
        var journalEntry = CreateTestJournalEntry();
        journalEntry.AddLineItem(Guid.NewGuid(), "1000", DebitCreditIndicator.Debit, new Money(1000m, "USD"), "Cash");
        journalEntry.AddLineItem(Guid.NewGuid(), "4000", DebitCreditIndicator.Credit, new Money(1000m, "USD"), "Revenue");
        return journalEntry;
    }
} 