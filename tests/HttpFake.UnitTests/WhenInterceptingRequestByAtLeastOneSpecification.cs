using System.Net;
using FluentAssertions;
using HttpFake.Specifications;

namespace HttpFake.UnitTests;

public sealed class WhenInterceptingRequestByAtLeastOneSpecification
{
    private static readonly Uri AnyBaseUri = new("https://pablocompany.com");
    private static readonly HttpResponseMessage ConfiguredResponse = new(HttpStatusCode.Found);

    [Fact]
    public async Task MatchesRequestWhenAtLeastOneSpecificationIsSatisfied()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAtLeastOneSpecification(
                new QueryParamsSpecification(new Dictionary<string, string> { { "First", "firstValue" } }), 
                new AbsolutePathSpecification("/path"))
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/path?First=unmatched"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeTrue();
        interceptionResult.ConfiguredRequest!.Response.Should().Be(ConfiguredResponse);
    }

    [Fact]
    public async Task DoesNotInterceptRequestWhenNoneOfTheSpecificationsAreSatisfied()
    {
        var interceptor = new ConfiguredHttpRequestsInterceptor();
        interceptor.Register(new ConfiguredResponseBuilder()
            .WithAtLeastOneSpecification(
                new QueryParamsSpecification(new Dictionary<string, string> { { "First", "firstValue" } }), 
                new AbsolutePathSpecification("/path"))
            .RespondWith(ConfiguredResponse)
            .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(AnyBaseUri, "/unmatched?First=unmatched"));
        var interceptionResult = await interceptor.Intercept(request);
        
        interceptionResult.HasMatchedWithConfiguredResponse.Should().BeFalse();
        interceptionResult.ConfiguredRequest.Should().BeNull();
    }
}