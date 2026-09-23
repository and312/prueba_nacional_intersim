import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-perfil-seccion-edit-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule
  ],
  templateUrl: './perfil-seccion-edit-modal.component.html',
  styleUrls: ['./perfil-seccion-edit-modal.component.scss']
})
export class PerfilSeccionEditModalComponent {
  private dialogRef = inject(MatDialogRef<PerfilSeccionEditModalComponent>);
  private fb = inject(FormBuilder);

  numeroSeccion: number;
  nombreSeccion: string;
  editForm!: FormGroup;
  motivoForm!: FormGroup;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { numeroSeccion: number; nombreSeccion: string; contenido: string }) {
    this.numeroSeccion = data.numeroSeccion;
    this.nombreSeccion = data.nombreSeccion;
    this.initForms();
  }

  initForms(): void {
    // Formulario de Motivo (Requerido para bitácora de auditoría)
    this.motivoForm = this.fb.group({
      motivo: ['', Validators.required]
    });

    if (this.numeroSeccion >= 1 && this.numeroSeccion <= 5) {
      // Texto simple
      this.editForm = this.fb.group({
        texto: [this.data.contenido || '', Validators.required]
      });
    } else if (this.numeroSeccion >= 6 && this.numeroSeccion <= 11) {
      // Arreglo JSON
      let items: string[] = [];
      try {
        items = JSON.parse(this.data.contenido);
        if (!Array.isArray(items)) items = [];
      } catch {
        items = [];
      }

      this.editForm = this.fb.group({
        items: this.fb.array(items.map(item => this.fb.control(item, Validators.required)))
      });
    } else if (this.numeroSeccion === 12) {
      // Objeto JSON con Herramientas, KPIs y Riesgos
      let parsedObj: any = {};
      try {
        parsedObj = JSON.parse(this.data.contenido);
      } catch {
        parsedObj = {};
      }

      const herramientas = parsedObj.Herramientas || [];
      const kpis = parsedObj.Kpis || [];
      const riesgos = parsedObj.Riesgos || [];
      const observacionesIA = parsedObj.ObservacionesIA || '';

      this.editForm = this.fb.group({
        herramientas: this.fb.array(herramientas.map((item: string) => this.fb.control(item, Validators.required))),
        kpis: this.fb.array(kpis.map((item: string) => this.fb.control(item, Validators.required))),
        riesgos: this.fb.array(riesgos.map((item: string) => this.fb.control(item, Validators.required))),
        observacionesIA: [observacionesIA]
      });
    }
  }

  // Getters para FormArray
  get itemsFormArray(): FormArray {
    return this.editForm.get('items') as FormArray;
  }

  get herramientasFormArray(): FormArray {
    return this.editForm.get('herramientas') as FormArray;
  }

  get kpisFormArray(): FormArray {
    return this.editForm.get('kpis') as FormArray;
  }

  get riesgosFormArray(): FormArray {
    return this.editForm.get('riesgos') as FormArray;
  }

  // Métodos para agregar/quitar elementos del FormArray
  addItem(arrayName: string = 'items'): void {
    const array = this.editForm.get(arrayName) as FormArray;
    array.push(this.fb.control('', Validators.required));
  }

  removeItem(index: number, arrayName: string = 'items'): void {
    const array = this.editForm.get(arrayName) as FormArray;
    array.removeAt(index);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.editForm.invalid || this.motivoForm.invalid) {
      this.editForm.markAllAsTouched();
      this.motivoForm.markAllAsTouched();
      return;
    }

    let serializedContent = '';
    if (this.numeroSeccion >= 1 && this.numeroSeccion <= 5) {
      serializedContent = this.editForm.value.texto;
    } else if (this.numeroSeccion >= 6 && this.numeroSeccion <= 11) {
      serializedContent = JSON.stringify(this.editForm.value.items);
    } else if (this.numeroSeccion === 12) {
      serializedContent = JSON.stringify({
        Herramientas: this.editForm.value.herramientas,
        Kpis: this.editForm.value.kpis,
        Riesgos: this.editForm.value.riesgos,
        ObservacionesIA: this.editForm.value.observacionesIA
      });
    }

    this.dialogRef.close({
      contenido: serializedContent,
      motivo: this.motivoForm.value.motivo
    });
  }
}
