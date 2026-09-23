import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    // Verificar si la ruta requiere roles específicos
    const expectedRoles = route.data['roles'] as Array<string>;
    if (expectedRoles && expectedRoles.length > 0) {
      const userRole = authService.getUserRole();
      if (!expectedRoles.includes(userRole)) {
        router.navigate(['/403']);
        return false;
      }
    }
    return true;
  }

  router.navigate(['/login']);
  return false;
};
