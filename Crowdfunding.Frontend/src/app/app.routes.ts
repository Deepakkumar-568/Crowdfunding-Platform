import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { Landing } from './features/landing/landing';
import { StartupList } from './features/startups/list/list';
import { StartupDetails } from './features/startups/details/details';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { ForgotPassword } from './features/auth/forgot/forgot';
import { ResetPassword } from './features/auth/reset/reset';
import { FounderDashboardComponent } from './features/dashboards/founder/founder';
import { InvestorDashboardComponent } from './features/dashboards/investor/investor';
import { AdminDashboardComponent } from './features/dashboards/admin/admin';

export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'startups', component: StartupList },
  { path: 'startups/:id', component: StartupDetails },
  { path: 'auth/login', component: Login },
  { path: 'auth/register', component: Register },
  { path: 'auth/forgot', component: ForgotPassword },
  { path: 'auth/reset', component: ResetPassword },
  
  // Dashboard routes protected by guards
  { 
    path: 'dashboard/founder', 
    component: FounderDashboardComponent, 
    canActivate: [authGuard], 
    data: { roles: ['Founder'] } 
  },
  { 
    path: 'dashboard/investor', 
    component: InvestorDashboardComponent, 
    canActivate: [authGuard], 
    data: { roles: ['Investor'] } 
  },
  { 
    path: 'dashboard/admin', 
    component: AdminDashboardComponent, 
    canActivate: [authGuard], 
    data: { roles: ['Admin'] } 
  },

  { path: '**', redirectTo: '' }
];
