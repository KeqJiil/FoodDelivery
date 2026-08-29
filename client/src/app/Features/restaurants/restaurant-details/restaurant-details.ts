import { Component, inject, type Signal } from '@angular/core';
import type { IRestaurantDetails } from '../models/IRestaurantDetails';
import { RestaurantsService } from '../restaurants-service';
import { ActivatedRoute } from '@angular/router';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { map, type Observable } from 'rxjs';

@Component({
  selector: 'app-restaurant-details',
  standalone: true,
  imports: [],
  templateUrl: './restaurant-details.html',
  styleUrl: './restaurant-details.css',
})
export class RestaurantDetails {
  private readonly _route: ActivatedRoute = inject(ActivatedRoute);
  private readonly _restaurantId: Signal<string> = toSignal(
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
      id: this._restaurantId(),
    }),

    stream: ({ params }: { params: { id: string } }): Observable<IRestaurantDetails> =>
      this._restaurantService.getRestaurantById(params.id),
  });
}
