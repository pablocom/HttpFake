namespace HttpFake.Specifications;

public sealed class OrSpecification : IHttpRequestSpecification
{
    private readonly IHttpRequestSpecification _firstSpecification;
    private readonly IHttpRequestSpecification _secondSpecification;

    public OrSpecification(IHttpRequestSpecification firstSpecification, IHttpRequestSpecification secondSpecification)
    {
        _firstSpecification = firstSpecification;
        _secondSpecification = secondSpecification;
    }

    public async ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        return await _firstSpecification.IsSatisfiedBy(request) 
               || await _secondSpecification.IsSatisfiedBy(request);
    }
}