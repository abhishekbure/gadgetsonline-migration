using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebInfrastructure.Infrastructure;

/// <summary>Logs the start and the end of every controller action with its duration (design section 5.2).</summary>
public class RequestLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<RequestLoggingFilter> _logger;

    public RequestLoggingFilter(ILogger<RequestLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var action = $"{context.RouteData.Values["controller"]}/{context.RouteData.Values["action"]}";
        _logger.LogInformation("web_infrastructure.request.started {Action}", action);

        var timer = Stopwatch.StartNew();
        var executed = await next();
        timer.Stop();

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            _logger.LogError(executed.Exception, "web_infrastructure.request.failed {Action} after {ElapsedMs} ms", action, timer.ElapsedMilliseconds);
            return;
        }

        _logger.LogInformation(
            "web_infrastructure.request.completed {Action} status {StatusCode} in {ElapsedMs} ms",
            action,
            context.HttpContext.Response.StatusCode,
            timer.ElapsedMilliseconds);
    }
}
