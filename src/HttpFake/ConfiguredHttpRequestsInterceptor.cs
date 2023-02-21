using System.Collections.Concurrent;

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
}

internal sealed class ConfiguredRequestRegistrationRemover : IDisposable
{
    private readonly Action _callOnDispose;

    internal ConfiguredRequestRegistrationRemover(Action toCall) => _callOnDispose = toCall;

    public void Dispose() => _callOnDispose();
}
