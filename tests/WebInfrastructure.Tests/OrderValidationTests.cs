using System.ComponentModel.DataAnnotations;
using WebInfrastructure.Models;

namespace WebInfrastructure.Tests;

/// <summary>Tests of what the checkout form accepts.</summary>
public class OrderValidationTests
{
    private static Order ValidOrder() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Address = "1 Analytical Way",
        City = "London",
        State = "LDN",
        PostalCode = "N1 9GU",
        Country = "UK",
        Phone = "555-0100",
        Email = "ada@example.com",
    };

    private static List<ValidationResult> Validate(Order order)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(order, new ValidationContext(order), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ACompleteOrderIsValid()
    {
        Assert.Empty(Validate(ValidOrder()));
    }

    [Theory]
    [InlineData("FirstName", "First Name is required")]
    [InlineData("LastName", "Last Name is required")]
    [InlineData("Address", "Address is required")]
    [InlineData("City", "City is required")]
    [InlineData("State", "State is required")]
    [InlineData("PostalCode", "Postal Code is required")]
    [InlineData("Country", "Country is required")]
    [InlineData("Phone", "Phone is required")]
    [InlineData("Email", "Email Address is required")]
    public void EachRequiredFieldIsReportedWhenMissing(string property, string message)
    {
        var order = ValidOrder();
        typeof(Order).GetProperty(property)!.SetValue(order, string.Empty);

        var results = Validate(order);

        Assert.Contains(results, r => r.ErrorMessage == message);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("ada@")]
    [InlineData("@example.com")]
    public void AnInvalidEmailIsRefused(string email)
    {
        var order = ValidOrder();
        order.Email = email;

        Assert.Contains(Validate(order), r => r.ErrorMessage == "Email is is not valid.");
    }

    [Fact]
    public void AnEmailLongerThanTheLimitIsRefused()
    {
        var order = ValidOrder();
        order.Email = new string('a', 250) + "@example.com";

        Assert.NotEmpty(Validate(order));
    }

    [Fact]
    public void AnEmailAtTheLimitIsAccepted()
    {
        var order = ValidOrder();
        order.Email = new string('a', 254 - "@example.com".Length) + "@example.com";

        Assert.Empty(Validate(order));
    }

    [Fact]
    public void ATooLongAddressIsRefused()
    {
        var order = ValidOrder();
        order.Address = new string('x', 71);

        Assert.NotEmpty(Validate(order));
    }
}
