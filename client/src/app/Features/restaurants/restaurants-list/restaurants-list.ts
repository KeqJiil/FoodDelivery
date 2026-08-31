import { ChangeDetectionStrategy, Component, input } from "@angular/core";
import { MatCardModule } from "@angular/material/card";
import { RouterLink } from "@angular/router";
import type { IRestaurantDetails } from "../models/IRestaurantDetails";

@Component({
  selector: 'app-restaurants-list',
  imports: [MatCardModule, RouterLink],
  templateUrl: './restaurants-list.html',
  styleUrl: './restaurants-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantsList {
  public readonly restaurantsList = input.required<IRestaurantDetails[]>();
}
