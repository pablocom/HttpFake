namespace HttpFake.Specifications;

/// <summary>
/// Represents a specification that an <see cref="HttpRequestMessage"/> must satisfy at least one condition in a list of specifications.
/// </summary>

public sealed class AtLeastOneSpecification : IHttpRequestSpecification
{
    private readonly IEnumerable<IHttpRequestSpecification> _specifications;

    /// <summary>
    /// Initializes a new instance of <see cref="AtLeastOneSpecification"/>
    /// </summary>
    /// <param name="specifications">The list of specifications to be satisfied.</param>
    public AtLeastOneSpecification(IEnumerable<IHttpRequestSpecification> specifications)
    {
        _specifications = specifications;
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request, CancellationToken cancellationToken = default)
    {
        foreach (var specification in _specifications)
        {
            if (await specification.IsSatisfiedBy(request, cancellationToken))
                return true;
        }

        return false;
    }
}
