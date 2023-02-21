namespace HttpFake.Specifications;

public sealed class CookiesSpecification : IHttpRequestSpecification
{
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        throw new NotImplementedException();
    }
}
