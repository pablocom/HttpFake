using FluentAssertions;

namespace HttpFake.UnitTests;

public sealed class WhenBuildingConfiguredRequests
{
    [Fact]
    public void ThrowsExceptionIfNoSpecificationsAreConfigured()
    {
        var action = () => new ConfiguredResponseBuilder().Build();
        
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No request specifications configured. Cannot created a configured HTTP response without any specification");
    }
}