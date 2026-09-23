import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileStatus } from '../../models/profile-status';
import { PROFILE_STATUS_CONFIGS, ProfileStatusConfig } from '../../constants/profile-status.config';

@Component({
  selector: 'app-profile-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-status-badge.component.html',
  styleUrls: ['./profile-status-badge.component.scss']
})
export class ProfileStatusBadgeComponent {
  @Input({ required: true }) estado!: ProfileStatus;
  @Input() estadoCodigo?: string;
  @Input() rol?: 'RRHH' | 'AreaSol' | 'Administrador';

  get config(): ProfileStatusConfig {
    const isAreaSol = this.rol === 'AreaSol';
    const code = this.estadoCodigo;

    if (code) {
      if (isAreaSol) {
        switch (code) {
          case 'PERF-REV-AREA':
            return {
              label: 'En Revisión Solicitante',
              cssClass: 'badge-solicitante',
              visualType: 'review',
              icon: 'forward_to_inbox'
            };
          case 'PERF-OBS-AREA':
          case 'PERF-COR-RRHH':
            return {
              label: 'Observado por Área',
              cssClass: 'badge-observed',
              visualType: 'observed',
              icon: 'warning'
            };
          case 'PERF-REV-RRHH':
          case 'PERF-RES-GEN':
            return {
              label: 'En Revisión RRHH',
              cssClass: 'badge-review',
              visualType: 'review',
              icon: 'rate_review'
            };
          case 'PERF-APR-AREA':
            return {
              label: 'Aprobado por Área',
              cssClass: 'badge-approved',
              visualType: 'approved',
              icon: 'check_circle'
            };
          case 'PERF-APR-FIN':
            return {
              label: 'Aprobado Final',
              cssClass: 'badge-approved',
              visualType: 'approved',
              icon: 'verified'
            };
          case 'PERF-PEN-GEN':
          default:
            return {
              label: 'Generando Perfil',
              cssClass: 'badge-pending',
              visualType: 'pending',
              icon: 'hourglass_empty'
            };
        }
      } else {
        // RRHH / Admin
        switch (code) {
          case 'PERF-REV-RRHH':
          case 'PERF-RES-GEN':
            return {
              label: 'En Revisión RRHH',
              cssClass: 'badge-review',
              visualType: 'review',
              icon: 'rate_review'
            };
          case 'PERF-REV-AREA':
            return {
              label: 'En Revisión Solicitante',
              cssClass: 'badge-solicitante',
              visualType: 'review',
              icon: 'forward_to_inbox'
            };
          case 'PERF-OBS-AREA':
          case 'PERF-COR-RRHH':
            return {
              label: 'Observado por Área',
              cssClass: 'badge-observed',
              visualType: 'observed',
              icon: 'warning'
            };
          case 'PERF-APR-AREA':
            return {
              label: 'Aprobado por Área',
              cssClass: 'badge-approved',
              visualType: 'approved',
              icon: 'check_circle'
            };
          case 'PERF-APR-FIN':
            return {
              label: 'Aprobado Final',
              cssClass: 'badge-approved',
              visualType: 'approved',
              icon: 'verified'
            };
          case 'PERF-PEN-GEN':
          default:
            return {
              label: 'Generando Perfil',
              cssClass: 'badge-pending',
              visualType: 'pending',
              icon: 'hourglass_empty'
            };
        }
      }
    }

    if (isAreaSol && (this.estado === ProfileStatus.Observada || this.estado === ProfileStatus.Corregida)) {
      return {
        label: 'Observado por solicitante',
        cssClass: 'badge-observed',
        visualType: 'observed',
        icon: 'warning'
      };
    }

    return PROFILE_STATUS_CONFIGS[this.estado] || {
      label: this.estado || 'Desconocido',
      cssClass: 'badge-automatic',
      visualType: 'automatic',
      icon: 'help_outline'
    };
  }
}
