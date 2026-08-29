import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestaurantScheduleManagment } from './restaurant-schedule-managment';

describe('RestaurantScheduleManagment', () => {
  let component: RestaurantScheduleManagment;
  let fixture: ComponentFixture<RestaurantScheduleManagment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestaurantScheduleManagment],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantScheduleManagment);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
