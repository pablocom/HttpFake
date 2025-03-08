namespace HttpFake.SampleWebApi;

public sealed record DummyObject(
    int Number, 
    DateTimeOffset Timestamp, 
    string Text);