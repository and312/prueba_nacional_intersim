import { Component, Input, Output, EventEmitter, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { ConfiguracionCatalogoParametrizacion } from '../../models/configuracion-catalogo-parametrizacion.model';
import { AreaCargo } from '../../models/area-cargo.model';
import { FormularioCatalogoComponent } from '../formulario-catalogo/formulario-catalogo.component';

@Component({
  selector: 'app-modal-mantenimiento-catalogo',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    FormularioCatalogoComponent
  ],
  templateUrl: './modal-mantenimiento-catalogo.component.html',
  styleUrls: ['./modal-mantenimiento-catalogo.component.scss']
})
export class ModalMantenimientoCatalogoComponent {
  @Input({ required: true }) config!: ConfiguracionCatalogoParametrizacion;
  @Input({ required: true }) registros: any[] = [];
  @Input() listadoAreasCargo: AreaCargo[] = [];

  @Output() cerrar = new EventEmitter<void>();
  @Output() crear = new EventEmitter<{ nombre: string; areaCargoId?: number; activo: boolean }>();
  @Output() actualizar = new EventEmitter<{ id: number; nombre: string; areaCargoId?: number }>();
  @Output() cambiarEstado = new EventEmitter<{ id: number; activo: boolean }>();
  @Output() eliminar = new EventEmitter<number>();

  mostrarFormularioCreacion = signal(false);
  modoEdicionId = signal<number | null>(null);

  // Buscador
  terminoBusqueda = signal('');

  get registrosFiltrados(): any[] {
    const termino = this.terminoBusqueda().trim().toLowerCase();
    if (!termino) return this.registros;
    return this.registros.filter(item => {
      const matchNombre = (item.nombre || '').toLowerCase().includes(termino);
      const matchArea = (item.areaCargoNombre || '').toLowerCase().includes(termino);
      return matchNombre || matchArea;
    });
  }

  get totalFiltrados(): number {
    return this.registrosFiltrados.length;
  }

  limpiarBusqueda(): void {
    this.terminoBusqueda.set('');
  }

  puedeEliminarRegistro(item: any): boolean {
    return item.puedeEliminar !== false && !(item.cantidadUsos && item.cantidadUsos > 0);
  }

  onEliminar(item: any): void {
    const id = this.obtenerId(item);
    if (!this.puedeEliminarRegistro(item)) {
      const relacionMsg = item.cantidadUsos ? `\n\nEste registro está relacionado con ${item.cantidadUsos} registros.` : '';
      alert(
        'No es posible eliminar este registro porque ya está siendo utilizado en procesos existentes. ' +
        'Puede inactivarlo para evitar su uso en nuevos registros.' +
        relacionMsg
      );
      return;
    }

    const mensajeConfirmar = `¿Está seguro de eliminar '${item.nombre}'? Esta acción no se puede deshacer.`;
    if (confirm(mensajeConfirmar)) {
      this.eliminar.emit(id);
    }
  }

  displayedColumns = ['id', 'nombre', 'estado', 'acciones'];

  obtenerId(item: any): number {
    return (
      item.id ||
      item.idAreaCargo ||
      item.idCargo ||
      item.idObservacion ||
      item.idRegional ||
      item.idTipoSolicitud ||
      item.idModalidad ||
      0
    );
  }

  obtenerNombresExistentes(): string[] {
    return this.registros.map(r => r.nombre);
  }

  obtenerNombreArea(areaCargoId?: number): string {
    if (!areaCargoId) return 'Sin área';
    const area = this.listadoAreasCargo.find(a => a.idAreaCargo === areaCargoId);
    return area ? area.nombre : `Área #${areaCargoId}`;
  }

  onCerrar(): void {
    this.cerrar.emit();
  }

  onIniciarCreacion(): void {
    this.mostrarFormularioCreacion.set(true);
    this.modoEdicionId.set(null);
    this.terminoBusqueda.set(''); // Limpiar búsqueda al agregar
  }

  onCancelarCreacion(): void {
    this.mostrarFormularioCreacion.set(false);
  }

  onGuardarCreacion(data: { nombre: string; areaCargoId?: number }): void {
    this.crear.emit({ nombre: data.nombre, areaCargoId: data.areaCargoId, activo: true });
    this.mostrarFormularioCreacion.set(false);
  }

  onIniciarEdicion(item: any): void {
    this.modoEdicionId.set(this.obtenerId(item));
    this.mostrarFormularioCreacion.set(false);
  }

  onCancelarEdicion(): void {
    this.modoEdicionId.set(null);
  }

  onGuardarEdicion(id: number, data: { nombre: string; areaCargoId?: number }): void {
    // Actualización inmediata local en UI para retroalimentación instantánea
    const item = this.registros.find(r => this.obtenerId(r) === id);
    if (item) {
      item.nombre = data.nombre;
      if (data.areaCargoId) {
        item.areaCargoId = data.areaCargoId;
        const area = this.listadoAreasCargo.find(a => a.idAreaCargo === data.areaCargoId);
        if (area) {
          item.areaCargoNombre = area.nombre;
        }
      }
    }
    this.actualizar.emit({ id, nombre: data.nombre, areaCargoId: data.areaCargoId });
    this.modoEdicionId.set(null);
  }

  onToggleEstado(item: any, event: Event): void {
    const input = event.target as HTMLInputElement;
    const nuevoEstado = input.checked;
    const id = this.obtenerId(item);

    if (!nuevoEstado) {
      const mensaje =
        'Inactivar este registro evitará que se utilice en nuevos procesos. ' +
        'Los registros históricos conservarán su valor.\n\n' +
        '¿Está seguro de que desea inactivar este elemento?';

      if (!confirm(mensaje)) {
        input.checked = true;
        return;
      }
    } else {
      if (!confirm('¿Desea activar este elemento para su uso en nuevos registros?')) {
        input.checked = false;
        return;
      }
    }

    this.cambiarEstado.emit({ id, activo: nuevoEstado });
  }
}
