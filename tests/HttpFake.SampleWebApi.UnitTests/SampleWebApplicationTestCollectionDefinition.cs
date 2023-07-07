namespace HttpFake.SampleWebApi.UnitTests;

[CollectionDefinition(nameof(SampleWebApplicationTestCollectionDefinition))]
public sealed class SampleWebApplicationTestCollectionDefinition : ICollectionFixture<SampleWebApplicationFactory>
{ }
