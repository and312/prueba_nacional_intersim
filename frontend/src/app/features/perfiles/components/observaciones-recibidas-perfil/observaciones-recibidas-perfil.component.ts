import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ObservacionPerfil } from '../../models/observacion-perfil.model';
import { EstadoObservacionPerfil } from '../../models/estado-observacion-perfil.enum';
import { EstadoPerfil } from '../../models/estado-perfil.enum';
import { ObservacionesPerfilService } from '../../services/observaciones-perfil.service';

@Component({
  selector: 'app-observaciones-recibidas-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './observaciones-recibidas-perfil.component.html',
  styleUrls: ['./observaciones-recibidas-perfil.component.scss']
})
export class ObservacionesRecibidasPerfilComponent {
  @Input({ required: true }) observaciones: ObservacionPerfil[] = [];
  @Input({ required: true }) userRole!: 'RRHH' | 'AreaSol' | 'Administrador';
  @Input({ required: true }) estadoPerfil!: EstadoPerfil;

  @Output() resolucionCambio = new EventEmitter<{ idObservacionPerfil: number; resuelta: boolean }>();

  private observacionesService = inject(ObservacionesPerfilService);
  
  EstadoObservacionPerfil = EstadoObservacionPerfil;

  puedeResolver(obs: ObservacionPerfil): boolean {
    const esRrhhOAdmin = this.userRole === 'RRHH' || this.userRole === 'Administrador';
    const est = String(this.estadoPerfil || '').toLowerCase();
    const esEstadoApto = this.estadoPerfil === EstadoPerfil.Observada || 
                         this.estadoPerfil === EstadoPerfil.Corregida || 
                         this.estadoPerfil === EstadoPerfil.EnRevisionRRHHPE ||
                         est.includes('obs') ||
                         est.includes('correg') ||
                         est.includes('rev');
    return esRrhhOAdmin && esEstadoApto;
  }

  marcarComoResuelta(obs: ObservacionPerfil, event: Event): void {
    const target = event.target as HTMLInputElement;

    if (!this.puedeResolver(obs)) {
      target.checked = !target.checked;
      return;
    }

    const isChecked = target.checked;

    // Emisión optimista inmediata (0ms) para que los Signals reactivos de la cabecera respondan al instante
    this.resolucionCambio.emit({ idObservacionPerfil: obs.idObservacionPerfil, resuelta: isChecked });

    if (isChecked) {
      this.observacionesService.resolverObservacionPerfil(obs.idObservacionPerfil).subscribe({
        error: (err) => {
          alert('Error al resolver la observación: ' + (err?.message || err));
          target.checked = false;
          // Rollback en memoria si falla la API
          this.resolucionCambio.emit({ idObservacionPerfil: obs.idObservacionPerfil, resuelta: false });
        }
      });
    } else {
      this.observacionesService.reabrirObservacionPerfil(obs.idObservacionPerfil).subscribe({
        error: (err) => {
          alert('Error al reabrir la observación: ' + (err?.message || err));
          target.checked = true;
          // Rollback en memoria si falla la API
          this.resolucionCambio.emit({ idObservacionPerfil: obs.idObservacionPerfil, resuelta: true });
        }
      });
    }
  }
}
