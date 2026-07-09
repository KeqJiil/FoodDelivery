namespace SharedKernel.Domain;

public abstract class AggregateRoot<T> where T : TypedId
{
    private List<DomainEvent<T>> _events = [];

    public IReadOnlyList<DomainEvent<T>> Events => _events.AsReadOnly();

    protected void AddEvent(DomainEvent<T> newEvent)
    {
        _events.Add(newEvent);
    }

    public void ClearEvents()
    {
        _events = [];
    }
}