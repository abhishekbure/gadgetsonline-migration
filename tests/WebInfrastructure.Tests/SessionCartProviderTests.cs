using System.Security.Claims;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of how the cart identifier is kept in the session.</summary>
public class SessionCartProviderTests
{
    [Fact]
    public void ASessionKeepsTheSameCartIdentifier()
    {
        var provider = new SessionCartProvider();
        var context = TestHttp.NewContext();

        var first = provider.GetCartId(context);
        var second = provider.GetCartId(context);

        Assert.Equal(first, second);
        Assert.True(Guid.TryParse(first, out _));
    }

    [Fact]
    public void DifferentSessionsGetDifferentCartIdentifiers()
    {
        var provider = new SessionCartProvider();

        var one = provider.GetCartId(TestHttp.NewContext());
        var two = provider.GetCartId(TestHttp.NewContext());

        Assert.NotEqual(one, two);
    }

    [Fact]
    public void ASignedInUsersNameIsTheirCartIdentifier()
    {
        var provider = new SessionCartProvider();
        var context = TestHttp.NewContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "ada") }, "test"));

        Assert.Equal("ada", provider.GetCartId(context));
    }
}
