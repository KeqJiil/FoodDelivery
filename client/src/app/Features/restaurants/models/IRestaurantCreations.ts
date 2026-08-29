import type { IOpeningWindow } from '@/app/Features/restaurants/models/IRestaurantDetails';

export interface IRestaurantCreation {
  name: string;
  description: string;
  amount: number;
  currency: number;
  schedules: IOpeningWindow[];
}
