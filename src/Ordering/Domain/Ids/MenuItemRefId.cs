using SharedKernel.Domain;

namespace Ordering.Domain.Ids;

public record MenuItemRefId: TypedId
{
   public MenuItemRefId(Guid id) : base(id)
   {
      
   }
}
