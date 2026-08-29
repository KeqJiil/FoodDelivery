import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestaurantManagment } from './restaurant-managment';

describe('RestaurantManagment', () => {
  let component: RestaurantManagment;
  let fixture: ComponentFixture<RestaurantManagment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestaurantManagment],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantManagment);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
