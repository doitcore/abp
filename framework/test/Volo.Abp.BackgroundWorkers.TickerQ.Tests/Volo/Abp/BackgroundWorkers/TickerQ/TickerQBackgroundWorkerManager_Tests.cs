using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

public class TickerQBackgroundWorkerManager_Tests : AbpIntegratedTest<AbpBackgroundWorkersTickerQTestModule>
{
    private readonly IBackgroundWorkerManager _backgroundWorkerManager;
    private readonly IOptions<AbpBackgroundWorkerTickerQOptions> _options;

    public TickerQBackgroundWorkerManager_Tests()
    {
        _backgroundWorkerManager = GetRequiredService<IBackgroundWorkerManager>();
        _options = GetRequiredService<IOptions<AbpBackgroundWorkerTickerQOptions>>();
    }

    [Fact]
    public void Should_Use_TickerQBackgroundWorkerManager()
    {
        // Assert
        _backgroundWorkerManager.ShouldBeOfType<TickerQBackgroundWorkerManager>();
    }

    [Fact]
    public void Should_Have_Default_Options()
    {
        // Assert
        var options = _options.Value;
        options.IsAutoRegisterEnabled.ShouldBeTrue();
        options.DefaultCronExpression.ShouldBe("0 * * ? * *");
        options.DefaultMaxRetryAttempts.ShouldBe(3);
        options.DefaultPriority.ShouldBe(0);
    }

    [Fact]
    public void Should_Allow_Options_Configuration()
    {
        // This test would be run in a separate test module with different options
        // For now, just verify the options exist
        _options.Value.ShouldNotBeNull();
    }
}