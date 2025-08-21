using System;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Volo.Abp.TestApp.Domain;

public class TestSharedEntity : SharedEntity<Guid>, IMultiTenant, ISoftDelete, IHasExtraProperties
{
    public Guid? TenantId { get; set; }

    public bool IsDeleted { get; set; }

    public TestSharedEntity()
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }

    public TestSharedEntity(Guid id)
        : base(id)
    {
        ExtraProperties = new ExtraPropertyDictionary();
        this.SetDefaultsForExtraProperties();
    }

    public ExtraPropertyDictionary ExtraProperties { get; set; }
}
