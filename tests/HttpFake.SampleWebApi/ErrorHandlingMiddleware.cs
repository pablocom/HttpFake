namespace HttpFake.SampleWebApi;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await Results.Problem(
                    detail: exception.StackTrace, 
                    title: exception.Message,
                    statusCode: StatusCodes.Status500InternalServerError)
                .ExecuteAsync(context);
        }
    }
}

public sealed record DummyObject(int Number, DateTimeOffset Timestamp, string Text);