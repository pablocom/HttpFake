namespace HttpFake.Specifications;

public sealed class HttpMethodSpecification : IHttpRequestSpecification
{
    private readonly HttpMethod _method;

    public HttpMethodSpecification(HttpMethod method)
    {
        _method = method;
    }

    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        if (request?.RequestUri is null)
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(_method == request.Method);
    }
}
