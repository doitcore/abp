using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Volo.Abp.BackgroundWorkers.TickerQ;

public class TickerQBackgroundWorkerBase_Tests : AbpIntegratedTest<AbpBackgroundWorkersTickerQTestModule>
{
    [Fact]
    public void Should_Have_Default_Properties()
    {
        // Arrange & Act
        var worker = new TestTickerQWorker();

        // Assert
        worker.AutoRegister.ShouldBeTrue();
        worker.Priority.ShouldBe(0);
        worker.MaxRetryAttempts.ShouldBe(3);
        worker.JobId.ShouldBeNull();
        worker.CronExpression.ShouldBeNull();
    }

    [Fact]
    public void Should_Allow_Custom_Configuration()
    {
        // Arrange & Act
        var worker = new TestTickerQWorker
        {
            JobId = "CustomJobId",
            CronExpression = "0 0 12 * * ?",
            Priority = 5,
            MaxRetryAttempts = 10,
            AutoRegister = false
        };

        // Assert
        worker.JobId.ShouldBe("CustomJobId");
        worker.CronExpression.ShouldBe("0 0 12 * * ?");
        worker.Priority.ShouldBe(5);
        worker.MaxRetryAttempts.ShouldBe(10);
        worker.AutoRegister.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Execute_DoWorkAsync()
    {
        // Arrange
        var worker = new TestTickerQWorker();

        // Act
        await worker.DoWorkAsync();

        // Assert
        worker.ExecutionCount.ShouldBe(1);
    }

    private class TestTickerQWorker : TickerQBackgroundWorkerBase
    {
        public int ExecutionCount { get; private set; }

        public override Task DoWorkAsync(CancellationToken cancellationToken = default)
        {
            ExecutionCount++;
            return Task.CompletedTask;
        }
    }
}