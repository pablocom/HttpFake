using Microsoft.AspNetCore.Http;

namespace HttpFake;

/// <summary>
/// Delegating handler that intercepts HTTP requests and replaces them with configured responses, if any are matching.
/// </summary>
public sealed class ConfiguredResponseInterceptionDelegatingHandler : DelegatingHandler
{
    private readonly ConfiguredHttpRequestsInterceptor _requestsInterceptor;
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredResponseInterceptionDelegatingHandler"/> class with the provided interceptor.
    /// </summary>
    /// <param name="requestsInterceptor">The interceptor used to match and replace HTTP requests.</param>
    public ConfiguredResponseInterceptionDelegatingHandler(ConfiguredHttpRequestsInterceptor requestsInterceptor, IHttpContextAccessor contextAccessor)
    {
        _requestsInterceptor = requestsInterceptor;
        _contextAccessor = contextAccessor;
    }
        
    /// <summary>
    /// Intercepts HTTP requests and replaces them with configured responses if is matching any <see cref="ConfiguredResponse"/>,
    /// if not it sends the HTTP request to the next handler.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!_contextAccessor.HttpContext!.Request.Headers.TryGetValue(InterceptionRequestHeaders.InterceptionScope, out var scopeId))
            throw new InvalidOperationException();
        
        if (!Guid.TryParse(scopeId, out var scopeIdAsGuid))
            throw new InvalidOperationException();

        var result = await _requestsInterceptor.Intercept(request, scopeIdAsGuid);
        if (result.HasMatchedWithConfiguredResponse)
        {
            return result.ConfiguredRequest.Response;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}