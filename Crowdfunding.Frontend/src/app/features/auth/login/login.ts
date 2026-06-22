import { Component, OnInit, signal, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = '';
  password = '';
  errorMessage = signal('');
  loading = signal(false);
  returnUrl = '/';

  ngOnInit() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    // Redirect if already logged in
    if (this.authService.isLoggedIn()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.email || !this.password) return;

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.email, this.password).subscribe({
      next: (res) => {
        this.loading.set(false);
        // Route to dashboard based on role or returnUrl
        if (this.returnUrl !== '/') {
          this.router.navigateByUrl(this.returnUrl);
        } else {
          this.redirectDashboard(res.user.role);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Invalid email or password.');
      }
    });
  }

  private redirectDashboard(role: string) {
    if (role === 'Founder') this.router.navigate(['/dashboard/founder']);
    else if (role === 'Investor') this.router.navigate(['/dashboard/investor']);
    else if (role === 'Admin') this.router.navigate(['/dashboard/admin']);
    else this.router.navigate(['/']);
  }
}
