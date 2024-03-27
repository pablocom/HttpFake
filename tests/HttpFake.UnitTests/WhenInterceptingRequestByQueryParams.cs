using System.Net;
using FluentAssertions;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByQueryParams
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWithConfiguredResponseOnlyByQueryParams()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithQueryParams(new Dictionary<string, string>
            {
                { "First", "firstValue" },
                { "Second", "secondValue" }
            })
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path?First=firstValue&Second=secondValue"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ConfiguredResponse);
    }
    
    [Fact]
    public async Task DoesNotInterceptRequestIfQueryParamsAreNotPresentInUri()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithQueryParams(new Dictionary<string, string> 
            { 
                { "First", "firstValue" },
                { "Second", "secondValue" }
            })
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/path?First=firstValue"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }

    [Fact]
    public async Task DoesNotInterceptRequestIfAnyQueryParamValueIsNotEqual()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithQueryParams(new Dictionary<string, string>
            {
                { "First", "firstValue" },
                { "Second", "secondValue" }
            })
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/path?First=firstValue&Second=different"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}