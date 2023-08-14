using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByJsonContent
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    private sealed record DummyContent(string Property);

    [Fact]
    public async Task MatchesRequestIfJsonContentsAreEquivalent()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        var expectedContent = new DummyContent("value");
        using var _ = interceptor.Register(
            new ConfiguredResponseBuilder().WithJsonContent(new DummyContent("NonMatchingValue")).Build(),
            new ConfiguredResponseBuilder()
                .WithJsonContent(expectedContent)
                .RespondWith(ConfiguredResponse)
                .Build(),
            new ConfiguredResponseBuilder().WithJsonContent(new DummyContent("AnotherNonMatchingValue")).Build());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = AnyBaseUri,
            Content = JsonContent.Create(expectedContent)
        };
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ConfiguredResponse);
    }

    [Fact]
    public async Task DoesNotInterceptRequestWhenJsonContentDoesNotMatch()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        var expectedContent = new DummyContent("value");
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithJsonContent(expectedContent)
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = AnyBaseUri,
            Content = JsonContent.Create(new DummyContent("different"))
        };
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}