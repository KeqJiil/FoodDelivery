import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RestaurantsService } from './restaurants-service';
import { environment } from '@/environments/environment';
import type { IRestaurantsList } from './models/IRestaurantsList';
import { Status, type IRestaurantDetails } from './models/IRestaurantDetails';
import type { IRestaurantCreation } from './models/IRestaurantCreations';
import type { ICreatedResource } from '@/app/Shared/models/ICreatedResource';

describe('RestaurantsService', (): void => {
  let service: RestaurantsService;
  let httpMock: HttpTestingController;

  beforeEach((): void => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(RestaurantsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach((): void => {
    httpMock.verify();
  });

  it('get restaurants list test', (): void => {
    const fakeResponse: IRestaurantsList = {
      restaurants: [],
      page: 1,
      pageSize: 10,
      hasNextPage: false,
      hasPreviousPage: false,
    };

    service.getRestaurantsList({ pageSize: 10, currentPage: 1 }).subscribe((result): void => {
      expect(result).toEqual(fakeResponse);
    });

    const req = httpMock.expectOne(
      (r): boolean =>
        r.url === `${environment.apiUrl}/Restaurants` && r.params.get('pageSize') === '10',
    );
    expect(req.request.method).toBe('GET');

    req.flush(fakeResponse);
  });

  it('get restaurant by id', (): void => {
    const fakeResponse: IRestaurantDetails = {
      id: '123',
      name: 'asdadasd',
      description: 'description description',
      minimalOrderPrice: { amount: 123, currency: 1 },
      status: Status.Active,
      openingWindows: [],
      menuItems: [],
    };

    service.getRestaurantById('123').subscribe((result): void => {
      expect(result).toEqual(fakeResponse);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Restaurants/123`);
    expect(req.request.method).toBe('GET');

    req.flush(fakeResponse);
  });

  it('create a restaurant', (): void => {
    const fakeReq: IRestaurantCreation = {
      name: 'new name',
      description: 'new description for restaurant',
      amount: 12,
      currency: 1,
      schedules: [],
    };
    const fakeRes: ICreatedResource = { id: '123' };

    service.createRestaurant(fakeReq).subscribe((result): void => {
      expect(result).toEqual(fakeRes);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/Restaurants`);
    expect(req.request.method).toBe('POST');

    req.flush(fakeRes);
  });
});
