/* eslint-disable @typescript-eslint/no-unsafe-assignment */
/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import { type ComponentFixture, TestBed } from '@angular/core/testing';

import { Pagination } from './pagination';
import type { IRestaurantListRequest } from '@/app/Features/restaurants/restaurants-service';

describe('Pagination', (): void => {
  let component: Pagination;
  let fixture: ComponentFixture<Pagination>;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [Pagination],
    }).compileComponents();

    fixture = TestBed.createComponent(Pagination);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', (): void => {
    expect(component).toBeTruthy();
  });

  it('emits next page request when Next is clicked and hasNextPage is true', (): void => {
    fixture.componentRef.setInput('page', 2);
    fixture.componentRef.setInput('pageSize', 10);
    fixture.componentRef.setInput('hasNextPage', true);
    fixture.detectChanges();

    let emitted: IRestaurantListRequest | undefined;
    component.outputNextPageReq.subscribe((value): void => {
      emitted = value;
    });

    const btn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[aria-label="Next page"]',
    );
    btn.click();

    expect(emitted).toEqual({ pageSize: 10, currentPage: 3 });
  });

  it("doesn't emit if there is no next page", (): void => {
    fixture.componentRef.setInput('page', 2);
    fixture.componentRef.setInput('pageSize', 10);
    fixture.componentRef.setInput('hasNextPage', false);
    fixture.detectChanges();
    
    let emitted: IRestaurantListRequest | undefined;
    component.outputNextPageReq.subscribe((value): void => {
      emitted = value;
    });

    const btn: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[aria-label="Next page"]',
    );
    btn.click();

    expect(emitted).toEqual(undefined);
  });
});
