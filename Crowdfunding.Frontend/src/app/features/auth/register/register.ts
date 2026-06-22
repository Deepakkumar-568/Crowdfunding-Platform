import { Component, OnInit, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [RouterLink, FormsModule],
  templateUrl: './register.html',
  styleUrl: '../login/login.scss' // Reuses auth layout styling
})
export class Register implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = '';
  email = '';
  password = '';
  role: 'Founder' | 'Investor' = 'Investor';
  errorMessage = signal('');
  loading = signal(false);

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.username || !this.email || !this.password || !this.role) return;

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.register(this.username, this.email, this.password, this.role).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.redirectDashboard(res.user.role);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Registration failed. Check user details.');
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
