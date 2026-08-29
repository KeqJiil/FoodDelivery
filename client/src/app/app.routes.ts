import type { Routes } from '@angular/router';
import { RestaurantsPage } from './Features/restaurants/restaurants-page/restaurants-page';
import { RestaurantDetails } from './Features/restaurants/restaurant-details/restaurant-details';

export const AppRoutes: Routes = [
  {
    path: "",
    redirectTo: "restaurants",
    pathMatch: "full"
  },
  {
    path: "restaurants",
    component: RestaurantsPage
  },
  {
    path: "restaurants/:id",
    component: RestaurantDetails
  }
];
