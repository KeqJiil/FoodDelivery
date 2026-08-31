import { ChangeDetectionStrategy, Component, inject, input, type ResourceRef } from '@angular/core';
import { RestaurantsService } from '../restaurants-service';
import { type Observable } from 'rxjs';
import { Status, type IMenuItem, type IOpeningWindow, type IRestaurantDetails } from '../models/IRestaurantDetails';
import type { IRestaurantMenuItemCreation } from '../models/IRestaurantMenuItemCreation';
import { OptimisticService, type IWritableValue } from '@/app/Shared/services/optimistic.service';
import { RestaurantScheduleManagment } from "../restaurant-schedule-managment/restaurant-schedule-managment";
import { MenuItemManagment } from "../menu-item-managment/menu-item-managment";
import { MenuItemCreation } from "../menu-item-creation/menu-item-creation";
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-restaurant-managment',
  imports: [
    RestaurantScheduleManagment,
    MenuItemManagment,
    MenuItemCreation,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './restaurant-managment.html',
  styleUrl: './restaurant-managment.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantManagment {
  public readonly restaurant = input.required<ResourceRef<IRestaurantDetails>>();

  protected Status = Status;

  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

  public changeName(newName: string): void {
    const restaurant = this.restaurant().value();

    const id = restaurant.id;

    this.optimistic(
      (r): IRestaurantDetails => ({ ...r, name: newName }),
      this._restaurantService.changeName(id, newName),
    );
  }

  public changeDescription(newDescription: string): void {
    const restaurant = this.restaurant().value();

    const id = restaurant.id;

    this.optimistic(
      (r): IRestaurantDetails  => ({ ...r, description: newDescription }),
      this._restaurantService.changeDescription(id, newDescription),
    );
  }

  public onSaveSchedule(schedule: IOpeningWindow[]): void {
    const restaurant = this.restaurant().value();

    this.optimistic(
      (r): IRestaurantDetails => ({ ...r, openingWindows: schedule }),
      this._restaurantService.changeSchedule(restaurant.id, schedule),
    );
  }

  public toggleActivation(): void {
    const restaurant = this.restaurant().value();

    const id = restaurant.id;
    const wasActive = restaurant.status === Status.Active;

    this.optimistic(
      (r): IRestaurantDetails => ({ ...r, status: wasActive ? Status.Inactive : Status.Active }),
      wasActive ? this._restaurantService.deactivate(id) : this._restaurantService.activate(id),
    );
  }

  public addMenuItem(menuItem: IRestaurantMenuItemCreation): void {
    const restaurant = this.restaurant().value();
    const id = restaurant.id;

    const optimisticItem: IMenuItem = { id: crypto.randomUUID(), ...menuItem, price: { currency: menuItem.currency, amount: menuItem.amount }};

    this.optimistic(
      (r): IRestaurantDetails => ({...r, menuItems: [ ...r.menuItems, optimisticItem ] }),
      this._restaurantService.createMenuItem(id, menuItem)
    );
  }

  public removeMenuItem(menuItemId: string): void {
    const restaurant = this.restaurant().value();
    const id = restaurant.id;

    this.optimistic(
      (r): IRestaurantDetails => ({...r, menuItems: ((): IMenuItem[] => r.menuItems.filter((x): boolean => x.id !== menuItemId ))() }),
      this._restaurantService.removeMenuItem(id, menuItemId)
    );
  }

  public menuItemResource(menuItemId: string): IWritableValue<IMenuItem> {
    const restaurant = this.restaurant();

    const findItem = (r: IRestaurantDetails): IMenuItem => {
      const item = r.menuItems.find((m): boolean => m.id === menuItemId);

      if (!item) {
        throw new Error(`Menu item ${menuItemId} not found`);
      }

      return item;
    };

    const replaceItem = (r: IRestaurantDetails, value: IMenuItem): IRestaurantDetails => ({
      ...r,
      menuItems: r.menuItems.map((m): IMenuItem => (m.id === menuItemId ? value : m)),
    });

    return {
      value: (): IMenuItem => findItem(restaurant.value()),
      set: (value: IMenuItem): void => {
        restaurant.set(replaceItem(restaurant.value(), value));
      },
      update: (updater: (value: IMenuItem) => IMenuItem): void => {
        restaurant.update((r): IRestaurantDetails => replaceItem(r, updater(findItem(r))));
      },
    };
  }

  private optimistic<TReturn>(
    apply: (r: IRestaurantDetails) => IRestaurantDetails,
    request: Observable<TReturn>,
  ): void {
    OptimisticService.optimisticUpdate<TReturn, IRestaurantDetails>({
      apply: apply,
      request: request,
      signal: this.restaurant(),
    });
  }
}
