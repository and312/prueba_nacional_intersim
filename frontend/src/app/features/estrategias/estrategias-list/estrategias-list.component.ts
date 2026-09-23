import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { AuthService } from '../../../core/services/auth.service';
import { EstrategiasService } from '../services/estrategias.service';
import {
  EstrategiaItem,
  EstrategiaSummary,
  EstadoEstrategia,
  ESTADO_ESTRATEGIA_CONFIG
} from '../models/estrategia.model';

@Component({
  selector: 'app-estrategias-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LoaderComponent,
    DatePipe
  ],
  templateUrl: './estrategias-list.component.html',
  styleUrls: ['./estrategias-list.component.scss']
})
export class EstrategiasListComponent implements OnInit {
  private estrategiasService = inject(EstrategiasService);
  private authService = inject(AuthService);
  private router = inject(Router);

  // States & Signals
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);
  rawItems = signal<EstrategiaItem[]>([]);
  
  // Filters
  searchTerm = signal<string>('');
  selectedEstado = signal<string>('');
  selectedArea = signal<string>('');
  selectedCargo = signal<string>('');

  // Pagination
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);
  pageSizeOptions = [10, 25, 50];
  isPageSizeMenuOpen = signal<boolean>(false);

  // User Role Check
  userRole = computed(() => this.authService.getUserRole());
  isRRHH = computed(() => this.userRole() === 'RRHH');
  isAdmin = computed(() => this.userRole() === 'Administrador');

  // Summary Metrics
  summary = computed<EstrategiaSummary>(() =>
    this.estrategiasService.getSummary(this.rawItems())
  );

  // Filtered List (Ordenado por última actualización / ID descendente - más recientes al inicio)
  filteredItems = computed(() => {
    let result = [...this.rawItems()];
    const search = this.searchTerm().trim().toLowerCase();
    const estado = this.selectedEstado();
    const area = this.selectedArea();
    const cargo = this.selectedCargo();

    if (search) {
      result = result.filter(
        item =>
          item.codigoEstrategia.toLowerCase().includes(search) ||
          item.codigoPerfil.toLowerCase().includes(search) ||
          item.cargo.toLowerCase().includes(search) ||
          item.area.toLowerCase().includes(search)
      );
    }

    if (estado) {
      result = result.filter(item => item.estado === estado);
    }

    if (area) {
      result = result.filter(item => item.area === area);
    }

    if (cargo) {
      result = result.filter(item => item.cargo === cargo);
    }

    result.sort((a, b) => {
      const tA = this.parseFechaToTimestamp(a.ultimaActualizacion) || a.estrategiaId;
      const tB = this.parseFechaToTimestamp(b.ultimaActualizacion) || b.estrategiaId;
      return tB - tA;
    });

    return result;
  });

  private parseFechaToTimestamp(fechaStr?: string): number {
    if (!fechaStr) return 0;
    const trimmed = fechaStr.trim();
    if (trimmed.includes('/')) {
      const parts = trimmed.split(' ');
      const dateParts = parts[0].split('/');
      if (dateParts.length === 3) {
        const day = parseInt(dateParts[0], 10);
        const month = parseInt(dateParts[1], 10) - 1;
        const year = parseInt(dateParts[2], 10);

        let hours = 0;
        let minutes = 0;
        if (parts[1]) {
          const timeParts = parts[1].split(':');
          if (timeParts.length >= 2) {
            hours = parseInt(timeParts[0], 10);
            minutes = parseInt(timeParts[1], 10);
          }
        }
        return new Date(year, month, day, hours, minutes).getTime();
      }
    }
    const ts = new Date(trimmed).getTime();
    return isNaN(ts) ? 0 : ts;
  }

  // Unique lists for filter dropdowns
  areasList = computed(() => {
    const set = new Set(this.rawItems().map(i => i.area));
    return Array.from(set).sort();
  });

  cargosList = computed(() => {
    const set = new Set(this.rawItems().map(i => i.cargo));
    return Array.from(set).sort();
  });

  // Paginated Items
  paginatedItems = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredItems().slice(start, start + this.pageSize());
  });

  totalItems = computed(() => this.filteredItems().length);
  totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize()) || 1);

  rangeStart = computed(() => (this.totalItems() === 0 ? 0 : (this.currentPage() - 1) * this.pageSize() + 1));
  rangeEnd = computed(() => Math.min(this.currentPage() * this.pageSize(), this.totalItems()));

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.estrategiasService.getEstrategias().subscribe({
      next: (items) => {
        this.rawItems.set(items);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar ejecuciones:', err);
        this.errorMessage.set('Ocurrió un problema al consultar las ejecuciones de matching.');
        this.isLoading.set(false);
      }
    });
  }

  onSearchChange(term: string): void {
    this.searchTerm.set(term);
    this.currentPage.set(1);
  }

  onEstadoChange(estado: string): void {
    this.selectedEstado.set(estado);
    this.currentPage.set(1);
  }

  onAreaChange(area: string): void {
    this.selectedArea.set(area);
    this.currentPage.set(1);
  }

  onCargoChange(cargo: string): void {
    this.selectedCargo.set(cargo);
    this.currentPage.set(1);
  }

  limpiarFiltros(): void {
    this.searchTerm.set('');
    this.selectedEstado.set('');
    this.selectedArea.set('');
    this.selectedCargo.set('');
    this.currentPage.set(1);
  }

  hasActiveFilters(): boolean {
    return !!(this.searchTerm() || this.selectedEstado() || this.selectedArea() || this.selectedCargo());
  }

  getEstadoConfig(estado: EstadoEstrategia) {
    return ESTADO_ESTRATEGIA_CONFIG[estado] || {
      label: estado,
      chipClass: 'chip-pendiente'
    };
  }

  verEstrategia(id: number): void {
    this.router.navigate(['/estrategias', id]);
  }

  // Pagination Handlers
  togglePageSizeMenu(): void {
    this.isPageSizeMenuOpen.update(v => !v);
  }

  updatePageSizeValue(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.isPageSizeMenuOpen.set(false);
  }

  goToFirstPage(): void {
    this.currentPage.set(1);
  }

  goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
    }
  }

  goToLastPage(): void {
    this.currentPage.set(this.totalPages());
  }
}
