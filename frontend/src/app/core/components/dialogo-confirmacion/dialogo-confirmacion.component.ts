import { Component, Inject, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { ConfiguracionDialogoConfirmacion } from './configuracion-dialogo-confirmacion.model';

@Component({
  selector: 'app-dialogo-confirmacion',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './dialogo-confirmacion.component.html',
  styleUrls: ['./dialogo-confirmacion.component.scss']
})
export class DialogoConfirmacionComponent {
  private dialogRef = inject(MatDialogRef<DialogoConfirmacionComponent>);
  
  confirmado = signal(false);
  isLoading = signal(false);

  constructor(@Inject(MAT_DIALOG_DATA) public data: ConfiguracionDialogoConfirmacion) {
    // Al abrir o instanciar, el checkbox inicia desmarcado (confirmado = false)
    this.confirmado.set(false);
  }

  esValido = computed(() => {
    if (this.data.requiereCheckbox) {
      return this.confirmado();
    }
    return true;
  });

  onToggleCheckbox(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.confirmado.set(input.checked);
  }

  onCancelar(): void {
    if (this.isLoading()) return;
    this.dialogRef.close(false);
  }

  onConfirmar(): void {
    if (!this.esValido() || this.isLoading()) return;

    if (this.data.onConfirmar) {
      this.isLoading.set(true);
      const result = this.data.onConfirmar();

      if (result instanceof Observable) {
        result.pipe(
          finalize(() => {
            this.isLoading.set(false);
          })
        ).subscribe({
          next: () => {
            this.dialogRef.close(true);
          },
          error: (err) => {
            console.error('Error al procesar la confirmacion del dialogo:', err);
          }
        });
      } else if (result instanceof Promise) {
        result.then(() => {
          this.isLoading.set(false);
          this.dialogRef.close(true);
        }).catch((err) => {
          this.isLoading.set(false);
          console.error('Error al procesar la confirmacion del dialogo:', err);
        });
      } else {
        this.isLoading.set(false);
        this.dialogRef.close(true);
      }
    } else {
      this.dialogRef.close(true);
    }
  }
}
