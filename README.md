# HttpFake

HttpFake is a .NET library that helps you to intercept requests and configure responses for HTTP requests, providing a powerful tool for integration testing in ASP.NET Core applications. This library is especially useful when used in conjunction with `Microsoft.AspNetCore.Mvc.Testing`, as it allows you to test your application's HTTP communication without sending real network requests. 
Basically it adds a delegating handler to every HttpClient created with the HttpClientFactory that will perform the interception

## Getting Started

Firstly, install the HttpFake library via NuGet:

```shell
Install-Package HttpFake
```

## Usage with Microsoft.AspNetCore.Mvc.Testing

Next, you need to register HttpFake services in your test startup configuration:

```csharp
public sealed class SampleWebApplicationFactory : WebApplicationFactory<IAssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClientFactoryInterceptor();
        });
    }
}
```

You can use HttpFake with `Microsoft.AspNetCore.Mvc.Testing` to avoid sending actual HTTP requests during testing. This allows you to isolate your tests, providing consistent and controlled responses to HTTP requests.

Below is an example of how to set up and use HttpFake within a test:

```csharp
[Collection(nameof(SampleWebApplicationTestCollectionDefinition))]
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
        // Arrange
        const string endpointPath = "/configured-request-by-absolute-path";
        var configuredResponse = new DummyObject(97, new DateTimeOffset(2023, 1, 23, 1, 2, 3, TimeSpan.Zero), "Text");
        var interceptor = _webApplicationFactory.Services.GetRequiredService<ConfiguredHttpRequestsInterceptor>();

        using var _ = interceptor.Register(new ConfiguredResponseBuilder()
            .WithAbsolutePath("/absolute/path")
            .RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(configuredResponse) })
            .Build());
        
        // Act
        using var httpClient = _webApplicationFactory.CreateClient();
        using var response = await httpClient.GetAsync(endpointPath);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: await response.Content.ReadAsStringAsync());
        var responseContent = await response.Content.ReadFromJsonAsync<DummyObject>();
        responseContent.Should().BeEquivalentTo(configuredResponse);
    }
}
```

In this example, the HTTP GET request to `"/configured-request-by-absolute-path"` will create an HTTP client with the `IHttpClientFactory` and send another request to `{AnyHostUrl}/absolute/path` that will be intercepted and a pre-configured response will be returned.


## Contributing

HttpFake is an open-source project and we welcome contributions! Feel free to submit a pull request with enhancements, bug fixes, or other improvements.

## License

HttpFake is licensed under the MIT License. For more information, please refer to the LICENSE file in the repository.
