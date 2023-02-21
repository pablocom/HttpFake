using HttpFake.Specifications;

namespace HttpFake;

public sealed class ConfiguredResponse
{
    public HttpResponseMessage Response { get; }

    private readonly IHttpRequestSpecification[] _specifications;

    internal ConfiguredResponse(IHttpRequestSpecification[] specifications, HttpResponseMessage response)
    {
        Response = response;
        _specifications = specifications;
    }

    internal async Task<bool> IsMatching(HttpRequestMessage request)
    {
        for (var i = 0; i < _specifications.Length; i++)
        {
            if (!await _specifications[i].IsSatisfiedBy(request))
            {
                break;
            }

            var isLastSpecification = i == _specifications.Length - 1;
            if (isLastSpecification)
            {
                return true;
            }
        }

        return false;
    }
}
