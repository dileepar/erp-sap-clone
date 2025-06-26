using FluentAssertions;
using SAP.Core.Domain.Financial.Entities;
using SAP.Core.Domain.Financial.Enums;
using SAP.Core.Domain.Shared;
using Xunit;

namespace SAP.Core.Domain.Tests.Financial.Entities;

/// <summary>
/// Unit tests for the Account entity.
/// Tests business rules, validation, and account hierarchy logic.
/// </summary>
public class AccountTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateAccount()
    {
        // Arrange & Act
        var account = new Account(
            "1000",
            "Cash",
            "Main cash account",
            AccountType.Asset,
            "USD");

        // Assert
        account.AccountNumber.Should().Be("1000");
        account.Name.Should().Be("Cash");
        account.Description.Should().Be("Main cash account");
        account.AccountType.Should().Be(AccountType.Asset);
        account.Currency.Should().Be("USD");
        account.IsActive.Should().BeTrue();
        account.IsControlAccount.Should().BeFalse();
        account.ParentAccountId.Should().BeNull();
        account.CurrentBalance.Should().Be(Money.Zero("USD"));
        account.Id.Should().NotBe(Guid.Empty);
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithParentAccount_ShouldSetParentAccountId()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        // Act
        var account = new Account(
            "1001",
            "Petty Cash",
            "Small cash fund",
            AccountType.Asset,
            "USD",
            parentId);

        // Assert
        account.ParentAccountId.Should().Be(parentId);
    }

    [Fact]
    public void Constructor_AsControlAccount_ShouldSetIsControlAccount()
    {
        // Arrange & Act
        var account = new Account(
            "1000",
            "Current Assets",
            "Control account for current assets",
            AccountType.Asset,
            "USD",
            isControlAccount: true);

        // Assert
        account.IsControlAccount.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidAccountNumber_ShouldThrowArgumentException(string? accountNumber)
    {
        // Act & Assert
        Action act = () => new Account(accountNumber, "Cash", "", AccountType.Asset, "USD");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Account number cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidName_ShouldThrowArgumentException(string? name)
    {
        // Act & Assert
        Action act = () => new Account("1000", name, "", AccountType.Asset, "USD");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Account name cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidCurrency_ShouldThrowArgumentException(string? currency)
    {
        // Act & Assert
        Action act = () => new Account("1000", "Cash", "", AccountType.Asset, currency);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency cannot be null or empty*");
    }

    [Fact]
    public void Constructor_InvalidCurrencyLength_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Account("1000", "Cash", "", AccountType.Asset, "US");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency must be a 3-letter ISO code*");
    }

    [Fact]
    public void Constructor_LowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange & Act
        var account = new Account("1000", "Cash", "", AccountType.Asset, "usd");

        // Assert
        account.Currency.Should().Be("USD");
    }

    [Fact]
    public void UpdateBalance_SameCurrency_ShouldUpdateBalance()
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");
        var newBalance = new Money(1000.50m, "USD");

        // Act
        account.UpdateBalance(newBalance);

        // Assert
        account.CurrentBalance.Should().Be(newBalance);
        account.UpdatedAt.Should().NotBeNull();
        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateBalance_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");
        var newBalance = new Money(1000.50m, "EUR");

        // Act & Assert
        Action act = () => account.UpdateBalance(newBalance);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Balance currency EUR does not match account currency USD");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");

        // Act
        account.Deactivate();

        // Assert
        account.IsActive.Should().BeFalse();
        account.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");
        account.Deactivate();

        // Act
        account.Activate();

        // Assert
        account.IsActive.Should().BeTrue();
        account.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateInfo_ValidData_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var account = new Account("1000", "Cash", "Old description", AccountType.Asset, "USD");

        // Act
        account.UpdateInfo("Updated Cash", "New description");

        // Assert
        account.Name.Should().Be("Updated Cash");
        account.Description.Should().Be("New description");
        account.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateInfo_InvalidName_ShouldThrowArgumentException(string? name)
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");

        // Act & Assert
        Action act = () => account.UpdateInfo(name, "Description");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Account name cannot be null or empty*");
    }

    [Fact]
    public void UpdateInfo_NullDescription_ShouldSetEmptyDescription()
    {
        // Arrange
        var account = new Account("1000", "Cash", "Old", AccountType.Asset, "USD");

        // Act
        account.UpdateInfo("Cash", null!);

        // Assert
        account.Description.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData(AccountType.Asset, DebitCreditIndicator.Debit)]
    [InlineData(AccountType.Expense, DebitCreditIndicator.Debit)]
    [InlineData(AccountType.Liability, DebitCreditIndicator.Credit)]
    [InlineData(AccountType.Equity, DebitCreditIndicator.Credit)]
    [InlineData(AccountType.Revenue, DebitCreditIndicator.Credit)]
    public void GetNormalBalanceSide_ShouldReturnCorrectSide(AccountType accountType, DebitCreditIndicator expectedSide)
    {
        // Arrange
        var account = new Account("1000", "Test Account", "", accountType, "USD");

        // Act
        var normalSide = account.GetNormalBalanceSide();

        // Assert
        normalSide.Should().Be(expectedSide);
    }

    [Fact]
    public void CanHaveChildren_ControlAccount_ShouldReturnTrue()
    {
        // Arrange
        var account = new Account("1000", "Current Assets", "", AccountType.Asset, "USD", isControlAccount: true);

        // Act
        var canHaveChildren = account.CanHaveChildren();

        // Assert
        canHaveChildren.Should().BeTrue();
    }

    [Fact]
    public void CanHaveChildren_NonControlAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account("1001", "Cash", "", AccountType.Asset, "USD", isControlAccount: false);

        // Act
        var canHaveChildren = account.CanHaveChildren();

        // Assert
        canHaveChildren.Should().BeFalse();
    }

    [Fact]
    public void CanBeUsedInTransactions_ActiveNonControlAccount_ShouldReturnTrue()
    {
        // Arrange
        var account = new Account("1001", "Cash", "", AccountType.Asset, "USD", isControlAccount: false);

        // Act
        var canBeUsed = account.CanBeUsedInTransactions();

        // Assert
        canBeUsed.Should().BeTrue();
    }

    [Fact]
    public void CanBeUsedInTransactions_InactiveAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account("1001", "Cash", "", AccountType.Asset, "USD");
        account.Deactivate();

        // Act
        var canBeUsed = account.CanBeUsedInTransactions();

        // Assert
        canBeUsed.Should().BeFalse();
    }

    [Fact]
    public void CanBeUsedInTransactions_ControlAccount_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account("1000", "Current Assets", "", AccountType.Asset, "USD", isControlAccount: true);

        // Act
        var canBeUsed = account.CanBeUsedInTransactions();

        // Assert
        canBeUsed.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");

        // Act
        var result = account.ToString();

        // Assert
        result.Should().Be("1000 - Cash (Asset)");
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange & Act
        var account1 = new Account("1000", "Cash", "", AccountType.Asset, "USD");
        var account2 = new Account("1001", "Bank", "", AccountType.Asset, "USD");

        // Assert
        account1.Id.Should().NotBe(account2.Id);
        account1.Id.Should().NotBe(Guid.Empty);
        account2.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldInitializeChildAccountsCollection()
    {
        // Arrange & Act
        var account = new Account("1000", "Cash", "", AccountType.Asset, "USD");

        // Assert
        account.ChildAccounts.Should().NotBeNull();
        account.ChildAccounts.Should().BeEmpty();
    }
} 