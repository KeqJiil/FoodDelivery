import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderCreation } from './order-creation';

describe('OrderCreation', () => {
  let component: OrderCreation;
  let fixture: ComponentFixture<OrderCreation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderCreation],
    }).compileComponents();

    fixture = TestBed.createComponent(OrderCreation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
