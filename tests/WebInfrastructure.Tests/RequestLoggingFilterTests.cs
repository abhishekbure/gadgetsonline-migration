using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using WebInfrastructure.Infrastructure;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the request log events the design asks for (section 5.2).</summary>
public class RequestLoggingFilterTests
{
    private sealed class ListLogger : ILogger<RequestLoggingFilter>
    {
        public List<string> Messages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }

    private static ActionExecutingContext NewContext(HttpContext http)
    {
        var routeData = new RouteData();
        routeData.Values["controller"] = "Store";
        routeData.Values["action"] = "Browse";
        var actionContext = new ActionContext(http, routeData, new ActionDescriptor());
        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }

    [Fact]
    public async Task ALoggedActionHasAStartAndACompletedEvent()
    {
        var logger = new ListLogger();
        var filter = new RequestLoggingFilter(logger);
        var context = NewContext(new DefaultHttpContext());

        await filter.OnActionExecutionAsync(context, () =>
            Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object())));

        Assert.Equal(2, logger.Messages.Count);
        Assert.Contains("web_infrastructure.request.started Store/Browse", logger.Messages[0]);
        Assert.Contains("web_infrastructure.request.completed Store/Browse status 200", logger.Messages[1]);
    }

    [Fact]
    public async Task AFailingActionHasAFailedEvent()
    {
        var logger = new ListLogger();
        var filter = new RequestLoggingFilter(logger);
        var context = NewContext(new DefaultHttpContext());

        await filter.OnActionExecutionAsync(context, () =>
            Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object())
            {
                Exception = new InvalidOperationException("database unavailable"),
            }));

        Assert.Contains("web_infrastructure.request.failed Store/Browse", logger.Messages[1]);
    }
}
