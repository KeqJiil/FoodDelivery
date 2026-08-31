import { Currency } from '@/app/Shared/enums/currency';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RestaurantsService } from '../restaurants-service';
import type { IOpeningWindow } from '../models/IRestaurantDetails';
import type { IRestaurantCreation } from '../models/IRestaurantCreations';
import type { ICreatedResource } from '@/app/Shared/models/ICreatedResource';
import { finalize } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RestaurantScheduleManagment } from '../restaurant-schedule-managment/restaurant-schedule-managment';

@Component({
  selector: 'app-restaurant-creation',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatCardModule,
    MatProgressSpinnerModule,
    RestaurantScheduleManagment,
  ],
  templateUrl: './restaurant-creation.html',
  styleUrl: './restaurant-creation.css',
  changeDetection: ChangeDetectionStrategy.OnPush
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
  protected readonly Currency = Currency;

  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);
  private readonly _router: Router = inject(Router);

  public onScheduleChange(newSchedule: IOpeningWindow[]): void {
    if (!(newSchedule.length > 0)) return;
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
        next: (created: ICreatedResource): void => {
          void this._router.navigate(['/restaurants', created.id]);
        },
      });
  }
}
