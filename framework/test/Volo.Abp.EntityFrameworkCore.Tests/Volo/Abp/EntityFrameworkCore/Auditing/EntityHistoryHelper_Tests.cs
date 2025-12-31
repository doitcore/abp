using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore.EntityHistory;
using Volo.Abp.TestApp.Domain;
using Volo.Abp.TestApp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Auditing;

public class EntityHistoryHelper_Tests : EntityFrameworkCoreTestBase
{
    private readonly IEntityHistoryHelper _entityHistoryHelper;
    private readonly IRepository<AppEntityWithJsonProperty, Guid> _appEntityWithJsonRepository;
    private readonly IRepository<TestSharedEntity, Guid> _testSharedEntityRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public EntityHistoryHelper_Tests()
    {
        _entityHistoryHelper = GetRequiredService<IEntityHistoryHelper>();
        _appEntityWithJsonRepository = GetRequiredService<IRepository<AppEntityWithJsonProperty, Guid>>();
        _testSharedEntityRepository = GetRequiredService<IRepository<TestSharedEntity, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public async Task CreateChangeList_Should_Track_Nested_Json_Property_Changes_As_Separate_Property_Changes()
    {
        // Arrange & Act
        EntityChangeInfo entityChange = null;

        await WithUnitOfWorkAsync(async () =>
        {
            var entity = new AppEntityWithJsonProperty(Guid.NewGuid(), "Test Entity")
            {
                Data = new JsonPropertyObject()
                {
                    { "Name", "String Name" },
                    { "Value", "String Value"}
                },
                Count = 10
            };

            await _appEntityWithJsonRepository.InsertAsync(entity);

            var dbContext = await GetDbContextAsync();

            var entries = dbContext.ChangeTracker.Entries().ToList();
            var entityChanges = _entityHistoryHelper.CreateChangeList(entries);

            entityChange = entityChanges.FirstOrDefault(x => x.EntityTypeFullName.Contains(nameof(AppEntityWithJsonProperty)));
        });

        // Assert
        entityChange.ShouldNotBeNull();
        var dataPropertyChange = entityChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == nameof(AppEntityWithJsonProperty.Data));
        dataPropertyChange.ShouldBeNull();
        var jsonNamePropertyChange = entityChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == nameof(AppEntityWithJsonProperty.Data) + "." + "Name");
        jsonNamePropertyChange.ShouldNotBeNull();
        jsonNamePropertyChange.PropertyTypeFullName.ShouldBe(typeof(string).FullName);
        jsonNamePropertyChange.NewValue.ShouldBe("\"String Name\"");
        
        var jsonValuePropertyChange = entityChange.PropertyChanges.FirstOrDefault(x => x.PropertyName == "Value");
        jsonValuePropertyChange.ShouldNotBeNull();
        jsonValuePropertyChange.PropertyTypeFullName.ShouldBe(typeof(string).FullName);
        jsonValuePropertyChange.NewValue.ShouldBe("\"String Value\"");
    }

    [Fact]
    public async Task CreateChangeList_Should_Track_Shared_Entities_With_Their_Respective_Entity_Names()
    {
        // Arrange & Act
        List<EntityChangeInfo> entityChanges = null;

        await WithUnitOfWorkAsync(async () =>
        {
            var entity = new TestSharedEntity(Guid.NewGuid())
            {
                TenantId = null,
                IsDeleted = false,
                Name = "Test Person1",
                Age = 10,
                Birthday = DateTime.Now
            }.SetProperty("testProperty", "Test Value1");

            _testSharedEntityRepository.SetEntityName("TestSharedEntity1");
            await _testSharedEntityRepository.InsertAsync(entity);
            
            var entity2 = new TestSharedEntity(Guid.NewGuid())
            {
                TenantId = null,
                IsDeleted = false,
                Name = "Test Person2",
                Age = 20,
                Birthday = DateTime.Now
            }.SetProperty("testProperty", "Test Value2");
            
            _testSharedEntityRepository.SetEntityName("TestSharedEntity2");
            await _testSharedEntityRepository.InsertAsync(entity2);

            var dbContext = await GetDbContextAsync();

            var entries = dbContext.ChangeTracker.Entries().ToList();
            entityChanges = _entityHistoryHelper.CreateChangeList(entries);
        });
        
        entityChanges.ShouldContain(x => x.EntityTypeFullName == "TestSharedEntity1");
        entityChanges.ShouldContain(x => x.EntityTypeFullName == "TestSharedEntity2");
    }

    private async Task<TestAppDbContext> GetDbContextAsync()
    {
        var uow = _unitOfWorkManager.Current;
        if (uow == null)
        {
            throw new InvalidOperationException("No active unit of work found");
        }

        var dbContextProvider = uow.ServiceProvider.GetRequiredService<IDbContextProvider<TestAppDbContext>>();
        return await dbContextProvider.GetDbContextAsync();
    }
}

