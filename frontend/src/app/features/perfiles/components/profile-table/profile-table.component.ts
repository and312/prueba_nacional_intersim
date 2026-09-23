import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ProfileListItem } from '../../models/profile.model';
import { ProfileStatus } from '../../models/profile-status';
import { ProfileStatusBadgeComponent } from '../profile-status-badge/profile-status-badge.component';

export interface ProfileRowAction {
  label: string;
  action: string;
  icon: string;
  cssClass: string;
}

@Component({
  selector: 'app-profile-table',
  standalone: true,
  imports: [CommonModule, DatePipe, ProfileStatusBadgeComponent],
  templateUrl: './profile-table.component.html',
  styleUrls: ['./profile-table.component.scss']
})
export class ProfileTableComponent {
  @Input({ required: true }) items!: ProfileListItem[];
  @Input({ required: true }) showAreaColumn!: boolean;
  @Input({ required: true }) userRole!: 'RRHH' | 'AreaSol' | 'Administrador';
  @Output() actionClick = new EventEmitter<{ action: string, profile: ProfileListItem }>();

  getRowAction(item: ProfileListItem): ProfileRowAction {
    const isRrhhOrAdmin = this.userRole === 'RRHH' || this.userRole === 'Administrador';
    const code = item.estadoCodigo;

    if (isRrhhOrAdmin) {
      switch (code) {
        case 'PERF-REV-RRHH':
          return { label: 'Revisar perfil', action: 'revisar', icon: 'rate_review', cssClass: 'action-review' };
        case 'PERF-OBS-AREA':
          return { label: 'Ver observaciones', action: 'detalle', icon: 'visibility', cssClass: 'action-detail' };
        case 'PERF-COR-RRHH':
          return { label: 'Corregir perfil', action: 'corregir', icon: 'edit', cssClass: 'action-correct' };
        case 'PERF-REV-AREA':
          return { label: 'Ver seguimiento', action: 'seguimiento', icon: 'sync', cssClass: 'action-track' };
        case 'PERF-APR-AREA':
          return { label: 'Revisar y aprobar', action: 'revisar_aprobar_final', icon: 'task_alt', cssClass: 'action-approve' };
        case 'PERF-APR-FIN':
          return { label: 'Ver detalle', action: 'detalle', icon: 'visibility', cssClass: 'action-detail' };
        default:
          return { label: 'Ver detalle', action: 'detalle', icon: 'visibility', cssClass: 'action-detail' };
      }
    } else {
      // Área Solicitante
      switch (code) {
        case 'PERF-REV-AREA':
          return { label: 'Revisar perfil', action: 'revisar', icon: 'rate_review', cssClass: 'action-review' };
        case 'PERF-OBS-AREA':
        case 'PERF-COR-RRHH':
          return { label: 'Ver seguimiento', action: 'seguimiento', icon: 'sync', cssClass: 'action-track' };
        case 'PERF-APR-AREA':
        case 'PERF-APR-FIN':
          return { label: 'Ver detalle', action: 'detalle', icon: 'visibility', cssClass: 'action-detail' };
        default:
          return { label: 'Ver detalle', action: 'detalle', icon: 'visibility', cssClass: 'action-detail' };
      }
    }
  }

  obtenerOrigenBadge(canalOrigen: string | null | undefined): { label: string; icon: string; cssClass: string } {
    const canal = (canalOrigen || 'Web').toLowerCase();
    if (canal.includes('whatsapp') || canal.includes('n8n') || canal.includes('chat')) {
      return {
        label: 'WhatsApp',
        icon: 'chat',
        cssClass: 'origin-badge origin-whatsapp'
      };
    }
    return {
      label: 'Web',
      icon: 'desktop_windows',
      cssClass: 'origin-badge origin-web'
    };
  }

  obtenerPrioridadInfo(prioridad: string | null | undefined): { label: string; cssClass: string } {
    const p = (prioridad || 'Media').toLowerCase();
    if (p.includes('alta') || p.includes('critica')) {
      return { label: 'ALTA', cssClass: 'priority-badge priority-high' };
    }
    if (p.includes('baja')) {
      return { label: 'BAJA', cssClass: 'priority-badge priority-low' };
    }
    return { label: 'MEDIA', cssClass: 'priority-badge priority-medium' };
  }

  onActionClick(action: string, profile: ProfileListItem, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.actionClick.emit({ action, profile });
  }

  trackByProfileId(index: number, item: ProfileListItem): number {
    return item.id;
  }
}
