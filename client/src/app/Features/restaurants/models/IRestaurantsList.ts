import type { IRestaurantDetails } from './IRestaurantDetails';

export interface IRestaurantsList {
  restaurants: IRestaurantDetails[];
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
