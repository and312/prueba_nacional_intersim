import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../../core/services/component-loading.service';

// Modelos y Constantes
import { ProfileListItem, ProfileListSummary, ProfileListQuery } from '../models/profile.model';
import { ProfileStatus } from '../models/profile-status';
import { PROFILE_ROLE_CONFIGS, ProfileRoleConfig } from '../constants/profile-role.config';

// Servicios
import { ProfilesListService } from '../services/profiles-list.service';

// Subcomponentes
import { ProfileSummaryCardsComponent } from '../components/profile-summary-cards/profile-summary-cards.component';
import { ProfileFiltersComponent } from '../components/profile-filters/profile-filters.component';
import { ProfileTableComponent } from '../components/profile-table/profile-table.component';

@Component({
  selector: 'app-perfiles-list',
  standalone: true,
  imports: [
    CommonModule,
    LoaderComponent,
    ProfileSummaryCardsComponent,
    ProfileFiltersComponent,
    ProfileTableComponent
  ],
  templateUrl: './perfiles-list.component.html',
  styleUrls: ['./perfiles-list.component.scss']
})
export class PerfilesListComponent implements OnInit {
  authService = inject(AuthService);
  private profilesService = inject(ProfilesListService);
  private router = inject(Router);
  componentLoading = inject(ComponentLoadingService);

  // Determinar rol y área
  userRole = signal<'RRHH' | 'AreaSol' | 'Administrador'>('AreaSol');
  userArea = signal<string>('');
  
  // Configuración de pantalla según rol
  roleConfig = computed<ProfileRoleConfig>(() => {
    const role = this.userRole();
    return PROFILE_ROLE_CONFIGS[role] || PROFILE_ROLE_CONFIGS.AreaSol;
  });

  // Señal Maestra con todos los perfiles de la base de datos
  allProfiles = signal<ProfileListItem[]>([]);
  summary = signal<ProfileListSummary | null>(null);
  isLoading = signal(false);
  isError = signal(false);
  errorMessage = signal('');
  successMessage = signal<string | null>(null);

  // Paginación y Filtros
  currentPage = signal(1);
  pageSize = signal(8);
  pageSizeOptions = [5, 8, 10, 20, 50];
  isPageSizeMenuOpen = signal(false);
  
  // Guardar filtros activos
  activeFilters = signal<Partial<ProfileListQuery>>({});
  soloMisPerfiles = signal<boolean>(false);
  activeCardKey = signal<string | null>(null);

  // Computed Signal: Filtrado Reactivo Instantáneo en Memoria (0ms Latencia)
  filteredProfiles = computed(() => {
    let list = [...this.allProfiles()];
    const userRole = this.userRole();
    const userArea = this.userArea();

    if (userRole === 'AreaSol') {
      list = list.filter(p => p.areaNombre.toLowerCase() === userArea.toLowerCase());
    }

    const filters = this.activeFilters();

    // 1. Filtro por Tarjeta KPI activa
    const cardKey = this.activeCardKey();
    if (cardKey) {
      switch (cardKey) {
        case 'enRevisionRRHH':
          list = list.filter(p => p.estado === ProfileStatus.EnRevisionRRHHPE);
          break;
        case 'observadosPorCorregir':
          list = list.filter(p => p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida);
          break;
        case 'pendientesAprobacionFinal':
        case 'aprobadosPorArea':
          list = list.filter(p => p.estado === ProfileStatus.Aprobada);
          break;
        case 'pendientesRevisionArea':
          list = list.filter(p => p.estado === ProfileStatus.EnRevisionAreaSol || p.estado === ProfileStatus.ResumenEjecutivoGenerado);
          break;
        case 'perfilesFinalizados':
          list = list.filter(p => p.estado === ProfileStatus.PerfilAprobadoFinal);
          break;
      }
    }

    // 2. Buscador por código, solicitud, cargo, área o solicitante
    if (filters.search) {
      const query = filters.search.toLowerCase().trim();
      list = list.filter(p =>
        (p.codigo || '').toLowerCase().includes(query) ||
        (p.solicitudCodigo || '').toLowerCase().includes(query) ||
        (p.cargo || '').toLowerCase().includes(query) ||
        (p.areaNombre || '').toLowerCase().includes(query) ||
        (p.solicitante || '').toLowerCase().includes(query)
      );
    }

    // 3. Filtro por Estado (desde dropdown)
    if (filters.estado && !cardKey) {
      if (filters.estado === ProfileStatus.EnRevisionAreaSol) {
        list = list.filter(p => p.estado === ProfileStatus.EnRevisionAreaSol || p.estado === ProfileStatus.ResumenEjecutivoGenerado);
      } else if (filters.estado === ProfileStatus.Observada) {
        list = list.filter(p => p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida);
      } else {
        list = list.filter(p => p.estado === filters.estado);
      }
    }

    // 4. Filtro por Área
    if (filters.areaId) {
      list = list.filter(p => p.areaId === Number(filters.areaId));
    }

    // 6. Rangos de fecha
    if (filters.fechaDesde) {
      const desde = new Date(filters.fechaDesde);
      list = list.filter(p => new Date(p.ultimaActualizacion) >= desde);
    }
    if (filters.fechaHasta) {
      const hasta = new Date(filters.fechaHasta);
      hasta.setHours(23, 59, 59, 999);
      list = list.filter(p => new Date(p.ultimaActualizacion) <= hasta);
    }

    return list;
  });

  // Computed Signal: Paginación Reactiva en Memoria (0ms Latencia)
  paginatedProfiles = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    return this.filteredProfiles().slice(start, start + this.pageSize());
  });

  totalItems = computed(() => this.filteredProfiles().length);

  // Rangos calculados para paginación
  totalPages = computed(() => {
    const pages = Math.ceil(this.totalItems() / this.pageSize());
    return pages > 0 ? pages : 1;
  });

  rangeStart = computed(() => {
    if (this.totalItems() === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize() + 1;
  });

  rangeEnd = computed(() => {
    if (this.totalItems() === 0) return 0;
    return Math.min(this.currentPage() * this.pageSize(), this.totalItems());
  });

  ngOnInit(): void {
    this.determineUserRole();
    const role = this.userRole();
    if (role === 'RRHH' || role === 'Administrador') {
      this.soloMisPerfiles.set(false);
    } else {
      this.soloMisPerfiles.set(true);
    }
    this.loadData();
  }

  setSoloMisPerfiles(val: boolean): void {
    this.soloMisPerfiles.set(val);
    this.currentPage.set(1);
    this.loadData();
  }

  determineUserRole(): void {
    const role = this.authService.getUserRole();
    const area = this.authService.getUserArea();

    if (role === 'Administrador') {
      this.userRole.set('Administrador');
    } else if (role === 'RRHH' || role === 'Reclutador' || area === 'Recursos Humanos') {
      this.userRole.set('RRHH');
    } else if (role === 'Solicitante' || role === 'AreaSol') {
      this.userRole.set('AreaSol');
    } else {
      this.userRole.set('AreaSol');
    }

    this.userArea.set(area || 'Área Solicitante');
    console.log(`[PerfilesList] Determinado rol: ${this.userRole()} y área: ${this.userArea()}`);
  }

  loadData(): void {
    this.isLoading.set(true);
    this.isError.set(false);
    this.componentLoading.show('perfiles-list');

    const filterQuery: ProfileListQuery = {
      page: 1,
      pageSize: 500,
      soloMisPerfiles: this.soloMisPerfiles()
    };

    // Cargar Lista Paginada Maestra
    this.profilesService.getProfiles(filterQuery, this.userRole(), this.userArea()).subscribe({
      next: (res) => {
        this.allProfiles.set(res.items);
        this.isLoading.set(false);
        this.componentLoading.hide('perfiles-list');
      },
      error: (err) => {
        console.error('Error detallado al cargar perfiles:', err);
        this.isError.set(true);
        this.errorMessage.set('No fue posible cargar los perfiles. Intenta nuevamente.');
        this.isLoading.set(false);
        this.componentLoading.hide('perfiles-list');
      }
    });

    // Cargar Resumen de Métricas
    this.profilesService.getSummary(this.userRole(), this.userArea()).subscribe({
      next: (sum) => {
        this.summary.set(sum);
      },
      error: () => {}
    });
  }

  onCardSelect(key: string): void {
    if (this.activeCardKey() === key && key !== 'totalEnProceso' && key !== 'perfilesFinalizados') {
      this.activeCardKey.set(null);
      this.activeFilters.update(f => ({ ...f, estado: undefined }));
    } else {
      this.activeCardKey.set(key);
      let targetEstado: ProfileStatus | undefined = undefined;

      switch (key) {
        case 'enRevisionRRHH':
          targetEstado = ProfileStatus.EnRevisionRRHHPE;
          break;
        case 'observadosPorCorregir':
          targetEstado = ProfileStatus.Observada;
          break;
        case 'pendientesAprobacionFinal':
        case 'aprobadosPorArea':
          targetEstado = ProfileStatus.Aprobada;
          break;
        case 'pendientesRevisionArea':
          targetEstado = ProfileStatus.EnRevisionAreaSol;
          break;
        case 'perfilesFinalizados':
          targetEstado = ProfileStatus.PerfilAprobadoFinal;
          break;
        case 'totalEnProceso':
        default:
          targetEstado = undefined;
          break;
      }

      this.activeFilters.update(f => ({ ...f, estado: targetEstado }));
    }

    this.currentPage.set(1);
    // Filtrado puramente reactivo en memoria: 0ms latencia, sin activador de spinner
  }

  onFilterChange(newFilters: Partial<ProfileListQuery>): void {
    if (newFilters.estado !== this.activeFilters().estado) {
      this.activeCardKey.set(null);
    }
    this.activeFilters.set(newFilters);
    this.currentPage.set(1);
    // Filtrado puramente reactivo en memoria: 0ms latencia, sin activador de spinner
  }

  handleActionClick(event: { action: string; profile: ProfileListItem }): void {
    const { action, profile } = event;
    const queryParams = this.soloMisPerfiles() ? { modo: 'solicitante' } : {};

    if (action === 'revisar') {
      this.router.navigate(['/perfiles', profile.id, 'revisar'], { queryParams });
    } else if (action === 'corregir') {
      this.router.navigate(['/perfiles', profile.id, 'corregir'], { queryParams });
    } else if (action === 'aprobar_final') {
      this.isLoading.set(true);
      this.profilesService.aprobarPerfilFinal(profile.id).subscribe({
        next: (success) => {
          this.isLoading.set(false);
          if (success) {
            this.showSuccessNotification(`El perfil ${profile.codigo} fue aprobado finalmente con éxito.`);
            this.loadData();
          } else {
            alert('No se pudo aprobar finalmente el perfil.');
          }
        },
        error: () => {
          this.isLoading.set(false);
          alert('Error al intentar realizar la aprobación final.');
        }
      });
    } else if (action === 'detalle' || action === 'seguimiento' || action === 'observacion' || action === 'revisar_aprobar_final') {
      this.router.navigate(['/perfiles', profile.id], { queryParams });
    }
  }

  retry(): void {
    this.loadData();
  }

  // Notificaciones Simples
  showSuccessNotification(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 5000);
  }

  showTodoAlert(featureDescription: string): void {
    console.log(`[TODO] ${featureDescription}`);
    alert(`[TODO] Acción: ${featureDescription}\nEsta funcionalidad se conectará en la siguiente fase de desarrollo.`);
  }

  // Controladores de Paginación
  togglePageSizeMenu(): void {
    this.isPageSizeMenuOpen.set(!this.isPageSizeMenuOpen());
  }

  updatePageSizeValue(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.isPageSizeMenuOpen.set(false);
  }

  goToFirstPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(1);
    }
  }

  goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.currentPage() + 1);
    }
  }

  goToLastPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.totalPages());
    }
  }

  toggleErrorMode(event: any): void {
    this.profilesService.setShouldFail(event.target.checked);
    this.loadData();
  }
}
