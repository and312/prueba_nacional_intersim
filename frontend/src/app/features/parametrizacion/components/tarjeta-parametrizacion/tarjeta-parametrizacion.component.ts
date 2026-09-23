import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ConfiguracionCatalogoParametrizacion, ClaveCatalogoParametrizacion } from '../../models/configuracion-catalogo-parametrizacion.model';

@Component({
  selector: 'app-tarjeta-parametrizacion',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './tarjeta-parametrizacion.component.html',
  styleUrls: ['./tarjeta-parametrizacion.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TarjetaParametrizacionComponent {
  @Input({ required: true }) config!: ConfiguracionCatalogoParametrizacion;
  @Input({ required: true }) total = 0;
  @Input({ required: true }) activos = 0;
  @Input({ required: true }) vistaPrevia: string[] = [];
  @Output() administrar = new EventEmitter<ClaveCatalogoParametrizacion>();

  onAdministrar(): void {
    this.administrar.emit(this.config.clave);
  }

  get tieneMas(): boolean {
    return this.total > this.vistaPrevia.length;
  }

  get cantidadMas(): number {
    return this.total - this.vistaPrevia.length;
  }
}
