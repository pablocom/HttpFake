using System.Net;
using System.Net.Http.Json;
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
        using var scope = interceptor.InitRequestInterceptionScope();

        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAtLeastOneSpecification(new HttpMethodSpecification(HttpMethod.Get))
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint"));
        var interceptionResult = await interceptor.Intercept(request, scope.Id);

        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeTrue();
        interceptionResult.ConfiguredRequest!.Response.ShouldBe(ConfiguredResponse);
    }

    [Fact]
    public async Task DoesNotInterceptRequestIfMethodIsNotEqual()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var scope = interceptor.InitRequestInterceptionScope();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAtLeastOneSpecification(new HttpMethodSpecification(HttpMethod.Get))
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage(HttpMethod.Patch, new Uri(AnyBaseUri, "/api/endpoint/path?First=firstValue&Second=secondValue"));
        var interceptionResult = await interceptor.Intercept(request, scope.Id);

        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeFalse();
        interceptionResult.ConfiguredRequest.ShouldBeNull();
    }
}


public sealed class WhenInterceptingRequestByJsonContent
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    private sealed record DummyContent(string Property);

    [Fact]
    public async Task MatchesRequestWhenJsonContentMatches()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var scope = interceptor.InitRequestInterceptionScope();
        var expectedContent = new DummyContent("value");
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithJsonContent(expectedContent)
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = AnyBaseUri,
            Content = JsonContent.Create(expectedContent)
        };
        var interceptionResult = await interceptor.Intercept(request, scope.Id);
        
        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeTrue();
        interceptionResult.ConfiguredRequest!.Response.ShouldBe(ConfiguredResponse);
    }

    [Fact]
    public async Task DoesNotInterceptRequestWhenJsonContentDoesNotMatch()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var scope = interceptor.InitRequestInterceptionScope();
        var expectedContent = new DummyContent("value");
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithJsonContent(expectedContent)
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = AnyBaseUri,
            Content = JsonContent.Create(new DummyContent("different"))
        };
        var interceptionResult = await interceptor.Intercept(request, scope.Id);
        
        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeFalse();
        interceptionResult.ConfiguredRequest.ShouldBeNull();
    }
}