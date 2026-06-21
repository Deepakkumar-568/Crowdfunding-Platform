import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  imports: [RouterLink, FormsModule],
  templateUrl: './reset.html',
  styleUrl: '../login/login.scss'
})
export class ResetPassword {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  token = '';
  newPassword = '';
  errorMessage = signal('');
  successMessage = signal('');
  loading = signal(false);

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.email || !this.token || !this.newPassword) return;

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const payload = {
      email: this.email,
      token: this.token,
      newPassword: this.newPassword
    };

    this.authService.resetPassword(payload).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        this.successMessage.set(res.message || 'Password updated successfully!');
        setTimeout(() => this.router.navigate(['/auth/login']), 3000);
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Password reset failed. Ensure token and email are valid.');
      }
    });
  }
}
