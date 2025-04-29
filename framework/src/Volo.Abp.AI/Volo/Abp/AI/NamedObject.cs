namespace Volo.Abp.AI;

public class NamedObject //TODO: Move to Core package?
{
    public string Name { get; }

    public NamedObject(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
    }
}