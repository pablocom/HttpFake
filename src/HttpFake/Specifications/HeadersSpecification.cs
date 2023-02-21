namespace HttpFake.Specifications;

public sealed class HeadersSpecification : IHttpRequestSpecification
{
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        throw new NotImplementedException();
    }
}