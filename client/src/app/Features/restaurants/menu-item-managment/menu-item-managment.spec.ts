/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import type { IMenuItem } from '../models/IRestaurantDetails';
import { Currency } from '@/app/Shared/enums/currency';
import { environment } from '@/environments/environment';
import { MenuItemManagment } from './menu-item-managment';
import { TestBed, type ComponentFixture } from '@angular/core/testing';
import { createFakeWritable } from '@/app/Shared/services/optimistic.service';

describe('MenuItemManagment', (): void => {
  let component: MenuItemManagment;
  let fixture: ComponentFixture<MenuItemManagment>;
  let httpMock: HttpTestingController;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [MenuItemManagment],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(MenuItemManagment);
    httpMock = TestBed.inject(HttpTestingController);
    component = fixture.componentInstance;
  });

  it('should create', (): void => {
    expect(component).toBeTruthy();
  });

  it('should change name', (): void => {
    const fakeMenuItem: IMenuItem = {
      name: 'sushi',
      description: 'tasty sushi',
      id: 'sushi-123',
      price: { amount: 11, currency: Currency.Usd },
    };
    const menuItemSignal = createFakeWritable(fakeMenuItem);

    fixture.componentRef.setInput('menuItem', menuItemSignal);
    fixture.componentRef.setInput('restaurantId', 'rest-123');
    fixture.detectChanges();

    const nameInput: HTMLInputElement = fixture.nativeElement.querySelector('input');
    nameInput.value = 'New Name';

    const btn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[aria-label="Save name"]',
    );
    btn.click();

    expect(menuItemSignal.value().name).toBe('New Name');

    const req = httpMock.expectOne(
      `${environment.apiUrl}/Restaurants/rest-123/menu-items/sushi-123/name`,
    );
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'New Name' });

    req.flush(null);
  });

  it('should change description', (): void => {
    const fakeMenuItem: IMenuItem = {
      name: 'sushi',
      description: 'tasty sushi',
      id: 'sushi-123',
      price: { amount: 11, currency: Currency.Usd },
    };
    const menuItemSignal = createFakeWritable(fakeMenuItem);

    fixture.componentRef.setInput('menuItem', menuItemSignal);
    fixture.componentRef.setInput('restaurantId', 'rest-123');
    fixture.detectChanges();

    const descriptionInput: HTMLInputElement = fixture.nativeElement.querySelector('textarea');
    descriptionInput.value = 'New Description';

    const btn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[aria-label="Save description"]',
    );
    btn.click();

    expect(menuItemSignal.value().description).toBe('New Description');

    const req = httpMock.expectOne(
      `${environment.apiUrl}/Restaurants/rest-123/menu-items/sushi-123/description`,
    );
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ description: 'New Description' });

    req.flush(null);
  });

  it('should change price', (): void => {
    const fakeMenuItem: IMenuItem = {
      name: 'sushi',
      description: 'tasty sushi',
      id: 'sushi-123',
      price: { amount: 11, currency: Currency.Usd },
    };
    const menuItemSignal = createFakeWritable(fakeMenuItem);

    fixture.componentRef.setInput('menuItem', menuItemSignal);
    fixture.componentRef.setInput('restaurantId', 'rest-123');
    fixture.detectChanges();

    component.changePrice(Currency.Usd, 20);

    expect(menuItemSignal.value().price).toEqual({ currency: Currency.Usd, amount: 20 });

    const req = httpMock.expectOne(
      `${environment.apiUrl}/Restaurants/rest-123/menu-items/sushi-123/price`,
    );
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ currency: Currency.Usd, amount: 20 });

    req.flush(null);
  });
});
