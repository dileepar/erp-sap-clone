using FluentAssertions;
using SAP.Core.Domain.Shared;
using Xunit;

namespace SAP.Core.Domain.Tests.Shared;

/// <summary>
/// Unit tests for the Money value object.
/// Critical for financial accuracy in all calculations.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Constructor_ValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange & Act
        var money = new Money(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_NullCurrency_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Money(100m, null!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency cannot be null or empty*");
    }

    [Fact]
    public void Constructor_EmptyCurrency_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Money(100m, "");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency cannot be null or empty*");
    }

    [Fact]
    public void Constructor_InvalidCurrencyLength_ShouldThrowArgumentException()
    {
        // Act & Assert
        Action act = () => new Money(100m, "US");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Currency must be a 3-letter ISO code*");
    }

    [Fact]
    public void Constructor_LowercaseCurrency_ShouldConvertToUppercase()
    {
        // Arrange & Act
        var money = new Money(100m, "usd");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(100.555, 100.56)] // Round up
    [InlineData(100.554, 100.55)] // Round down
    [InlineData(100.565, 100.57)] // Round up (away from zero)
    public void Constructor_RoundsAmountToTwoDecimalPlaces(decimal input, decimal expected)
    {
        // Arrange & Act
        var money = new Money(input, "USD");

        // Assert
        money.Amount.Should().Be(expected);
    }

    [Fact]
    public void Zero_ShouldCreateZeroAmount()
    {
        // Arrange & Act
        var money = Money.Zero("USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
        money.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Addition_SameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(200.25m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        result.Amount.Should().Be(300.75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Addition_DifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(100m, "EUR");

        // Act & Assert
        Action act = () => _ = money1 + money2;
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot perform operation on different currencies: USD and EUR");
    }

    [Fact]
    public void Subtraction_SameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = new Money(300.75m, "USD");
        var money2 = new Money(100.25m, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        result.Amount.Should().Be(200.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtraction_DifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act & Assert
        Action act = () => _ = money1 - money2;
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Cannot perform operation on different currencies: USD and EUR");
    }

    [Fact]
    public void Multiplication_ByDecimal_ShouldMultiplyAmount()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money * 2.5m;

        // Assert
        result.Amount.Should().Be(251.25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Division_ByDecimal_ShouldDivideAmount()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money / 4m;

        // Assert
        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Division_ByZero_ShouldThrowDivideByZeroException()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act & Assert
        Action act = () => _ = money / 0m;
        act.Should().Throw<DivideByZeroException>()
           .WithMessage("Cannot divide money by zero");
    }

    [Fact]
    public void Negate_ShouldReturnNegativeAmount()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money.Negate();

        // Assert
        result.Amount.Should().Be(-100.50m);
        result.Currency.Should().Be("USD");
    }

    [Theory]
    [InlineData(100.50, true, false, false)]
    [InlineData(-100.50, false, true, false)]
    [InlineData(0, false, false, true)]
    public void Properties_ShouldReturnCorrectValues(decimal amount, bool isPositive, bool isNegative, bool isZero)
    {
        // Arrange & Act
        var money = new Money(amount, "USD");

        // Assert
        money.IsPositive.Should().Be(isPositive);
        money.IsNegative.Should().Be(isNegative);
        money.IsZero.Should().Be(isZero);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(1234.56m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("$1,234.56 USD");
    }

    [Fact]
    public void ToString_WithFormat_ShouldReturnCustomFormattedString()
    {
        // Arrange
        var money = new Money(1234.56m, "USD");

        // Act
        var result = money.ToString("F4");

        // Assert
        result.Should().Be("1234.5600 USD");
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
        (money1 != money2).Should().BeFalse();
    }

    [Fact]
    public void Equality_DifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.51m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 == money2).Should().BeFalse();
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameAmountAndCurrency_ShouldHaveSameHashCode()
    {
        // Arrange
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(100.50m, "USD");

        // Act & Assert
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }
} 