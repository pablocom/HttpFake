namespace HttpFake.Specifications;

public interface IHttpRequestSpecification
{
    ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request);
}
