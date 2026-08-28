import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';

import { RestaurantDetails } from './restaurant-details';

describe('RestaurantDetails', (): void => {
  let component: RestaurantDetails;
  let fixture: ComponentFixture<RestaurantDetails>;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [RestaurantDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', (): void => {
    expect(component).toBeTruthy();
  });
});
