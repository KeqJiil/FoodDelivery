import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RestaurantsPage } from './restaurants-page';

describe('RestaurantsPage', () => {
  let component: RestaurantsPage;
  let fixture: ComponentFixture<RestaurantsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RestaurantsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(RestaurantsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
