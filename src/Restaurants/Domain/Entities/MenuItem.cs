using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Domain.Entities;

public class MenuItem
{
    public MenuItemId Id { get; }
    public Name Name { get; private set; }
    public Description Description { get; private set; }
    public Money Price { get; private set; }

    private MenuItem(MenuItemId id, Name name, Description description, Money price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }

    private MenuItem()
    {
        Id = null!;
        Name = null!;
        Description = null!;
        Price = null!;
    }

    public static MenuItem Create(MenuItemId id, Name name, Description description, Money price)
    {
        return new MenuItem(id, name, description, price);
    }

    public void ChangeName(Name newName)
    {
        Name = newName;
    }

    public void ChangeDescription(Description newDescription)
    {
        Description = newDescription;
    }

    public Result<Error> ChangePrice(Money price)
    {
        if (price.Currency != Price.Currency)
            return Result<Error>.Fail(new Error(ErrorEnum.Validation, "Wrong Currency"));
        Price = price;
        return Result<Error>.Success();
    }
}