using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TestApp.Domain;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Repositories;

public class SharedEntity_Repository_Tests : EntityFrameworkCoreTestBase
{
    protected readonly IRepository<TestSharedEntity> TestSharedTypeEntityRepository;
    protected readonly ICurrentTenant CurrentTenant;
    protected readonly IDataFilter<ISoftDelete> dataFilter;

    public SharedEntity_Repository_Tests()
    {
        TestSharedTypeEntityRepository = GetRequiredService<IRepository<TestSharedEntity>>();
        CurrentTenant = GetRequiredService<ICurrentTenant>();
        dataFilter = GetRequiredService<IDataFilter<ISoftDelete>>();
    }

    [Fact]
    public async Task SharedEntity_Test()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            TestSharedTypeEntityRepository.SetCustomEntityName("TestSharedEntity1");

            var tenantId = Guid.NewGuid();
            await TestSharedTypeEntityRepository.InsertManyAsync(new List<TestSharedEntity>()
            {
                new TestSharedEntity(Guid.NewGuid())
                {
                    TenantId = null,
                    IsDeleted = false,

                    ["Name"] = "Test Person1",
                    ["Age"] = 10,
                    ["Birthday"] = DateTime.Now
                }.SetProperty("testProperty", "Test Value1"),
                new TestSharedEntity(Guid.NewGuid())
                {
                    TenantId = tenantId,
                    IsDeleted = false,

                    ["Name"] = "Test Person2",
                    ["Age"] = 20,
                    ["Birthday"] = DateTime.Now
                },
                new TestSharedEntity(Guid.NewGuid())
                {
                    TenantId = tenantId,
                    IsDeleted = true,

                    ["Name"] = "Test Person3",
                    ["Age"] = 30,
                    ["Birthday"] = DateTime.Now
                },
                new TestSharedEntity(Guid.NewGuid())
                {
                    TenantId = null,
                    IsDeleted = true,

                    ["Name"] = "Test Person4",
                    ["Age"] = 40,
                    ["Birthday"] = DateTime.Now
                }
            }, true);

            var entities = (await TestSharedTypeEntityRepository.GetListAsync()).OrderBy(x => x["Name"]).ToList();
            entities.Count.ShouldBe(1);
            entities[0].TenantId.ShouldBeNull();
            entities[0].IsDeleted.ShouldBe(false);
            entities[0]["Name"].ShouldBe("Test Person1");
            entities[0]["Age"].ShouldBe(10);
            entities[0].GetProperty("testProperty").ShouldBe("Test Value1");

            using (CurrentTenant.Change(tenantId))
            {
                entities = (await TestSharedTypeEntityRepository.GetListAsync()).OrderBy(x => x["Name"]).ToList();
                entities.Count.ShouldBe(1);
                entities[0].TenantId.ShouldBe(tenantId);
                entities[0].IsDeleted.ShouldBe(false);
                entities[0]["Name"].ShouldBe("Test Person2");
                entities[0]["Age"].ShouldBe(20);
            }

            using (dataFilter.Disable())
            {
                entities = (await TestSharedTypeEntityRepository.GetListAsync()).OrderBy(x => x["Name"]).ToList();
                entities.Count.ShouldBe(2);

                entities[0].TenantId.ShouldBeNull();
                entities[0].IsDeleted.ShouldBe(false);
                entities[0]["Name"].ShouldBe("Test Person1");
                entities[0]["Age"].ShouldBe(10);

                entities[1].TenantId.ShouldBeNull();
                entities[1].IsDeleted.ShouldBe(true);
                entities[1]["Name"].ShouldBe("Test Person4");
                entities[1]["Age"].ShouldBe(40);
            }

            using (CurrentTenant.Change(tenantId))
            {
                using (dataFilter.Disable())
                {
                    entities = (await TestSharedTypeEntityRepository.GetListAsync()).OrderBy(x => x["Name"]).ToList();
                    entities.Count.ShouldBe(2);

                    entities[0].TenantId.ShouldBe(tenantId);
                    entities[0].IsDeleted.ShouldBe(false);
                    entities[0]["Name"].ShouldBe("Test Person2");
                    entities[0]["Age"].ShouldBe(20);

                    entities[1].TenantId.ShouldBe(tenantId);
                    entities[1].IsDeleted.ShouldBe(true);
                    entities[1]["Name"].ShouldBe("Test Person3");
                    entities[1]["Age"].ShouldBe(30);
                }
            }

            TestSharedTypeEntityRepository.SetCustomEntityName("TestSharedEntity2");
            await TestSharedTypeEntityRepository.InsertManyAsync(new List<TestSharedEntity>()
            {
                new TestSharedEntity(Guid.NewGuid())
                {
                    ["Name"] = "Test Person1 from Second Table",
                    ["Age"] = 110,
                    ["Birthday"] = DateTime.Now
                }
            }, true);

            var entitiesFromSecondTable = (await TestSharedTypeEntityRepository.GetListAsync()).OrderBy(x => x["Name"]).ToList();
            entitiesFromSecondTable.Count.ShouldBe(1);
            entitiesFromSecondTable[0].TenantId.ShouldBeNull();
            entitiesFromSecondTable[0].IsDeleted.ShouldBe(false);
            entitiesFromSecondTable[0]["Name"].ShouldBe("Test Person1 from Second Table");
            entitiesFromSecondTable[0]["Age"].ShouldBe(110);
        });
    }
}
