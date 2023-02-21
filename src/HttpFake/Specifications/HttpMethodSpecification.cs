namespace HttpFake.Specifications;

public sealed class HttpMethodSpecification : IHttpRequestSpecification
{
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        throw new NotImplementedException();
    }
}
