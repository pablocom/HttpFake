﻿namespace HttpFake.Specifications;

public sealed class AbsolutePathSpecification : IHttpRequestSpecification
{
    private readonly string _absolutePath;

    public AbsolutePathSpecification(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(absolutePath);

        _absolutePath = absolutePath;
    }

    public ValueTask<bool> IsSatisfiedBy(HttpRequestMessage? request)
    {
        if (request?.RequestUri is null)
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(
            request.RequestUri.AbsolutePath.Equals(_absolutePath, StringComparison.InvariantCultureIgnoreCase)
        );
    }
}