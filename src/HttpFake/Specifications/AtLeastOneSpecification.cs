namespace HttpFake.Specifications;

public sealed class AtLeastOneSpecification : IHttpRequestSpecification
{
    private readonly IEnumerable<IHttpRequestSpecification> _specifications;

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
