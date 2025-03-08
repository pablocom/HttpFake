using System.Net;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByAbsolutePath
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWithConfiguredResponseOnlyByAbsolutePath()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var scope = interceptor.InitRequestInterceptionScope();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path?Something=more"));
        var interceptionResult = await interceptor.Intercept(request, scope.Id);
        
        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeTrue();
        interceptionResult.ConfiguredRequest!.Response.ShouldBe(ConfiguredResponse);
    }
    
    [Fact]
    public async Task DoesNotInterceptRequestIfAnySpecificationIsNotSatisfied()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        using var scope = interceptor.InitRequestInterceptionScope();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/api/endpoint/path")
            .RespondWith(ConfiguredResponse)
            .Build(), scope);

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/api/endpoint/path/different"));
        var interceptionResult = await interceptor.Intercept(request, scope.Id);
        
        interceptionResult.HasMatchedWithConfiguredResponse.ShouldBeFalse();
        interceptionResult.ConfiguredRequest.ShouldBeNull();
    }
}
