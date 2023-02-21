using System.Net;
using FluentAssertions;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByAbsolutePath
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWithConfiguredResponseOnlyByAbsolutePath()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path?Something=more"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ConfiguredResponse);
    }
    
    [Fact]
    public async Task DoesNotInterceptRequestIfAnySpecificationIsNotSatisfied()
    {
        var requestsInterceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = requestsInterceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path/different"));
        var interceptionResult = await requestsInterceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}
