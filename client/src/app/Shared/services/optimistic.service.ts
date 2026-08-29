import type { HttpErrorResponse } from "@angular/common/http";
import type { Observable } from "rxjs";
import type { ResourceRef } from "@angular/core";

// eslint-disable-next-line @typescript-eslint/no-extraneous-class
export class OptimisticService {
  public static optimisticUpdate<T, TReturn>(
    input: IOptimisticServiceData<T, TReturn>
  ): void {
    const previous = input.signal.value();
    input.signal.update(input.apply);

    input.request.subscribe({
      error: (_: HttpErrorResponse): void => {
        input.signal.set(previous);
      },
    });
  }
}

export interface IOptimisticServiceData<T, TReturn> {
  apply: (r: TReturn) => TReturn,
  request: Observable<T>,
  signal: ResourceRef<TReturn>,
};