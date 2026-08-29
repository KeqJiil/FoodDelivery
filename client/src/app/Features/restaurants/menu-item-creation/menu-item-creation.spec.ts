import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuItemCreation } from './menu-item-creation';

describe('MenuItemCreation', () => {
  let component: MenuItemCreation;
  let fixture: ComponentFixture<MenuItemCreation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuItemCreation],
    }).compileComponents();

    fixture = TestBed.createComponent(MenuItemCreation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
