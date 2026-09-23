import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormGroup, FormControl, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-reset-password',
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
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  private apiService = inject(ApiService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  userId: number | null = null;
  token: string = '';

  // Estados reactivos
  isValidating = signal(true);
  isTokenValid = signal(false);
  isSubmitting = signal(false);
  resetSuccess = signal(false);
  validationError = signal('');
  submitError = signal('');

  // Controladores de visibilidad
  hideNewPassword = signal(true);
  hideConfirmPassword = signal(true);

  resetForm = new FormGroup({
    nuevaContrasena: new FormControl('', [
      Validators.required,
      Validators.minLength(8),
      this.passwordStrengthValidator()
    ]),
    confirmarContrasena: new FormControl('', [Validators.required])
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    // Leer parámetros de query string (?userId=x&token=y)
    this.route.queryParams.subscribe(params => {
      const uidStr = params['userId'];
      const tk = params['token'];

      if (uidStr && tk) {
        this.userId = parseInt(uidStr, 10);
        this.token = tk;
        this.validateToken();
      } else {
        this.isValidating.set(false);
        this.isTokenValid.set(false);
        this.validationError.set('Parámetros de restablecimiento de contraseña incompletos en el enlace.');
      }
    });
  }

  private validateToken(): void {
    if (this.userId === null || !this.token) return;

    this.apiService.validateResetToken(this.userId, this.token).subscribe({
      next: () => {
        this.isValidating.set(false);
        this.isTokenValid.set(true);
      },
      error: (err) => {
        this.isValidating.set(false);
        this.isTokenValid.set(false);
        this.validationError.set(err.error?.message || 'El enlace de recuperación ha expirado o no es válido.');
      }
    });
  }

  // Validador personalizado de fortaleza de contraseña
  private passwordStrengthValidator() {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const value = control.value || '';
      if (!value) return null;

      const hasUpperCase = /[A-Z]/.test(value);
      const hasLowerCase = /[a-z]/.test(value);
      const hasNumeric = /[0-9]/.test(value);
      const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(value);

      const isValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecial;
      
      if (!isValid) {
        return {
          passwordStrength: {
            hasUpperCase,
            hasLowerCase,
            hasNumeric,
            hasSpecial
          }
        };
      }
      return null;
    };
  }

  // Validador de coincidencia de contraseñas
  private passwordMatchValidator(group: AbstractControl): { [key: string]: any } | null {
    const pass = group.get('nuevaContrasena')?.value;
    const confirmPass = group.get('confirmarContrasena')?.value;
    return pass === confirmPass ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.resetForm.invalid || this.userId === null || !this.token) return;

    this.isSubmitting.set(true);
    this.submitError.set('');

    const nuevaClave = this.resetForm.value.nuevaContrasena!;

    this.apiService.resetPassword(this.userId, this.token, nuevaClave).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.resetSuccess.set(true);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.submitError.set(err.error?.message || 'Ocurrió un error al intentar cambiar la contraseña. Intente de nuevo.');
      }
    });
  }

  get nuevaContrasenaValue(): string {
    return this.resetForm.get('nuevaContrasena')?.value || '';
  }

  get hasMinLength(): boolean {
    return this.nuevaContrasenaValue.length >= 8;
  }

  get hasUpperCase(): boolean {
    return /[A-Z]/.test(this.nuevaContrasenaValue);
  }

  get hasLowerCase(): boolean {
    return /[a-z]/.test(this.nuevaContrasenaValue);
  }

  get hasNumeric(): boolean {
    return /[0-9]/.test(this.nuevaContrasenaValue);
  }

  get hasSpecial(): boolean {
    return /[!@#$%^&*(),.?":{}|<>]/.test(this.nuevaContrasenaValue);
  }

  irAlLogin(): void {
    this.router.navigate(['/login']);
  }
}
