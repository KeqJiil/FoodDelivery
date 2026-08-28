export interface IRestaurantDetails {
  id: string;
  name: string;
  description: string;
  minimalOrderPrice: IMoney;
  status: number;
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
  openDay: number;
  openTime: Date;
  closeDay: number;
  closeTime: Date;
}
