import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { SeccionPerfil, CampoPerfil } from '../../models/perfil.model';

@Component({
  selector: 'app-seccion-perfil',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './seccion-perfil.component.html',
  styleUrls: ['./seccion-perfil.component.scss']
})
export class SeccionPerfilComponent {
  @Input({ required: true }) seccion!: SeccionPerfil;
  @Input({ required: true }) modoEdicion!: boolean;
  @Input({ required: true }) parentForm!: FormGroup;

  esCampoEditable(campo: CampoPerfil): boolean {
    return this.modoEdicion && campo.editable && campo.origen !== 'SOLICITUD_VACANTE';
  }

  esLista(valor: any): boolean {
    return Array.isArray(valor) && this.seccion.id !== 11; // Avoid treating matrix array of objects as simple string list
  }

  comoLista(valor: any): string[] {
    return Array.isArray(valor) ? valor : [];
  }

  obtenerFilasCriterio(valor: any): any[] {
    if (!Array.isArray(valor)) return [];
    return valor.filter((item: any) => item && String(item.criterio || '').toLowerCase().trim() !== 'total');
  }

  obtenerTotalPonderacion(valor: any): number {
    if (!Array.isArray(valor)) return 0;
    const filas = valor.filter((item: any) => item && String(item.criterio || '').toLowerCase().trim() !== 'total');
    return filas.reduce((acc: number, item: any) => acc + (Number(item.ponderacion) || 0), 0);
  }

  agregarFilaCriterio(campoId: string): void {
    const control = this.parentForm.get(campoId);
    if (!control) return;

    const valorActual = Array.isArray(control.value) ? control.value.map((x: any) => ({ ...x })) : [];
    const filas = valorActual.filter((item: any) => item && String(item.criterio || '').toLowerCase().trim() !== 'total');
    filas.push({ criterio: '', ponderacion: 0 });

    control.setValue(filas);
    control.markAsDirty();
    control.updateValueAndValidity();
  }

  eliminarFilaCriterio(campoId: string, index: number): void {
    const control = this.parentForm.get(campoId);
    if (!control) return;

    const valorActual = Array.isArray(control.value) ? control.value.map((x: any) => ({ ...x })) : [];
    const filas = valorActual.filter((item: any) => item && String(item.criterio || '').toLowerCase().trim() !== 'total');
    if (index >= 0 && index < filas.length) {
      filas.splice(index, 1);
      control.setValue(filas);
      control.markAsDirty();
      control.updateValueAndValidity();
    }
  }

  actualizarValorCriterio(campoId: string, index: number, nuevoCriterio: string): void {
    const control = this.parentForm.get(campoId);
    if (!control) return;
    
    const valorActual = Array.isArray(control.value) ? control.value.map((x: any) => ({ ...x })) : [];
    let originalIndex = -1;
    let editIndex = 0;
    for (let i = 0; i < valorActual.length; i++) {
      if (String(valorActual[i]?.criterio || '').toLowerCase().trim() !== 'total') {
        if (editIndex === index) {
          originalIndex = i;
          break;
        }
        editIndex++;
      }
    }
    
    if (originalIndex !== -1) {
      valorActual[originalIndex].criterio = nuevoCriterio;
      control.setValue(valorActual);
      control.markAsDirty();
      control.updateValueAndValidity();
    }
  }

  actualizarValorPonderacion(campoId: string, index: number, nuevaPonderacion: number): void {
    const control = this.parentForm.get(campoId);
    if (!control) return;
    
    const valorActual = Array.isArray(control.value) ? control.value.map((x: any) => ({ ...x })) : [];
    let originalIndex = -1;
    let editIndex = 0;
    for (let i = 0; i < valorActual.length; i++) {
      if (String(valorActual[i]?.criterio || '').toLowerCase().trim() !== 'total') {
        if (editIndex === index) {
          originalIndex = i;
          break;
        }
        editIndex++;
      }
    }
    
    if (originalIndex !== -1) {
      valorActual[originalIndex].ponderacion = nuevaPonderacion;
      control.setValue(valorActual);
      control.markAsDirty();
      control.updateValueAndValidity();
    }
  }
}
