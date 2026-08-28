import { inject, Injectable } from '@angular/core';
import type { IRestaurantsList } from './models/IRestaurantsList';
import { HttpClient, HttpParams } from '@angular/common/http';
import { type Observable } from 'rxjs';
import type {
  IMoney,
  IOpeningWindow,
  IRestaurantDetails,
} from '@/app/Features/restaurants/models/IRestaurantDetails';
import type { IRestaurantCreation } from '@/app/Features/restaurants/models/IRestaurantCreations';
import type { IRestaurantMenuItemCreation } from '@/app/Features/restaurants/models/IRestaurantMenuItemCreation';
import type { ICreatedResource } from '@/app/Shared/models/ICreatedResource';
import { environment } from '@/environments/environment';

@Injectable({ providedIn: 'root' })
export class RestaurantsService {
  private readonly _http: HttpClient;

  public constructor() {
    this._http = inject(HttpClient);
  }

  public getRestaurantsList(
    pageSize: number,
    currentPage: number | null,
  ): Observable<IRestaurantsList> {
    const params = new HttpParams().set('page', currentPage ?? 1).set('pageSize', pageSize);

    return this._http.get<IRestaurantsList>(`${environment.apiUrl}/Restaurants`, { params });
  }

  public getRestaurantById(id: string): Observable<IRestaurantDetails> {
    return this._http.get<IRestaurantDetails>(`${environment.apiUrl}/Restaurants/${id}`);
  }

  public createRestaurant(data: IRestaurantCreation): Observable<ICreatedResource> {
    return this._http.post<ICreatedResource>(`${environment.apiUrl}/Restaurants`, data);
  }

  public createMenuItem(
    restaurantId: string,
    data: IRestaurantMenuItemCreation,
  ): Observable<ICreatedResource> {
    return this._http.post<ICreatedResource>(
      `${environment.apiUrl}/Restaurants/${restaurantId}/menu-items`,
      data,
    );
  }

  public removeMenuItem(restaurantId: string, menuItemId: string): Observable<unknown> {
    return this._http.delete<unknown>(
      `${environment.apiUrl}/Restaurants/${restaurantId}/menu-items/${menuItemId}`,
    );
  }

  public changeMenuItemName(
    restaurantId: string,
    menuItemId: string,
    newName: string,
  ): Observable<unknown> {
    return this._http.put<unknown>(
      `${environment.apiUrl}/Restaurants/${restaurantId}/menu-items/${menuItemId}/name`,
      { name: newName },
    );
  }

  public changeMenuItemDescription(
    restaurantId: string,
    menuItemId: string,
    newDescription: string,
  ): Observable<unknown> {
    return this._http.put<unknown>(
      `${environment.apiUrl}/Restaurants/${restaurantId}/menu-items/${menuItemId}/description`,
      { description: newDescription },
    );
  }

  public changeMenuItemPrice(
    restaurantId: string,
    menuItemId: string,
    price: IMoney,
  ): Observable<unknown> {
    return this._http.put<unknown>(
      `${environment.apiUrl}/Restaurants/${restaurantId}/menu-items/${menuItemId}/price`,
      price,
    );
  }

  public changeName(id: string, newName: string): Observable<unknown> {
    return this._http.patch<unknown>(`${environment.apiUrl}/Restaurants/${id}/name`, { name: newName });
  }

  public changeDescription(id: string, newDescription: string): Observable<unknown> {
    return this._http.patch<unknown>(`${environment.apiUrl}/Restaurants/${id}/description`, {
      description: newDescription,
    });
  }

  public changeMinPrice(id: string, price: IMoney): Observable<unknown> {
    return this._http.patch<unknown>(`${environment.apiUrl}/Restaurants/${id}/minimal-order-price`, price);
  }

  public changeSchedule(id: string, schedule: IOpeningWindow[]): Observable<unknown> {
    return this._http.patch<unknown>(`${environment.apiUrl}/Restaurants/${id}/schedule`, { schedule });
  }

  public activate(id: string): Observable<unknown> {
    return this._http.post<unknown>(`${environment.apiUrl}/Restaurants/${id}/activate`, null);
  }

  public deactivate(id: string): Observable<unknown> {
    return this._http.post<unknown>(`${environment.apiUrl}/Restaurants/${id}/deactivate`, null);
  }
}
