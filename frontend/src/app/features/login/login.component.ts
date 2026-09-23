import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { GlobalLoadingService } from '../../core/services/global-loading.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  authService = inject(AuthService);
  private apiService = inject(ApiService);
  private route = inject(ActivatedRoute);
  private globalLoadingService = inject(GlobalLoadingService);
  router = inject(Router);

  ngOnInit(): void {
    // Limpiar cualquier sesión residual o bloqueo al entrar al login
    this.authService.logout();
    this.authService.unlockSession();

    this.route.queryParams.subscribe(params => {
      if (params['expired'] === 'true') {
        this.errorMessage.set('Su sesión ha expirado o es inválida. Por favor, inicie sesión nuevamente.');
      }
    });
  }

  loginForm = new FormGroup({
    correo: new FormControl('', [Validators.required, Validators.email]),
    contrasena: new FormControl('', [Validators.required, Validators.minLength(6)])
  });

  forgotPasswordForm = new FormGroup({
    correo: new FormControl('', [Validators.required, Validators.email])
  });

  isLoading = signal(false);
  errorMessage = signal('');
  hidePassword = signal(true);

  // Estados del flujo de Olvido de Contraseña
  showForgotPassword = signal(false);
  forgotLoading = signal(false);
  forgotSuccess = signal(false);
  forgotError = signal('');

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const { correo, contrasena } = this.loginForm.value;

    this.authService.login(correo!, contrasena!).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.globalLoadingService.show(true); // Mostrar splash screen inmediatamente
        this.router.navigate(['/dashboard']).then(() => {
          this.globalLoadingService.hide();
        });
      },
      error: (err) => {
        this.isLoading.set(false);
        const msg = err.error?.message || 'Credenciales inválidas. Por favor, intente nuevamente.';
        this.errorMessage.set(msg);
      }
    });
  }

  toggleForgotPassword(show: boolean): void {
    this.showForgotPassword.set(show);
    this.forgotSuccess.set(false);
    this.forgotError.set('');
    this.forgotPasswordForm.reset();
  }

  onForgotPasswordSubmit(): void {
    if (this.forgotPasswordForm.invalid) return;

    this.forgotLoading.set(true);
    this.forgotError.set('');
    this.forgotSuccess.set(false);

    const correo = this.forgotPasswordForm.value.correo!;

    this.apiService.forgotPassword(correo).subscribe({
      next: () => {
        this.forgotLoading.set(false);
        this.forgotSuccess.set(true);
      },
      error: (err) => {
        this.forgotLoading.set(false);
        this.forgotError.set(err.error?.message || 'El correo electrónico ingresado no está registrado.');
      }
    });
  }
}
