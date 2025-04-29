using System;

namespace Volo.Abp.AI;

public class NamedAction<T> : NamedObject //TODO: Move to Core package?
{
    public Action<T> Action { get; set; }
    
    public NamedAction(string name, Action<T> action)
    : base(name)
    {
        Action = Check.NotNull(action, nameof(action));
    }
}