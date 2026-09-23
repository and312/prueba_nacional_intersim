import { Component, OnInit, inject, signal, ViewChild, TemplateRef } from '@angular/core';
import { DatePipe, CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';

// Componentes y Diálogos
import { DialogoConfirmacionComponent } from '../../../core/components/dialogo-confirmacion/dialogo-confirmacion.component';

// Servicios Compartidos
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';

// Servicios de Catálogos de Parametrización
import { RegionalesService } from '../../parametrizacion/services/regionales.service';
import { TiposSolicitudService } from '../../parametrizacion/services/tipos-solicitud.service';
import { ModalidadesService } from '../../parametrizacion/services/modalidades.service';
import { AreasCargoService } from '../../parametrizacion/services/areas-cargo.service';
import { CargosService } from '../../parametrizacion/services/cargos.service';

// Modelos
import { Regional } from '../../parametrizacion/models/regional.model';
import { TipoSolicitud } from '../../parametrizacion/models/tipo-solicitud.model';
import { Modalidad } from '../../parametrizacion/models/modalidad.model';
import { AreaCargo } from '../../parametrizacion/models/area-cargo.model';
import { CargoItem } from '../../parametrizacion/models/cargo.model';
import { SolicitudCreateRequest } from '../../../core/models/solicitud.model';

@Component({
  selector: 'app-nueva-solicitud',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    DatePipe
  ],
  templateUrl: './nueva-solicitud.component.html',
  styleUrls: ['./nueva-solicitud.component.scss']
})
export class NuevaSolicitudComponent implements OnInit {
  @ViewChild('reenviarDialog') reenviarDialogTmpl!: TemplateRef<any>;
  private dialogRefReenvio: any = null;

  private apiService = inject(ApiService);
  authService = inject(AuthService);
  private regionalesService = inject(RegionalesService);
  private tiposSolicitudService = inject(TiposSolicitudService);
  private modalidadesService = inject(ModalidadesService);
  private areasCargoService = inject(AreasCargoService);
  private cargosService = inject(CargosService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  // Estados de control
  isEditMode = signal(false);
  solicitudId = signal<number | null>(null);
  solicitud = signal<any | null>(null);
  isSubmitting = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  // Fecha del sistema formateada localmente
  fechaHoyFormateada = '';

  // Datos de Catálogos Parametrizables
  regionales = signal<Regional[]>([]);
  tiposSolicitud = signal<TipoSolicitud[]>([]);
  modalidades = signal<Modalidad[]>([]);
  areasCargo = signal<AreaCargo[]>([]);
  cargosTodos = signal<CargoItem[]>([]);
  cargosFiltrados = signal<CargoItem[]>([]);

  catalogosCargando = signal(true);
  catalogosError = signal(false);

  // Formulario Reactivo principal adaptado a la plantilla oficial de vacante
  solicitudForm = new FormGroup({
    // Sección 1: Datos generales que completa el usuario
    areaCargoId: new FormControl<number | null>(null, [Validators.required]),
    cargoId: new FormControl<number | null>(null, [Validators.required]),
    cargo: new FormControl('', [Validators.required, Validators.minLength(2)]),
    regionalId: new FormControl<number | null>(null, [Validators.required]),
    cantidadVacantes: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    tipoSolicitudId: new FormControl<number | null>(null, [Validators.required]),
    motivo: new FormControl('', [Validators.required, Validators.minLength(10)]),

    // Sección 2: Perfil Requerido - Obligatorios
    objetivoCargo: new FormControl('', [Validators.required, Validators.minLength(10)]),
    experienciaMinima: new FormControl('', [Validators.required, Validators.minLength(4)]),
    conocimientosTecnicos: new FormControl('', [Validators.required, Validators.minLength(10)]),
    funciones: new FormControl('', [Validators.required, Validators.minLength(10)]),

    // Sección 2: Perfil Requerido - Opcionales
    formacionAcademica: new FormControl(''),
    experienciaIndispensable: new FormControl(''),
    herramientasSistemas: new FormControl(''),
    competenciasClave: new FormControl(''),
    criteriosExcluyentes: new FormControl(''),
    criteriosDeseables: new FormControl(''),

    // Sección 3: Condiciones de la vacante - Obligatorios
    modalidadTrabajoId: new FormControl<number | null>(null, [Validators.required]),
    seniority: new FormControl('Junior', [Validators.required]),
    prioridad: new FormControl('Media', [Validators.required]),

    // Sección 3: Condiciones de la vacante - Opcional
    disponibilidadRequerida: new FormControl('')
  });

  ngOnInit(): void {
    const pipe = new DatePipe('es-BO');
    this.fechaHoyFormateada = pipe.transform(new Date(), 'dd/MM/yyyy') || '';

    // Cargar catálogos primero
    this.cargarCatalogos();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = parseInt(idParam, 10);
      if (!isNaN(id)) {
        this.solicitudId.set(id);
        this.isEditMode.set(true);
        this.loadSolicitudForEdit(id);
      }
    }
  }

  limpiarFormulario(): void {
    const dialogRef = this.dialog.open(DialogoConfirmacionComponent, {
      data: {
        titulo: '¿Limpiar formulario de solicitud?',
        descripcion: 'Se restablecerán todos los campos de la vacante a sus valores iniciales. Esta acción no se puede deshacer.',
        icono: 'restart_alt',
        variante: 'advertencia',
        textoBotonPrincipal: 'Sí, limpiar',
        textoBotonCancelar: 'Cancelar'
      },
      width: '440px'
    });

    dialogRef.afterClosed().subscribe((confirmado) => {
      if (confirmado) {
        this.solicitudForm.reset({
          cantidadVacantes: 1,
          seniority: 'Junior',
          prioridad: 'Media'
        });
        this.cargosFiltrados.set([]);
      }
    });
  }

  cargarCatalogos(): void {
    this.catalogosCargando.set(true);
    this.catalogosError.set(false);

    forkJoin({
      regionales: this.regionalesService.listar(true),
      tipos: this.tiposSolicitudService.listar(true),
      modalidades: this.modalidadesService.listar(true),
      areasCargo: this.areasCargoService.listar(true),
      cargos: this.cargosService.listar(undefined, true)
    }).subscribe({
      next: (res) => {
        this.regionales.set((res.regionales || []).filter(r => r.activo));
        this.tiposSolicitud.set((res.tipos || []).filter(t => t.activo));
        this.modalidades.set((res.modalidades || []).filter(m => m.activo));
        this.areasCargo.set((res.areasCargo || []).filter(a => a.activo));
        this.cargosTodos.set((res.cargos || []).filter(c => c.activo));
        this.catalogosCargando.set(false);

        // Si estamos editando y ya había un areaCargoId cargada, aplicar filtro
        const currentAreaId = this.solicitudForm.controls.areaCargoId.value;
        if (currentAreaId) {
          this.onAreaCargoChange(currentAreaId, false);
        }
      },
      error: (err) => {
        console.error('Error al cargar catálogos desde Parametrización:', err);
        this.catalogosError.set(true);
        this.catalogosCargando.set(false);
      }
    });
  }

  onAreaCargoChange(areaId: any, resetCargo = true): void {
    const numId = Number(areaId);
    if (numId > 0) {
      const filtered = this.cargosTodos().filter(c => c.areaCargoId === numId && c.activo);
      this.cargosFiltrados.set(filtered);
    } else {
      this.cargosFiltrados.set([]);
    }
    if (resetCargo) {
      this.solicitudForm.patchValue({ cargoId: null, cargo: '' });
    }
  }

  onCargoSelectChange(cargoId: any): void {
    const numId = Number(cargoId);
    const selected = this.cargosTodos().find(c => c.idCargo === numId);
    if (selected) {
      this.solicitudForm.patchValue({ cargo: selected.nombre });
    }
  }

  loadSolicitudForEdit(id: number): void {
    this.apiService.getSolicitudById(id).subscribe({
      next: (sol) => {
        this.solicitud.set(sol);
        
        // Control de autorización de edición
        const userRole = this.authService.getUserRole();
        const userId = this.authService.getUserId();
        const isSolicitante = userRole === 'Solicitante' || (userRole !== 'RRHH' && this.authService.getUserArea() !== 'Recursos Humanos');
        
        const estadoCod = sol.estadoCodigo || '';
        const estadoNom = sol.estadoNombre || '';
        const esEstadoEditable = estadoCod === 'SOL-REG' || estadoCod === 'SOL-PEN' || estadoCod === 'SOL-OBS' || estadoCod === 'SOL-BOR' ||
                                 estadoNom === 'Solicitud Registrada' || estadoNom === 'Solicitud Pendiente' || 
                                 estadoNom === 'Solicitud Observada' || estadoNom === 'Observada' || 
                                 estadoNom === 'Con observaciones' || estadoNom === 'Borrador';

        if (isSolicitante) {
          const esPropietario = Number(sol.solicitanteId) === Number(userId);
          if (!esPropietario || !esEstadoEditable) {
            this.catalogosCargando.set(false);
            alert('No tienes autorización para editar esta solicitud.');
            this.router.navigate(['/solicitudes']);
            return;
          }
        }

        // Rellenar formulario con compatibilidad técnica
        this.solicitudForm.patchValue({
          cargo: sol.cargo,
          regionalId: sol.regional?.id || null,
          cantidadVacantes: sol.cantidadVacantes || 1,
          tipoSolicitudId: sol.tipoSolicitud?.id || null,
          motivo: sol.motivo || '',
          objetivoCargo: sol.objetivoCargo || '',
          experienciaMinima: sol.experienciaMinima || sol.experiencia || '',
          conocimientosTecnicos: sol.conocimientosTecnicos || sol.skills || '',
          funciones: sol.funciones || '',
          formacionAcademica: sol.formacionAcademica || sol.educacion || '',
          experienciaIndispensable: sol.experienciaIndispensable || '',
          herramientasSistemas: sol.herramientasSistemas || '',
          competenciasClave: sol.competenciasClave || '',
          criteriosExcluyentes: sol.criteriosExcluyentes || '',
          criteriosDeseables: sol.criteriosDeseables || '',
          modalidadTrabajoId: sol.modalidadTrabajo?.id || null,
          seniority: sol.seniority || 'Junior',
          prioridad: sol.prioridad || 'Media',
          disponibilidadRequerida: sol.disponibilidadRequerida || ''
        });
      },
      error: (err) => {
        console.error(err);
        this.catalogosCargando.set(false);
        alert('Error al cargar la solicitud para edición.');
        this.router.navigate(['/solicitudes']);
      }
    });
  }

  // Verifica si un catálogo cargado contiene el id seleccionado o devuelve "No especificado"
  obtenerNombreCatalogo(tipo: 'regional' | 'tipo' | 'modalidad', id: any): string {
    if (id === null || id === undefined || id === '' || id === 'null') return 'No especificado';
    const numId = Number(id);

    if (tipo === 'regional') {
      const match = this.regionales().find(r => (
        r.idRegional == id ||
        (r as any).id == id ||
        (!isNaN(numId) && (r.idRegional === numId || (r as any).id === numId))
      ));
      if (match) return match.nombre;
      if (typeof id === 'string' && isNaN(numId) && id.trim().length > 0) return id;
      return 'No especificado';
    } else if (tipo === 'tipo') {
      const match = this.tiposSolicitud().find(t => (
        t.idTipoSolicitud == id ||
        (t as any).id == id ||
        (!isNaN(numId) && (t.idTipoSolicitud === numId || (t as any).id === numId))
      ));
      if (match) return match.nombre;
      if (typeof id === 'string' && isNaN(numId) && id.trim().length > 0) return id;
      return 'No especificado';
    } else if (tipo === 'modalidad') {
      const match = this.modalidades().find(m => (
        m.idModalidad == id ||
        (m as any).id == id ||
        (!isNaN(numId) && (m.idModalidad === numId || (m as any).id === numId))
      ));
      if (match) return match.nombre;
      if (typeof id === 'string' && isNaN(numId) && id.trim().length > 0) return id;
      return 'No especificado';
    }
    return 'No especificado';
  }

  obtenerTextoObservacionRRHH(): string {
    const obs = this.solicitud()?.ultimaObservacionRRHH;
    if (!obs) return '';
    if (typeof obs === 'string') return obs;
    if (typeof obs === 'object' && obs.texto) return obs.texto;
    if (typeof obs === 'object' && obs.comentario) return obs.comentario;
    return String(obs);
  }

  onPrimarySubmit(): void {
    if (this.solicitudForm.invalid || this.catalogosCargando()) {
      this.solicitudForm.markAllAsTouched();
      this.enfocarPrimerCampoInvalido();
      return;
    }

    if (this.isEditMode() && this.solicitud()?.estadoCodigo === 'SOL-OBS') {
      this.dialogRefReenvio = this.dialog.open(this.reenviarDialogTmpl, {
        width: '520px'
      });
      return;
    }

    this.enviarSolicitud();
  }

  cerrarModalReenvio(): void {
    if (this.dialogRefReenvio) {
      this.dialogRefReenvio.close();
      this.dialogRefReenvio = null;
    }
  }

  confirmarReenvio(comentario: string): void {
    this.cerrarModalReenvio();
    this.enviarSolicitud(comentario.trim() || undefined);
  }

  enviarSolicitud(comentarioOpcional?: string): void {
    if (this.solicitudForm.invalid || this.catalogosCargando()) {
      this.solicitudForm.markAllAsTouched();
      this.enfocarPrimerCampoInvalido();
      return;
    }

    this.isSubmitting.set(true);
    const formVal = this.solicitudForm.value;

    // Construcción del payload estructurado
    const payload: SolicitudCreateRequest = {
      cargo: (formVal.cargo || '').trim(),
      regionalId: Number(formVal.regionalId),
      cantidadVacantes: Number(formVal.cantidadVacantes),
      tipoSolicitudId: Number(formVal.tipoSolicitudId),
      motivo: (formVal.motivo || '').trim(),
      objetivoCargo: (formVal.objetivoCargo || '').trim(),
      formacionAcademica: (formVal.formacionAcademica || '').trim() || undefined,
      experienciaMinima: (formVal.experienciaMinima || '').trim(),
      experienciaIndispensable: (formVal.experienciaIndispensable || '').trim() || undefined,
      conocimientosTecnicos: (formVal.conocimientosTecnicos || '').trim(),
      herramientasSistemas: (formVal.herramientasSistemas || '').trim() || undefined,
      competenciasClave: (formVal.competenciasClave || '').trim() || undefined,
      criteriosExcluyentes: (formVal.criteriosExcluyentes || '').trim() || undefined,
      criteriosDeseables: (formVal.criteriosDeseables || '').trim() || undefined,
      funciones: (formVal.funciones || '').trim(),
      modalidadTrabajoId: Number(formVal.modalidadTrabajoId),
      disponibilidadRequerida: (formVal.disponibilidadRequerida || '').trim() || undefined,
      seniority: formVal.seniority || 'Junior',
      prioridad: formVal.prioridad || 'Media',
      observaciones: comentarioOpcional || undefined,
      usuarioId: this.authService.getUserId()
    };

    if (this.isEditMode()) {
      const id = this.solicitudId();
      if (!id) return;

      const isObserved = this.solicitud()?.estadoCodigo === 'SOL-OBS';
      const successMessageText = isObserved
        ? `¡Solicitud de "${payload.cargo}" actualizada y reenviada a revisión de RRHH con éxito!`
        : '¡Cambios guardados con éxito en la solicitud!';

      this.apiService.actualizarSolicitud(id, payload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/solicitudes'], {
            state: { successMessage: successMessageText }
          });
        },
        error: (err) => {
          this.isSubmitting.set(false);
          console.error('Error al actualizar la solicitud:', err);
          this.errorMessage.set('Error al actualizar la solicitud en el servidor.');
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }
      });
    } else {
      this.apiService.crearSolicitud(payload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigate(['/solicitudes'], {
            state: { successMessage: `¡Solicitud de "${payload.cargo}" registrada y enviada a revisión de RRHH con éxito!` }
          });
        },
        error: (err) => {
          this.isSubmitting.set(false);
          console.error('Error al registrar la solicitud:', err);
          this.errorMessage.set('Error al registrar la solicitud en el servidor.');
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }
      });
    }
  }

  private enfocarPrimerCampoInvalido(): void {
    const primerInvalido = document.querySelector('.ng-invalid:not(form)');
    if (primerInvalido) {
      primerInvalido.scrollIntoView({ behavior: 'smooth', block: 'center' });
      if (primerInvalido instanceof HTMLElement) {
        primerInvalido.focus();
      }
    }
  }
}
