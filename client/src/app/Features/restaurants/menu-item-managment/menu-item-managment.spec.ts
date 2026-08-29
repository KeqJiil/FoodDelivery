import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuItemManagment } from './menu-item-managment';

describe('MenuItemManagment', () => {
  let component: MenuItemManagment;
  let fixture: ComponentFixture<MenuItemManagment>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuItemManagment],
    }).compileComponents();

    fixture = TestBed.createComponent(MenuItemManagment);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
