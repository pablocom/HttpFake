using System.Collections.Concurrent;
using HttpFake.Specifications;

namespace HttpFake;

public sealed class ConfiguredHttpRequestsInterceptor
{
    private readonly ConcurrentBag<HttpRequestMessage> _receivedRequests = new();
    private readonly ConcurrentDictionary<Guid, ConfiguredResponse> _configuredRequests = new();
    private readonly InterceptionBehaviour _interceptionBehaviour;

    public ConfiguredHttpRequestsInterceptor(InterceptionBehaviour interceptionBehaviour = InterceptionBehaviour.Lax)
    {
        _interceptionBehaviour = interceptionBehaviour;
    }

    public IDisposable Register(ConfiguredResponse configuredResponse)
    {
        var id = Guid.NewGuid();
        var added = _configuredRequests.TryAdd(id, configuredResponse);
        if (!added)
            throw new Exception("Could not register configured request");

        return new ConfiguredRequestRegistrationRemover(() => _configuredRequests.Remove(id, out var _));
    }

    internal async Task<HttpRequestInterceptionResult> Intercept(HttpRequestMessage request)
    {
        _receivedRequests.Add(request);

        foreach (var configuredRequest in _configuredRequests.Values)
        {
            if (await configuredRequest.IsMatching(request))
                return HttpRequestInterceptionResult.ConfiguredResponseFound(configuredRequest);
        }

        if (_interceptionBehaviour is InterceptionBehaviour.Strict)
            throw new InvalidOperationException($"Request {request.Method.Method} {request.RequestUri?.ToString()} didn't matched any configured response");

        return HttpRequestInterceptionResult.NotIntercepted;
    }

    public async Task AssertReceivedHttpRequestMatching(IEnumerable<IHttpRequestSpecification> specifications)
    {
        if (specifications == null || !specifications.Any())
        {
            throw new ArgumentException("You must provide at least one specification.", nameof(specifications));
        }

        foreach (var request in _receivedRequests)
        {
            if (await RequestMatchesAllSpecifications(request, specifications))
            {
                return;
            }
        }

        throw new InvalidOperationException("No received HTTP request matches all the provided specifications.");
    }

    private static async ValueTask<bool> RequestMatchesAllSpecifications(HttpRequestMessage request, 
        IEnumerable<IHttpRequestSpecification> specifications)
    {
        foreach (var specification in specifications)
        {
            if (!await specification.IsSatisfiedBy(request))
            {
                return false;
            }
        }

        return true;
    }

}
