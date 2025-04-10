﻿using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace Volo.Abp.BackgroundJobs;

public class BackgroundJobsTestDataBuilder : ITransientDependency
{
    private readonly BackgroundJobsTestData _testData;
    private readonly IBackgroundJobRepository _backgroundJobRepository;
    private readonly IClock _clock;

    public BackgroundJobsTestDataBuilder(
        BackgroundJobsTestData testData,
        IBackgroundJobRepository backgroundJobRepository,
        IClock clock)
    {
        _testData = testData;
        _backgroundJobRepository = backgroundJobRepository;
        _clock = clock;
    }

    public async Task BuildAsync()
    {
        await _backgroundJobRepository.InsertAsync(
            new BackgroundJobRecord(_testData.JobId1)
            {
                ApplicationName = "App1",
                JobName = "TestJobName",
                JobArgs = "{ value: 1 }",
                NextTryTime = _clock.Now.Subtract(TimeSpan.FromMinutes(1)),
                Priority = BackgroundJobPriority.Normal,
                IsAbandoned = false,
                LastTryTime = null,
                CreationTime = _clock.Now.Subtract(TimeSpan.FromMinutes(2)),
                TryCount = 0
            }
        );

        await _backgroundJobRepository.InsertAsync(
            new BackgroundJobRecord(_testData.JobId2)
            {
                ApplicationName = "App2",
                JobName = "TestJobName",
                JobArgs = "{ value: 2 }",
                NextTryTime = _clock.Now.AddMinutes(42),
                Priority = BackgroundJobPriority.AboveNormal,
                IsAbandoned = true,
                LastTryTime = _clock.Now.Subtract(TimeSpan.FromDays(1)),
                CreationTime = _clock.Now.Subtract(TimeSpan.FromDays(2)),
                TryCount = 3
            }
        );

        await _backgroundJobRepository.InsertAsync(
            new BackgroundJobRecord(_testData.JobId3)
            {
                ApplicationName = "App1",
                JobName = "TestJobName",
                JobArgs = "{ value: 3 }",
                NextTryTime = _clock.Now,
                Priority = BackgroundJobPriority.BelowNormal,
                IsAbandoned = false,
                LastTryTime = _clock.Now.Subtract(TimeSpan.FromMinutes(60)),
                CreationTime = _clock.Now.Subtract(TimeSpan.FromMinutes(90)),
                TryCount = 2
            }
        );
    }
}
