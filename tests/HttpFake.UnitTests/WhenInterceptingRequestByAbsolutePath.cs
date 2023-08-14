using System.Net;
using FluentAssertions;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByAbsolutePath
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ExpectedResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWithConfiguredResponseOnlyByAbsolutePath()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var _1 = interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/non/matching/endpoint/path")
            .Build());
        using var _2 = interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ExpectedResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path?Something=more"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ExpectedResponse);
    }
    
    [Fact]
    public async Task DoesNotInterceptRequestIfAnySpecificationIsNotSatisfied()
    {
        var requestsInterceptor = new ConfiguredHttpRequestsInterceptor();
        using var _ = requestsInterceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ExpectedResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path/different"));
        var interceptionResult = await requestsInterceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}
