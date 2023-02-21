namespace HttpFake;

public sealed class ConfiguredResponseInterceptionDelegatingHandler : DelegatingHandler
{
    private readonly ConfiguredHttpRequestsInterceptor _requestsInterceptor;

    public ConfiguredResponseInterceptionDelegatingHandler(ConfiguredHttpRequestsInterceptor requestsInterceptor)
    {
        _requestsInterceptor = requestsInterceptor;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var result = await _requestsInterceptor.Intercept(request);
        if (result.HasMatchedWithConfiguredResponse)
        {
            return result.ConfiguredRequest.Response;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}