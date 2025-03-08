using System.Collections.Concurrent;

namespace HttpFake;

/// <summary>
/// 
/// </summary>
public sealed class RequestInterceptionScope : IDisposable
{
    public Guid Id { get; }
    
    private readonly Action _callOnDispose;

    internal RequestInterceptionScope(Guid id, Action callOnDispose)
    {
        Id = id;
        _callOnDispose = callOnDispose;
    }

    /// <inheritdoc />
    public void Dispose() => _callOnDispose();
}

/// <summary>
/// Provides functionality to intercept and record HTTP requests and responses.
/// This class is thread-safe.
/// </summary>
public sealed class ConfiguredHttpRequestsInterceptor
{
    private readonly Dictionary<Guid, InterceptionBehaviour> _interceptionBehavioursByScope = new();
    private readonly Dictionary<Guid, List<ConfiguredResponse>> _configuredRequestsNew = new();
    private readonly Dictionary<Guid, ConcurrentBag<HttpRequestMessage>> _receivedRequestsNew = new();

    public RequestInterceptionScope InitRequestInterceptionScope(InterceptionBehaviour behaviour = InterceptionBehaviour.Lax)
    {
        var id = Guid.NewGuid();
        
        _interceptionBehavioursByScope.Add(id, behaviour);
        _configuredRequestsNew.Add(id, []);
        _receivedRequestsNew.Add(id, []);

        return new RequestInterceptionScope(id, () =>
        {
            _interceptionBehavioursByScope.Remove(id);
            _configuredRequestsNew.Remove(id);
            _receivedRequestsNew.Remove(id);
        });
    }

    /// <summary>
    /// Registers a <see cref="ConfiguredResponse"/> to this interceptor.
    /// </summary>
    /// <param name="configuredResponse">The configured response.</param>
    /// <param name="interceptionScope">The interception scope.</param>
    /// <returns>An IDisposable that removes the registered configured response when disposed.</returns>
    public void Register(ConfiguredResponse configuredResponse, RequestInterceptionScope interceptionScope)
    {
        if (!_configuredRequestsNew.TryGetValue(interceptionScope.Id, out var configuredResponsesInScope))
            throw new InvalidOperationException("Cannot find the specified scope");
        
        configuredResponsesInScope.Add(configuredResponse);
    }
    
    internal async Task<HttpRequestInterceptionResult> Intercept(HttpRequestMessage request, Guid scopeId)
    {
        if (!_receivedRequestsNew.TryGetValue(scopeId, out var receivedRequestsInScope))
            throw new InvalidOperationException("Cannot find the specified scope");
        
        receivedRequestsInScope.Add(request);
        
        if (!_configuredRequestsNew.TryGetValue(scopeId, out var configuredResponsesInScope))
            throw new InvalidOperationException("Cannot find the specified scope");

        foreach (var configuredRequest in configuredResponsesInScope)
        {
            if (await configuredRequest.IsMatching(request))
                return HttpRequestInterceptionResult.ConfiguredResponseFound(configuredRequest);
        }

        if (_interceptionBehavioursByScope[scopeId] is InterceptionBehaviour.Strict)
            throw new InvalidOperationException($"Request {request.Method.Method} {request.RequestUri} didn't match any configured response");

        return HttpRequestInterceptionResult.NotIntercepted;
    }

    /// <summary>
    /// Asserts that there was a previously intercepted HTTP request matching the provided predicate.
    /// </summary>
    /// <param name="predicate">A function to test each sent HTTP request for a condition.</param>
    /// <param name="interceptionScope">The interception scope.</param>
    public void AssertSentHttpRequestMatching(Func<HttpRequestMessage, bool> predicate, RequestInterceptionScope interceptionScope)
    {
        if (!_receivedRequestsNew.TryGetValue(interceptionScope.Id, out var receivedRequestsInScope))
            throw new InvalidOperationException("Cannot find the specified scope");
        
        ArgumentNullException.ThrowIfNull(predicate);
        
        if (receivedRequestsInScope.Any(predicate))
            return;

        throw new Exception("No received HTTP request matches the provided predicate.");
    }

    /// <summary>
    /// Asserts asynchronously that there was any sent HTTP request matching the provided asynchronous predicate.
    /// </summary>
    /// <param name="predicate">A function to test each sent HTTP request for a condition.</param>
    /// <param name="interceptionScope">The interception scope.</param>
    public async Task AssertSentHttpRequestMatchingAsync(Func<HttpRequestMessage, Task<bool>> predicate, RequestInterceptionScope interceptionScope)
    {
        if (!_receivedRequestsNew.TryGetValue(interceptionScope.Id, out var receivedRequestsInScope))
            throw new InvalidOperationException("Cannot find the specified scope");
        
        ArgumentNullException.ThrowIfNull(predicate);
        
        foreach (var request in receivedRequestsInScope)
        {
            if (await predicate(request))
                return;
        }

        throw new Exception("No received HTTP request matches the provided predicate.");
    }
}