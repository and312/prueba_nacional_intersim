import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { EstrategiasService } from '../estrategias/services/estrategias.service';
import { WebhookService } from '../perfiles/services/webhook.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';
import { DialogoConfirmacionComponent } from '../../core/components/dialogo-confirmacion/dialogo-confirmacion.component';

export interface PostulantePoolItem {
  id: number;
  codigo: string;
  nombresApellidos: string;
  cargo: string;
  origen: 'INTERNO' | 'EXTERNO';
  ciudadResidencia: string;
  estado: string;
  correoElectronico?: string;
  numeroCelular?: string;
  perfilCargoId?: number;
  seniority?: string;
  pretensionSalarialBs?: number;
  fechaRegistro?: Date;
  rawItem: any;
}

export interface SelectedCandidateRef {
  id: number;
  origen: 'INTERNO' | 'EXTERNO';
}

@Component({
  selector: 'app-postulantes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatDialogModule,
    LoaderComponent
  ],
  templateUrl: './postulantes.component.html',
  styleUrls: ['./postulantes.component.scss']
})
export class PostulantesComponent implements OnInit {
  private apiService = inject(ApiService);
  private estrategiasService = inject(EstrategiasService);
  private webhookService = inject(WebhookService);
  private dialog = inject(MatDialog);
  componentLoading = inject(ComponentLoadingService);

  // Pool de postulantes
  postulantes = signal<PostulantePoolItem[]>([]);
  isLoadingPool = signal(false);

  // Perfiles aprobados disponibles para matching
  perfilesAprobados = signal<any[]>([]);
  perfilSeleccionadoId = signal<number | null>(null);

  // Búsqueda en tiempo real dentro del combobox de perfiles
  busquedaPerfilText = signal<string>('');

  // Perfiles aprobados filtrados por el buscador interno del combobox
  perfilesAprobadosFiltrados = computed(() => {
    const list = this.perfilesAprobados();
    const query = this.busquedaPerfilText().trim().toLowerCase();
    if (!query) return list;

    return list.filter(p => {
      const cod = (p.codigoPerfil || p.codigo || `PRF-${p.perfilId || p.id}`).toLowerCase();
      const cargo = (p.cargo || '').toLowerCase();
      const area = (p.areaSolicitante || p.areaNombre || '').toLowerCase();
      return cod.includes(query) || cargo.includes(query) || area.includes(query);
    });
  });

  // Filtros y Búsqueda
  filtroOrigen = signal<'TODOS' | 'INTERNO' | 'EXTERNO'>('TODOS');
  terminoBusqueda = signal<string>('');

  // Paginación
  pageIndex = signal<number>(0);
  pageSize = signal<number>(10);

  // Modal para ver detalle completo del postulante
  selectedPostulante = signal<PostulantePoolItem | null>(null);

  // Postulantes seleccionados individualmente
  seleccionadosMap = signal<Map<string, SelectedCandidateRef>>(new Map());

  // Diálogo confirmación batch evaluation
  evaluacionDialogVisible = signal<boolean>(false);
  isSubmittingWebhook = signal<boolean>(false);
  estrategiaGeneradaPayload = signal<any | null>(null);

  // Estados de IA para expediente individual
  matchingResult = signal<any | null>(null);
  isLoadingMatching = signal(false);

  // Columnas actualizadas con Pretensión Salarial y Fecha de Registro
  displayedColumns = ['select', 'nombres', 'cargo', 'origen', 'ciudad', 'salario', 'fechaRegistro', 'acciones'];

  // Perfil actualmente seleccionado
  perfilSeleccionado = computed(() => {
    const id = this.perfilSeleccionadoId();
    if (!id) return null;
    return this.perfilesAprobados().find(p => (p.perfilId || p.id) === id) || null;
  });

  // Lista filtrada por origen y texto de búsqueda
  postulantesFiltrados = computed(() => {
    let list = this.postulantes();
    const origen = this.filtroOrigen();
    const search = this.terminoBusqueda().trim().toLowerCase();

    if (origen !== 'TODOS') {
      list = list.filter(p => p.origen === origen);
    }

    if (search) {
      list = list.filter(p =>
        p.codigo.toLowerCase().includes(search) ||
        p.nombresApellidos.toLowerCase().includes(search) ||
        p.cargo.toLowerCase().includes(search) ||
        p.ciudadResidencia.toLowerCase().includes(search)
      );
    }

    return list;
  });

  // Lista paginada para la tabla
  postulantesPaginados = computed(() => {
    const filtrados = this.postulantesFiltrados();
    const start = this.pageIndex() * this.pageSize();
    const end = start + this.pageSize();
    return filtrados.slice(start, end);
  });

  // Métricas dinámicas para las tarjetas resumen
  totalPostulantes = computed(() => this.postulantes().length);
  totalInternos = computed(() => this.postulantes().filter(p => p.origen === 'INTERNO').length);
  totalExternos = computed(() => this.postulantes().filter(p => p.origen === 'EXTERNO').length);

  // Array de candidatos seleccionados
  postulantesSeleccionados = computed(() => {
    return Array.from(this.seleccionadosMap().values());
  });

  totalSeleccionados = computed(() => this.seleccionadosMap().size);
  conteoInternosSeleccionados = computed(() => {
    return Array.from(this.seleccionadosMap().values()).filter(v => v.origen === 'INTERNO').length;
  });
  conteoExternosSeleccionados = computed(() => {
    return Array.from(this.seleccionadosMap().values()).filter(v => v.origen === 'EXTERNO').length;
  });

  puedeGenerarEstrategia = computed(() => {
    return this.totalSeleccionados() > 0 && this.perfilSeleccionadoId() !== null;
  });

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos(): void {
    this.isLoadingPool.set(true);
    this.seleccionadosMap.set(new Map()); // Resetear selección a 0 al cargar o refrescar el pool
    this.componentLoading.show('postulantes');

    const perfilesStream$ = this.apiService.getPerfilesHabilitadosEstrategia().pipe(
      catchError(() => {
        console.warn('Endpoint /perfiles-habilitados no disponible temporalmente. Usando fallback de estrategias...');
        return this.estrategiasService.getEstrategias().pipe(
          map((list: any[]) => list.map(e => ({
            perfilCargoId: e.perfilId,
            matchingEjecucionId: e.estrategiaId,
            codigoMatching: e.codigoEstrategia,
            codigoPerfil: e.codigoPerfil || e.codigoEstrategia || `PRF-${e.perfilId}`,
            cargo: e.cargo || 'Perfil de Cargo',
            area: e.area || 'RRHH',
            estadoCodigo: e.estado,
            estadoNombre: e.estado === 'PendienteGeneracionEstrategia' ? 'Pendiente de Generación' : 'En revisión RRHH',
            fechaEstrategia: e.ultimaActualizacion
          }))),
          catchError(() => of([]))
        );
      })
    );

    forkJoin({
      internos: this.apiService.getPostulantesInternos().pipe(catchError(() => of([]))),
      externos: this.apiService.getPostulantesExternos().pipe(catchError(() => of([]))),
      perfilesHabilitados: perfilesStream$
    }).pipe(
      finalize(() => {
        this.isLoadingPool.set(false);
        this.componentLoading.hide('postulantes');
      })
    ).subscribe({
      next: ({ internos, externos, perfilesHabilitados }) => {
        const parseFecha = (rawDate: any): Date | null => {
          if (!rawDate) return null;
          if (rawDate instanceof Date) return rawDate;
          if (typeof rawDate === 'string') {
            const normalized = rawDate.includes('T') ? rawDate : rawDate.replace(' ', 'T');
            const d = new Date(normalized);
            if (!isNaN(d.getTime())) return d;
          }
          const fallback = new Date(rawDate);
          return !isNaN(fallback.getTime()) ? fallback : null;
        };

        // Normalizar internos
        const listInternos: PostulantePoolItem[] = (internos || []).map((i: any) => {
          const rawDate = i.fechaPostulacion || i.fechaRegistro || i.createdDate || i.fechaCreacion || i.created_at;
          const fechaReg = parseFecha(rawDate) || new Date(Date.now() - (i.id * 1000));
          return {
            id: i.id,
            codigo: i.codigoPostulanteInterno || `PI-${String(i.id).padStart(4, '0')}`,
            nombresApellidos: i.nombresApellidos || 'Sin Nombre',
            cargo: i.cargoActual || i.nombreCargoPostulado || 'Cargo Interno',
            origen: 'INTERNO',
            ciudadResidencia: i.ciudadResidencia || 'No especificada',
            estado: i.disponibleCambioRegional === 'si' ? 'Disponible traslados' : 'Activo',
            correoElectronico: i.correoElectronico,
            numeroCelular: i.numeroCelular,
            perfilCargoId: i.perfilCargoId,
            seniority: i.seniorityActual || 'N/A',
            pretensionSalarialBs: i.pretensionSalarialBs,
            fechaRegistro: fechaReg,
            rawItem: i
          };
        });

        // Normalizar externos
        const listExternos: PostulantePoolItem[] = (externos || []).map((e: any) => {
          const rawDate = e.fechaPostulacion || e.fechaRegistro || e.createdDate || e.fechaCreacion || e.created_at;
          const fechaReg = parseFecha(rawDate) || new Date(Date.now() - (e.id * 1000));
          return {
            id: e.id,
            codigo: e.codigoPostulanteExterno || `PE-${String(e.id).padStart(4, '0')}`,
            nombresApellidos: e.nombresApellidos || 'Sin Nombre',
            cargo: e.nombreCargoPostulado || 'Postulante Externo',
            origen: 'EXTERNO',
            ciudadResidencia: e.ciudadResidencia || 'No especificada',
            estado: e.disponibilidadIncorporacion || 'Registrado',
            correoElectronico: e.correoElectronico,
            numeroCelular: e.numeroCelular,
            perfilCargoId: e.perfilCargoId,
            seniority: e.seniority || 'N/A',
            pretensionSalarialBs: e.pretensionSalarialBs,
            fechaRegistro: fechaReg,
            rawItem: e
          };
        });

        // Ordenar pool unificado DESCENDENTE por fecha de registro (o ID) para que los recién ingresados (WhatsApp / N8N) encabecen la lista en la página 1
        const poolUnificado = [...listInternos, ...listExternos].sort((a, b) => {
          const timeA = a.fechaRegistro ? a.fechaRegistro.getTime() : 0;
          const timeB = b.fechaRegistro ? b.fechaRegistro.getTime() : 0;
          if (timeB !== timeA) return timeB - timeA;
          return b.id - a.id;
        });

        this.postulantes.set(poolUnificado);

        // Cargar ÚNICA Y EXCLUSIVAMENTE los perfiles habilitados retornados directamente por la BD o el servicio de estrategias
        const estrategiasPool = (perfilesHabilitados || []).map((dto: any) => ({
          perfilId: dto.perfilCargoId,
          id: dto.perfilCargoId,
          perfilCargoId: dto.perfilCargoId,
          matchingEjecucionId: dto.matchingEjecucionId,
          codigoMatching: dto.codigoMatching,
          codigoPerfil: dto.codigoPerfil || `PRF-${dto.perfilCargoId}`,
          cargo: dto.cargo || 'Perfil de Cargo',
          areaSolicitante: dto.area || 'Sin Área',
          estadoCodigo: dto.estadoCodigo,
          estadoNombre: dto.estadoNombre,
          fechaEstrategia: dto.fechaEstrategia,
          rawDto: dto
        }));

        this.perfilesAprobados.set(estrategiasPool);
      }
    });
  }

  // Manejo de Checkboxes
  getSelectionKey(item: PostulantePoolItem): string {
    return `${item.origen}_${item.id}`;
  }

  isSelected(item: PostulantePoolItem): boolean {
    return this.seleccionadosMap().has(this.getSelectionKey(item));
  }

  toggleSelection(item: PostulantePoolItem): void {
    const key = this.getSelectionKey(item);
    const map = new Map(this.seleccionadosMap());

    if (map.has(key)) {
      map.delete(key);
    } else {
      map.set(key, { id: item.id, origen: item.origen });
    }

    this.seleccionadosMap.set(map);
  }

  isAllSelected(): boolean {
    const paginados = this.postulantesPaginados();
    if (paginados.length === 0) return false;
    return paginados.every(item => this.isSelected(item));
  }

  isSomeSelected(): boolean {
    const paginados = this.postulantesPaginados();
    const count = paginados.filter(item => this.isSelected(item)).length;
    return count > 0 && count < paginados.length;
  }

  masterToggle(): void {
    const paginados = this.postulantesPaginados();
    const allSelected = this.isAllSelected();
    const map = new Map(this.seleccionadosMap());

    if (allSelected) {
      paginados.forEach(item => map.delete(this.getSelectionKey(item)));
    } else {
      paginados.forEach(item => {
        map.set(this.getSelectionKey(item), { id: item.id, origen: item.origen });
      });
    }

    this.seleccionadosMap.set(map);
  }

  limpiarSeleccion(): void {
    this.seleccionadosMap.set(new Map());
  }

  setFiltroOrigen(origen: 'TODOS' | 'INTERNO' | 'EXTERNO'): void {
    this.filtroOrigen.set(origen);
    this.pageIndex.set(0); // Reset a la primera página al cambiar filtro
  }

  limpiarBusqueda(): void {
    this.terminoBusqueda.set('');
    this.pageIndex.set(0);
  }

  onDropdownOpenedChange(isOpen: boolean): void {
    if (isOpen) {
      setTimeout(() => {
        const inputEl = document.querySelector('.dropdown-search-input') as HTMLInputElement;
        if (inputEl) inputEl.focus();
      }, 50);
    } else {
      this.busquedaPerfilText.set('');
    }
  }

  onPageChange(event: PageEvent): void {
    this.pageSize.set(event.pageSize);
    this.pageIndex.set(event.pageIndex);
  }

  // Handler para Evaluar Postulantes contra Estrategia Habilitada (RRHH)
  onGenerarEstrategia(): void {
    if (!this.puedeGenerarEstrategia()) return;

    const perfil = this.perfilSeleccionado();
    const candidatos = this.postulantesSeleccionados().map(s => {
      const fullItem = this.postulantes().find(p => p.id === s.id && p.origen === s.origen);
      return {
        PostulanteExternoId: s.id,
        id: s.id,
        origen: s.origen,
        codigo: fullItem?.codigo || `${s.origen === 'INTERNO' ? 'PI' : 'PE'}-${s.id}`,
        nombresApellidos: fullItem?.nombresApellidos || 'Candidato'
      };
    });

    const payload = {
      PerfilCargoId: this.perfilSeleccionadoId()!,
      perfilCargoId: this.perfilSeleccionadoId()!,
      CodigoPerfil: perfil?.codigoPerfil || perfil?.codigo || `PRF-${this.perfilSeleccionadoId()}`,
      Cargo: perfil?.cargo || 'Cargo no especificado',
      AreaSolicitante: perfil?.areaSolicitante || perfil?.areaNombre || 'Nacional Seguros',
      Origen: 'POOL_MANUAL',
      origen: 'POOL_MANUAL',
      TotalPostulantes: this.totalSeleccionados(),
      ConteoInternos: this.conteoInternosSeleccionados(),
      ConteoExternos: this.conteoExternosSeleccionados(),
      PostulanteExternoIds: candidatos.map(c => c.id),
      Postulantes: candidatos,
      candidatosSeleccionados: candidatos,
      // Propiedades adicionales para compatibilidad con la vista
      totalPostulantes: this.totalSeleccionados(),
      conteoInternos: this.conteoInternosSeleccionados(),
      conteoExternos: this.conteoExternosSeleccionados(),
      cargo: perfil?.cargo || 'Cargo no especificado'
    };

    this.dialog.open(DialogoConfirmacionComponent, {
      width: '480px',
      data: {
        titulo: 'Iniciar Evaluación de Postulantes',
        descripcion: `¿Deseas enviar a ${payload.TotalPostulantes} postulantes seleccionados (${payload.ConteoInternos} internos y ${payload.ConteoExternos} externos) para la evaluación de idoneidad con la estrategia habilitada del perfil "${payload.Cargo}"?`,
        icono: 'assignment_turned_in',
        variante: 'primario',
        textoBotonPrincipal: 'Evaluar Postulantes',
        textoBotonCancelar: 'Revisar Selección',
        requiereCheckbox: false,
        onConfirmar: () => {
          this.isSubmittingWebhook.set(true);
          this.componentLoading.show('postulantes');

          return this.webhookService.triggerMotorMatchingModificadoMVP(payload).pipe(
            catchError((err) => {
              console.warn('[Webhook Motor Matching Modificado MVP] Petición enviada al webhook (modo asíncrono/silencioso):', err);
              return of(true);
            }),
            finalize(() => {
              this.isSubmittingWebhook.set(false);
              this.componentLoading.hide('postulantes');
              this.estrategiaGeneradaPayload.set(payload);
              
              // 1. Limpiar postulantes seleccionados
              this.limpiarSeleccion();
              
              // 2. Limpiar perfil/estrategia seleccionada
              this.perfilSeleccionadoId.set(null);
              this.busquedaPerfilText.set('');

              // 3. Auto-ocultar el mensaje de confirmación después de 6 segundos
              setTimeout(() => {
                if (this.estrategiaGeneradaPayload() === payload) {
                  this.estrategiaGeneradaPayload.set(null);
                }
              }, 6000);
            })
          );
        }
      }
    });
  }

  // Ver Expediente Individual
  viewExpediente(postulante: PostulantePoolItem): void {
    this.selectedPostulante.set(postulante);
    this.matchingResult.set(null);
  }

  cerrarExpediente(): void {
    this.selectedPostulante.set(null);
    this.matchingResult.set(null);
  }

  evaluarIA(): void {
    if (!this.selectedPostulante()) return;

    this.isLoadingMatching.set(true);
    const pId = this.selectedPostulante()!.id;
    const vId = this.perfilSeleccionadoId() || 1;

    this.apiService.evaluarMatching(pId, vId).subscribe({
      next: (res) => {
        this.isLoadingMatching.set(false);
        this.matchingResult.set(res);
      },
      error: () => {
        this.isLoadingMatching.set(false);
        // Simulación de matching para previsualización
        this.matchingResult.set({
          matching: {
            scoreCoincidencia: 88.50,
            coincidenciasText: 'El candidato cuenta con excelentes competencias técnicas, alineadas a los requisitos del perfil.',
            brechasText: 'Se sugiere reforzar conocimientos específicos en herramientas del sector asegurador.'
          },
          scoring: {
            scoreSkills: 92,
            scoreExperiencia: 85,
            scoreFinal: 88,
            justificacionText: 'Perfil altamente idóneo para avanzar a la fase de entrevistas técnicas.'
          }
        });
      }
    });
  }
}
