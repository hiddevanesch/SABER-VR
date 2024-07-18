using System.Collections.Generic;

public class SoftwareComponent
{
    private string _id;
    private string _name;
    private List<SoftwareComponent> _children = new List<SoftwareComponent>();
    private List<string> _contains = new List<string>();
    private SoftwareComponent _parent;
    private Dictionary<SoftwareComponent, int> _invokes = new Dictionary<SoftwareComponent, int>();
    private int _indirectChildrenCount = 0;

    public SoftwareComponent(string id, string name)
    {
        this._id = id;
        this._name = name;
    }

    public string Id
    {
        get => _id;
        set => _id = value;
    }

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public IReadOnlyList<SoftwareComponent> Children
    {
        get => _children.AsReadOnly();
    }

    public void AddChild(SoftwareComponent child)
    {
        _children.Add(child);
    }

    public IReadOnlyList<string> Contains
    {
        get => _contains.AsReadOnly();
    }

    public void AddContainment(string contains)
    {
        _contains.Add(contains);
    }

    public SoftwareComponent Parent
    {
        get => _parent;
        set => _parent = value;
    }

    public IReadOnlyDictionary<SoftwareComponent, int> Invokes
    {
        get => _invokes;
    }

    public int IndirectChildrenCount
    {
        get => _indirectChildrenCount;
        set => _indirectChildrenCount = value;
    }

    public void AddInvocation(SoftwareComponent invoke, int amount)
    {
        if (_invokes.ContainsKey(invoke))
        {
            _invokes[invoke] += amount;
        }
        else
        {
            _invokes.Add(invoke, amount);
        }
    }

    public void AddInvocations(IReadOnlyDictionary<SoftwareComponent, int> invokes)
    {
        foreach (KeyValuePair<SoftwareComponent, int> invoke in invokes)
        {
            AddInvocation(invoke.Key, invoke.Value);
        }
    }

    public bool AllChildrenArePackages()
    {
        foreach (SoftwareComponent child in _children)
        {
            if (child is not Package)
            {
                return false;
            }
        }
        return true;
    }

    public bool AllChildrenAreClasses()
    {
        foreach (SoftwareComponent child in _children)
        {
            if (child is not Class)
            {
                return false;
            }
        }
        return true;
    }

    public bool AllChildrenAreMethods()
    {
        foreach (SoftwareComponent child in _children)
        {
            if (child is not Method)
            {
                return false;
            }
        }
        return true;
    }
}