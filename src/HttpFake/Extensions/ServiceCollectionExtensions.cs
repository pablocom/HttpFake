using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace HttpFake.Extensions;

/// <summary>
/// Contains extension methods for IServiceCollection to configure HTTP request interception.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures services necessary to intercept and manipulate HttpClient requests using HttpFake.
    /// This includes adding <see cref="ConfiguredResponseInterceptionDelegatingHandler"/> and <see cref="ConfiguredHttpRequestsInterceptor"/> to the service collection.
    /// <see cref="ConfiguredResponseInterceptionDelegatingHandler"/> will intercept each outgoing request, analyze it, and potentially provide a configured response.
    /// <see cref="ConfiguredHttpRequestsInterceptor"/> will be registered as singleton, so in other places configured requests can be added as well.
    /// It's responsible for holding the registered configured responses and matching them against each incoming request.
    /// </summary>
    /// <param name="services">The IServiceCollection to which the HttpFake services will be added.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddHttpClientFactoryInterceptor(this IServiceCollection services)
    {
        services.AddScoped<ConfiguredResponseInterceptionDelegatingHandler>()
            .AddSingleton<ConfiguredHttpRequestsInterceptor>();
        
        services.ConfigureAll<HttpClientFactoryOptions>(options =>
        {
            options.HttpMessageHandlerBuilderActions.Add(httpHandlerBuilder =>
            {
                httpHandlerBuilder.AdditionalHandlers.Add(
                    httpHandlerBuilder.Services.GetRequiredService<ConfiguredResponseInterceptionDelegatingHandler>()
                );
            });
        });
        
        return services;
    }
}
