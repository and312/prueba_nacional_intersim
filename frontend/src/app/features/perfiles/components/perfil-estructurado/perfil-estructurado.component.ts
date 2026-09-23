import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { DetallePerfil } from '../../models/perfil.model';
import { SeccionPerfilComponent } from '../seccion-perfil/seccion-perfil.component';

@Component({
  selector: 'app-perfil-estructurado',
  standalone: true,
  imports: [CommonModule, SeccionPerfilComponent],
  templateUrl: './perfil-estructurado.component.html',
  styleUrls: ['./perfil-estructurado.component.scss']
})
export class PerfilEstructuradoComponent implements OnInit {
  @Input({ required: true }) perfil!: DetallePerfil;
  @Input({ required: true }) modoEdicion!: boolean;
  @Input({ required: true }) parentForm!: FormGroup;

  seccionesAbiertas = signal<Record<number, boolean>>({ 1: true });

  ngOnInit(): void {
    this.seccionesAbiertas.set({ 1: true });
  }

  toggleSeccion(id: number): void {
    const estadoActual = this.seccionesAbiertas();
    this.seccionesAbiertas.set({
      ...estadoActual,
      [id]: !estadoActual[id]
    });
  }

  estaAbierta(id: number): boolean {
    return !!this.seccionesAbiertas()[id];
  }

  tieneCamposSolicitud(seccionId: number): boolean {
    const seccion = this.perfil.secciones.find(s => s.id === seccionId);
    if (!seccion) return false;
    return seccion.campos.some(c => c.origen === 'SOLICITUD_VACANTE');
  }

  tieneCamposModificados(seccionId: number): boolean {
    const seccion = this.perfil.secciones.find(s => s.id === seccionId);
    if (!seccion) return false;
    return seccion.campos.some(c => c.origen === 'AJUSTADO_RRHH');
  }
}
