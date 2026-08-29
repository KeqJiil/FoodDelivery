import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestaurantCreation } from './restaurant-creation';

describe('RestaurantCreation', () => {
  let component: RestaurantCreation;
  let fixture: ComponentFixture<RestaurantCreation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestaurantCreation],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantCreation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
