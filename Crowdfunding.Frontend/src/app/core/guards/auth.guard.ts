import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    // Check role eligibility if specified in route data
    const expectedRoles = route.data['roles'] as Array<string>;
    if (expectedRoles && expectedRoles.length > 0) {
      const userRole = authService.currentUser()?.role;
      if (userRole && expectedRoles.includes(userRole)) {
        return true;
      }
      // Access denied - redirect to landing or appropriate dashboard
      router.navigate(['/']);
      return false;
    }
    return true;
  }

  // Not logged in - redirect to login
  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
