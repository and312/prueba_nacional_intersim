import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pestanas-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pestanas-perfil.component.html',
  styleUrls: ['./pestanas-perfil.component.scss']
})
export class PestanasPerfilComponent {
  @Input({ required: true }) userRole!: 'RRHH' | 'AreaSol' | 'Administrador';
  @Input({ required: true }) pestanaActiva!: 'resumen' | 'estructurado';
  @Output() pestanaCambio = new EventEmitter<'resumen' | 'estructurado'>();

  seleccionarPestana(pestana: 'resumen' | 'estructurado'): void {
    this.pestanaCambio.emit(pestana);
  }
}
