using HttpFake.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace HttpFake.SampleWebApi.UnitTests;

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