using HttpFake.Specifications;

namespace HttpFake;

/// <summary>
/// Represents a configured HTTP response with associated request specifications.
/// </summary>
public sealed class ConfiguredResponse
{
    /// <summary>
    /// Gets the configured HTTP response message.
    /// </summary>
    public HttpResponseMessage Response { get; }

    private readonly IHttpRequestSpecification[] _specifications;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfiguredResponse"/> class.
    /// </summary>
    /// <param name="specifications">The request specifications associated with this response.</param>
    /// <param name="response">The configured HTTP response message.</param>
    public ConfiguredResponse(IHttpRequestSpecification[] specifications, HttpResponseMessage response)
    {
        Response = response;
        _specifications = specifications;
    }

    /// <summary>
    /// Determines whether the provided HTTP request matches the specifications of this instance.
    /// </summary>
    /// <param name="request">The HTTP request to match against the specifications.</param>
    /// <returns>True if the provided HTTP request matches all specifications, false otherwise.</returns>
    internal async Task<bool> IsMatching(HttpRequestMessage request)
    {
        for (var i = 0; i < _specifications.Length; i++)
        {
            if (!await _specifications[i].IsSatisfiedBy(request))
            {
                break;
            }

            var isLastSpecification = i == _specifications.Length - 1;
            if (isLastSpecification)
            {
                return true;
            }
        }

        return false;
    }
}