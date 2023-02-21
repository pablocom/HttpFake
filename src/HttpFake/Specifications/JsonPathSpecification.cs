namespace HttpFake.Specifications;

public sealed class JsonPathSpecification : IHttpRequestSpecification
{
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        throw new NotImplementedException();
    }
}
