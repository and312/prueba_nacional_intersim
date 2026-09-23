import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { Observable, of, forkJoin } from 'rxjs';
import { timeout, catchError, concatMap, tap, finalize } from 'rxjs/operators';

// Modelos y Enums
import { EstadoPerfil } from '../models/estado-perfil.enum';
import { DetallePerfil, PermisosPerfil, ObservacionPerfil, SeccionPerfil, AccionCabeceraPerfil, PerfilDocumento, EstadoObservacionPerfil } from '../models/perfil.model';
import { InsumoResumenPerfil } from '../models/resumen-ejecutivo-perfil.model';

// Servicios
import { PerfilesService } from '../services/perfiles.service';
import { ObservacionesPerfilService } from '../services/observaciones-perfil.service';
import { AuthService } from '../../../core/services/auth.service';
import { ComponentLoadingService } from '../../../core/services/component-loading.service';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { ApiService } from '../../../core/services/api.service';
import { WebhookService } from '../services/webhook.service';
import { normalizeProfileList } from '../../../core/utils/profile-list-normalizer.utils';

// Subcomponentes
import { EncabezadoPerfilComponent } from '../components/encabezado-perfil/encabezado-perfil.component';
import { PestanasPerfilComponent } from '../components/pestanas-perfil/pestanas-perfil.component';
import { PerfilEstructuradoComponent } from '../components/perfil-estructurado/perfil-estructurado.component';
import { ObservacionesPerfilComponent } from '../components/observaciones-perfil/observaciones-perfil.component';
import { TrazabilidadPerfilComponent } from '../components/trazabilidad-perfil/trazabilidad-perfil.component';
import { DocumentoPerfilComponent } from '../components/documento-perfil/documento-perfil.component';
import { ResumenEjecutivoPerfilComponent } from '../components/resumen-ejecutivo-perfil/resumen-ejecutivo-perfil.component';
import { InsumosResumenPerfilComponent } from '../components/insumos-resumen-perfil/insumos-resumen-perfil.component';
import { EstadoProcesoPerfilComponent } from '../components/estado-proceso-perfil/estado-proceso-perfil.component';
import { ObservacionesRecibidasPerfilComponent } from '../components/observaciones-recibidas-perfil/observaciones-recibidas-perfil.component';
import { DialogoConfirmacionComponent } from '../../../core/components/dialogo-confirmacion/dialogo-confirmacion.component';
import { ConfiguracionDialogoConfirmacion } from '../../../core/components/dialogo-confirmacion/configuracion-dialogo-confirmacion.model';

@Component({
  selector: 'app-detalle-perfil',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    LoaderComponent,
    EncabezadoPerfilComponent,
    PestanasPerfilComponent,
    PerfilEstructuradoComponent,
    ObservacionesPerfilComponent,
    TrazabilidadPerfilComponent,
    DocumentoPerfilComponent,
    ResumenEjecutivoPerfilComponent,
    InsumosResumenPerfilComponent,
    EstadoProcesoPerfilComponent,
    ObservacionesRecibidasPerfilComponent
  ],
  templateUrl: './detalle-perfil.component.html',
  styleUrls: ['./detalle-perfil.component.scss']
})
export class DetallePerfilComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private perfilesService = inject(PerfilesService);
  private observacionesService = inject(ObservacionesPerfilService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private apiService = inject(ApiService);
  private webhookService = inject(WebhookService);
  componentLoading = inject(ComponentLoadingService);

  // Estados de carga e interfaz
  perfilId = 0;
  perfil = signal<DetallePerfil | null>(null);
  observaciones = signal<ObservacionPerfil[]>([]);
  isLoading = signal(false);
  isError = signal(false);
  errorMessage = signal('');
  successMessage = signal<string | null>(null);
  errorMessageDetalle = signal<string | null>(null);
  processingMessage = signal<string | null>(null);
  mostrarBotonReintentarResumen = signal(false);
  private pollingInterval: any = null;
  private autoRefreshTimer: any = null;
  private visibilityHandler: (() => void) | null = null;
  private focusHandler: (() => void) | null = null;
  private routeSub: any = null;
  private queryParamsSub: any = null;

  // Control de rol y modo edición
  userRole = signal<'RRHH' | 'AreaSol' | 'Administrador'>('AreaSol');
  userArea = signal('');
  modoEdicion = signal(false);
  pestanaActiva = signal<'resumen' | 'estructurado'>('resumen'); // Por defecto Resumen ejecutivo
  insumos = signal<InsumoResumenPerfil[]>([]);
  formTieneCambios = signal(false);
  areasList = signal<any[]>([]);

  // Formulario reactivo principal
  perfilForm!: FormGroup;

  puedeEditarPerfil(): boolean {
    const rol = this.userRole();
    const p = this.perfil();
    if (!p) return false;
    const esRrhhOAdmin = rol === 'RRHH' || rol === 'Administrador';
    const code = p.estadoCodigo;
    return esRrhhOAdmin && (
      code === 'PERF-REV-RRHH' || 
      code === 'PERF-COR-RRHH' || 
      code === 'PERF-RES-GEN' || 
      code === 'PERF-OBS-AREA' ||
      code === 'EnRevisionRRHH' ||
      code === 'PerfilCorregidoRRHH' ||
      code === 'ObservadoAreaSol' ||
      code === 'ObservadoSolicitante'
    );
  }

  // Permisos dinámicos del perfil
  permisos = computed<PermisosPerfil>(() => {
    const rol = this.userRole();
    const p = this.perfil();
    const esEditMode = this.modoEdicion();
    
    if (!p) {
      return {
        puedeEditar: false,
        puedeGuardar: false,
        puedeEnviarAreaSolicitante: false,
        puedeEnviarCorreccionesArea: false,
        puedeAprobarArea: false,
        puedeRegistrarObservaciones: false,
        puedeAprobarFinal: false,
        puedeVerPdf: false
      };
    }

    const esRrhhOAdmin = rol === 'RRHH' || rol === 'Administrador';
    const esAreaSol = rol === 'AreaSol';

    // Validación robusta del Área Solicitante
    const perteneceAlArea = esAreaSol && this.checkUserAreaMatch(p);

    // Formulario sucio / modificado
    const tieneCambiosForm = this.formTieneCambios();

    // Acciones basadas en estadoCodigo (fuente de verdad técnica)
    const code = p.estadoCodigo;

    return {
      puedeEditar: this.puedeEditarPerfil() && !esEditMode,
      puedeGuardar: esEditMode && tieneCambiosForm && this.puedeEditarPerfil(),
      puedeEnviarAreaSolicitante: esRrhhOAdmin && (
        code === 'PERF-REV-RRHH' || 
        code === 'PERF-RES-GEN' || 
        code === 'PERF-COR-RRHH' || 
        code === 'PERF-OBS-AREA' ||
        code === 'EnRevisionRRHH' ||
        code === 'PerfilCorregidoRRHH' ||
        code === 'ObservadoAreaSol' ||
        code === 'ObservadoSolicitante'
      ) && !esEditMode,
      puedeEnviarCorreccionesArea: esRrhhOAdmin && (code === 'PERF-COR-RRHH' || code === 'PerfilCorregidoRRHH') && !esEditMode,
      puedeAprobarArea: perteneceAlArea && (code === 'PERF-REV-AREA' || code === 'EnRevisionAreaSolicitante'),
      puedeRegistrarObservaciones: perteneceAlArea && (code === 'PERF-REV-AREA' || code === 'EnRevisionAreaSolicitante'),
      puedeAprobarFinal: esRrhhOAdmin && (code === 'PERF-APR-AREA' || code === 'AprobadoAreaSolicitante'),
      puedeVerPdf: true
    };
  });

  hayObservacionesPendientes = computed(() => {
    const list = this.observaciones();
    if (!list || list.length === 0) return false;
    return list.some(o => 
      o.estado === EstadoObservacionPerfil.Pendiente || 
      (typeof o.estado === 'string' && (o.estado as string).toUpperCase() === 'PENDIENTE')
    );
  });

  // Acciones de cabecera unificadas
  accionesCabecera = computed<AccionCabeceraPerfil[]>(() => {
    const p = this.perfil();
    if (!p) return [];

    const rol = this.userRole();
    const pestana = this.pestanaActiva();
    const esRrhhOAdmin = rol === 'RRHH' || rol === 'Administrador';
    const esAreaSol = rol === 'AreaSol';
    const isProcessing = this.isLoading() || !!this.processingMessage();
    const tienePendientes = this.hayObservacionesPendientes();

    const acciones: AccionCabeceraPerfil[] = [];

    if (pestana === 'resumen') {
      if (esRrhhOAdmin) {
        acciones.push({
          id: 'aprobarFinal',
          etiqueta: 'Aprobar perfil final',
          icono: 'verified',
          claseCss: 'exito',
          deshabilitado: !this.permisos().puedeAprobarFinal || tienePendientes || isProcessing,
          tooltip: tienePendientes
            ? 'Debe marcar como resueltas todas las observaciones antes de aprobar el perfil.'
            : undefined
        });
      } else if (esAreaSol) {
        if (this.permisos().puedeAprobarArea) {
          acciones.push({
            id: 'aprobarArea',
            etiqueta: 'Aprobar perfil',
            icono: 'thumb_up',
            claseCss: 'exito',
            deshabilitado: tienePendientes || isProcessing,
            tooltip: tienePendientes
              ? 'Tiene observaciones registradas pendientes de envío. Envíelas a RRHH o elimínelas para poder aprobar el perfil.'
              : undefined
          });
        }

        if (this.permisos().puedeRegistrarObservaciones) {
          acciones.push({
            id: 'enviarObservaciones',
            etiqueta: 'Enviar observaciones',
            icono: 'send',
            claseCss: 'advertencia',
            deshabilitado: !tienePendientes || isProcessing,
            tooltip: !tienePendientes
              ? 'Debe registrar al menos una nueva observación antes de poder enviarla a RRHH.'
              : undefined
          });
        }
      }
    } else if (pestana === 'estructurado') {
      if (esRrhhOAdmin) {
        if (this.modoEdicion()) {
          acciones.push({
            id: 'guardarAjustes',
            etiqueta: 'Guardar ajustes',
            icono: 'save',
            claseCss: 'primario',
            deshabilitado: !this.permisos().puedeGuardar || isProcessing
          });
          acciones.push({
            id: 'cancelarEdicion',
            etiqueta: 'Cancelar',
            icono: 'cancel',
            claseCss: 'peligro',
            deshabilitado: isProcessing
          });
        } else {
          if (this.permisos().puedeEditar) {
            acciones.push({
              id: 'iniciarEdicion',
              etiqueta: 'Editar perfil',
              icono: 'edit',
              claseCss: 'secundario',
              deshabilitado: isProcessing
            });
          }

          if (this.permisos().puedeEnviarAreaSolicitante) {
            acciones.push({
              id: 'enviarArea',
              etiqueta: 'Enviar al Área Solicitante',
              icono: 'send',
              claseCss: 'advertencia',
              deshabilitado: tienePendientes || isProcessing,
              tooltip: tienePendientes
                ? 'Debe marcar como resueltas todas las observaciones registradas por el Área Solicitante antes de volver a enviar el perfil.'
                : undefined
            });
          }

          acciones.push({
            id: 'aprobarFinal',
            etiqueta: 'Aprobar perfil final',
            icono: 'verified',
            claseCss: 'exito',
            deshabilitado: !this.permisos().puedeAprobarFinal || tienePendientes || isProcessing,
            tooltip: !this.permisos().puedeAprobarFinal
              ? 'El perfil debe ser previamente aprobado por el Área Solicitante para poder realizar la aprobación final.'
              : (tienePendientes
                ? 'Debe marcar como resueltas todas las observaciones antes de aprobar el perfil.'
                : undefined)
          });
        }
      } else if (esAreaSol) {
        if (this.permisos().puedeAprobarArea) {
          acciones.push({
            id: 'aprobarArea',
            etiqueta: 'Aprobar perfil',
            icono: 'thumb_up',
            claseCss: 'exito',
            deshabilitado: tienePendientes || isProcessing,
            tooltip: tienePendientes
              ? 'Tiene observaciones registradas pendientes de envío. Envíelas a RRHH o elimínelas para poder aprobar el perfil.'
              : undefined
          });
        }

        if (this.permisos().puedeRegistrarObservaciones) {
          acciones.push({
            id: 'enviarObservaciones',
            etiqueta: 'Enviar observaciones',
            icono: 'send',
            claseCss: 'advertencia',
            deshabilitado: !tienePendientes || isProcessing,
            tooltip: !tienePendientes
              ? 'Debe registrar al menos una nueva observación antes de poder enviarla a RRHH.'
              : undefined
          });
        }
      }
    }

    return acciones;
  });

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id && id !== this.perfilId) {
        this.perfilId = id;
        this.determinarRolUsuario();
        this.cargarAreasYDatos();
      }
    });
    this.queryParamsSub = this.route.queryParams.subscribe(params => {
      this.determinarRolUsuario();
    });
    this.iniciarSmartPolling();
  }

  ngOnDestroy(): void {
    this.detenerSmartPolling();
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
    if (this.queryParamsSub) {
      this.queryParamsSub.unsubscribe();
    }
  }

  iniciarSmartPolling(): void {
    this.detenerSmartPolling();

    // Sondeo silencioso periódico (cada 6 segundos) si la pestaña está visible y no se está editando
    this.autoRefreshTimer = setInterval(() => {
      if (document.visibilityState === 'visible' && !this.modoEdicion() && !this.isLoading() && !this.processingMessage()) {
        this.cargarDatos(true);
      }
    }, 6000);

    // Refresco inmediato al volver a enfocar la pestaña (ej. al regresar desde WhatsApp Web)
    this.visibilityHandler = () => {
      if (document.visibilityState === 'visible' && !this.modoEdicion() && !this.isLoading() && !this.processingMessage()) {
        this.cargarDatos(true);
      }
    };
    this.focusHandler = () => {
      if (!this.modoEdicion() && !this.isLoading() && !this.processingMessage()) {
        this.cargarDatos(true);
      }
    };

    document.addEventListener('visibilitychange', this.visibilityHandler);
    window.addEventListener('focus', this.focusHandler);
  }

  detenerSmartPolling(): void {
    if (this.autoRefreshTimer) {
      clearInterval(this.autoRefreshTimer);
      this.autoRefreshTimer = null;
    }
    if (this.visibilityHandler) {
      document.removeEventListener('visibilitychange', this.visibilityHandler);
      this.visibilityHandler = null;
    }
    if (this.focusHandler) {
      window.removeEventListener('focus', this.focusHandler);
      this.focusHandler = null;
    }
  }

  determinarRolUsuario(): void {
    const role = this.authService.getUserRole();
    const area = this.authService.getUserArea();
    const modoQuery = this.route.snapshot.queryParamMap.get('modo');

    if (modoQuery === 'solicitante') {
      this.userRole.set('AreaSol');
    } else if (role === 'Administrador') {
      this.userRole.set('Administrador');
    } else if (role === 'RRHH' || role === 'Reclutador' || area === 'Recursos Humanos') {
      this.userRole.set('RRHH');
    } else if (role === 'Solicitante' || role === 'AreaSol') {
      this.userRole.set('AreaSol');
    } else {
      this.userRole.set('AreaSol');
    }

    this.pestanaActiva.set('resumen'); // Por defecto Resumen ejecutivo
    this.userArea.set(area || 'Área Solicitante');
  }

  cargarAreasYDatos(): void {
    this.apiService.getAreas().subscribe({
      next: (areas) => {
        this.areasList.set(areas || []);
        this.cargarDatos();
      },
      error: () => {
        this.cargarDatos();
      }
    });
  }

  checkUserAreaMatch(p: DetallePerfil): boolean {
    const userAreaId = this.authService.getUserAreaId();
    const userAreaName = this.authService.getUserArea();
    
    // 1. Comparar por IDs técnicos si ambos son válidos (> 0)
    if (userAreaId > 0 && p.areaId > 0) {
      if (userAreaId === p.areaId) return true;
    }
    
    // 2. Fallback: Normalizar y comparar los nombres de área
    return this.normalizeText(userAreaName) === this.normalizeText(p.areaNombre);
  }

  normalizeText(text: string): string {
    if (!text) return '';
    return text
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '') // Elimina acentos
      .toLowerCase()
      .trim()
      .replace(/\s+/g, ' '); // Colapsa múltiples espacios
  }

  cargarDatos(silent: boolean = false): void {
    if (!this.perfilId) return;

    if (!silent) {
      this.isLoading.set(true);
      this.isError.set(false);
      this.componentLoading.show('detalle-perfil');
    }

    this.perfilesService.obtenerPerfilPorId(this.perfilId).subscribe({
      next: (data) => {
        // Mapear el areaId real usando areasList
        const matchedArea = this.areasList().find(a => this.normalizeText(a.nombre) === this.normalizeText(data.areaNombre));
        if (matchedArea) {
          data.areaId = matchedArea.areaId;
        }

        if (data && data.id && data.id !== this.perfilId) {
          this.perfilId = data.id;
          this.router.navigate(['/perfiles', data.id], { replaceUrl: true });
        }

        this.perfil.set(data);
        if (!this.modoEdicion()) {
          this.inicializarFormulario(data);
        }
        this.cargarObservaciones();

        if (!silent) {
          this.isLoading.set(false);
          this.componentLoading.hide('detalle-perfil');
        }
      },
      error: (err) => {
        if (!silent) {
          this.isError.set(true);
          this.errorMessage.set(err.message || 'Error al recuperar la información del perfil.');
          this.isLoading.set(false);
          this.componentLoading.hide('detalle-perfil');
        }
      }
    });
  }

  cargarObservaciones(): void {
    this.observacionesService.obtenerObservacionesPerfil(this.perfilId).subscribe(list => {
      this.observaciones.set(list || []);
    });
  }

  onResolucionCambio(event: { idObservacionPerfil: number; resuelta: boolean }): void {
    const targetState = event.resuelta ? EstadoObservacionPerfil.Resuelta : EstadoObservacionPerfil.Pendiente;
    this.observaciones.update(list =>
      list.map(o => o.idObservacionPerfil === event.idObservacionPerfil
        ? { ...o, estado: targetState }
        : o
      )
    );
  }

  inicializarFormulario(data: DetallePerfil): void {
    const grupo: any = {};
    data.secciones.forEach(sec => {
      sec.campos.forEach(c => {
        let val = c.valor;
        if (Array.isArray(val) && c.tipo !== 'tabla') {
          val = val.join('\n'); // Convertir arrays a saltos de línea para el textarea
        }
        grupo[c.id] = new FormControl(val);
      });
    });
    this.perfilForm = new FormGroup(grupo);
    this.formTieneCambios.set(false);
    this.perfilForm.valueChanges.subscribe(() => {
      this.formTieneCambios.set(this.perfilForm.dirty);
    });
  }

  iniciarEdicion(): void {
    const current = this.perfil();
    if (current) {
      this.inicializarFormulario(current);
    }
    this.modoEdicion.set(true);
  }

  cancelarEdicion(): void {
    const currentData = this.perfil();
    if (currentData) {
      this.inicializarFormulario(currentData);
    }
    this.modoEdicion.set(false);
  }

  reintentarGeneracionResumen(): void {
    this.mostrarBotonReintentarResumen.set(false);
    this.errorMessageDetalle.set(null);
    this.processingMessage.set('Generando resumen ejecutivo...');
    this.isLoading.set(true);

    const currentId = this.perfilId;
    this.perfilesService.obtenerPerfilPorId(currentId).subscribe({
      next: (mappedPerfil) => {
        const payload = {
          perfilCargoId: mappedPerfil.id,
          PerfilCargoId: mappedPerfil.id,
          solicitudId: mappedPerfil.solicitudId,
          SolicitudId: mappedPerfil.solicitudId,
          codigoPerfil: mappedPerfil.codigo,
          CodigoPerfil: mappedPerfil.codigo,
          version: mappedPerfil.version,
          Version: mappedPerfil.version
        };

        this.webhookService.generarResumen(payload).subscribe({
          next: () => {
            let attempts = 0;
            const maxAttempts = 15;

            this.pollingInterval = setInterval(() => {
              attempts++;
              this.apiService.getPerfilById(currentId).subscribe({
                next: (perfilDet) => {
                  const res = perfilDet?.resumenEjecutivo;
                  const isPending = !res || !res.resumen || res.resumen.trim() === '';

                  if (!isPending || attempts >= maxAttempts) {
                    if (this.pollingInterval) {
                      clearInterval(this.pollingInterval);
                      this.pollingInterval = null;
                    }

                    this.processingMessage.set(null);
                    this.isLoading.set(false);
                    this.perfil.set(mappedPerfil);
                    this.inicializarFormulario(mappedPerfil);
                    if (!isPending) {
                      this.mostrarAlertaExito('Resumen ejecutivo actualizado exitosamente.');
                    } else {
                      this.mostrarAlertaError('La generación del resumen ejecutivo está demorando. Se completará en segundo plano.');
                    }
                  }
                },
                error: (err) => {
                  console.error('[Polling error]', err);
                  if (attempts >= maxAttempts) {
                    if (this.pollingInterval) {
                      clearInterval(this.pollingInterval);
                      this.pollingInterval = null;
                    }
                    this.processingMessage.set(null);
                    this.isLoading.set(false);
                    this.cargarDatos(true);
                  }
                }
              });
            }, 2000);
          },
          error: (err) => {
            this.processingMessage.set(null);
            this.isLoading.set(false);
            this.mostrarBotonReintentarResumen.set(true);
            const errorMsg = err.error?.message || err.error?.Message || 'Error al conectar con el webhook.';
            this.mostrarAlertaError(`La actualización de perfil fue guardada correctamente, pero falló la generación del resumen ejecutivo. Puedes reintentar generar el resumen. Detalle: ${errorMsg}`);
          }
        });
      },
      error: () => {
        this.isLoading.set(false);
        this.mostrarAlertaError('No se pudo recargar la información del perfil para reintentar.');
      }
    });
  }

  guardarAjustes(): void {
    if (this.perfilForm.invalid || !this.perfilId) return;

    if (!this.puedeEditarPerfil()) {
      this.mostrarAlertaError('No se puede modificar el perfil en su estado actual.');
      return;
    }

    // Helper to split lines
    const splitLines = (val: string): string[] => {
      if (!val) return [];
      return val.split('\n').map(x => x.trim()).filter(x => x);
    };

    const updates: { seccion: number; contenido: string }[] = [];

    // 1. Section 3 (Condiciones Contractuales) - reporta a ('1d') y salario ('1g')
    const reportaA = this.perfilForm.get('1d')?.value || '';
    const salarioVal = this.perfilForm.get('1g')?.value || this.perfilForm.get('1g_min')?.value || '';
    const banda = salarioVal;
    if (this.perfilForm.get('1d')?.value !== this.getInitialValue('1d') ||
        this.perfilForm.get('1g')?.value !== this.getInitialValue('1g') ||
        this.perfilForm.get('1g_min')?.value !== this.getInitialValue('1g_min') ||
        this.perfilForm.get('1g_max')?.value !== this.getInitialValue('1g_max')) {
      updates.push({
        seccion: 3,
        contenido: `Reporta a: ${reportaA}\nBanda Salarial/Presupuesto: ${banda}`
      });
    }

    // 2. Section 7 (Funciones Principales) - funciones ('6a')
    if (this.areListsDifferent(this.perfilForm.get('6a')?.value, this.getInitialValue('6a'))) {
      const funciones = splitLines(this.perfilForm.get('6a')?.value || '');
      updates.push({
        seccion: 7,
        contenido: JSON.stringify(funciones)
      });
    }

    // 3. Section 10 (Competencias Blandas) - perfil ideal ('9a')
    if (this.perfilForm.get('9a')?.value !== this.getInitialValue('9a')) {
      const perfilIdeal = this.perfilForm.get('9a')?.value || '';
      updates.push({
        seccion: 10,
        contenido: JSON.stringify([perfilIdeal])
      });
    }

    // 4. Section 12 (Herramientas, KPIs y Riesgos) - herramientas ('5a') y KPIs ('8a')
    if (this.areListsDifferent(this.perfilForm.get('5a')?.value, this.getInitialValue('5a')) ||
        this.areListsDifferent(this.perfilForm.get('8a')?.value, this.getInitialValue('8a'))) {
      const herramientas = splitLines(this.perfilForm.get('5a')?.value || '');
      const kpis = splitLines(this.perfilForm.get('8a')?.value || '');
      updates.push({
        seccion: 12,
        contenido: JSON.stringify({
          Herramientas: herramientas,
          Kpis: kpis,
          Riesgos: [],
          ObservacionesIA: ''
        })
      });
    }

    // 5. Section 8 (Responsabilidades Clave / Matriz) - matriz ponderacion ('11a')
    if (this.areTablesDifferent(this.perfilForm.get('11a')?.value, this.getInitialValue('11a'))) {
      const matriz = (this.perfilForm.get('11a')?.value || []).filter((item: any) => item && String(item.criterio || '').toLowerCase().trim() !== 'total');
      const totalPonderacion = matriz.reduce((acc: number, item: any) => acc + (Number(item?.ponderacion) || 0), 0);
      if (totalPonderacion !== 100) {
        this.mostrarAlertaError(`La sumatoria de la matriz de ponderación debe ser exactamente 100%. La suma actual es ${totalPonderacion}%.`);
        return;
      }
      updates.push({
        seccion: 8,
        contenido: JSON.stringify(matriz)
      });
    }

    // 6. Section 11 (Certificaciones Requeridas / Alto ajuste) - perfil tipo alto ajuste ('12a')
    if (this.perfilForm.get('12a')?.value !== this.getInitialValue('12a')) {
      const altoAjuste = this.perfilForm.get('12a')?.value || '';
      updates.push({
        seccion: 11,
        contenido: JSON.stringify([altoAjuste])
      });
    }

    if (updates.length === 0) {
      this.mostrarAlertaExito('No existen cambios para guardar.');
      this.modoEdicion.set(false);
      return;
    }

    this.processingMessage.set('Guardando ajustes del perfil...');
    this.isLoading.set(true);

    let currentId = this.perfilId;
    const ejecutarGuardadoSecuencial = (index: number) => {
      if (index >= updates.length) {
        // Desactivar loader focalizado de guardado y conmutar a modo lectura IMG4 con notificación de éxito
        this.isLoading.set(false);
        this.processingMessage.set(null);
        this.componentLoading.hide('detalle-perfil');
        this.modoEdicion.set(false);
        this.formTieneCambios.set(false);
        this.mostrarAlertaExito('Ajustes del perfil guardados exitosamente.');
        this.cargarDatos(true);

        // Generación de Resumen Ejecutivo por IA y sondeo SILENCIOSO en segundo plano (sin recargas de pantalla)
        this.perfilesService.obtenerPerfilPorId(currentId).subscribe({
          next: (mappedPerfil) => {
            const payload = {
              perfilCargoId: mappedPerfil.id,
              PerfilCargoId: mappedPerfil.id,
              solicitudId: mappedPerfil.solicitudId,
              SolicitudId: mappedPerfil.solicitudId,
              codigoPerfil: mappedPerfil.codigo,
              CodigoPerfil: mappedPerfil.codigo,
              version: mappedPerfil.version,
              Version: mappedPerfil.version
            };

            console.log('[DEBUG-WEBHOOK] Enviando payload silencioso al webhook perfil_vacante_resumen:', payload);
            this.webhookService.generarResumen(payload).subscribe({
              next: () => {
                let attempts = 0;
                const maxAttempts = 15;

                if (this.pollingInterval) {
                  clearInterval(this.pollingInterval);
                }
                this.pollingInterval = setInterval(() => {
                  attempts++;
                  this.apiService.getPerfilById(currentId).subscribe({
                    next: (perfilDet) => {
                      const res = perfilDet?.resumenEjecutivo;
                      const isPending = !res || !res.resumen || res.resumen.trim() === '';

                      if (!isPending || attempts >= maxAttempts) {
                        if (this.pollingInterval) {
                          clearInterval(this.pollingInterval);
                          this.pollingInterval = null;
                        }
                        this.cargarDatos(true);
                      }
                    },
                    error: () => {
                      if (attempts >= maxAttempts && this.pollingInterval) {
                        clearInterval(this.pollingInterval);
                        this.pollingInterval = null;
                      }
                    }
                  });
                }, 2000);
              },
              error: (err) => {
                console.error('[DEBUG-WEBHOOK] Error silencioso al conectar con el webhook:', err);
              }
            });
          },
          error: () => {}
        });
        return;
      }

      const upd = updates[index];
      this.apiService.actualizarPerfilSeccion(currentId, upd.seccion, {
        contenido: upd.contenido,
        motivo: 'Ajuste del Perfil estructurado desde Backoffice'
      }).subscribe({
        next: (res: any) => {
          if (res && res.perfilId) {
            currentId = res.perfilId;
            this.perfilId = res.perfilId;
          }
          ejecutarGuardadoSecuencial(index + 1);
        },
        error: (err) => {
          this.isLoading.set(false);
          this.processingMessage.set(null);
          this.componentLoading.hide('detalle-perfil');
          this.modoEdicion.set(false);
          const errMsg = err.error?.message || err.error?.Message || 'Error de conexión';
          this.mostrarAlertaError(`No se pudieron guardar algunos ajustes: ${errMsg}`);
        }
      });
    };

    ejecutarGuardadoSecuencial(0);
  }

  enviarAlArea(): void {
    const p = this.perfil();
    if (!p) return;

    const config: ConfiguracionDialogoConfirmacion = {
      titulo: 'Enviar al Área Solicitante',
      descripcion: '¿Deseas enviar este perfil al Área Solicitante? Una vez enviado, el área solicitante podrá revisar la información y aprobar u observar el perfil.',
      icono: 'send',
      variante: 'advertencia',
      resumen: {
        codigo: p.codigo,
        cargo: p.cargo,
        solicitudOrigen: p.solicitudCodigo,
        area: p.areaNombre,
        version: p.version.toString()
      },
      requiereCheckbox: false,
      textoBotonPrincipal: 'Enviar al Área Solicitante',
      textoBotonCancelar: 'Cancelar',
      onConfirmar: () => {
        this.isLoading.set(true);

        const obtenerMensajeError = (err: any): string => {
          if (err?.error?.message) return err.error.message;
          if (err?.error?.Message) return err.error.Message;
          if (err?.error?.detail) return err.error.detail;
          if (err?.message) return err.message;
          return 'Ha ocurrido un error inesperado al procesar la solicitud.';
        };

        const splitToList = (val: any): string[] => {
          return normalizeProfileList(val);
        };

        return this.apiService.getPerfilById(this.perfilId).pipe(
          catchError((err) => {
            console.error('[Error getPerfilById]', err);
            alert('No se pudo recuperar la información del perfil para validar su estado.');
            throw err;
          }),
          concatMap((rawPerfil) => {
            let guardarResumen$ = of<any>(null);

            if (rawPerfil.estadoCodigo === 'PERF-REV-RRHH') {
              const rawResumen = rawPerfil.resumenEjecutivo;
              const updatePayload = {
                perfilId: rawPerfil.perfilId,
                solicitudId: rawPerfil.solicitudId,
                resumenEjecutivoRol: {
                  resumen: rawResumen?.resumen || rawPerfil.perfilEstructurado?.perfilRequerido?.objetivoCargo || '',
                  objetivoCargo: rawResumen?.objetivoCargo || rawPerfil.perfilEstructurado?.perfilRequerido?.objetivoCargo || '',
                  funcionesPrincipales: rawResumen?.funcionesPrincipales || splitToList(rawPerfil.perfilEstructurado?.perfilRequerido?.funciones),
                  requisitosMinimos: rawResumen?.requisitosMinimos || splitToList(rawPerfil.perfilEstructurado?.perfilRequerido?.criteriosExcluyentes),
                  formacionExperiencia: rawResumen?.formacionExperiencia || rawPerfil.perfilEstructurado?.perfilRequerido?.formacionAcademica || '',
                  hardSkills: rawResumen?.hardSkills || splitToList(rawPerfil.perfilEstructurado?.perfilRequerido?.conocimientosTecnicos),
                  softSkills: rawResumen?.softSkills || splitToList(rawPerfil.perfilEstructurado?.perfilRequerido?.competenciasClave),
                  modalidad: rawResumen?.modalidad || rawPerfil.perfilEstructurado?.condicionesVacante?.modalidadTrabajo || '',
                  ubicacion: rawResumen?.ubicacion || rawPerfil.perfilEstructurado?.datosGenerales?.regional || '',
                  bandaSalarial: rawResumen?.bandaSalarial || '',
                  criteriosEvaluacion: rawResumen?.criteriosEvaluacion ? (Array.isArray(rawResumen.criteriosEvaluacion) ? rawResumen.criteriosEvaluacion.join('\n') : rawResumen.criteriosEvaluacion) : '',
                  caracteristicasClave: rawResumen?.caracteristicasClave ? (Array.isArray(rawResumen.caracteristicasClave) ? rawResumen.caracteristicasClave.join('\n') : rawResumen.caracteristicasClave) : '',
                  valoracionPerfil: rawResumen?.valoracionPerfil || ''
                }
              };

              guardarResumen$ = this.apiService.guardarResumen(this.perfilId, updatePayload).pipe(
                catchError((err) => {
                  console.error('[Error guardarResumen]', err);
                  const msg = obtenerMensajeError(err);
                  alert(`Debes generar el Resumen ejecutivo antes de enviar el perfil. Detalle: ${msg}`);
                  throw err;
                })
              );
            }

            const payload = {
              perfilId: rawPerfil.perfilId,
              perfilCargoId: rawPerfil.perfilId,
              codigoPerfil: rawPerfil.codigoPerfil,
              solicitudId: rawPerfil.solicitudId,
              codigoSolicitud: rawPerfil.codigoSolicitud,
              estadoActual: rawPerfil.estadoCodigo,
              accion: 'ENVIAR_AREA_SOLICITANTE',
              usuarioAccion: {
                id: this.authService.getUserId().toString(),
                nombre: this.authService.getUserFullName(),
                rol: 'RRHH'
              },
              fechaAccion: new Date().toISOString()
            };

            const generateA4$ = this.webhookService.generateExecutiveSummaryPdf(payload).pipe(
              timeout(35000),
              catchError((err) => {
                console.warn('[A4 Webhook Warning/Timeout]', err);
                return of('');
              })
            );

            const generateA5$ = this.webhookService.generateStructuredProfilePdf(payload).pipe(
              timeout(35000),
              catchError((err) => {
                console.warn('[A5 Webhook Warning/Timeout]', err);
                return of('');
              })
            );

            const notify$ = this.webhookService.notifySolicitanteProfileUnderReview(payload).pipe(
              timeout(35000),
              catchError((err) => {
                console.warn('[Notification Webhook Warning/Timeout]', err);
                // No bloquear la operación principal si falla la notificación por WhatsApp
                return of('');
              })
            );

            return guardarResumen$.pipe(
              concatMap(() => forkJoin([generateA4$, generateA5$, notify$])),
              concatMap(() => 
                this.apiService.enviarPerfilArea(this.perfilId).pipe(
                  catchError((err) => {
                    console.error('[Error enviarPerfilArea]', err);
                    const msg = obtenerMensajeError(err);
                    alert(`El perfil no se encuentra en un estado válido para enviarlo al Área Solicitante. Detalle: ${msg}`);
                    throw err;
                  })
                )
              ),
              concatMap(() => 
                this.perfilesService.obtenerPerfilPorId(this.perfilId).pipe(
                  catchError((err) => {
                    console.error('[Error de recarga posterior]', err);
                    alert('El perfil fue enviado, pero no se pudo actualizar la vista.');
                    return of(null);
                  })
                )
              )
            );
          }),
          tap((updatedProfile) => {
            if (updatedProfile) {
              this.perfil.set(updatedProfile);
              this.inicializarFormulario(updatedProfile);
              this.formTieneCambios.set(false);
            }
            this.mostrarAlertaExito('El perfil fue enviado al Área Solicitante correctamente.');
          }),
          finalize(() => {
            this.isLoading.set(false);
          })
        );
      }
    };

    this.dialog.open(DialogoConfirmacionComponent, {
      width: '512px',
      disableClose: true,
      data: config
    });
  }

  atenderObservaciones(): void {
    this.isLoading.set(true);
    this.apiService.atenderObservaciones(this.perfilId).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.mostrarAlertaExito('Observaciones atendidas e inicio de corrección por RRHH.');
        this.cargarDatos();
      },
      error: (err) => {
        this.isLoading.set(false);
        alert(err.error?.message || 'Error al atender las observaciones.');
      }
    });
  }

  aprobarArea(): void {
    const p = this.perfil();
    if (!p) return;

    const config: ConfiguracionDialogoConfirmacion = {
      titulo: 'Aprobar perfil',
      descripcion: 'Al aprobar este perfil, confirmas que representa la necesidad del Área Solicitante. RRHH recibirá la validación y podrá realizar la aprobación final.',
      icono: 'check_circle',
      variante: 'exito',
      resumen: {
        codigo: p.codigo,
        cargo: p.cargo,
        solicitudOrigen: p.solicitudCodigo,
        area: p.areaNombre,
        version: p.version.toString()
      },
      requiereCheckbox: true,
      textoCheckbox: 'Confirmo que revisé el perfil estructurado y estoy de acuerdo con su contenido.',
      textoBotonPrincipal: 'Confirmar aprobación',
      textoBotonCancelar: 'Cancelar',
      onConfirmar: () => {
        return new Observable((subscriber) => {
          this.apiService.aprobarSolicitante(this.perfilId).subscribe({
            next: () => {
              this.mostrarAlertaExito('Perfil aprobado por el Área Solicitante. Pendiente de aprobación final de RRHH.');
              this.cargarDatos();
              subscriber.next(true);
              subscriber.complete();
            },
            error: (err) => {
              alert(err.error?.message || 'No se pudo transitar el estado del perfil.');
              subscriber.error(err);
            }
          });
        });
      }
    };

    this.dialog.open(DialogoConfirmacionComponent, {
      width: '512px',
      disableClose: true,
      data: config
    });
  }

  aprobarFinal(): void {
    const p = this.perfil();
    if (!p) return;

    const obtenerMensajeError = (err: any): string => {
      if (err?.error?.message) return err.error.message;
      if (err?.error?.Message) return err.error.Message;
      if (err?.error?.detail) return err.error.detail;
      if (err?.message) return err.message;
      return 'Ha ocurrido un error inesperado al procesar la solicitud.';
    };

    const config: ConfiguracionDialogoConfirmacion = {
      titulo: 'Aprobar perfil para estrategia',
      descripcion: 'Esta acción aprobará la versión final del perfil y habilitará la siguiente etapa del proceso: estrategia de búsqueda.',
      icono: 'verified',
      variante: 'exito',
      resumen: {
        codigo: p.codigo,
        cargo: p.cargo,
        solicitudOrigen: p.solicitudCodigo,
        area: p.areaNombre,
        version: p.version.toString()
      },
      requiereCheckbox: true,
      textoCheckbox: 'Confirmo que revisé la validación del Área Solicitante y apruebo este perfil para continuar con la estrategia.',
      textoBotonPrincipal: 'Aprobar y habilitar estrategia',
      textoBotonCancelar: 'Cancelar',
      onConfirmar: () => {
        this.isLoading.set(true);

        const matchingPayload = {
          PerfilCargoId: p.id,
          SolicitudId: p.solicitudId,
          CodigoPerfil: p.codigo,
          CodigoSolicitud: p.solicitudCodigo
        };

        return this.apiService.aprobarFinal(this.perfilId).pipe(
          catchError((err) => {
            console.error('[Error aprobarFinal]', err);
            const msg = obtenerMensajeError(err);
            alert(`No se pudo transitar el estado del perfil. Detalle: ${msg}`);
            throw err;
          }),
          concatMap(() =>
            this.webhookService.triggerMotorMatching(matchingPayload).pipe(
              timeout(15000),
              catchError((err) => {
                console.error('[Motor Matching Webhook Error]', err);
                // Si el webhook del motor de matching falla o expira, no bloqueamos el flujo principal
                return of('');
              })
            )
          ),
          concatMap(() => 
            this.perfilesService.obtenerPerfilPorId(this.perfilId).pipe(
              catchError((err) => {
                console.error('[Error de recarga posterior]', err);
                alert('El perfil fue aprobado, pero no se pudo actualizar la vista.');
                return of(null);
              })
            )
          ),
          tap((updatedProfile) => {
            if (updatedProfile) {
              this.perfil.set(updatedProfile);
              this.inicializarFormulario(updatedProfile);
              this.formTieneCambios.set(false);
            }
            this.mostrarAlertaExito('El perfil fue aprobado de forma final correctamente y se habilitó la estrategia.');
          }),
          finalize(() => {
            this.isLoading.set(false);
          })
        );
      }
    };

    this.dialog.open(DialogoConfirmacionComponent, {
      width: '512px',
      disableClose: true,
      data: config
    });
  }

  abrirPdf(): void {
    alert('[TODO] Apertura del documento PDF oficial del perfil (Profesiograma).\nEsta funcionalidad de exportación se integrará en la siguiente fase de desarrollo.');
  }

  volverAlListado(): void {
    this.router.navigate(['/perfiles']);
  }

  cambiarPestana(pestana: 'resumen' | 'estructurado'): void {
    if (this.pestanaActiva() === 'estructurado' && pestana === 'resumen' && this.modoEdicion() && this.perfilForm.dirty) {
      const confirmacion = confirm('Tiene cambios sin guardar en el perfil estructurado.\n\n¿Desea guardarlos antes de cambiar de pestaña?\n\n- Aceptar: Guardar y salir\n- Cancelar: Descartar y salir\n(Si desea permanecer editando, cancele en el siguiente paso)');
      if (confirmacion) {
        this.guardarAjustes();
      } else {
        const permanecer = !confirm('¿Está seguro de que desea cambiar de pestaña descartando todos los cambios realizados?\n\n(Aceptar = Descartar y cambiar, Cancelar = Permanecer editando)');
        if (permanecer) {
          return;
        }
        this.cancelarEdicion();
      }
    }
    this.pestanaActiva.set(pestana);
  }

  alCargarInsumos(lista: InsumoResumenPerfil[]): void {
    this.insumos.set(lista || []);
  }

  registrarObservacionesResumen(): void {
    this.cambiarPestana('estructurado');
    setTimeout(() => {
      const elemento = document.getElementById('inputComentario') || document.querySelector('app-observaciones-perfil');
      if (elemento) {
        elemento.scrollIntoView({ behavior: 'smooth', block: 'center' });
        const input = elemento.querySelector('textarea') || elemento;
        if (input) {
          (input as HTMLElement).focus();
        }
      }
    }, 150);
  }

  enviarObservaciones(): void {
    if (this.observaciones().length === 0) return;
    
    this.isLoading.set(true);
    const p = this.perfil();
    if (!p) return;
    
    const payload = {
      perfilId: p.id,
      perfilCargoId: p.id,
      codigoPerfil: p.codigo,
      solicitudId: p.solicitudId,
      codigoSolicitud: p.solicitudCodigo,
      estadoActual: p.estadoCodigo,
      accion: 'ENVIAR_OBSERVACIONES',
      usuarioAccion: {
        id: this.authService.getUserId().toString(),
        nombre: this.authService.getUserFullName(),
        rol: 'AreaSol'
      },
      fechaAccion: new Date().toISOString()
    };

    this.apiService.enviarObservacionesSolicitante(this.perfilId).pipe(
      catchError((err) => {
        console.error('[Error enviarObservacionesSolicitante]', err);
        const msg = err?.error?.message || err?.error?.Message || 'Error al enviar las observaciones.';
        alert(`No fue posible enviar las observaciones: ${msg}`);
        throw err;
      }),
      concatMap(() => 
        this.webhookService.notifySolicitanteProfileUnderReview(payload).pipe(
          timeout(10000),
          catchError((err) => {
            console.error('[Notification Webhook Error]', err);
            return of('');
          })
        )
      ),
      concatMap(() => this.perfilesService.obtenerPerfilPorId(this.perfilId)),
      finalize(() => {
        this.isLoading.set(false);
      })
    ).subscribe({
      next: (updatedProfile) => {
        if (updatedProfile) {
          const matchedArea = this.areasList().find(a => this.normalizeText(a.nombre) === this.normalizeText(updatedProfile.areaNombre));
          if (matchedArea) {
            updatedProfile.areaId = matchedArea.areaId;
          }
          this.perfil.set(updatedProfile);
          this.inicializarFormulario(updatedProfile);
        }
        this.mostrarAlertaExito('Observaciones enviadas a Recursos Humanos con éxito.');
      },
      error: () => {
        this.mostrarAlertaExito('Observaciones enviadas a Recursos Humanos con éxito.');
        this.cargarDatos();
      }
    });
  }

  ejecutarAccion(accionId: string): void {
    switch (accionId) {
      case 'verPdf':
        this.abrirPdf();
        break;
      case 'irEstructurado':
        this.cambiarPestana('estructurado');
        break;
      case 'aprobarFinal':
        this.aprobarFinal();
        break;
      case 'aprobarArea':
        this.aprobarArea();
        break;
      case 'enviarObservaciones':
        this.enviarObservaciones();
        break;
      case 'registrarObservacionesResumen':
        this.registrarObservacionesResumen();
        break;
      case 'iniciarEdicion':
        this.iniciarEdicion();
        break;
      case 'guardarAjustes':
        this.guardarAjustes();
        break;
      case 'cancelarEdicion':
        this.cancelarEdicion();
        break;
      case 'enviarArea':
      case 'enviarCorrecciones':
        this.enviarAlArea();
        break;
      case 'atenderObservaciones':
        this.atenderObservaciones();
        break;
    }
  }

  mostrarAlertaExito(msg: string): void {
    this.successMessage.set(msg);
    setTimeout(() => {
      this.successMessage.set(null);
    }, 6000);
  }

  mostrarAlertaError(msg: string): void {
    this.errorMessageDetalle.set(msg);
    setTimeout(() => {
      if (!this.mostrarBotonReintentarResumen()) {
        this.errorMessageDetalle.set(null);
      }
    }, 10000);
  }

  cerrarBannerError(): void {
    this.errorMessageDetalle.set(null);
    this.mostrarBotonReintentarResumen.set(false);
  }

  retry(): void {
    this.cargarDatos();
  }

  // Simulación de error de conexión
  toggleErrorMode(event: any): void {
    this.perfilesService.setShouldFail(event.target.checked);
    this.cargarDatos();
  }

  getInitialValue(fieldId: string): any {
    const p = this.perfil();
    if (!p) return '';
    for (const sec of p.secciones) {
      const campo = sec.campos.find(c => c.id === fieldId);
      if (campo) {
        if (campo.tipo === 'tabla') {
          return campo.valor;
        }
        if (Array.isArray(campo.valor)) {
          return campo.valor.join('\n');
        }
        return campo.valor || '';
      }
    }
    return '';
  }

  areTablesDifferent(val1: any, val2: any): boolean {
    const arr1 = Array.isArray(val1) ? val1 : [];
    const arr2 = Array.isArray(val2) ? val2 : [];
    if (arr1.length !== arr2.length) return true;
    for (let i = 0; i < arr1.length; i++) {
      if (arr1[i]?.criterio !== arr2[i]?.criterio) return true;
      if (arr1[i]?.ponderacion !== arr2[i]?.ponderacion) return true;
    }
    return false;
  }

  areListsDifferent(val1: string, val2: string): boolean {
    const normalize = (v: string) => {
      if (!v) return '';
      return v.split('\n')
        .map(x => x.trim())
        .filter(x => x)
        .join('\n');
    };
    return normalize(val1) !== normalize(val2);
  }

  abrirPdfResumen(doc?: PerfilDocumento | null): void {
    const tieneDoc = doc || this.perfil()?.documentos?.find(d => d.tipoDocumento === 'RESUMEN_EJECUTIVO_PDF');
    if (!tieneDoc) {
      alert('El documento PDF del Resumen Ejecutivo aún no ha sido generado o cargado para este perfil.');
      return;
    }

    if (tieneDoc.publicUrl && tieneDoc.publicUrl.startsWith('http')) {
      window.open(tieneDoc.publicUrl, '_blank');
      return;
    }

    this.apiService.descargarPdfResumen(this.perfilId).subscribe({
      next: (blob: Blob) => {
        if (blob.type && blob.type.includes('json')) {
          blob.text().then(text => {
            console.error('Error del servidor:', text);
            alert('El archivo PDF del Resumen Ejecutivo no se encuentra disponible en el servidor.');
          });
          return;
        }
        const blobUrl = URL.createObjectURL(blob);
        window.open(blobUrl, '_blank');
      },
      error: (err) => {
        console.error('Error al descargar PDF Resumen:', err);
        alert('No se pudo abrir el documento PDF del Resumen Ejecutivo.');
      }
    });
  }

  abrirPdfEstructurado(doc?: PerfilDocumento | null): void {
    const tieneDoc = doc || this.perfil()?.documentos?.find(d => d.tipoDocumento === 'PERFIL_ESTRUCTURADO_PDF');
    if (!tieneDoc) {
      alert('El documento PDF del Perfil Estructurado aún no ha sido generado o cargado para este perfil.');
      return;
    }

    if (tieneDoc.publicUrl && tieneDoc.publicUrl.startsWith('http')) {
      window.open(tieneDoc.publicUrl, '_blank');
      return;
    }

    this.apiService.descargarPdfEstructurado(this.perfilId).subscribe({
      next: (blob: Blob) => {
        if (blob.type && blob.type.includes('json')) {
          blob.text().then(text => {
            console.error('Error del servidor:', text);
            alert('El archivo PDF del Perfil Estructurado no se encuentra disponible en el servidor.');
          });
          return;
        }
        const blobUrl = URL.createObjectURL(blob);
        window.open(blobUrl, '_blank');
      },
      error: (err) => {
        console.error('Error al descargar PDF Estructurado:', err);
        alert('No se pudo abrir el documento PDF del Perfil Estructurado.');
      }
    });
  }

  getBackendSectionName(num: number): string {
    const nombres: Record<number, string> = {
      1: "Objetivo del Cargo",
      2: "Seniority",
      3: "Condiciones Contractuales",
      4: "Formación Académica",
      5: "Experiencia Requerida",
      6: "Idiomas",
      7: "Funciones Principales",
      8: "Responsabilidades Clave",
      9: "Competencias Técnicas",
      10: "Competencias Blandas",
      11: "Certificaciones Requeridas",
      12: "Herramientas, KPIs y Riesgos"
    };
    return nombres[num] || `Sección ${num}`;
  }
}

