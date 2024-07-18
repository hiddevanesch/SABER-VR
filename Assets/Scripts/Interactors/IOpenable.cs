public interface IOpenable : IInteractable
{
    public enum State
    {
        Open,
        Closed
    }

    public State CurrentState { get; set; }

    void IInteractable.Interact()
    {
        if (CurrentState == State.Closed)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    void Open();
    void Close();
}
