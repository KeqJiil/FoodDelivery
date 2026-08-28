import { type ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AppRoutes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';

// eslint-disable-next-line @typescript-eslint/naming-convention
export const appConfig: ApplicationConfig = {
  providers: [provideBrowserGlobalErrorListeners(), provideRouter(AppRoutes), provideHttpClient()],
};
