import { Currency } from '@/app/Shared/enums/currency';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { RestaurantsService } from '../restaurants-service';
import type { IOpeningWindow } from '../models/IRestaurantDetails';
import type { IRestaurantCreation } from '../models/IRestaurantCreations';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import type { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-restaurant-creation',
  imports: [],
  templateUrl: './restaurant-creation.html',
  styleUrl: './restaurant-creation.css',
})
export class RestaurantCreation {
  public readonly restaurantCreationForm = new FormGroup({
    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(30)],
    }),
    description: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(10), Validators.maxLength(200)],
    }),
    amount: new FormControl<number>(0.01, {
      nonNullable: true,
      validators: [Validators.min(0.01), Validators.required],
    }),
    currency: new FormControl<Currency>(Currency.Usd, {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });
  public readonly isRequestInFlight = signal<boolean>(false);

  protected readonly schedules = signal<IOpeningWindow[]>([]);

  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

  public onScheduleChange(newSchedule: IOpeningWindow[]): void {
    if (!(this.schedules().length > 0)) return;
    this.schedules.set(newSchedule);
  }

  public createNewRestaurant(): void {
    if (this.restaurantCreationForm.invalid) {
      this.restaurantCreationForm.markAllAsTouched();
      return;
    }
    if (!(this.schedules().length > 0) || this.isRequestInFlight()) return;

    const requestData: IRestaurantCreation = {
      ...this.restaurantCreationForm.getRawValue(),
      schedules: this.schedules(),
    };

    this.isRequestInFlight.set(true);

    this._restaurantService
      .createRestaurant(requestData)
      .pipe(
        finalize((): void => {
          this.isRequestInFlight.set(false);
        }),
      )
      .subscribe({
        error: (err: HttpErrorResponse): void => {
          
        },
      });
  }
}
