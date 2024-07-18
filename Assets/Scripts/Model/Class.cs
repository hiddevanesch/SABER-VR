using System.Collections.Generic;

public class Class : SoftwareComponent
{
    private List<Class> _specializes = new List<Class>();

    public Class(string id, string name) : base(id, name)
    {
    }

    public IReadOnlyList<Class> Specializes
    {
        get => _specializes.AsReadOnly();
    }

    public void AddSpecialization(Class specialization)
    {
        _specializes.Add(specialization);
    }
}
