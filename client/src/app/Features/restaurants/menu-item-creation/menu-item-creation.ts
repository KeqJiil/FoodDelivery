import { Currency } from '@/app/Shared/enums/currency';
import { ChangeDetectionStrategy, Component, input, output, type ResourceRef } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import type { IRestaurantDetails } from '../models/IRestaurantDetails';
import type { IRestaurantMenuItemCreation } from '../models/IRestaurantMenuItemCreation';

@Component({
  selector: 'app-menu-item-creation',
  imports: [],
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
  }
}
