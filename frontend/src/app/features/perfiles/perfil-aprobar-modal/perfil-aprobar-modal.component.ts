import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-perfil-aprobar-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './perfil-aprobar-modal.component.html',
  styleUrls: ['./perfil-aprobar-modal.component.scss']
})
export class PerfilAprobarModalComponent {
  private dialogRef = inject(MatDialogRef<PerfilAprobarModalComponent>);
  private fb = inject(FormBuilder);
  
  cargoNombre: string;
  confirmForm: FormGroup;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { cargoNombre: string }) {
    this.cargoNombre = data.cargoNombre;
    this.confirmForm = this.fb.group({
      comentario: ['']
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    this.dialogRef.close(this.confirmForm.value.comentario || '');
  }
}
