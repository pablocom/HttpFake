namespace HttpFake;

/// <summary>
/// Specifies the behaviour of the interceptor when an HTTP request does not match any configured responses.
/// </summary>
public enum InterceptionBehaviour
{
    /// <summary>
    /// When set to Strict, an exception is thrown if an HTTP request does not match any configured responses.
    /// </summary>
    Strict = 0, 

    /// <summary>
    /// When set to Lax, the interceptor allows HTTP requests that do not match any configured responses to proceed.
    /// </summary>
    Lax = 1
}
