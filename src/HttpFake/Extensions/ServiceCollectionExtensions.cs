using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace HttpFake.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpRequestsInterceptor(this IServiceCollection services)
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