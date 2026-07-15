using SharedKernel.Domain;

namespace Restaurants.Domain.Ids;

public record MenuItemId : TypedId
{
    public MenuItemId(Guid id) : base(id)
    {
    }
}