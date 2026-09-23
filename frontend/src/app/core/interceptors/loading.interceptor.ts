import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { GlobalLoadingService } from '../services/global-loading.service';
import { ComponentLoadingService } from '../services/component-loading.service';
import { finalize } from 'rxjs/operators';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const globalLoadingService = inject(GlobalLoadingService);
  const componentLoadingService = inject(ComponentLoadingService);

  const url = req.url.toLowerCase();
  let loaderKey: string | null = null;

  // Determinar si es una carga global (ej. login o inicialización)
  const isGlobal = url.includes('/auth/login') || url.includes('/auth/me') || url.includes('/initialize');

  if (isGlobal) {
    globalLoadingService.show();
  } else {
    // Mapeo automático de URL a loader local por componente
    if (url.includes('/solicitudes')) {
      loaderKey = 'solicitudes';
    } else if (url.includes('/postulantes')) {
      loaderKey = 'postulantes';
    } else if (url.includes('/vacantes')) {
      loaderKey = 'vacantes';
    } else if (url.includes('/usuarios') || url.includes('/roles') || url.includes('/areas')) {
      loaderKey = 'usuarios';
    } else if (url.includes('/catalogos')) {
      loaderKey = 'catalogos';
    } else if (url.includes('/dashboard') || url.includes('/perfiles') || url.includes('/integraciones')) {
      loaderKey = 'dashboard';
    }

    if (loaderKey) {
      componentLoadingService.show(loaderKey);
    }
  }

  return next(req).pipe(
    finalize(() => {
      if (isGlobal) {
        globalLoadingService.hide();
      } else if (loaderKey) {
        componentLoadingService.hide(loaderKey);
      }
    })
  );
};
