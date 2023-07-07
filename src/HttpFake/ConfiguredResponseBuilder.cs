using System.Net;
using System.Text.Json;
using HttpFake.Specifications;

namespace HttpFake;

public sealed class ConfiguredResponseBuilder
{
    private readonly ICollection<IHttpRequestSpecification> _specifications = new List<IHttpRequestSpecification>();
    private HttpResponseMessage _response = new(HttpStatusCode.OK);

    public ConfiguredResponse Build()
    {
        if (!_specifications.Any())
            throw new InvalidOperationException("No request specifications configured. Cannot created a configured HTTP response without any specification");
        
        return new ConfiguredResponse(_specifications.ToArray(), _response);
    }

    public ConfiguredResponseBuilder WithAbsolutePath(string absolutePath)
    {
        _specifications.Add(new AbsolutePathSpecification(absolutePath));
        return this;
    }

    public ConfiguredResponseBuilder WithJsonContent<TValue>(TValue content)
    {
        _specifications.Add(new JsonContentSpecification(JsonSerializer.Serialize(content)));  
        return this;
    }
    
    public ConfiguredResponseBuilder WithAtLeastOneSpecification(params IHttpRequestSpecification[] specifications)
    {
        ArgumentNullException.ThrowIfNull(specifications);
        if (specifications.Length == 0)
            throw new ArgumentException("Invalid empty specifications array for AtLeastOne constraint", nameof(specifications));

        _specifications.Add(new AtLeastOneSpecification(specifications));
        return this;
    }
    
    public ConfiguredResponseBuilder RespondWith(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        
        _response = response;
        return this;
    }

    public ConfiguredResponseBuilder WithQueryParams(IReadOnlyDictionary<string, string> dictionary)
    {
        _specifications.Add(new QueryParamsSpecification(dictionary));
        return this;
    }
}
