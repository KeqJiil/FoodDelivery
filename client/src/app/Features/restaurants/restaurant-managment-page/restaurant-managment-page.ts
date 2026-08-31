import { ChangeDetectionStrategy, Component, inject, type Signal } from '@angular/core';
import type { IRestaurantDetails } from '../models/IRestaurantDetails';
import { RestaurantsService } from '../restaurants-service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { map, type Observable } from 'rxjs';
import { RestaurantManagment } from '../restaurant-managment/restaurant-managment';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-restaurant-managment-page',
  imports: [RestaurantManagment, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './restaurant-managment-page.html',
  styleUrl: './restaurant-managment-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RestaurantManagmentPage {
  private readonly _route: ActivatedRoute = inject(ActivatedRoute);
  // eslint-disable-next-line @typescript-eslint/member-ordering
  protected readonly restaurantId: Signal<string> = toSignal(
    this._route.paramMap.pipe(
      map((params): string => {
        const id = params.get('id');

        if (!id) {
          throw new Error('Restaurant ID is missing');
        }

        return id;
      }),
    ),
    { requireSync: true },
  );

  private readonly _restaurantService: RestaurantsService = inject(RestaurantsService);

  // eslint-disable-next-line @typescript-eslint/member-ordering
  public readonly restaurant = rxResource({
    params: (): { id: string } => ({
      id: this.restaurantId(),
    }),

    stream: ({ params }: { params: { id: string } }): Observable<IRestaurantDetails> =>
      this._restaurantService.getRestaurantById(params.id),
  });
}
