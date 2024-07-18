public interface ISelectable : ITriggerable
{
    public bool IsSelected { get; }

    void ITriggerable.Trigger()
    {
        if (!IsSelected)
        {
            Select();
        }
        else
        {
            Deselect();
        }
    }

    void Select();
    void Deselect();
}
