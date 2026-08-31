import type { Routes } from '@angular/router';
import { RestaurantsPage } from './Features/restaurants/restaurants-page/restaurants-page';
import { RestaurantDetails } from './Features/restaurants/restaurant-details/restaurant-details';
import { RestaurantCreation } from './Features/restaurants/restaurant-creation/restaurant-creation';
import { RestaurantManagmentPage } from './Features/restaurants/restaurant-managment-page/restaurant-managment-page';

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
    path: "restaurants/create",
    component: RestaurantCreation
  },
  {
    path: "restaurants/:id/manage",
    component: RestaurantManagmentPage
  },
  {
    path: "restaurants/:id",
    component: RestaurantDetails
  }
];
