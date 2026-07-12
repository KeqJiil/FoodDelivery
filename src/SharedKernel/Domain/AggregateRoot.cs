namespace SharedKernel.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<object> GetDomainEvents();
    void ClearDomainEvents();
}

public abstract class AggregateRoot<T> : IHasDomainEvents where T : TypedId
{
    private List<DomainEvent<T>> _events = [];

    public IReadOnlyList<DomainEvent<T>> Events => _events.AsReadOnly();

    protected void AddEvent(DomainEvent<T> newEvent)
    {
        _events.Add(newEvent);
    }

    private void ClearEvents()
    {
        _events = [];
    }

    public IReadOnlyList<object> GetDomainEvents()
    {
        return _events;
    }

    public void ClearDomainEvents()
    {
        ClearEvents();
    }
}