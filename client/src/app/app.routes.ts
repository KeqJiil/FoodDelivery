import type { Routes } from '@angular/router';
import { RestaurantsList } from './Features/restaurants/restaurants-list/restaurants-list';
import {RestaurantDetails} from './Features/restaurants/restaurant-details/restaurant-details';

export const AppRoutes: Routes = [
  {
    path: "restaurants",
    component: RestaurantsList
  },
  {
    path: "restaurants/:id",
    component: RestaurantDetails
  }
];
