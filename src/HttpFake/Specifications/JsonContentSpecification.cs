namespace HttpFake.Specifications;

public sealed class JsonContentSpecification : IHttpRequestSpecification 
{
    private readonly string _content;

    public JsonContentSpecification(string content)
    {
        _content = content;
    }

    public async ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        if (request?.Content is null)
            return false;

        var requestContent = await request.Content.ReadAsStringAsync();
        return _content.Equals(requestContent);
    }
}
