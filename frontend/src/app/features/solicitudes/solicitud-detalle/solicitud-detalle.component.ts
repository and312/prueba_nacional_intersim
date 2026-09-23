import { Component, OnInit, OnDestroy, inject, signal, ViewChild, TemplateRef } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subscription, interval } from 'rxjs';
import { take } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { normalizeProfileList } from '../../../core/utils/profile-list-normalizer.utils';

@Component({
  selector: 'app-solicitud-detalle',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    DatePipe,
    DecimalPipe,
    FormsModule
  ],
  templateUrl: './solicitud-detalle.component.html',
  styleUrls: ['./solicitud-detalle.component.scss']
})
export class SolicitudDetalleComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  public authService = inject(AuthService);
  private dialog = inject(MatDialog);

  solicitudId = signal<number | null>(null);
  solicitud = signal<any | null>(null);
  historial = signal<any[]>([]);
  modo = signal<string | null>(null);

  private queryParamsSub: Subscription | null = null;
  private paramMapSub: Subscription | null = null;
  
  // Control de vistas
  showTrazabilidad = signal(false);

  // Modales de RRHH
  @ViewChild('observarDialog') observarDialogTmpl!: TemplateRef<any>;
  @ViewChild('aprobarDialog') aprobarDialogTmpl!: TemplateRef<any>;
  @ViewChild('rechazarDialog') rechazarDialogTmpl!: TemplateRef<any>;
  @ViewChild('costWarningDialog') costWarningDialogTmpl!: TemplateRef<any>;
  
  pendingComment = '';
  costCheckData = signal<any | null>(null);
  // Variables de formulario para Registrar Observación
  tipoObservacion = signal('Comentario');
  campoRelacionado = signal('Jornada laboral');

  // Variables de formulario para Aprobar Solicitud
  confirmarCheckAprobacion = signal(false);

  private dialogRef: any;
  isSubmittingAction = signal(false);
  
  // Variables para Responder Observaciones (Diseño de Stitch)
  isSubsanando = signal(false);
  jornadaSeleccionada = signal('L-V 08:30 a 18:30');
  fechaIngresoSeleccionada = signal('');
  comentarioSubsanacion = signal('');
  confirmacionCheck = signal(false);
  activeTab = signal('Condiciones');

  // Variables para el Resumen Estructurado (Diseño Stitch para RRHH)
  confianzaCaptura = signal(92);
  camposDetectados = signal('18/20');
  estadoIA = signal('OK');
  explicacionIA = signal('La solicitud es consistente y suficiente para revisión inicial. Se detectan 2 campos pendientes que deben validarse antes de aprobar.');
  datosPendientes = signal([
    { campo: 'Jornada laboral', estado: 'PENDIENTE' },
    { campo: 'Fecha ideal de ingreso', estado: 'PENDIENTE' }
  ]);
  canalOrigenNombre = signal('WhatsApp Business');
  fechaCaptura = signal('24/10/2024');
  agenteReceptor = signal('Bot RRHH v2.4');


  iniciarObservar(): void {
    this.tipoObservacion.set('Comentario');
    this.campoRelacionado.set('Jornada laboral');
    this.costCheckData.set(null);
    
    const id = this.solicitudId();
    if (!id) return;

    this.isSubmittingAction.set(true);
    this.apiService.checkWhatsAppCost(id).subscribe({
      next: (res: any) => {
        this.isSubmittingAction.set(false);
        this.costCheckData.set(res);
        this.dialogRef = this.dialog.open(this.observarDialogTmpl, {
          width: '560px',
          disableClose: true
        });
      },
      error: (err: any) => {
        this.isSubmittingAction.set(false);
        console.error(err);
        this.costCheckData.set({
          mensajeTiempo: 'Hace poco tiempo',
          requiereConfirmacionWhatsapp: false,
          estadoConversacionWhatsapp: 'Activa'
        });
        this.dialogRef = this.dialog.open(this.observarDialogTmpl, {
          width: '560px',
          disableClose: true
        });
      }
    });
  }

  iniciarAprobar(): void {
    this.confirmarCheckAprobacion.set(false);

    this.dialogRef = this.dialog.open(this.aprobarDialogTmpl, {
      width: '512px',
      disableClose: true
    });
  }

  iniciarRechazar(): void {
    this.dialogRef = this.dialog.open(this.rechazarDialogTmpl, {
      width: '512px',
      disableClose: true
    });
  }

  closeDialog(): void {
    if (this.dialogRef) {
      this.dialogRef.close();
      this.dialogRef = null;
    }
    this.dialog.closeAll();
  }

  confirmarObservacion(comentario: string): void {
    if (!comentario || comentario.trim().length === 0) {
      alert('Por favor, ingrese el motivo de la observación.');
      return;
    }
    const id = this.solicitudId();
    if (!id) return;

    const fullComment = `[Tipo: ${this.tipoObservacion()}] [Campo: ${this.campoRelacionado()}] ${comentario}`;
    const costData = this.costCheckData();

    if (costData && costData.requiereConfirmacionWhatsapp) {
      this.pendingComment = fullComment;
      this.closeDialog();
      
      this.dialogRef = this.dialog.open(this.costWarningDialogTmpl, {
        width: '512px',
        disableClose: true
      });
    } else {
      this.transitar('SOL-OBS', fullComment, false, false);
    }
  }

  closeCostWarning(confirm: boolean): void {
    this.closeDialog();
    if (this.pendingComment) {
      this.transitar('SOL-OBS', this.pendingComment, true, confirm);
    }
    this.pendingComment = '';
  }

  confirmarAprobacion(comentario: string): void {
    if (!this.confirmarCheckAprobacion()) {
      alert('Debe confirmar que revisó la información marcando la casilla de confirmación.');
      return;
    }
    this.transitar('SOL-APR', comentario || 'Aprobado por RRHH');
  }

  confirmarRechazo(comentario: string): void {
    if (!comentario || comentario.trim().length === 0) {
      alert('Por favor, ingrese la justificación del rechazo.');
      return;
    }
    this.transitar('SOL-RECH', comentario);
  }

  iniciarSubsanar(): void {
    const s = this.solicitud();
    if (s) {
      this.jornadaSeleccionada.set(s.jornada || 'L-V 08:30 a 18:30');
      this.fechaIngresoSeleccionada.set(s.fechaIdeal ? new Date(s.fechaIdeal).toISOString().substring(0, 10) : '');
      this.comentarioSubsanacion.set('');
      this.confirmacionCheck.set(false);
      this.activeTab.set('Condiciones');
      this.isSubsanando.set(true);
    }
  }

  cancelarSubsanar(): void {
    this.isSubsanando.set(false);
  }

  enviarSubsanacion(): void {
    const s = this.solicitud();
    if (!s) return;

    if (!this.confirmacionCheck()) {
      alert('Debe confirmar que completó la información observada.');
      return;
    }
    if (!this.fechaIngresoSeleccionada()) {
      alert('La fecha ideal de ingreso es obligatoria.');
      return;
    }
    if (!this.jornadaSeleccionada()) {
      alert('La jornada laboral es obligatoria.');
      return;
    }

    this.isSubmittingAction.set(true);

    let modalidadVal = s.modalidad || 'Presencial';
    if (modalidadVal === 'Híbrido') modalidadVal = 'Hibrido';
    if (modalidadVal === 'Remoto') modalidadVal = 'Teletrabajo';

    let seniorityVal = s.seniority || 'Senior';
    if (seniorityVal === 'Semi-Senior') seniorityVal = 'SemiSenior';

    let prioridadVal = s.prioridad || 'Media';
    if (prioridadVal === 'Alta' || prioridadVal === 'Alta / Urgente') prioridadVal = 'Alta';

    const funcString = Array.isArray(s.funciones) ? s.funciones.join('\n') : (s.funciones || '');
    const skillsString = Array.isArray(s.skillsList) ? s.skillsList.join('\n') : (s.skills || '');

    const payload = {
      cargo: s.cargo || s.nombreCargo || 'Cargo',
      area: s.area || s.areaSolicitante || 'Área',
      modalidad: modalidadVal,
      seniority: seniorityVal,
      prioridad: prioridadVal,
      fechaIdeal: new Date(this.fechaIngresoSeleccionada()).toISOString(),
      funciones: funcString,
      skills: skillsString,
      remuneracionOfrecida: 5000,
      justificacion: s.justificacion || s.descripcionRequerimientos || 'Subsanación de observaciones',
      jornada: this.jornadaSeleccionada(),
      motivo: s.motivo || s.justificacion || s.descripcionRequerimientos || 'Subsanación de observaciones',
      cantidadVacantes: s.cantidadVacantes || 1,
      ubicacion: s.ubicacion || s.sede || 'Santa Cruz (Edificio Nacional)',
      observaciones: s.observaciones || '',
      tipoSolicitud: s.tipoSolicitud || 'Nueva Posición'
    };

    this.apiService.actualizarSolicitud(s.solicitudId || s.id, payload).subscribe({
      next: () => {
        const transText = `[Subsanación] ${this.comentarioSubsanacion() || 'Se corrigieron las observaciones señaladas.'}`;
        this.apiService.transitarSolicitud(s.solicitudId || s.id, 'SOL-ENV', transText).subscribe({
          next: () => {
            this.isSubmittingAction.set(false);
            this.isSubsanando.set(false);
            alert('Subsanación enviada con éxito a RRHH.');
            this.loadSolicitud(s.solicitudId || s.id);
          },
          error: (err) => {
            this.isSubmittingAction.set(false);
            console.error(err);
            alert('Se actualizaron los datos, pero ocurrió un error al enviar el cambio de estado.');
          }
        });
      },
      error: (err) => {
        this.isSubmittingAction.set(false);
        console.error(err);
        alert('Ocurrió un error al intentar actualizar los datos de la solicitud.');
      }
    });
  }

  isActingAsSolicitante(): boolean {
    if (this.modo() === 'solicitante') {
      return true;
    }
    const role = this.authService.getUserRole();
    const area = this.authService.getUserArea();
    return role === 'Solicitante' || (role !== 'RRHH' && area !== 'Recursos Humanos' && role !== 'Administrador');
  }

  isActingAsRrhh(): boolean {
    if (this.modo() === 'solicitante') {
      return false;
    }
    const role = this.authService.getUserRole();
    const area = this.authService.getUserArea();
    return role === 'RRHH' || area === 'Recursos Humanos';
  }

  isEstadoPendienteRevision(): boolean {
    const s = this.solicitud();
    if (!s) return false;
    return s.estadoCodigo === 'SOL-ENV' || 
           s.estadoNombre?.trim().toLowerCase().includes('enviada') || 
           s.estadoNombre?.trim().toLowerCase().includes('validación') || 
           s.estadoNombre?.trim().toLowerCase().includes('revisión');
  }

  enviarARrhh(): void {
    const s = this.solicitud();
    const id = this.solicitudId();
    if (!s || !id) return;
    if (confirm('¿Está seguro de enviar esta solicitud a revisión de RRHH?')) {
      this.isSubmittingAction.set(true);
      this.apiService.enviarSolicitudARRhh(id).subscribe({
        next: () => {
          this.isSubmittingAction.set(false);
          alert('Solicitud enviada a RRHH con éxito.');
          this.loadSolicitud(id);
        },
        error: (err) => {
          this.isSubmittingAction.set(false);
          console.error(err);
          const errorMsg = err?.error?.Message || err?.error?.message || 'Error al intentar enviar la solicitud a RRHH.';
          alert(`No se pudo enviar la solicitud:\n\n${errorMsg}`);
        }
      });
    }
  }

  private transitar(estado: string, comentario: string, seMostroAdvertencia?: boolean, usuarioConfirmoEnvio?: boolean): void {
    const id = this.solicitudId();
    if (!id) return;

    this.isSubmittingAction.set(true);
    this.apiService.transitarSolicitud(id, estado, comentario, seMostroAdvertencia, usuarioConfirmoEnvio).subscribe({
      next: () => {
        this.isSubmittingAction.set(false);
        this.closeDialog();
        this.loadSolicitud(id);
      },
      error: () => {
        this.isSubmittingAction.set(false);
        alert('Ocurrió un error al procesar el cambio de estado.');
      }
    });
  }

  isPollingPdf = signal(false);
  private pdfPollingSubscription: Subscription | null = null;

  ngOnInit(): void {
    this.queryParamsSub = this.route.queryParams.subscribe(params => {
      this.modo.set(params['modo'] || null);
    });

    this.paramMapSub = this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        const id = parseInt(idParam, 10);
        this.solicitudId.set(id);
        this.loadSolicitud(id);
      }
    });
  }

  ngOnDestroy(): void {
    this.stopPollingPdf();
    if (this.queryParamsSub) {
      this.queryParamsSub.unsubscribe();
    }
    if (this.paramMapSub) {
      this.paramMapSub.unsubscribe();
    }
  }

  iniciarPollingPdf(id: number): void {
    if (this.pdfPollingSubscription) return;
    this.isPollingPdf.set(true);

    this.pdfPollingSubscription = interval(3000).pipe(take(6)).subscribe({
      next: () => {
        this.apiService.getSolicitudById(id).subscribe({
          next: (res) => {
            if (res && res.pdfDocumentUrl) {
              const mapped = { ...res };
              if (typeof mapped.funciones === 'string') {
                mapped.funciones = mapped.funciones.split('\n').map((f: string) => f.trim()).filter((f: string) => f.length > 0);
              }
              this.solicitud.set(mapped);
              this.stopPollingPdf();
            }
          }
        });
      },
      complete: () => {
        this.stopPollingPdf();
      }
    });
  }

  stopPollingPdf(): void {
    if (this.pdfPollingSubscription) {
      this.pdfPollingSubscription.unsubscribe();
      this.pdfPollingSubscription = null;
    }
    this.isPollingPdf.set(false);
  }

  loadSolicitud(id: number): void {
    this.apiService.getSolicitudById(id).subscribe({
      next: (res) => {
        const mapped = { ...res };
        if (typeof mapped.funciones === 'string') {
          mapped.funciones = mapped.funciones.split('\n').map((f: string) => f.trim()).filter((f: string) => f.length > 0);
        }

        let educacion = '';
        let experiencia = '';
        let skillsLimpias = mapped.skills || '';
        
        if (skillsLimpias.includes('Educación:')) {
          const parts = skillsLimpias.split('Educación:');
          skillsLimpias = parts[0].trim();
          const rest = parts[1] || '';
          if (rest.includes('Experiencia:')) {
            const subparts = rest.split('Experiencia:');
            educacion = subparts[0].trim();
            experiencia = subparts[1].trim();
          } else {
            educacion = rest.trim();
          }
        }

        if (skillsLimpias) {
          mapped.skillsList = skillsLimpias.split('\n').map((s: string) => s.trim()).filter((s: string) => s.length > 0);
        } else {
          mapped.skillsList = [];
        }

        mapped.educacion = educacion;
        mapped.experiencia = experiencia;

        this.solicitud.set(mapped);

        // Polling reactivo para el PDF si la solicitud está aprobada pero aún no tiene pdfDocumentUrl
        const est = (mapped.estadoCodigo || mapped.estadoNombre || '').toLowerCase();
        const esAprobada = est.includes('apr') || est.includes('sol-apr');
        if (esAprobada && !mapped.pdfDocumentUrl) {
          this.iniciarPollingPdf(id);
        } else if (mapped.pdfDocumentUrl) {
          this.stopPollingPdf();
        }
      },
      error: () => {
        // Fallback para el MVP con los datos exactos del diseño de Stitch
        this.solicitud.set({
          id: id,
          nombreCargo: id === 43 ? 'Asistente Administrativo' : 'Analista Actuarial',
          areaSolicitante: id === 43 ? 'Operaciones' : 'Finanzas',
          motivoContratacion: id === 43 ? 'Crecimiento' : 'Nueva Posición',
          descripcionRequerimientos: id === 43 ? 'Apoyo administrativo, gestión de archivos y atención de llamadas.' : 'Conocimientos en .NET 8, C#, SQL Server.',
          estadoNombre: id === 43 ? 'Aprobada para perfil' : 'En revisión',
          prioridad: id === 43 ? 'Baja' : 'Alta',
          solicitante: id === 43 ? 'Carla Rojas' : 'Juan Delgado',
          jefatura: id === 43 ? 'Gerencia de Operaciones' : 'Gerencia de Finanzas',
          modalidad: 'Presencial',
          sede: 'Santa Cruz',
          jornada: 'Administrativa (8:30 - 18:30)',
          fechaIdeal: '01/11/2026',
          seniority: id === 43 ? 'Junior' : 'Senior',
          experiencia: id === 43 ? '1 año' : '3 años',
          funciones: [
            'Apoyo administrativo general al área.',
            'Gestión, foliación y orden de archivos físicos y digitales.',
            'Atención y canalización de llamadas telefónicas.'
          ],
          requisitos: ['Excel intermedio', 'Organización', 'Comunicación asertiva'],
          skillsList: ['Excel intermedio', 'Organización', 'Comunicación asertiva']
        });
      }
    });

    this.apiService.getHistorialSolicitud(id).subscribe({
      next: (history) => {
        this.historial.set(history);
      },
      error: () => {
        this.historial.set([]);
      }
    });
  }

  getPdfEstadoText(): string {
    if (this.isPollingPdf()) return 'Generando PDF...';
    const s = this.solicitud();
    if (!s) return 'Generando documento...';
    if (s.pdfDocumentUrl) return 'Generado automáticamente';

    const est = (s.estadoCodigo || s.estadoNombre || '').toLowerCase();
    if (est.includes('rech') || est.includes('sol-rech')) return 'No generado por rechazo';
    if (est.includes('obs') || est.includes('sol-obs')) return 'Pausado por observación';
    if (est.includes('can') || est.includes('sol-can')) return 'No generado por cancelación';
    if (est.includes('apr') || est.includes('sol-apr')) return 'Generando PDF...';
    return 'En proceso de validación...';
  }

  getPdfEstadoColor(): string {
    if (this.isPollingPdf()) return '#0284c7';
    const s = this.solicitud();
    if (!s) return '#d97706';
    if (s.pdfDocumentUrl) return '#047857';

    const est = (s.estadoCodigo || s.estadoNombre || '').toLowerCase();
    if (est.includes('rech') || est.includes('sol-rech')) return '#dc2626';
    if (est.includes('obs') || est.includes('sol-obs')) return '#d97706';
    if (est.includes('can') || est.includes('sol-can')) return '#64748b';
    if (est.includes('apr') || est.includes('sol-apr')) return '#0284c7';
    return '#d97706';
  }

  getPdfFechaText(): string {
    const s = this.solicitud();
    if (!s) return 'Pendiente';
    if (s.pdfDocumentUrl) return '';

    const est = (s.estadoCodigo || s.estadoNombre || '').toLowerCase();
    if (est.includes('rech') || est.includes('sol-rech') || est.includes('can') || est.includes('sol-can')) return 'No aplica';
    if (est.includes('obs') || est.includes('sol-obs')) return 'Pendiente de subsanación';
    if (est.includes('apr') || est.includes('sol-apr') || this.isPollingPdf()) return 'Generando...';
    return 'Pendiente';
  }

  getPdfTooltipText(): string {
    if (this.isPollingPdf()) return 'El documento PDF se está compilando en segundo plano y se activará automáticamente en unos segundos...';
    const s = this.solicitud();
    if (!s) return 'Documento no disponible';
    if (s.pdfDocumentUrl) return 'Abrir documento PDF';

    const est = (s.estadoCodigo || s.estadoNombre || '').toLowerCase();
    if (est.includes('rech') || est.includes('sol-rech')) return 'No se generó documento PDF debido a que la solicitud fue rechazada';
    if (est.includes('obs') || est.includes('sol-obs')) return 'El documento estará disponible al subsanar y aprobar la solicitud';
    if (est.includes('can') || est.includes('sol-can')) return 'No se generó documento PDF debido a que la solicitud fue cancelada';
    return 'Se generará automáticamente tras la aprobación';
  }

  getEstadoColor(estado: string): string {
    const est = (estado || '').toLowerCase();
    if (est.includes('aprobada') || est.includes('apr')) return '#16a34a'; // Verde
    if (est.includes('rechazada') || est.includes('rech')) return '#dc2626'; // Rojo
    if (est.includes('observada') || est.includes('obs')) return '#e65300'; // Naranja
    if (est.includes('enviada') || est.includes('env') || est.includes('revisión') || est.includes('validación')) return '#005b96'; // Azul
    if (est.includes('pendiente') || est.includes('pen')) return '#64748b'; // Gris
    return '#004370'; // Azul oscuro institucional
  }

  getEstadoIcon(estado: string): string {
    const est = (estado || '').toLowerCase();
    if (est.includes('aprobada') || est.includes('apr')) return 'check';
    if (est.includes('rechazada') || est.includes('rech')) return 'close';
    if (est.includes('observada') || est.includes('obs')) return 'warning';
    if (est.includes('enviada') || est.includes('env')) return 'send';
    if (est.includes('pendiente') || est.includes('pen')) return 'schedule';
    return 'person';
  }

  isWhatsAppOrigin(item?: any): boolean {
    const canal = (this.solicitud()?.canalOrigen || item?.canalOrigen || '').toLowerCase();
    return canal.includes('whatsapp') || canal.includes('chat') || canal.includes('bot');
  }

  getTimelineItemIcon(item: any, isFirst: boolean): string {
    if (isFirst) {
      if (this.isWhatsAppOrigin(item)) {
        return 'chat';
      }
      return 'desktop_windows';
    }
    return this.getEstadoIcon(item.estadoNuevo);
  }

  getTimelineItemColor(item: any, isFirst: boolean): string {
    if (isFirst) {
      const canal = (this.solicitud()?.canalOrigen || item?.canalOrigen || '').toLowerCase();
      if (canal.includes('whatsapp') || canal.includes('chat') || canal.includes('bot')) {
        return '#25D366'; // Verde WhatsApp
      }
      return '#005b96'; // Azul Backoffice
    }
    return this.getEstadoColor(item.estadoNuevo);
  }

  getTimelineItemCanalBadge(item: any, isFirst: boolean) {
    const estadoAnterior = (item?.estadoAnterior || '').toLowerCase();
    const estadoNuevo = (item?.estadoNuevo || '').toLowerCase();

    // Mostrar badge en primer evento o en eventos de corrección realizada por el área solicitante
    const esCorreccionArea = estadoAnterior.includes('observad') || estadoNuevo.includes('corregida') || estadoAnterior.includes('corregida');

    if (!isFirst && !esCorreccionArea) return null;

    const canal = (this.solicitud()?.canalOrigen || item?.canalOrigen || '').toLowerCase();
    if (canal.includes('whatsapp') || canal.includes('chat') || canal.includes('bot')) {
      return {
        texto: 'Origen: WhatsApp',
        color: '#15803d',
        bg: '#dcfce7',
        border: '#86efac'
      };
    }
    return {
      texto: 'Origen: Backoffice',
      color: '#0369a1',
      bg: '#e0f2fe',
      border: '#7dd3fc'
    };
  }

  getActorNombre(item: any, isFirst: boolean): string {
    const estadoNuevo = (item?.estadoNuevo || '').toLowerCase();
    const estadoAnterior = (item?.estadoAnterior || '').toLowerCase();

    // 1. Evento inicial o corrección enviada por el Solicitante (desde WhatsApp o Backoffice)
    if (
      isFirst ||
      estadoNuevo.includes('corregida') ||
      estadoAnterior.includes('corregida') ||
      estadoAnterior.includes('observad') ||
      (estadoAnterior.includes('recibida') && estadoNuevo.includes('pendiente'))
    ) {
      const nombreSol =
        this.solicitud()?.solicitanteNombre ||
        this.solicitud()?.nombreSolicitante ||
        this.solicitud()?.solicitante ||
        this.solicitud()?.createdBy;
      if (nombreSol && !nombreSol.toLowerCase().includes('admin')) {
        return nombreSol;
      }
      if (item?.cambiadoPor && !item.cambiadoPor.toLowerCase().includes('admin')) {
        return item.cambiadoPor;
      }
      return nombreSol || 'Yohana Pelaez';
    }

    // 2. Transición inicial automática del bot (Pendiente de Datos -> Solicitud Enviada a RRHH)
    if (
      (estadoAnterior.includes('pendiente') || estadoAnterior.includes('recibida')) &&
      (estadoNuevo.includes('enviada a rrhh') || estadoNuevo.includes('sol-env'))
    ) {
      return 'Agente IA Solicitudes';
    }

    // 3. Decisiones u observaciones de RRHH
    if (estadoNuevo.includes('observad') || estadoNuevo.includes('aprob') || estadoNuevo.includes('rechaz')) {
      if (item?.cambiadoPor && !item.cambiadoPor.toLowerCase().includes('admin')) {
        return item.cambiadoPor;
      }
      return 'Usuario RRHH';
    }

    // 4. Default fallback por el nombre registrado
    if (item?.cambiadoPor && !item.cambiadoPor.toLowerCase().includes('admin')) {
      return item.cambiadoPor;
    }
    return this.solicitud()?.solicitanteNombre || 'Yohana Pelaez';
  }

  getActorRol(item: any, isFirst: boolean): string {
    const estadoNuevo = (item?.estadoNuevo || '').toLowerCase();
    const estadoAnterior = (item?.estadoAnterior || '').toLowerCase();

    // 1. Solicitante / Área Solicitante (registro o corrección desde WSSP o Backoffice)
    if (
      isFirst ||
      estadoNuevo.includes('corregida') ||
      estadoAnterior.includes('corregida') ||
      estadoAnterior.includes('observad') ||
      (estadoAnterior.includes('recibida') && estadoNuevo.includes('pendiente'))
    ) {
      return 'Área Solicitante';
    }

    // 2. Agente IA (evaluación inicial)
    if (
      (estadoAnterior.includes('pendiente') || estadoAnterior.includes('recibida')) &&
      (estadoNuevo.includes('enviada a rrhh') || estadoNuevo.includes('sol-env'))
    ) {
      return 'Bot IA';
    }

    // 3. RRHH
    if (estadoNuevo.includes('observad') || estadoNuevo.includes('aprob') || estadoNuevo.includes('rechaz')) {
      return item?.rol && !item.rol.toLowerCase().includes('admin') ? item.rol : 'RRHH';
    }

    return item?.rol || 'Área Solicitante';
  }

  getJustificacion(item: any, isFirst?: boolean): string {
    const com = item?.justificacion || item?.comentario || '';
    const estadoAnterior = (item?.estadoAnterior || '').toLowerCase();
    const estadoNuevo = (item?.estadoNuevo || '').toLowerCase();

    const esCorreccion =
      estadoAnterior.includes('observad') ||
      estadoAnterior.includes('corregida') ||
      estadoNuevo.includes('corregida');

    if (esCorreccion) {
      if (
        com &&
        !com.toUpperCase().includes('APROBADO POR EL AGENTE') &&
        !com.toUpperCase().includes('TRANSICIÓN DE ESTADO REGISTRADA')
      ) {
        return com;
      }
      return 'SE CORRIGIERON LOS PUNTOS OBSERVADOS Y SE RE-ENVIÓ A RRHH';
    }

    return com;
  }

  getMarkerClass(estado: string, isLast: boolean): string {
    const est = (estado || '').toLowerCase();
    if (isLast) {
      if (est.includes('rechazada') || est.includes('rech') || est.includes('observada') || est.includes('obs')) {
        return 'detail-timeline-marker-observed';
      }
      if (est.includes('aprobada') || est.includes('apr')) {
        return 'detail-timeline-marker-complete';
      }
      return 'detail-timeline-marker-current';
    }
    
    if (est.includes('observada') || est.includes('obs') || est.includes('rechazada') || est.includes('rech')) {
      return 'detail-timeline-marker-observed';
    }
    if (est.includes('pendiente') || est.includes('pen')) {
      return 'detail-timeline-marker-pending';
    }
    return 'detail-timeline-marker-complete';
  }

  getCompletitudPorcentaje(sol: any): number {
    if (!sol) return 60;
    if (typeof sol === 'number') return sol;

    if (typeof sol === 'object' && typeof sol.completitudPorcentaje === 'number' && sol.completitudPorcentaje > 0) {
      return Math.min(100, Math.max(0, Math.round(sol.completitudPorcentaje)));
    }

    const text = (sol.estadoCodigo || sol.estadoNombre || (typeof sol === 'string' ? sol : '')).toLowerCase();
    if (text.includes('borr') || text.includes('sol-bor')) return 20;
    if (text.includes('pendiente') || text.includes('sol-pen')) return 40;
    if (text.includes('enviada') || text.includes('revisi') || text.includes('sol-env')) return 60;
    if (text.includes('observad') || text.includes('sol-obs')) return 75;
    if (text.includes('aprob') || text.includes('sol-apr') || text.includes('rechaz') || text.includes('sol-rech')) return 100;
    return 60;
  }

  hasValue(val: any): boolean {
    if (val === null || val === undefined) return false;
    if (typeof val === 'string') {
      const trimmed = val.trim().toLowerCase();
      return trimmed !== '' && 
             trimmed !== 'no especificado' && 
             trimmed !== 'null' && 
             trimmed !== 'undefined' && 
             trimmed !== '—';
    }
    if (typeof val === 'object') {
      if (Array.isArray(val)) return val.length > 0;
      return !!val.nombre || !!val.codigo;
    }
    return true;
  }

  formatArrayOrText(val: any): string[] {
    return normalizeProfileList(val);
  }

  formatTextParagraph(val: any): string {
    if (!val) return '';
    if (Array.isArray(val)) {
      return val.map(x => String(x).trim()).filter(Boolean).join(' ');
    }
    let str = String(val).trim();
    if (str.length > 0) {
      str = str.charAt(0).toUpperCase() + str.slice(1);
    }
    return str;
  }

  volver(): void {
    this.router.navigate(['/solicitudes']);
  }

  descargarPdf(event?: Event): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    const s = this.solicitud();
    if (!s) return;

    if (s.pdfDocumentUrl && s.pdfDocumentUrl.startsWith('http')) {
      window.open(s.pdfDocumentUrl, '_blank');
      return;
    }

    const id = s.solicitudId || s.id;
    if (!id) return;

    this.apiService.descargarPdfSolicitud(id).subscribe({
      next: (blob: Blob) => {
        if (blob.type && blob.type.includes('json')) {
          blob.text().then(text => {
            console.error('Error del servidor:', text);
            alert('El archivo PDF de la solicitud no se encuentra disponible en el servidor.');
          });
          return;
        }
        const blobUrl = URL.createObjectURL(blob);
        window.open(blobUrl, '_blank');
      },
      error: (err) => {
        console.error('Error al descargar PDF de solicitud:', err);
        if (s.pdfDocumentUrl) {
          const targetUrl = s.pdfDocumentUrl.startsWith('/') ? s.pdfDocumentUrl : `/${s.pdfDocumentUrl}`;
          window.open(targetUrl, '_blank');
        } else {
          alert('No se pudo abrir el documento PDF de la solicitud.');
        }
      }
    });
  }
}
