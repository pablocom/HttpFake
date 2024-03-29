﻿namespace HttpFake.Specifications;

public sealed class AbsolutePathSpecification : IHttpRequestSpecification
{
    private readonly string _absolutePath;

    public AbsolutePathSpecification(string absolutePath)
    {
        _absolutePath = absolutePath ?? throw new ArgumentNullException(nameof(absolutePath));
    }

    /// <inheritdoc />
    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request, CancellationToken cancellationToken = default)
    {
        if (request?.RequestUri is null)
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(
            request.RequestUri.AbsolutePath.Equals(_absolutePath, StringComparison.InvariantCultureIgnoreCase)
        );
    }
}