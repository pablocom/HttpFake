using System.Net;
using FluentAssertions;
using HttpFake.Specifications;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByHttpMethod
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWithConfiguredResponseOnlyByHttpMethod()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithSpecification(new HttpMethodSpecification(HttpMethod.Get))
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint"));
        var interceptionResult = await interceptor.Intercept(request);

        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ConfiguredResponse);
    }

    [Fact]
    public async Task DoesNotInterceptRequestIfMethodIsNotEqual()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithSpecification(new HttpMethodSpecification(HttpMethod.Get))
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Patch, new Uri(AnyBaseUri, "/api/endpoint/path?First=firstValue&Second=secondValue"));
        var interceptionResult = await interceptor.Intercept(request);

        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}