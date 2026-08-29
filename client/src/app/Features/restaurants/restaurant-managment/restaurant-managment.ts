import { ChangeDetectionStrategy, Component, inject, input, type ResourceRef } from '@angular/core';
import { RestaurantsService } from '../restaurants-service';
import type { Observable } from 'rxjs';
import { Status, type IOpeningWindow, type IRestaurantDetails } from '../models/IRestaurantDetails';
import type { IRestaurantMenuItemCreation } from '../models/IRestaurantMenuItemCreation';
import { OptimisticService } from '@/app/Shared/services/optimistic.service';

@Component({
  selector: 'app-restaurant-managment',
  imports: [],
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

    this._restaurantService.createMenuItem(id, menuItem).subscribe({
    });
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
