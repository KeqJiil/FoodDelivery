import type { IErrorDetails } from "@/app/Shared/models/IErrorDetails";
import type { HttpInterceptorFn, HttpEvent, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { catchError, type Observable, throwError } from "rxjs";

export const ErrorInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<unknown>> => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse): Observable<never> => {
      // eslint-disable-next-line @typescript-eslint/consistent-type-assertions
      const problem = err.error as IErrorDetails;
      const firstFieldError = Object.values<string[]>(problem.errors ?? {})[0]?.[0];
      snackBar.open(problem.detail ?? firstFieldError ?? 'Something went wrong', 'Close', { duration: 4000 });
      return throwError((): HttpErrorResponse => err);
    }),
  );
}

