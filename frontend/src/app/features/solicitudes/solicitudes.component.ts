import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';

@Component({
  selector: 'app-solicitudes',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    LoaderComponent
  ],
  templateUrl: './solicitudes.component.html',
  styleUrls: ['./solicitudes.component.scss']
})
export class SolicitudesComponent implements OnInit {
  private apiService = inject(ApiService);
  authService = inject(AuthService);
  componentLoading = inject(ComponentLoadingService);

  solicitudes = signal<any[]>([]);
  totalCount = signal(0);
  isCreating = signal(false);
  isEditing = signal(false);
  selectedId = signal<number | null>(null);
  soloMisSolicitudes = signal<boolean>(false);
  // Señales para filtros y buscador
  searchQuery = signal('');
  selectedEstado = signal('');
  selectedArea = signal('');
  selectedPrioridad = signal('');
  selectedFechaDesde = signal('');
  selectedFechaHasta = signal('');
  currentPage = signal(1);
  pageSize = signal(8);
  pageSizeOptions = [5, 10, 20, 50];
  isPageSizeMenuOpen = signal(false);
  selectedCardFilter = signal<'ALL' | 'REV' | 'OBS' | 'APR'>('ALL');

  availableAreas = computed(() => {
    const set = new Set<string>();
    this.solicitudes().forEach(item => {
      const area = item.area || item.areaSolicitante;
      if (area && area.trim()) {
        set.add(area.trim());
      }
    });
    return Array.from(set).sort();
  });

  filterByCardFilter(type: 'ALL' | 'REV' | 'OBS' | 'APR'): void {
    if (this.selectedCardFilter() === type && type !== 'ALL') {
      this.selectedCardFilter.set('ALL');
    } else {
      this.selectedCardFilter.set(type);
    }
    this.selectedEstado.set('');
    this.resetPagination();
  }

  filteredSolicitudes = computed(() => {
    const list = this.solicitudes().filter(item => {
      // 1. Filtro por buscador (cargo, área, solicitante o código)
      const query = this.searchQuery().toLowerCase().trim();
      const matchesQuery = !query || 
        (item.cargo || item.nombreCargo || '').toLowerCase().includes(query) ||
        (item.area || item.areaSolicitante || '').toLowerCase().includes(query) ||
        (item.solicitanteNombre || item.createdBy || '').toLowerCase().includes(query) ||
        (item.codigo || '').toLowerCase().includes(query);

      // 2. Filtro por estado desde dropdown
      const estado = this.selectedEstado();
      const matchesEstado = !estado || 
        (item.estadoNombre === estado);

      // 3. Filtro por Área del Solicitante desde dropdown
      const selectedArea = this.selectedArea();
      const matchesArea = !selectedArea ||
        (item.area || item.areaSolicitante || '').trim() === selectedArea;

      // 4. Filtro por Tarjeta KPI Activa (Registradas, En revisión, Con observaciones, Aprobadas)
      const cardFilter = this.selectedCardFilter();
      let matchesCard = true;
      if (cardFilter === 'REV') {
        matchesCard = item.estadoCodigo === 'SOL-ENV' || item.estadoNombre === 'Solicitud Enviada a RRHH' || item.estadoNombre === 'En Validación' || item.estadoNombre === 'En revisión';
      } else if (cardFilter === 'OBS') {
        matchesCard = item.estadoCodigo === 'SOL-OBS' || item.estadoNombre === 'Solicitud Observada' || item.estadoNombre === 'Observada' || item.estadoNombre === 'Con observaciones';
      } else if (cardFilter === 'APR') {
        matchesCard = item.estadoCodigo === 'SOL-APR' || item.estadoNombre === 'Solicitud Aprobada' || item.estadoNombre === 'Aprobada';
      }

      // 5. Filtro por prioridad
      const prioridad = this.selectedPrioridad();
      const matchesPrioridad = !prioridad || 
        (item.prioridad === prioridad);

      // 6. Filtro por rango de fecha (Desde / Hasta)
      const fechaDesde = this.selectedFechaDesde();
      const fechaHasta = this.selectedFechaHasta();
      let matchesFecha = true;
      const itemDateRaw = item.createdDate || item.fecha;

      if (itemDateRaw) {
        const itemDate = new Date(itemDateRaw);
        if (!isNaN(itemDate.getTime())) {
          if (fechaDesde) {
            const desde = new Date(fechaDesde);
            desde.setHours(0, 0, 0, 0);
            if (itemDate < desde) {
              matchesFecha = false;
            }
          }
          if (fechaHasta) {
            const hasta = new Date(fechaHasta);
            hasta.setHours(23, 59, 59, 999);
            if (itemDate > hasta) {
              matchesFecha = false;
            }
          }
        }
      } else if (fechaDesde || fechaHasta) {
        matchesFecha = false;
      }

      // 7. Filtro por Rol (RRHH no ve solicitudes en estado 'Solicitud Pendiente')
      const userRole = this.authService.getUserRole();
      const userArea = this.authService.getUserArea();
      const isRrhh = userRole === 'RRHH' || userArea === 'Recursos Humanos';
      if (isRrhh && (item.estadoCodigo === 'SOL-PEN' || item.estadoNombre === 'Solicitud Pendiente')) {
        return false;
      }

      return matchesQuery && matchesEstado && matchesArea && matchesCard && matchesPrioridad && matchesFecha;
    });

    // Ordenar por la actividad más reciente (fecha de actualización o creación más reciente primero)
    const parseDate = (val: any): number => {
      if (!val) return 0;
      const t = new Date(val).getTime();
      return isNaN(t) ? 0 : t;
    };

    return list.sort((a: any, b: any) => {
      const timeA = Math.max(parseDate(a.lastModifiedDate), parseDate(a.createdDate), parseDate(a.fecha));
      const timeB = Math.max(parseDate(b.lastModifiedDate), parseDate(b.createdDate), parseDate(b.fecha));
      return timeB - timeA;
    });
  });

  totalItems = computed(() => this.filteredSolicitudes().length);

  totalPages = computed(() => {
    const pages = Math.ceil(this.totalItems() / this.pageSize());
    return pages > 0 ? pages : 1;
  });

  paginatedSolicitudes = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredSolicitudes().slice(start, start + this.pageSize());
  });

  rangeStart = computed(() => {
    if (this.totalItems() === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize() + 1;
  });

  rangeEnd = computed(() => {
    if (this.totalItems() === 0) return 0;
    return Math.min(this.currentPage() * this.pageSize(), this.totalItems());
  });

  enRevisionCount = computed(() => 
    this.solicitudes().filter(s => s.estadoCodigo === 'SOL-ENV' || s.estadoNombre === 'Solicitud Enviada a RRHH' || s.estadoNombre === 'En Validación').length
  );

  observadasCount = computed(() => 
    this.solicitudes().filter(s => s.estadoCodigo === 'SOL-OBS' || s.estadoNombre === 'Solicitud Observada' || s.estadoNombre === 'Observada').length
  );

  aprobadasCount = computed(() => 
    this.solicitudes().filter(s => s.estadoCodigo === 'SOL-APR' || s.estadoNombre === 'Solicitud Aprobada' || s.estadoNombre === 'Aprobada').length
  );

  successMessage = signal<string | null>(null);

  solicitudForm = new FormGroup({
    nombreCargo: new FormControl('', [Validators.required]),
    areaSolicitante: new FormControl('', [Validators.required]),
    motivoContratacion: new FormControl('', [Validators.required]),
    descripcionRequerimientos: new FormControl('', [Validators.required])
  });

  ngOnInit(): void {
    const userRole = this.authService.getUserRole();
    const userArea = this.authService.getUserArea();
    const isRrhhOrAdmin = userRole === 'RRHH' || userRole === 'Administrador' || userArea === 'Recursos Humanos';
    if (!isRrhhOrAdmin) {
      this.soloMisSolicitudes.set(true);
    } else {
      this.soloMisSolicitudes.set(false);
    }

    const state = history.state;
    if (state && state.successMessage) {
      this.successMessage.set(state.successMessage);
      setTimeout(() => {
        this.successMessage.set(null);
      }, 5000);
    }
    this.loadSolicitudes();
  }

  setSoloMisSolicitudes(val: boolean): void {
    this.soloMisSolicitudes.set(val);
    this.resetPagination();
    this.loadSolicitudes();
  }

  updateSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
    this.resetPagination();
  }

  updateEstado(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedEstado.set(select.value);
    this.resetPagination();
  }

  updateArea(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedArea.set(select.value);
    this.resetPagination();
  }

  updatePrioridad(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedPrioridad.set(select.value);
    this.resetPagination();
  }

  updateFechaDesde(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFechaDesde.set(input.value);
    this.resetPagination();
  }

  updateFechaHasta(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFechaHasta.set(input.value);
    this.resetPagination();
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedEstado.set('');
    this.selectedArea.set('');
    this.selectedPrioridad.set('');
    this.selectedCardFilter.set('ALL');
    this.selectedFechaDesde.set('');
    this.selectedFechaHasta.set('');
    this.resetPagination();
  }

  obtenerOrigenBadge(canalOrigen: string | null | undefined): { label: string; icon: string; cssClass: string } {
    const canal = (canalOrigen || 'BackOffice').toLowerCase();
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

  resetPagination(): void {
    this.currentPage.set(1);
  }

  goToFirstPage(): void {
    this.currentPage.set(1);
  }

  goToPreviousPage(): void {
    this.currentPage.update(page => Math.max(1, page - 1));
  }

  goToNextPage(): void {
    this.currentPage.update(page => Math.min(this.totalPages(), page + 1));
  }

  goToLastPage(): void {
    this.currentPage.set(this.totalPages());
  }

  updatePageSize(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.updatePageSizeValue(Number(select.value));
  }

  togglePageSizeMenu(): void {
    this.isPageSizeMenuOpen.update(isOpen => !isOpen);
  }

  updatePageSizeValue(value: number): void {
    this.pageSize.set(value);
    this.isPageSizeMenuOpen.set(false);
    this.resetPagination();
  }

  loadSolicitudes(): void {
    this.apiService.getSolicitudes(1, 1000, undefined, this.soloMisSolicitudes()).subscribe({ // Carga en memoria para métricas y filtrado reactivo
      next: (res) => {
        this.solicitudes.set(res.items || []);
        this.totalCount.set(res.totalCount || (res.items ? res.items.length : 0));
      },
      error: (err) => {
        console.error('Error al cargar solicitudes:', err);
        this.solicitudes.set([]);
        this.totalCount.set(0);
      }
    });
  }

  startCreate(): void {
    this.solicitudForm.reset();
    this.isCreating.set(true);
    this.isEditing.set(false);
  }

  cancel(): void {
    this.isCreating.set(false);
    this.isEditing.set(false);
    this.selectedId.set(null);
  }

  onSubmit(): void {
    if (this.solicitudForm.invalid) return;

    const payload = this.solicitudForm.value;

    if (this.isEditing() && this.selectedId() !== null) {
      this.apiService.actualizarSolicitud(this.selectedId()!, payload).subscribe(() => {
        this.loadSolicitudes();
        this.cancel();
      });
    } else {
      this.apiService.crearSolicitud(payload).subscribe(() => {
        this.loadSolicitudes();
        this.cancel();
      });
    }
  }

  edit(element: any): void {
    this.selectedId.set(element.solicitudId || element.id);
    this.solicitudForm.patchValue({
      nombreCargo: element.cargo || element.nombreCargo,
      areaSolicitante: element.area || element.areaSolicitante,
      motivoContratacion: element.motivoContratacion,
      descripcionRequerimientos: element.descripcionRequerimientos
    });
    this.isEditing.set(true);
    this.isCreating.set(false);
  }

  aprobar(element: any): void {
    this.apiService.transitarSolicitud(element.solicitudId || element.id, 'SOL-APR', 'Aprobado desde Frontend MVP').subscribe(() => {
      this.loadSolicitudes();
    });
  }

  obtenerAccion(item: any): { label: string; route: any[]; cssClass: string; icon: string } {
    const userRole = this.authService.getUserRole();
    const userArea = this.authService.getUserArea();
    // Si está en "Mis Solicitudes", actúa como Solicitante
    const isRrhh = !this.soloMisSolicitudes() && (userRole === 'RRHH' || userArea === 'Recursos Humanos');
    const isAdmin = userRole === 'Administrador';

    // 1. Administrador: siempre ve detalle
    if (isAdmin) {
      return {
        label: 'Ver detalle',
        route: ['/solicitudes', item.solicitudId || item.id],
        cssClass: 'solicitudes-row-action',
        icon: 'visibility'
      };
    }

    const estadoCod = item.estadoCodigo || '';
    const estadoNom = item.estadoNombre || '';

    // Categorías de estados
    const esBorradorOPendiente = estadoCod === 'SOL-REG' || estadoCod === 'SOL-PEN' || estadoNom === 'Solicitud Registrada' || estadoNom === 'Solicitud Pendiente' || estadoNom === 'Borrador';
    const esObservada = estadoCod === 'SOL-OBS' || estadoNom === 'Solicitud Observada' || estadoNom === 'Observada' || estadoNom === 'Con observaciones';
    const esEnviadaORevision = estadoCod === 'SOL-ENV' || estadoNom === 'Solicitud Enviada a RRHH' || estadoNom === 'En Validación' || estadoNom === 'En revisión' || estadoNom === 'En revisión RRHH';

    // 2. Rol RRHH
    if (isRrhh) {
      if (esEnviadaORevision) {
        return {
          label: 'Revisar',
          route: ['/solicitudes', item.solicitudId || item.id],
          cssClass: 'solicitudes-row-action action-filled',
          icon: 'visibility'
        };
      }
      
      // Fallback para RRHH (Aprobada, Rechazada, Borrador, etc.)
      return {
        label: 'Ver detalle',
        route: ['/solicitudes', item.solicitudId || item.id],
        cssClass: 'solicitudes-row-action',
        icon: 'visibility'
      };
    }

    // 3. Rol Solicitante / Área Solicitante
    if (esBorradorOPendiente) {
      return {
        label: 'Completar datos',
        route: ['/solicitudes/editar', item.solicitudId || item.id],
        cssClass: 'solicitudes-row-action action-warning',
        icon: 'edit'
      };
    }

    if (esObservada) {
      return {
        label: 'Corregir',
        route: ['/solicitudes/editar', item.solicitudId || item.id],
        cssClass: 'solicitudes-row-action action-warning',
        icon: 'edit'
      };
    }

    if (esEnviadaORevision) {
      return {
        label: 'Ver seguimiento',
        route: ['/solicitudes', item.solicitudId || item.id],
        cssClass: 'solicitudes-row-action',
        icon: 'visibility'
      };
    }

    // Fallback General (Aprobada, Rechazada, etc.)
    return {
      label: 'Ver detalle',
      route: ['/solicitudes', item.solicitudId || item.id],
      cssClass: 'solicitudes-row-action',
      icon: 'visibility'
    };
  }

  isSolicitante(): boolean {
    if (this.soloMisSolicitudes()) return true;
    const role = this.authService.getUserRole();
    const area = this.authService.getUserArea();
    return role === 'Solicitante' || (role !== 'RRHH' && area !== 'Recursos Humanos' && role !== 'Administrador');
  }
}
