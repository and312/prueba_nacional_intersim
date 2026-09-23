import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EstadoPerfil } from '../../models/estado-perfil.enum';

export interface PasoProceso {
  numero: number;
  nombre: string;
  estado: 'completado' | 'actual' | 'pendiente';
}

@Component({
  selector: 'app-estado-proceso-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './estado-proceso-perfil.component.html',
  styleUrls: ['./estado-proceso-perfil.component.scss']
})
export class EstadoProcesoPerfilComponent implements OnChanges {
  @Input({ required: true }) estadoPerfil!: EstadoPerfil;
  @Input() aprobadoPorArea = false;

  pasos: PasoProceso[] = [];

  ngOnChanges(): void {
    this.calcularPasos();
  }

  private calcularPasos(): void {
    const est = this.estadoPerfil;

    // Configuración inicial de los 5 pasos
    this.pasos = [
      { numero: 1, nombre: 'Solicitud aprobada', estado: 'pendiente' },
      { numero: 2, nombre: 'Perfil generado por agente', estado: 'pendiente' },
      { numero: 3, nombre: 'Revisión RRHH', estado: 'pendiente' },
      { numero: 4, nombre: 'Validación Área Solicitante', estado: 'pendiente' },
      { numero: 5, nombre: 'Aprobación final RRHH', estado: 'pendiente' }
    ];

    if (est === EstadoPerfil.SolicitudAprobada) {
      this.pasos[0].estado = 'actual';
    } else if (est === EstadoPerfil.PendienteGeneracionPerfil) {
      this.pasos[0].estado = 'completado';
      this.pasos[1].estado = 'actual';
    } else if (est === EstadoPerfil.EnRevisionRRHHPE || est === EstadoPerfil.ResumenEjecutivoGenerado) {
      this.pasos[0].estado = 'completado';
      this.pasos[1].estado = 'completado';
      if (this.aprobadoPorArea) {
        this.pasos[2].estado = 'completado';
        this.pasos[3].estado = 'completado';
        this.pasos[4].estado = 'actual';
      } else {
        this.pasos[2].estado = 'actual';
      }
    } else if (est === EstadoPerfil.EnRevisionAreaSol || est === EstadoPerfil.Observada || est === EstadoPerfil.Corregida) {
      this.pasos[0].estado = 'completado';
      this.pasos[1].estado = 'completado';
      this.pasos[2].estado = 'completado';
      this.pasos[3].estado = 'actual';
    } else if (est === EstadoPerfil.Aprobada) {
      this.pasos[0].estado = 'completado';
      this.pasos[1].estado = 'completado';
      this.pasos[2].estado = 'completado';
      this.pasos[3].estado = 'completado';
      this.pasos[4].estado = 'actual';
    } else if (est === EstadoPerfil.PerfilAprobadoFinal) {
      this.pasos.forEach(p => p.estado = 'completado');
    }
  }
}
