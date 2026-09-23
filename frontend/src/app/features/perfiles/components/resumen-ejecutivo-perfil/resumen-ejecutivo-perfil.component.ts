import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DatosResumenEjecutivoMockService } from '../../services/datos-resumen-ejecutivo-mock.service';
import { ResumenEjecutivoPerfil, InsumoResumenPerfil } from '../../models/resumen-ejecutivo-perfil.model';
import { EstadoGeneracionResumen } from '../../models/estado-generacion-resumen.enum';
import { ValoracionPerfilComponent } from '../valoracion-perfil/valoracion-perfil.component';

@Component({
  selector: 'app-resumen-ejecutivo-perfil',
  standalone: true,
  imports: [CommonModule, ValoracionPerfilComponent],
  templateUrl: './resumen-ejecutivo-perfil.component.html',
  styleUrls: ['./resumen-ejecutivo-perfil.component.scss']
})
export class ResumenEjecutivoPerfilComponent implements OnInit, OnChanges {
  @Input({ required: true }) perfilId!: number;
  @Output() insumosCargados = new EventEmitter<InsumoResumenPerfil[]>();

  private resumenService = inject(DatosResumenEjecutivoMockService);

  resumen = signal<ResumenEjecutivoPerfil | null>(null);
  isLoading = signal(false);
  isError = signal(false);
  errorMessage = signal('');

  EstadoGeneracionResumen = EstadoGeneracionResumen;

  ngOnInit(): void {
    if (!this.resumen()) {
      this.cargarResumen();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['perfilId']) {
      this.resumen.set(null); // Limpiar el estado anterior antes de cargar
      this.cargarResumen();
    }
  }

  cargarResumen(): void {
    if (!this.perfilId) return;

    this.isLoading.set(true);
    this.isError.set(false);

    this.resumenService.obtenerResumenPorPerfilId(this.perfilId).subscribe({
      next: (data) => {
        this.resumen.set(data);
        this.isLoading.set(false);
        if (data.insumosUtilizados) {
          this.insumosCargados.emit(data.insumosUtilizados);
        } else {
          this.insumosCargados.emit([]);
        }
      },
      error: (err) => {
        this.isError.set(true);
        this.errorMessage.set(err.message || 'No fue posible cargar el resumen ejecutivo.');
        this.isLoading.set(false);
        this.insumosCargados.emit([]);
      }
    });
  }

  reintentar(): void {
    this.cargarResumen();
  }

  obtenerColumnasCondiciones(r: ResumenEjecutivoPerfil): number {
    let count = 0;
    if (r.modalidad) count++;
    if (r.ubicacion) count++;
    if (r.salario) count++;
    return count;
  }
}
