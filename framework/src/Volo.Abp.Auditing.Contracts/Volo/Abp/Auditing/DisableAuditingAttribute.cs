using System;

namespace Volo.Abp.Auditing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class DisableAuditingAttribute : Attribute
{
    public bool UpdateModificationProps { get; set; } = true;

    public bool PublishEntityEvent { get; set; } = true;
}
