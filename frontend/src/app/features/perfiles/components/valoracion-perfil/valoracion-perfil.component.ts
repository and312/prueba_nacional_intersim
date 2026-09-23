import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ValoracionPerfil } from '../../models/resumen-ejecutivo-perfil.model';

@Component({
  selector: 'app-valoracion-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './valoracion-perfil.component.html',
  styleUrls: ['./valoracion-perfil.component.scss']
})
export class ValoracionPerfilComponent {
  @Input() valoracion: ValoracionPerfil | null | undefined = null;

  obtenerClaseCriticidad(nivel?: string | null): string {
    if (!nivel) return '';
    return `criticidad-${nivel.toLowerCase()}`;
  }

  obtenerClaseRiesgo(nivel?: string | null): string {
    if (!nivel) return '';
    return `riesgo-${nivel.toLowerCase()}`;
  }
}
