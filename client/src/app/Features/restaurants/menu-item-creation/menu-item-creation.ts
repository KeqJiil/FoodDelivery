import { Currency } from '@/app/Shared/enums/currency';
import { ChangeDetectionStrategy, Component, input, output, type ResourceRef } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import type { IRestaurantDetails } from '../models/IRestaurantDetails';
import type { IRestaurantMenuItemCreation } from '../models/IRestaurantMenuItemCreation';

@Component({
  selector: 'app-menu-item-creation',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
  ],
  templateUrl: './menu-item-creation.html',
  styleUrl: './menu-item-creation.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemCreation {
  public readonly restaurant = input.required<ResourceRef<IRestaurantDetails>>();
  public readonly menuItemForm = new FormGroup({
    name: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(30)],
    }),
    description: new FormControl<string>('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(10), Validators.maxLength(200)],
    }),
    currency: new FormControl<Currency>(Currency.Usd, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    amount: new FormControl<number>(0.1, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.1)],
    }),
  });
  
  public readonly menuItemOutput = output<IRestaurantMenuItemCreation>();

  protected readonly Currency = Currency;

  public onMenuItemSave(): void {
    if (this.menuItemForm.invalid) {
      this.menuItemForm.markAllAsTouched();
      return;
    }

    const data: IRestaurantMenuItemCreation = this.menuItemForm.getRawValue();
    this.menuItemOutput.emit(data);

    this.menuItemForm.reset({
      name: '',
      description: '',
      currency: Currency.Usd,
      amount: 0.1,
    });
  }
}
