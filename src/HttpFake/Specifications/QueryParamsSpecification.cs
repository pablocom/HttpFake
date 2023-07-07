using System.Web;

namespace HttpFake.Specifications;

public sealed class QueryParamsSpecification : IHttpRequestSpecification
{
    private readonly IReadOnlyDictionary<string, string> _queryParams;

    public QueryParamsSpecification(IReadOnlyDictionary<string, string> queryParams)
    {
        ArgumentNullException.ThrowIfNull(queryParams);

        if (queryParams.Count == 0)
            throw new ArgumentException("Query parameters cannot be null or empty");

        _queryParams = queryParams;
    }

    /// <inheritdoc />
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request, CancellationToken cancellationToken = default)
    {
        if (request?.RequestUri is null)
            return ValueTask.FromResult(false);

        var actualQueryParams = HttpUtility.ParseQueryString(request.RequestUri.Query);

        foreach (var expectedQueryParam in _queryParams)
        {
            var actualValue = actualQueryParams.Get(expectedQueryParam.Key);

            var queryParamIsNotFoundInUri = actualValue == null;
            if (queryParamIsNotFoundInUri || actualValue != expectedQueryParam.Value)
                return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(true);
    }
}