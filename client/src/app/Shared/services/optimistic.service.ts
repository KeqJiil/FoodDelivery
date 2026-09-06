import type { HttpErrorResponse } from '@angular/common/http';
import type { Observable } from 'rxjs';

// eslint-disable-next-line @typescript-eslint/no-extraneous-class
export class OptimisticService {
  public static optimisticUpdate<T, TReturn>(input: IOptimisticServiceData<T, TReturn>): void {
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
  apply: (r: TReturn) => TReturn;
  request: Observable<T>;
  signal: IWritableValue<TReturn>;
}

export interface IWritableValue<T> {
  value(): T;
  set(value: T): void;
  update(updater: (value: T) => T): void;
}

export function createFakeWritable<T>(initial: T): IWritableValue<T> {
  let current = initial;
  return {
    value: (): T => current,
    set: (v: T): void => {
      current = v;
    },
    update: (fn: (v: T) => T): void => {
      current = fn(current);
    },
  };
}
