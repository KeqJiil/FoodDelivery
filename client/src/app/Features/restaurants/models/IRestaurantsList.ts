import type { IRestaurantDetails } from './IRestaurantDetails';

export interface IRestaurantsList extends IPaginationMetadata {
  restaurants: IRestaurantDetails[];
}

export interface IPaginationMetadata {
  page: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
