namespace HttpFake;

internal sealed class ConfiguredRequestRegistrationRemover : IDisposable
{
    private readonly Action _callOnDispose;

    internal ConfiguredRequestRegistrationRemover(Action toCall) => _callOnDispose = toCall;

    public void Dispose() => _callOnDispose();
}
