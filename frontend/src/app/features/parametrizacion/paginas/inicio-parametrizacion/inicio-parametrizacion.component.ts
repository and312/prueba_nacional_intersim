import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, forkJoin } from 'rxjs';
import { CATALOGOS_PARAMETRIZACION } from '../../constants/catalogos-parametrizacion';
import { ClaveCatalogoParametrizacion } from '../../models/configuracion-catalogo-parametrizacion.model';
import { Observacion } from '../../models/observacion.model';
import { Regional } from '../../models/regional.model';
import { TipoSolicitud } from '../../models/tipo-solicitud.model';
import { Modalidad } from '../../models/modalidad.model';
import { AreaCargo } from '../../models/area-cargo.model';
import { CargoItem } from '../../models/cargo.model';
import { ObservacionesService } from '../../services/observaciones.service';
import { RegionalesService } from '../../services/regionales.service';
import { TiposSolicitudService } from '../../services/tipos-solicitud.service';
import { ModalidadesService } from '../../services/modalidades.service';
import { AreasCargoService } from '../../services/areas-cargo.service';
import { CargosService } from '../../services/cargos.service';
import { TarjetaParametrizacionComponent } from '../../components/tarjeta-parametrizacion/tarjeta-parametrizacion.component';
import { ModalMantenimientoCatalogoComponent } from '../../components/modal-mantenimiento-catalogo/modal-mantenimiento-catalogo.component';
import { LoaderComponent } from '../../../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../../../core/services/component-loading.service';

@Component({
  selector: 'app-inicio-parametrizacion',
  standalone: true,
  imports: [
    CommonModule,
    TarjetaParametrizacionComponent,
    ModalMantenimientoCatalogoComponent,
    LoaderComponent
  ],
  templateUrl: './inicio-parametrizacion.component.html',
  styleUrls: ['./inicio-parametrizacion.component.scss']
})
export class InicioParametrizacionComponent implements OnInit {
  private observacionesService = inject(ObservacionesService);
  private regionalesService = inject(RegionalesService);
  private tiposSolicitudService = inject(TiposSolicitudService);
  private modalidadesService = inject(ModalidadesService);
  private areasCargoService = inject(AreasCargoService);
  private cargosService = inject(CargosService);
  componentLoading = inject(ComponentLoadingService);

  // Catálogos definidos
  catalogosConfig = CATALOGOS_PARAMETRIZACION;

  // Estados locales
  observaciones = signal<Observacion[]>([]);
  regionales = signal<Regional[]>([]);
  tiposSolicitud = signal<TipoSolicitud[]>([]);
  modalidades = signal<Modalidad[]>([]);
  areasCargo = signal<AreaCargo[]>([]);
  cargos = signal<CargoItem[]>([]);

  isLoading = signal(false);
  isError = signal(false);
  errorMessage = signal('');
  successMessage = signal<string | null>(null);

  // Selección para Modal
  selectedCatalogo = signal<ClaveCatalogoParametrizacion | null>(null);

  // Configuración de la tarjeta activa seleccionada
  configuracionSeleccionada = computed(() => {
    const clave = this.selectedCatalogo();
    if (!clave) return null;
    return this.catalogosConfig.find(c => c.clave === clave) || null;
  });

  // Registros del catálogo activo seleccionado
  registrosSeleccionados = computed(() => {
    const clave = this.selectedCatalogo();
    if (!clave) return [];
    if (clave === 'OBSERVACIONES') return this.observaciones();
    if (clave === 'REGIONALES') return this.regionales();
    if (clave === 'TIPOS_SOLICITUD') return this.tiposSolicitud();
    if (clave === 'MODALIDADES') return this.modalidades();
    if (clave === 'AREAS_CARGO') return this.areasCargo();
    if (clave === 'CARGOS') return this.cargos();
    return [];
  });

  ngOnInit(): void {
    this.cargarTodos();
  }

  cargarTodos(silencioso = false): void {
    if (!silencioso) {
      this.isLoading.set(true);
      this.isError.set(false);
      this.componentLoading.show('inicio-parametrizacion');
    }

    forkJoin({
      obs: this.observacionesService.listar(),
      reg: this.regionalesService.listar(),
      sol: this.tiposSolicitudService.listar(),
      mod: this.modalidadesService.listar(),
      arc: this.areasCargoService.listar(),
      crg: this.cargosService.listar()
    }).subscribe({
      next: (res) => {
        const sortById = (a: any, b: any) => (a.id ?? a.idAreaCargo ?? a.idCargo ?? a.idObservacion ?? a.idRegional ?? a.idTipoSolicitud ?? a.idModalidad ?? 0)
          - (b.id ?? b.idAreaCargo ?? b.idCargo ?? b.idObservacion ?? b.idRegional ?? b.idTipoSolicitud ?? b.idModalidad ?? 0);
        this.observaciones.set((res.obs || []).slice().sort(sortById));
        this.regionales.set((res.reg || []).slice().sort(sortById));
        this.tiposSolicitud.set((res.sol || []).slice().sort(sortById));
        this.modalidades.set((res.mod || []).slice().sort(sortById));
        this.areasCargo.set((res.arc || []).slice().sort(sortById));
        this.cargos.set((res.crg || []).slice().sort((a, b) => a.idCargo - b.idCargo));
        if (!silencioso) {
          this.isLoading.set(false);
          this.componentLoading.hide('inicio-parametrizacion');
        }
      },
      error: (err) => {
        if (!silencioso) {
          this.isLoading.set(false);
          this.isError.set(true);
          this.errorMessage.set(err.message || 'Error al conectar con los servicios de parametrización.');
          this.componentLoading.hide('inicio-parametrizacion');
        }
      }
    });
  }

  // Helpers para obtener métricas agregadas por catálogo
  obtenerMetricas(clave: ClaveCatalogoParametrizacion): { total: number; activos: number; vistaPrevia: string[] } {
    let lista: any[] = [];
    if (clave === 'OBSERVACIONES') lista = this.observaciones();
    else if (clave === 'REGIONALES') lista = this.regionales();
    else if (clave === 'TIPOS_SOLICITUD') lista = this.tiposSolicitud();
    else if (clave === 'MODALIDADES') lista = this.modalidades();
    else if (clave === 'AREAS_CARGO') lista = this.areasCargo();
    else if (clave === 'CARGOS') lista = this.cargos();

    const activos = lista.filter(item => item.activo);
    const vistaPrevia = activos.slice(0, 3).map(item => item.nombre);

    return {
      total: lista.length,
      activos: activos.length,
      vistaPrevia
    };
  }

  onAdministrar(clave: ClaveCatalogoParametrizacion): void {
    this.selectedCatalogo.set(clave);
  }

  onCerrarModal(): void {
    this.selectedCatalogo.set(null);
  }

  onCrearRegistro(event: { nombre: string; activo: boolean; areaCargoId?: number }): void {
    const clave = this.selectedCatalogo();
    if (!clave) return;

    let obs$: Observable<any>;

    if (clave === 'OBSERVACIONES') {
      obs$ = this.observacionesService.crear(event);
    } else if (clave === 'REGIONALES') {
      obs$ = this.regionalesService.crear(event);
    } else if (clave === 'TIPOS_SOLICITUD') {
      obs$ = this.tiposSolicitudService.crear(event);
    } else if (clave === 'MODALIDADES') {
      obs$ = this.modalidadesService.crear(event);
    } else if (clave === 'AREAS_CARGO') {
      obs$ = this.areasCargoService.crear(event);
    } else {
      const defaultAreaId = event.areaCargoId || (this.areasCargo().length > 0 ? this.areasCargo()[0].idAreaCargo : 1);
      obs$ = this.cargosService.crear({ areaCargoId: defaultAreaId, nombre: event.nombre, activo: event.activo });
    }

    obs$.subscribe({
      next: (nuevoItem: any) => {
        // Inserción optimista al FINAL antes de recargar desde el servidor
        if (nuevoItem) {
          if (clave === 'CARGOS') {
            this.cargos.set([...this.cargos(), nuevoItem]);
          } else if (clave === 'AREAS_CARGO') {
            const areaNueva = { idAreaCargo: nuevoItem.id, nombre: nuevoItem.nombre, activo: nuevoItem.activo ?? true };
            this.areasCargo.set([...this.areasCargo(), areaNueva]);
          } else if (clave === 'REGIONALES') {
            this.regionales.set([...this.regionales(), nuevoItem]);
          } else if (clave === 'TIPOS_SOLICITUD') {
            this.tiposSolicitud.set([...this.tiposSolicitud(), nuevoItem]);
          } else if (clave === 'MODALIDADES') {
            this.modalidades.set([...this.modalidades(), nuevoItem]);
          } else if (clave === 'OBSERVACIONES') {
            this.observaciones.set([...this.observaciones(), nuevoItem]);
          }
        }
        // Recarga silenciosa en segundo plano para sincronizar con el servidor
        this.cargarTodos(true);
        this.mostrarAlertaExito('Elemento agregado con éxito.');
      },
      error: (err) => {
        const customMsg = err.error?.mensaje || err.error?.message || err.message || 'Error al agregar el elemento.';
        alert(customMsg);
      }
    });
  }

  onActualizarRegistro(event: { id: number; nombre: string; areaCargoId?: number }): void {
    const clave = this.selectedCatalogo();
    if (!clave) return;

    let obs$: Observable<any>;

    if (clave === 'OBSERVACIONES') {
      obs$ = this.observacionesService.actualizar(event.id, { nombre: event.nombre });
    } else if (clave === 'REGIONALES') {
      obs$ = this.regionalesService.actualizar(event.id, { nombre: event.nombre });
    } else if (clave === 'TIPOS_SOLICITUD') {
      obs$ = this.tiposSolicitudService.actualizar(event.id, { nombre: event.nombre });
    } else if (clave === 'MODALIDADES') {
      obs$ = this.modalidadesService.actualizar(event.id, { nombre: event.nombre });
    } else if (clave === 'AREAS_CARGO') {
      obs$ = this.areasCargoService.actualizar(event.id, { nombre: event.nombre });
    } else {
      const currentCargo = this.cargos().find(c => c.idCargo === event.id);
      const defaultAreaId = event.areaCargoId || currentCargo?.areaCargoId || 1;
      obs$ = this.cargosService.actualizar(event.id, { areaCargoId: defaultAreaId, nombre: event.nombre });
    }

    obs$.subscribe({
      next: () => {
        this.cargarTodos(true);
        this.mostrarAlertaExito('Nombre actualizado con éxito.');
      },
      error: (err) => {
        const customMsg = err.error?.mensaje || err.error?.message || err.message || 'Error al actualizar el elemento.';
        alert(customMsg);
      }
    });
  }

  onCambiarEstadoRegistro(event: { id: number; activo: boolean }): void {
    const clave = this.selectedCatalogo();
    if (!clave) return;

    let obs$: Observable<any>;

    if (clave === 'OBSERVACIONES') {
      obs$ = this.observacionesService.cambiarEstado(event.id, event.activo);
    } else if (clave === 'REGIONALES') {
      obs$ = this.regionalesService.cambiarEstado(event.id, event.activo);
    } else if (clave === 'TIPOS_SOLICITUD') {
      obs$ = this.tiposSolicitudService.cambiarEstado(event.id, event.activo);
    } else if (clave === 'MODALIDADES') {
      obs$ = this.modalidadesService.cambiarEstado(event.id, event.activo);
    } else if (clave === 'AREAS_CARGO') {
      obs$ = this.areasCargoService.cambiarEstado(event.id, event.activo);
    } else {
      obs$ = this.cargosService.cambiarEstado(event.id, event.activo);
    }

    obs$.subscribe({
      next: () => {
        this.cargarTodos(true);
        const mensaje = event.activo ? 'Elemento activado con éxito.' : 'Elemento inactivado con éxito.';
        this.mostrarAlertaExito(mensaje);
      },
      error: (err) => {
        alert('Error al cambiar el estado del elemento: ' + err.message);
      }
    });
  }

  onEliminarRegistro(id: number): void {
    const clave = this.selectedCatalogo();
    if (!clave) return;

    let obs$: Observable<void>;

    if (clave === 'OBSERVACIONES') {
      obs$ = this.observacionesService.eliminar(id);
    } else if (clave === 'REGIONALES') {
      obs$ = this.regionalesService.eliminar(id);
    } else if (clave === 'TIPOS_SOLICITUD') {
      obs$ = this.tiposSolicitudService.eliminar(id);
    } else if (clave === 'MODALIDADES') {
      obs$ = this.modalidadesService.eliminar(id);
    } else if (clave === 'AREAS_CARGO') {
      obs$ = this.areasCargoService.eliminar(id);
    } else {
      obs$ = this.cargosService.eliminar(id);
    }

    obs$.subscribe({
      next: () => {
        this.cargarTodos(true);
        this.mostrarAlertaExito('Registro eliminado correctamente.');
      },
      error: (err) => {
        const customMsg = err.error?.mensaje || err.error?.message || err.message || 'Error al eliminar el elemento.';
        alert(customMsg);
      }
    });
  }

  mostrarAlertaExito(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 4000);
  }

  retry(): void {
    this.cargarTodos();
  }
}

