using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace HttpFake.SampleWebApi.UnitTests;

[Collection(nameof(SampleWebApplicationTestsCollectionDefinition))]
public sealed class WhenInterceptingHttpRequest
{
    private readonly SampleWebApplicationFactory _webApplicationFactory;

    public WhenInterceptingHttpRequest(SampleWebApplicationFactory webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }
    
    [Fact]
    public async Task AvoidsSendingHttpRequestThroughTheNetworkAndReturnsConfiguredResponse()
    {
        const string endpointPath = "/configured-request-by-absolute-path";
        var configuredResponse = new DummyObject(97, new DateTimeOffset(2023, 1, 23, 1, 2, 3, TimeSpan.Zero), "Text");
        
        var interceptor = _webApplicationFactory.Services.GetRequiredService<ConfiguredHttpRequestsInterceptor>();
        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/absolute/path")
            .RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(configuredResponse) })
            .Build());
        
        using var httpClient = _webApplicationFactory.CreateClient();
        using var response = await httpClient.GetAsync(endpointPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: await response.Content.ReadAsStringAsync());
        var responseContent = await response.Content.ReadFromJsonAsync<DummyObject>();
        responseContent.Should().BeEquivalentTo(configuredResponse);
    }
}