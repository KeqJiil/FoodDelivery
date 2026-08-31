import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import type { IMenuItem, IMoney } from '../models/IRestaurantDetails';
import { RestaurantsService } from '../restaurants-service';
import { OptimisticService, type IWritableValue } from '@/app/Shared/services/optimistic.service';
import { Currency } from '@/app/Shared/enums/currency';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-menu-item-managment',
  imports: [MatButtonModule, MatIconModule, MatFormFieldModule, MatSelectModule, MatInputModule],
  templateUrl: './menu-item-managment.html',
  styleUrl: './menu-item-managment.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemManagment {
  public readonly menuItem = input.required<IWritableValue<IMenuItem>>();
  public readonly restaurantId = input.required<string>();

  protected readonly Currency = Currency;

  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

  public changeName(newName: string): void {
    const menuItem = this.menuItem().value();
    const id = menuItem.id;

    OptimisticService.optimisticUpdate({
      apply: (r): IMenuItem => ({ ...r, name: newName }),
      request: this._restaurantService.changeMenuItemName(this.restaurantId(), id, newName),
      signal: this.menuItem(),
    });
  }

  public changeDescription(newDescription: string): void {
    const menuItem = this.menuItem().value();
    const id = menuItem.id;

    OptimisticService.optimisticUpdate({
      apply: (r): IMenuItem => ({...r, description: newDescription}),
      request: this._restaurantService.changeMenuItemDescription(this.restaurantId(), id, newDescription),
      signal: this.menuItem(),
    })
  }

  public changePrice(currency: Currency, amount: number): void {
    const menuItem = this.menuItem().value();
    const id = menuItem.id;
    const money: IMoney = { currency, amount };

    OptimisticService.optimisticUpdate({
      apply: (r): IMenuItem => ({ ...r, price: money }),
      request: this._restaurantService.changeMenuItemPrice(this.restaurantId(), id, money),
      signal: this.menuItem()
    })
  }
}
