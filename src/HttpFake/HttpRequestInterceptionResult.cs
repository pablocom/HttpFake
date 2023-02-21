using System.Diagnostics.CodeAnalysis;

namespace HttpFake;

internal sealed class HttpRequestInterceptionResult
{
    public static HttpRequestInterceptionResult ConfiguredResponseFound(ConfiguredResponse configuredResponse) => new(true, configuredResponse);
    public static readonly HttpRequestInterceptionResult NotIntercepted = new(false);
    
    [MemberNotNullWhen(returnValue: true, nameof(ConfiguredRequest))]
    public bool HasMatchedWithConfiguredResponse { get; }
    public ConfiguredResponse? ConfiguredRequest { get; }

    private HttpRequestInterceptionResult(bool isMatchingConfiguredRequest, ConfiguredResponse? configuredRequest = null)
    {
        HasMatchedWithConfiguredResponse = isMatchingConfiguredRequest;
        ConfiguredRequest = configuredRequest;
    }
}