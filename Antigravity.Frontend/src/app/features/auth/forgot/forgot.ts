import { Component, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [RouterLink, FormsModule],
  templateUrl: './forgot.html',
  styleUrl: '../login/login.scss'
})
export class ForgotPassword {
  private authService = inject(AuthService);
  
  email = '';
  errorMessage = signal('');
  successMessage = signal('');
  loading = signal(false);

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.email) return;

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.authService.forgotPassword(this.email).subscribe({
      next: (res: any) => {
        this.loading.set(false);
        this.successMessage.set(res.message || 'Password reset code has been sent. Check your notifications/email.');
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(err.error?.message || 'Email address not found.');
      }
    });
  }
}
