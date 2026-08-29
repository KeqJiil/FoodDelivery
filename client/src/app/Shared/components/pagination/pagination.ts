import { Component, input, output } from '@angular/core';
import type { IRestaurantListRequest } from '@/app/Features/restaurants/restaurants-service';

@Component({
  selector: 'app-pagination',
  imports: [],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
})
export class Pagination {
  public readonly page = input<number>(PaginationDefaultValues.page);
  public readonly pageSize = input<number>(PaginationDefaultValues.pageSize);
  public readonly hasNextPage = input<boolean>(false);
  public readonly hasPreviousPage = input<boolean>(false);


  public outputNextPageReq = output<IRestaurantListRequest>();

  public onNextPage(): void {
    if (!this.hasNextPage()) return;
    this.outputNextPageReq.emit({ pageSize: this.pageSize(), currentPage: this.page() + 1 });
  }

  public onPreviousPage(): void {
    if (!this.hasPreviousPage()) return;
    this.outputNextPageReq.emit({ pageSize: this.pageSize(), currentPage: this.page() - 1 });
  }

  public onPageSizeChange(newSize: number): void {
    localStorage.setItem("pageSize", newSize.toString());
    this.outputNextPageReq.emit({ pageSize: newSize, currentPage: 1 });
  }
}

export const PaginationDefaultValues = {
  page: 1,
  pageSize: 10
};