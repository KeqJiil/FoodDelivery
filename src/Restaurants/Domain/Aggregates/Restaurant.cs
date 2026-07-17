using Restaurants.Domain.Entities;
using Restaurants.Domain.Enums;
using Restaurants.Domain.Events;
using Restaurants.Domain.Ids;
using Restaurants.Domain.ValueObjects;
using SharedKernel.Domain;
using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace Restaurants.Domain.Aggregates;

public class Restaurant : AggregateRoot<RestaurantId>
{
    public RestaurantId Id { get; }
    public RestaurantStatus Status { get; private set; }
    public Schedule Schedule { get; private set; }
    public Name Name { get; private set; }
    public Description Description { get; private set; }
    public Money MinimalOrderPrice { get; private set; }

    private readonly List<MenuItem> _menuItems;

    public IReadOnlyList<MenuItem> MenuItems => _menuItems.AsReadOnly();

    private Restaurant(RestaurantId id, Name name, Description description, Money minimalOrderPrice,
        Schedule schedule, RestaurantStatus status)
    {
        Id = id;
        Name = name;
        Description = description;
        MinimalOrderPrice = minimalOrderPrice;
        Schedule = schedule;
        Status = status;
        _menuItems = [];
    }

    private Restaurant()
    {
        Id = null!;
        Name = null!;
        Description = null!;
        MinimalOrderPrice = null!;
        Schedule = null!;
        _menuItems = [];
    }

    public static Restaurant Create(RestaurantId id, Name name, Description description, Money minimalOrderPrice,
        Schedule? schedule = null)
    {
        return new Restaurant(id, name, description, minimalOrderPrice, schedule ?? new Schedule(),
            RestaurantStatus.Active);
    }

    public Result<Error> ChangeName(Name newName)
    {
        Name = newName;

        return Result<Error>.Success();
    }

    public Result<Error> ChangeDescription(Description newDescription)
    {
        Description = newDescription;

        return Result<Error>.Success();
    }

    public Result<Error> AddMenuItem(MenuItemId id, Name name, Description description, Money price)
    {
        if (price.Currency != MinimalOrderPrice.Currency)
            return Result<Error>.Fail(Error.Validation("Different currency"));

        var entity = MenuItem.Create(id, name, description, price);
        _menuItems.Add(entity);

        return Result<Error>.Success();
    }

    public Result<Error> RemoveMenuItem(MenuItemId id)
    {
        var result = _menuItems.RemoveAll(x => x.Id == id);

        return result == 0
            ? Result<Error>.Fail(Error.NotFound("Item not found"))
            : Result<Error>.Success();
    }

    public Result<Error> ChangeMenuItemPrice(MenuItemId id, Money price)
    {
        var menuItem = _menuItems.FirstOrDefault(x => x.Id == id);
        if (menuItem == null) return Result<Error>.Fail(Error.NotFound("Item not found"));

        var result = menuItem.ChangePrice(price);
        if (!result.IsSuccess)
            return Result<Error>.Fail(result.Error ?? Error.Unexpected());

        AddEvent(new MenuItemPriceChanged(Id));

        return Result<Error>.Success();
    }

    public Result<Error> ChangeMenuItemName(MenuItemId id, Name name)
    {
        var menuItem = _menuItems.FirstOrDefault(x => x.Id == id);
        if (menuItem == null) return Result<Error>.Fail(Error.NotFound("Item not found"));

        menuItem.ChangeName(name);
        return Result<Error>.Success();
    }

    public Result<Error> ChangeMenuItemDescription(MenuItemId id, Description description)
    {
        var menuItem = _menuItems.FirstOrDefault(x => x.Id == id);
        if (menuItem == null) return Result<Error>.Fail(Error.NotFound("Item not found"));

        menuItem.ChangeDescription(description);
        return Result<Error>.Success();
    }

    public Result<Error> SetMinimalOrderPrice(Money price)
    {
        if (MinimalOrderPrice.Currency != price.Currency)
            return Result<Error>.Fail(Error.Validation("Wrong currency"));

        MinimalOrderPrice = price;

        return Result<Error>.Success();
    }

    public Result<Error> ChangeSchedule(Schedule newSchedule)
    {
        Schedule = newSchedule;

        return Result<Error>.Success();
    }

    public void Activate()
    {
        Status = RestaurantStatus.Active;
    }

    public void Deactivate()
    {
        Status = RestaurantStatus.Inactive;
    }

    public bool IsOpen(DateTimeOffset now)
    {
        return Schedule.IsOpenNow(now) && IsActive();
    }

    public bool IsActive()
    {
        return Status is RestaurantStatus.Active;
    }
}