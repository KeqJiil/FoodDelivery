import { Component, inject, input, type ResourceRef } from '@angular/core';
import { RestaurantsService } from '../restaurants-service';
import type { Observable } from 'rxjs';
import { Status, type IOpeningWindow, type IRestaurantDetails } from '../models/IRestaurantDetails';
import { MatSnackBar } from '@angular/material/snack-bar';
import type { HttpErrorResponse } from '@angular/common/http';
import type { IErrorDetails } from '@/app/Shared/models/IErrorDetails';

@Component({
  selector: 'app-restaurant-managment',
  imports: [],
  templateUrl: './restaurant-managment.html',
  styleUrl: './restaurant-managment.css',
})
export class RestaurantManagment {
  public readonly restaurant = input.required<ResourceRef<IRestaurantDetails | undefined>>();

  protected Status = Status;

  private readonly _snackBar: MatSnackBar = inject(MatSnackBar);
  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

  public changeName(newName: string): void {
    const restaurant = this.restaurant().value();
    if (!restaurant) return;

    const id = restaurant.id;

    this.optimisticUpdate(
      (r): IRestaurantDetails | undefined => r && { ...r, name: newName },
      this._restaurantService.changeName(id, newName),
    );
  }

  public changeDescription(newDescription: string): void {
    const restaurant = this.restaurant().value();
    if (!restaurant) return;

    const id = restaurant.id;

    this.optimisticUpdate(
      (r): IRestaurantDetails | undefined => r && { ...r, description: newDescription },
      this._restaurantService.changeDescription(id, newDescription),
    );
  }

  public onSaveSchedule(schedule: IOpeningWindow[]): void {
    const restaurant = this.restaurant().value();
    if (!restaurant) return;

    this.optimisticUpdate(
      (r): IRestaurantDetails | undefined => r && { ...r, openingWindows: schedule },
      this._restaurantService.changeSchedule(restaurant.id, schedule),
    );
  }

  public toggleActivation(): void {
    const restaurant = this.restaurant().value();
    if (!restaurant) return;

    const id = restaurant.id;
    const wasActive = restaurant.status === Status.Active;

    this.optimisticUpdate(
      (r): IRestaurantDetails | undefined =>
        r && { ...r, status: wasActive ? Status.Inactive : Status.Active },
      wasActive ? this._restaurantService.deactivate(id) : this._restaurantService.activate(id),
    );
  }

  private optimisticUpdate<T>(
    apply: (r: IRestaurantDetails | undefined) => IRestaurantDetails | undefined,
    request: Observable<T>,
  ): void {
    const previous = this.restaurant().value();
    this.restaurant().update(apply);

    request.subscribe({
      error: (err: HttpErrorResponse): void => {
        this.restaurant().set(previous);
        // eslint-disable-next-line @typescript-eslint/consistent-type-assertions
        const problem = err.error as IErrorDetails;
        const firstFieldError = Object.values<string[]>(problem.errors ?? {})[0]?.[0];
        this._snackBar.open(problem.detail ?? firstFieldError ?? 'Something went wrong', 'Close', {
          duration: 4000,
        });
      },
    });
  }
}
