namespace WebInfrastructure.Services;

/// <summary>Finds the identifier of the shopping cart that belongs to the current request's session.</summary>
public interface ICartIdProvider
{
    string GetCartId(HttpContext context);
}
