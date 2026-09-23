import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  // Generar un correlation ID único para observabilidad y auditoría extrema
  const correlationId = crypto.randomUUID();

  let clonedRequest = req.clone({
    headers: req.headers.set('X-Correlation-ID', correlationId)
  });

  if (token) {
    clonedRequest = clonedRequest.clone({
      headers: clonedRequest.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      console.warn(`[AuthInterceptor] Error ${error.status} capturado en petición a: ${req.url}`);
      // Si la API responde con 401 Unauthorized (token expirado o inválido) y el usuario ya estaba autenticado,
      // cerrar la sesión y redirigir al login con indicador de expiración.
      if (error.status === 401 && authService.isLoggedIn() && !req.url.includes('/auth/login')) {
        console.warn('[AuthInterceptor] Forzando redirección por sesión expirada o inválida (401).');
        authService.logout();
        router.navigate(['/login'], { queryParams: { expired: 'true' } });
      }
      return throwError(() => error);
    })
  );
};
