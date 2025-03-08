using HttpFake.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace HttpFake.SampleWebApi.UnitTests;

public sealed class SampleWebApplicationFactory : WebApplicationFactory<IAssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClientFactoryInterceptor();
            services.AddHttpContextAccessor();
        });
    }
}