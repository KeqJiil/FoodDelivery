import { Component, input } from "@angular/core";
import { MatCardModule } from "@angular/material/card";
import type { IRestaurantDetails } from "../models/IRestaurantDetails";

@Component({
  selector: 'app-restaurants-list',
  imports: [MatCardModule],
  standalone: true,
  templateUrl: './restaurants-list.html',
  styleUrl: './restaurants-list.css',
})
export class RestaurantsList {
  public readonly restaurantsList = input.required<IRestaurantDetails[]>();
}
