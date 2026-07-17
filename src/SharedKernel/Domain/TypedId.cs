namespace SharedKernel.Domain;

public abstract record TypedId
{
   public Guid Id { get; private init; }

   protected TypedId() : this(Guid.NewGuid())
   {
   }

   protected TypedId(Guid id)
   {
      ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
      Id = id;
   }
};