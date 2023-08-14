using System.Collections.Concurrent;

namespace HttpFake;

/// <summary>
/// Provides functionality to intercept and record HTTP requests and responses.
/// This class is thread-safe.
/// </summary>
public sealed class ConfiguredHttpRequestsInterceptor
{
    private readonly ConcurrentBag<HttpRequestMessage> _receivedRequests = new();
    private readonly ConcurrentDictionary<Guid, ConfiguredResponse> _configuredRequests = new();
    private readonly InterceptionBehaviour _interceptionBehaviour;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredHttpRequestsInterceptor"/> class.
    /// </summary>
    /// <param name="interceptionBehaviour">The interception behaviour of this instance. See <see cref="InterceptionBehaviour"/></param>
    public ConfiguredHttpRequestsInterceptor(InterceptionBehaviour interceptionBehaviour = InterceptionBehaviour.Lax)
    {
        _interceptionBehaviour = interceptionBehaviour;
    }

    /// <summary>
    /// Registers a <see cref="ConfiguredResponse"/> to this interceptor.
    /// </summary>
    /// <param name="configuredResponse">The configured response.</param>
    /// <returns>An IDisposable that removes the registered configured response when disposed.</returns>
    public IDisposable Register(ConfiguredResponse configuredResponse)
    {
        var id = Guid.NewGuid();
        var added = _configuredRequests.TryAdd(id, configuredResponse);
        if (!added)
            throw new Exception("Could not register configured request");

        return new ConfiguredRequestRegistrationRemover(() => _configuredRequests.Remove(id, out _));
    }

    /// <summary>
    /// Registers multiple <see cref="ConfiguredResponse"/>s to this interceptor.
    /// </summary>
    /// <param name="configuredResponses">The configured responses.</param>
    /// <returns>An IDisposable that removes the registered configured responses when disposed.</returns>
    public IDisposable Register(params ConfiguredResponse[] configuredResponses)
    {
        var configuredResponseIds = new List<Guid>(configuredResponses.Length);
        
        foreach (var response in configuredResponses)
        {
            var id = Guid.NewGuid();
            var added = _configuredRequests.TryAdd(id, response);
            if (!added)
                throw new Exception("Could not register configured request");
            
            configuredResponseIds.Add(id);
        }
        
        return new ConfiguredRequestRegistrationRemover(() =>
        {
            foreach (var id in configuredResponseIds)
            {
                _configuredRequests.Remove(id, out _);
            }
        });
    }
    
    /// <summary>
    /// Intercepts an HTTP request and finds a matching configured response.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>A <see cref="HttpRequestInterceptionResult"/> that contains information about the interception.</returns>
    internal async Task<HttpRequestInterceptionResult> Intercept(HttpRequestMessage request)
    {
        _receivedRequests.Add(request);

        foreach (var configuredRequest in _configuredRequests.Values)
        {
            if (await configuredRequest.IsMatching(request))
                return HttpRequestInterceptionResult.ConfiguredResponseFound(configuredRequest);
        }

        if (_interceptionBehaviour is InterceptionBehaviour.Strict)
            throw new InvalidOperationException($"Request {request.Method.Method} {request.RequestUri} didn't match any configured response");

        return HttpRequestInterceptionResult.NotIntercepted;
    }

    /// <summary>
    /// Asserts that there was a previously intercepted HTTP request matching the provided predicate.
    /// </summary>
    /// <param name="predicate">A function to test each sent HTTP request for a condition.</param>
    public void AssertSentHttpRequestMatching(Func<HttpRequestMessage, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (_receivedRequests.Any(predicate))
            return;

        throw new Exception("No received HTTP request matches the provided predicate.");
    }

    /// <summary>
    /// Asserts asynchronously that there was a sent HTTP request matching the provided asynchronous predicate.
    /// </summary>
    /// <param name="predicate">A function to test each sent HTTP request for a condition.</param>
    public async Task AssertSentHttpRequestMatchingAsync(Func<HttpRequestMessage, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (var request in _receivedRequests)
        {
            if (await predicate(request))
                return;
        }
        throw new Exception("No received HTTP request matches the provided predicate.");
    }

    /// <summary>
    /// Asserts asynchronously that there was a sent HTTP request matching the provided asynchronous predicate.
    /// </summary>
    public void Clear()
    {
        _receivedRequests.Clear();
        _configuredRequests.Clear();
    }
}