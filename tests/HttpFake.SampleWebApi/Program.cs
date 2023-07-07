using HttpFake.SampleWebApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("TestingClient", client =>
{
    var realHttpBaseAddress = new Uri("https://pablocompany.com");
    client.BaseAddress = realHttpBaseAddress;
});

var app = builder.Build();

app.MapGet("/configured-request-by-absolute-path", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("TestingClient");
    var response = await httpClient.GetAsync("/absolute/path");
    response.EnsureSuccessStatusCode();
    
    return Results.Ok(await response.Content.ReadFromJsonAsync<DummyObject>());
});

app.MapGet("/record-sent-http-request", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("TestingClient");
    var response = await httpClient.GetAsync("/request/path");
    response.EnsureSuccessStatusCode();
    
    return Results.Ok();
});

app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();