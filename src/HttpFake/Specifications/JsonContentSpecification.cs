using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;

namespace HttpFake.Specifications;

/// <summary>
/// Provides a specification for JSON content in an HTTP request.
/// </summary>
public sealed class JsonContentSpecification : IHttpRequestSpecification 
{
    private readonly Dictionary<string, JsonElement> _content;
    private readonly bool _caseSensitive;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonContentSpecification"/> class with the specified content.
    /// </summary>
    /// <param name="content">The JSON content that should match the HTTP request content.</param>
    /// <param name="caseSensitive">Indicates whether the JSON property name comparison should be case-sensitive.</param>
    /// <exception cref="SerializationException">Thrown if there is an error deserializing the content.</exception>
    public JsonContentSpecification(string content, bool caseSensitive = false)
    {
        _content = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content)
                   ?? throw new SerializationException("Error serializing content");
        _caseSensitive = caseSensitive;
            
        if (!_caseSensitive) 
            _content = new Dictionary<string, JsonElement>(_content, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request, CancellationToken cancellationToken = default)
    {
        if (request?.Content is null)
            return false;

        var requestContentObject = await request.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>(
            cancellationToken: cancellationToken);
        if (requestContentObject == null || _content.Count != requestContentObject.Count) 
            return false;

        if (!_caseSensitive)
        {
            requestContentObject = new Dictionary<string, JsonElement>(requestContentObject, StringComparer.OrdinalIgnoreCase);
        }

        foreach (var keyValuePair in _content)
        {
            if (!requestContentObject.TryGetValue(keyValuePair.Key, out var value) || 
                keyValuePair.Value.ToString() != value.ToString()) 
                return false;
        }

        return true;
    }
}
