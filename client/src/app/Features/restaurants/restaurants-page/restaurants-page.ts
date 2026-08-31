import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RestaurantsService, type IRestaurantListRequest } from '../restaurants-service';
import type { IRestaurantsList } from '../models/IRestaurantsList';
import { RestaurantsList } from '../restaurants-list/restaurants-list';
import { rxResource } from '@angular/core/rxjs-interop';
import { Pagination, PaginationDefaultValues } from '@/app/Shared/components/pagination/pagination';
import { type Observable } from 'rxjs';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-restaurants-page',
  imports: [RestaurantsList, Pagination, RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './restaurants-page.html',
  styleUrl: './restaurants-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantsPage {
  private readonly _route: ActivatedRoute = inject(ActivatedRoute);
  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

   // eslint-disable-next-line @typescript-eslint/member-ordering
  public readonly requestParams = signal<IRestaurantListRequest>({
    currentPage: (Number(this._route.snapshot.queryParamMap.get('page')) || 1),
    pageSize: this.readStoredPageSize(),
  });

  // eslint-disable-next-line @typescript-eslint/member-ordering
  public readonly restaurantsResource = rxResource({
    params: this.requestParams,
    stream: ({ params }: { params : IRestaurantListRequest }): Observable<IRestaurantsList> => this._restaurantService.getRestaurantsList(params),
  });


  public onNextPageReq(req: IRestaurantListRequest): void {
    this.requestParams.set(req);
  }

  private readStoredPageSize(): number {
    const raw = localStorage.getItem('pageSize');
    const parsed = Number(raw);
    return raw !== null && Number.isInteger(parsed) && parsed > 0
      ? parsed
      : PaginationDefaultValues.pageSize;
  }
}
