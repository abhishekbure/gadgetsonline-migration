namespace WebInfrastructure.Services;

/// <summary>
/// Keeps the cart identifier in the user's session: a signed-in user's name when there is one,
/// otherwise a new random identifier that stays the same for the rest of the session.
/// </summary>
public class SessionCartProvider : ICartIdProvider
{
    public const string CartSessionKey = "CartId";

    public string GetCartId(HttpContext context)
    {
        var existing = context.Session.GetString(CartSessionKey);
        if (existing is not null)
        {
            return existing;
        }

        var userName = context.User.Identity?.Name;
        var cartId = !string.IsNullOrWhiteSpace(userName) ? userName : Guid.NewGuid().ToString();
        context.Session.SetString(CartSessionKey, cartId);
        return cartId;
    }
}
