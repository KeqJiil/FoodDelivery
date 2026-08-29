export interface IRestaurantDetails {
  id: string;
  name: string;
  description: string;
  minimalOrderPrice: IMoney;
  status: Status;
  openingWindows: IOpeningWindow[];
  menuItems: IMenuItem[];
}

export interface IMoney {
  currency: number;
  amount: number;
}

export interface IMenuItem {
  id: string;
  name: string;
  description: string;
  price: IMoney;
}

export interface IOpeningWindow {
  openDay: Days;
  openTime: string;
  closeDay: Days;
  closeTime: string;
}

export enum Days {
  Monday,
  Tuesday,
  Wednesday,
  Thursday,
  Friday,
  Saturday,
  Sunday
}

export enum Status {
  Active,
  Inactive
}
