using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace HttpFake.SampleWebApi.UnitTests;

[Collection(nameof(SampleWebApplicationTestCollectionDefinition))]
public sealed class WhenInterceptingHttpRequest
{
    private readonly SampleWebApplicationFactory _webApplicationFactory;
    private readonly ConfiguredHttpRequestsInterceptor _interceptor;

    public WhenInterceptingHttpRequest(SampleWebApplicationFactory webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
        _interceptor = webApplicationFactory.Services.GetRequiredService<ConfiguredHttpRequestsInterceptor>();
    }
    
    [Fact]
    public async Task AvoidsSendingHttpRequestThroughTheNetworkAndReturnsConfiguredResponse()
    {
        const string endpointPath = "/configured-request-by-absolute-path";
        var configuredResponse = new DummyObject(97, new DateTimeOffset(2023, 1, 23, 1, 2, 3, TimeSpan.Zero), "Text");
        
        using var interceptionScope = _interceptor.InitRequestInterceptionScope();
        
        _interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/absolute/path")
            .RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(configuredResponse) })
            .Build(), interceptionScope);
        
        using var httpClient = _webApplicationFactory.CreateClient(interceptionScope);
        using var response = await httpClient.GetAsync(endpointPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, customMessage: await response.Content.ReadAsStringAsync());
        var responseContent = await response.Content.ReadFromJsonAsync<DummyObject>();
        responseContent.ShouldBeEquivalentTo(configuredResponse);
    }

    [Fact]
    public async Task RecordsSentHttpRequests()
    {
        const string configuredRequestAbsolutePath = "/request/path";
        
        using var interceptionScope = _interceptor.InitRequestInterceptionScope();

        _interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath(configuredRequestAbsolutePath)
            .Build(), interceptionScope);
        
        using var httpClient = _webApplicationFactory.CreateClient(interceptionScope);
        using var response = await httpClient.GetAsync("/record-sent-http-request");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, customMessage: await response.Content.ReadAsStringAsync());
        _interceptor.AssertSentHttpRequestMatching(request =>
        {
            request.RequestUri!.AbsolutePath.ShouldBe(configuredRequestAbsolutePath);
            return true;
        }, interceptionScope);
    }
}

public static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateClient<TEntryPoint>(this WebApplicationFactory<TEntryPoint> factory, RequestInterceptionScope scope) 
        where TEntryPoint : class
    {
        return CreateClient(factory, new(), scope);
    }

    public static HttpClient CreateClient<TEntryPoint>(this WebApplicationFactory<TEntryPoint> factory, 
        WebApplicationFactoryClientOptions options, RequestInterceptionScope scope) 
        where TEntryPoint : class
    {
        var httpClient = factory.CreateClient(options);
        httpClient.DefaultRequestHeaders.Add(InterceptionRequestHeaders.InterceptionScope, scope.Id.ToString());
        return httpClient;
    }
}