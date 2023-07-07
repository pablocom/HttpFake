# HttpFake

HttpFake is a .NET library designed to intercept and manipulate `HttpClient` requests, making it easier to test and debug applications that heavily rely on HTTP communication. It can be seamlessly integrated with `Microsoft.AspNetCore.Mvc.Testing` to make it a powerful tool for integration testing in ASP.NET Core applications, and helps a lot in mocking and stubbing HTTP requests.

## Getting Started

To use HttpFake, first install it via NuGet:

```shell
Install-Package HttpFake
```

Then, register the HttpFake services in your `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClientFactoryInterceptor();
}
```

## Basic Usage

### Configuring Responses

You can create pre-configured HTTP responses using the `ConfiguredResponseBuilder`. Here's an example:

```csharp
var content = new
{
    Message = "Hello, World!"
};

var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json")
};

var builder = new ConfiguredResponseBuilder();
builder.WithJsonContent(content).RespondWith(httpResponseMessage);

var configuredResponse = builder.Build();

var interceptor = new ConfiguredHttpRequestsInterceptor();
interceptor.Register(configuredResponse);
```

In this example, any HTTP request that contains a JSON body matching `content` will receive a 200 OK response with the same `content`.

### Intercepting Requests

The `ConfiguredHttpRequestsInterceptor` holds the registered configured responses and matches them against each incoming request:

```csharp
var interceptor = new ConfiguredHttpRequestsInterceptor();

// Register a configured response...
var configuredResponse = ...
interceptor.Register(configuredResponse);

// Intercept a request...
var httpRequestMessage = ...
var result = await interceptor.Intercept(httpRequestMessage);

if (result.HasMatchedWithConfiguredResponse)
{
    // The request matches the configured response...
}
```

### Asserting Sent Requests

You can assert whether any sent HTTP request matches a particular condition using the `AssertSentHttpRequestMatching` method:

```csharp
var interceptor = new ConfiguredHttpRequestsInterceptor();

// Assert that at least one request was sent to a particular URL...
interceptor.AssertSentHttpRequestMatching(request => request.RequestUri == "https://example.com/api");
```

## Using with Microsoft.AspNetCore.Mvc.Testing

You can integrate HttpFake with `Microsoft.AspNetCore.Mvc.Testing` to test your ASP.NET Core applications without needing to send real HTTP requests. This is great for unit tests or integration tests where you want to isolate certain parts of your application.

Here is an example:

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

In this example, the HTTP GET request to `"/configured-request-by-absolute-path"` is intercepted and a pre-configured response is returned.

## Contributing

We welcome contributions! Please submit a pull request with any enhancements, bug fixes, or other contributions.

## License

HttpFake is licensed under the MIT License.
