namespace HttpFake.Specifications;

/// <summary>
/// Represents a specification that determines whether an HTTP request message satisfies certain criteria.
/// </summary>
public interface IHttpRequestSpecification
{
    /// <summary>
    /// Determines whether the specified HTTP request message satisfies the criteria defined by this specification.
    /// </summary>
    /// <param name="request">The HTTP request message to examine.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that completes with <c>true</c> if the specified request satisfies this specification; otherwise, <c>false</c>.</returns>
    ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request, CancellationToken cancellationToken = default);
}