/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { RestaurantsList } from './restaurants-list';
import { Status } from '../models/IRestaurantDetails';

describe('RestaurantsList', (): void => {
  let component: RestaurantsList;
  let fixture: ComponentFixture<RestaurantsList>;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [RestaurantsList],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantsList);
    component = fixture.componentInstance;
  });

  it('should create', (): void => {
    fixture.componentRef.setInput('restaurantsList', []);
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should be empty', (): void => {
    fixture.componentRef.setInput('restaurantsList', []);
    fixture.detectChanges();

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).toContain('No restaurants found.');
  });

  it('should not be empty', (): void => {
    fixture.componentRef.setInput('restaurantsList', [
      {
        id: '123123',
        name: 'fakeName',
        description: '',
        minimalOrderPrice: { amount: 123, currency: 1 },
        status: Status.Active,
        openingWindows: [],
        menuItems: [],
      },
    ]);
    fixture.detectChanges();

    // eslint-disable-next-line @typescript-eslint/no-unsafe-call, @typescript-eslint/no-unsafe-member-access
    const link: HTMLAnchorElement = fixture.nativeElement.querySelector('a');

    const el: HTMLElement = fixture.nativeElement;
    expect(el.textContent).not.toContain('No restaurants found.');
    expect(el.textContent).toContain('fakeName');
    expect(link.getAttribute('href')).toContain('123123');
  });
});
