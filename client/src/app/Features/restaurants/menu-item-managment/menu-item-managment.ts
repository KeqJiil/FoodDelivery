import { ChangeDetectionStrategy, Component, inject, input, type ResourceRef } from '@angular/core';
import type { IMenuItem } from '../models/IRestaurantDetails';
import { RestaurantsService } from '../restaurants-service';
import { OptimisticService } from '@/app/Shared/services/optimistic.service';

@Component({
  selector: 'app-menu-item-managment',
  imports: [],
  templateUrl: './menu-item-managment.html',
  styleUrl: './menu-item-managment.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuItemManagment {
  public readonly menuItem = input.required<ResourceRef<IMenuItem>>();
  public readonly restaurantId = input.required<string>();

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
}
