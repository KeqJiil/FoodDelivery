import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';

import { RestaurantsList } from './restaurants-list';

describe('RestaurantsList', (): void => {
  let component: RestaurantsList;
  let fixture: ComponentFixture<RestaurantsList>;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [RestaurantsList],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', (): void => {
    expect(component).toBeTruthy();
  });
});
