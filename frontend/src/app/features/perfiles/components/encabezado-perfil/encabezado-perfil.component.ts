import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { DetallePerfil } from '../../models/perfil.model';
import { ProfileStatusBadgeComponent } from '../../components/profile-status-badge/profile-status-badge.component';
import { ProfileStatus } from '../../models/profile-status';

@Component({
  selector: 'app-encabezado-perfil',
  standalone: true,
  imports: [CommonModule, DatePipe, ProfileStatusBadgeComponent],
  templateUrl: './encabezado-perfil.component.html',
  styleUrls: ['./encabezado-perfil.component.scss']
})
export class EncabezadoPerfilComponent {
  @Input({ required: true }) perfil!: DetallePerfil;
  @Input() rol?: 'RRHH' | 'AreaSol' | 'Administrador';
  @Output() volverClick = new EventEmitter<void>();

  get estadoComoIngles(): ProfileStatus {
    return this.perfil.estado as unknown as ProfileStatus;
  }
}
