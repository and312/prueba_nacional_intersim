import { Component, OnInit, OnDestroy, signal, computed, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, timer } from 'rxjs';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { AuthService } from '../../../core/services/auth.service';
import { EstrategiasService } from '../services/estrategias.service';
import {
  EstrategiaDetalle,
  EstadoEstrategia,
  ESTADO_ESTRATEGIA_CONFIG,
  PostulanteEvaluadoManualItem
} from '../models/estrategia.model';

@Component({
  selector: 'app-detalle-estrategia',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LoaderComponent,
    DatePipe
  ],
  templateUrl: './detalle-estrategia.component.html',
  styleUrls: ['./detalle-estrategia.component.scss']
})
export class DetalleEstrategiaComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private estrategiasService = inject(EstrategiasService);
  private authService = inject(AuthService);
  private pollingSub: Subscription | null = null;

  // States & Signals
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);
  estrategia = signal<EstrategiaDetalle | null>(null);

  // Role permissions
  userRole = computed(() => this.authService.getUserRole());
  isRRHH = computed(() => this.userRole() === 'RRHH');
  isAdmin = computed(() => this.userRole() === 'Administrador');

  // Accordion
  isParametrosOpen = signal<boolean>(false);
  isPlanEvaluacionOpen = signal<boolean>(true);
  isPlanBusquedaOpen = signal<boolean>(true);
  isCriteriosDificilesOpen = signal<boolean>(true);
  isPerfilObjetivoOpen = signal<boolean>(true);
  isCanalesSugeridosOpen = signal<boolean>(true);
  isPlanAccionExternaOpen = signal<boolean>(true);
  isPlanAccionOpen = signal<boolean>(true);

  togglePlanAccion(): void {
    this.isPlanAccionOpen.update(v => !v);
  }

  togglePlanEvaluacion(): void {
    this.isPlanEvaluacionOpen.update(v => !v);
  }

  togglePlanBusqueda(): void {
    this.isPlanBusquedaOpen.update(v => !v);
  }

  toggleCriteriosDificiles(): void {
    this.isCriteriosDificilesOpen.update(v => !v);
  }

  togglePerfilObjetivo(): void {
    this.isPerfilObjetivoOpen.update(v => !v);
  }

  toggleCanalesSugeridos(): void {
    this.isCanalesSugeridosOpen.update(v => !v);
  }

  togglePlanAccionExterna(): void {
    this.isPlanAccionExternaOpen.update(v => !v);
  }

  // Modal Brief Signals
  isBriefModalOpen = signal<boolean>(false);
  briefModalTab = signal<'EMOJI' | 'ESTRUCTURADO'>('ESTRUCTURADO');
  briefCopiedToast = signal<boolean>(false);

  abrirVistaPreviaBrief(): void {
    this.isBriefModalOpen.set(true);
  }

  cerrarVistaPreviaBrief(): void {
    this.isBriefModalOpen.set(false);
  }

  setBriefModalTab(tab: 'EMOJI' | 'ESTRUCTURADO'): void {
    this.briefModalTab.set('ESTRUCTURADO');
  }

  copiarTextoBrief(): void {
    const brief = this.estrategia()?.estrategiaExterna?.briefEditable;
    
    const texto = `Título de publicación:
${brief?.titulo || 'Suscriptor Senior de Ramos Generales — La Paz'}

Modalidad:
${brief?.modalidad || 'Presencial · Tiempo completo'}

Regional:
${brief?.ubicacion || 'La Paz'}

Banda salarial:
Bs 12.500 – 15.800

Formación:
${brief?.formacion || 'Ing. Comercial, Economía, Administración o afín'}

Experiencia mínima:
${brief?.experiencia || '4 años en suscripción de ramos generales'}

Resumen del cargo:
${brief?.introduccion || 'Responsable de evaluar, suscribir y mantener carteras de riesgos de ramos generales cumpliendo la normativa interna y de la ASFI.'}

Objetivo del cargo:
${brief?.desafioPrincipal || 'Garantizar la calidad técnica de la suscripción y contribuir al crecimiento rentable de la cartera regional.'}

Funciones principales:
${(brief?.funciones || []).join(' · ') || 'Evaluación de riesgos · Suscripción de pólizas · Análisis de siniestralidad · Coordinación con comercial · Reportes técnicos'}

Requisitos obligatorios:
Experiencia en suscripción · Manejo de tarifarios · Conocimiento normativo

Requisitos deseables:
Experiencia en reaseguros · Inglés intermedio`;

    navigator.clipboard.writeText(texto).then(() => {
      this.briefCopiedToast.set(true);
      setTimeout(() => this.briefCopiedToast.set(false), 3000);
    });
  }

  // Brief Editable State & Services Integration
  isGuardandoBrief = signal<boolean>(false);
  isBriefModoEdicion = signal<boolean>(false);
  briefUltimoGuardado = signal<string>('Guardado 31/07/2026, 10:27 a. m.');

  briefForm = signal<{
    titulo: string;
    modalidad: string;
    ubicacion: string;
    bandaSalarial: string;
    formacion: string;
    experiencia: string;
    introduccion: string;
    desafioPrincipal: string;
    funciones: string;
    requisitosObligatorios: string;
    requisitosDeseables: string;
    hardSkills: string;
  }>({
    titulo: '',
    modalidad: '',
    ubicacion: '',
    bandaSalarial: '',
    formacion: '',
    experiencia: '',
    introduccion: '',
    desafioPrincipal: '',
    funciones: '',
    requisitosObligatorios: '',
    requisitosDeseables: '',
    hardSkills: ''
  });

  activarEdicionBrief(): void {
    this.isBriefModoEdicion.set(true);
  }

  cancelarEdicionBrief(): void {
    const est = this.estrategia();
    if (est) {
      this.initBriefForm(est);
    }
    this.isBriefModoEdicion.set(false);
  }

  updateBriefField(field: string, event: Event): void {
    const value = (event.target as HTMLInputElement | HTMLTextAreaElement)?.value || '';
    this.briefForm.update(current => ({ ...current, [field]: value }));
  }

  private initBriefForm(est: EstrategiaDetalle): void {
    const b = est?.estrategiaExterna?.briefEditable;
    this.briefForm.set({
      titulo: b?.titulo || '¡Estamos buscando un/a Analista de Cumplimiento Financiero!',
      modalidad: b?.modalidad || 'Presencial',
      ubicacion: b?.ubicacion || est.regional || 'La Paz',
      bandaSalarial: b?.bandaSalarial || (est.bandaSalarial ? `Bs ${est.bandaSalarial.minimo.toLocaleString('es-BO')} – ${est.bandaSalarial.maximo.toLocaleString('es-BO')}` : 'Bs 12.500 – 15.800'),
      formacion: b?.formacion || 'Ing. Comercial, Economía, Administración o afín',
      experiencia: b?.experiencia || '4 años en suscripción de ramos generales',
      introduccion: b?.introduccion || 'Responsable de evaluar, suscribir y mantener carteras de riesgos de ramos generales cumpliendo la normativa interna y de la ASFI.',
      desafioPrincipal: b?.desafioPrincipal || 'Garantizar la calidad técnica de la suscripción y contribuir al crecimiento rentable de la cartera regional.',
      funciones: Array.isArray(b?.funciones) ? b.funciones.join(' · ') : (b?.funciones || 'Evaluación de riesgos · Suscripción de pólizas · Análisis de siniestralidad · Coordinación con comercial · Reportes técnicos'),
      requisitosObligatorios: Array.isArray((b as any)?.requisitosObligatorios) ? (b as any).requisitosObligatorios.join(' · ') : ((b as any)?.requisitosObligatorios || 'Experiencia en suscripción · Manejo de tarifarios · Conocimiento normativo'),
      requisitosDeseables: Array.isArray((b as any)?.requisitosDeseables) ? (b as any).requisitosDeseables.join(' · ') : ((b as any)?.requisitosDeseables || 'Experiencia en reaseguros · Inglés intermedio'),
      hardSkills: Array.isArray((b as any)?.hardSkills) ? (b as any).hardSkills.join(' · ') : ((b as any)?.hardSkills || 'Excel avanzado · Sistemas core de seguros · Análisis actuarial básico')
    });

    const fechaMod = est?.estrategiaExterna?.fechaModificacion || est?.estrategiaExterna?.fechaCreacion;
    if (fechaMod) {
      const dt = new Date(fechaMod);
      if (!isNaN(dt.getTime())) {
        const pad = (n: number) => n.toString().padStart(2, '0');
        const day = pad(dt.getDate());
        const month = pad(dt.getMonth() + 1);
        const year = dt.getFullYear();
        let hours = dt.getHours();
        const ampm = hours >= 12 ? 'p. m.' : 'a. m.';
        hours = hours % 12 || 12;
        const minutes = pad(dt.getMinutes());
        this.briefUltimoGuardado.set(`Guardado ${day}/${month}/${year}, ${hours}:${minutes} ${ampm}`);
      }
    }
  }

  guardarBorradorBrief(): void {
    const currentEst = this.estrategia();
    if (!currentEst) return;

    const matchingId = (currentEst.ejecucionMatching as any)?.matchingEjecucionId || (currentEst.ejecucionMatching as any)?.id || currentEst.estrategiaId;
    const form = this.briefForm();
    const ext = currentEst.estrategiaExterna;

    const briefingEditableObj = {
      titulo: form.titulo,
      modalidad: form.modalidad,
      ubicacion: form.ubicacion,
      bandaSalarial: form.bandaSalarial,
      formacion: form.formacion,
      experiencia: form.experiencia,
      introduccion: form.introduccion,
      desafioPrincipal: form.desafioPrincipal,
      funciones: form.funciones.split('·').map(s => s.trim()).filter(Boolean),
      requisitosObligatorios: form.requisitosObligatorios.split('·').map(s => s.trim()).filter(Boolean),
      requisitosDeseables: form.requisitosDeseables.split('·').map(s => s.trim()).filter(Boolean),
      hardSkills: form.hardSkills.split('·').map(s => s.trim()).filter(Boolean)
    };

    const saveDto = {
      matchingEjecucionId: matchingId,
      prioridad: ext?.prioridad || 'PRIORITARIA',
      justificacion: ext?.justificacion || 'Atracción proactiva en el mercado laboral y portales especializados para cubrir la posición.',
      criteriosDificiles: ext?.criteriosDificiles || [],
      publicoObjetivo: ext?.publicoObjetivo || {},
      canalesSugeridos: ext?.canalesSugeridos || [],
      planAccion: ext?.planAccion || [],
      briefingEditable: briefingEditableObj,
      conclusion: ext?.conclusion || '',
      estadoId: ext?.estadoId || null
    };

    this.isGuardandoBrief.set(true);

    this.estrategiasService.guardarBriefExterna(saveDto).subscribe({
      next: (res) => {
        this.isGuardandoBrief.set(false);
        this.isBriefModoEdicion.set(false);

        const now = new Date();
        const pad = (n: number) => n.toString().padStart(2, '0');
        const day = pad(now.getDate());
        const month = pad(now.getMonth() + 1);
        const year = now.getFullYear();
        let hours = now.getHours();
        const ampm = hours >= 12 ? 'p. m.' : 'a. m.';
        hours = hours % 12 || 12;
        const minutes = pad(now.getMinutes());
        this.briefUltimoGuardado.set(`Guardado ${day}/${month}/${year}, ${hours}:${minutes} ${ampm}`);

        this.mostrarToast('success', 'Borrador del brief guardado exitosamente.');
        this.estrategia.update(prev => {
          if (!prev) return prev;
          return {
            ...prev,
            estrategiaExterna: {
              ...prev.estrategiaExterna,
              estrategiaExternaId: res?.estrategiaExternaId || prev.estrategiaExterna?.estrategiaExternaId || 0,
              matchingEjecucionId: matchingId,
              prioridad: prev.estrategiaExterna?.prioridad || 'PRIORITARIA',
              justificacion: prev.estrategiaExterna?.justificacion || '',
              criteriosDificiles: prev.estrategiaExterna?.criteriosDificiles || [],
              publicoObjetivo: prev.estrategiaExterna?.publicoObjetivo || ({} as any),
              canalesSugeridos: prev.estrategiaExterna?.canalesSugeridos || [],
              planAccion: prev.estrategiaExterna?.planAccion || [],
              briefEditable: briefingEditableObj as any,
              conclusion: prev.estrategiaExterna?.conclusion || ''
            }
          };
        });
      },
      error: (err) => {
        console.error('Error guardando brief en backend:', err);
        this.isGuardandoBrief.set(false);
        this.mostrarToast('error', 'Ocurrió un error al guardar el brief en el servidor.');
      }
    });
  }

  // Computed: Plan de Acción e Evaluación Interna
  // Computed: Conteo Consolidado Combinado (Internos + Históricos)
  totalViablesGlobal = computed(() => {
    const est = this.estrategia();
    const intCount = est?.talentoInterno?.length || 0;
    const extCount = est?.postulantesHistoricos?.length || 0;
    return intCount + extCount;
  });

  totalMatchAltoGlobal = computed(() => {
    const est = this.estrategia();
    const intAlto = (est?.talentoInterno || []).filter(item => this.isMatchAlto(item.clasificacion)).length;
    const extAlto = (est?.postulantesHistoricos || []).filter(post => this.isMatchAlto(post.clasificacion)).length;
    return intAlto + extAlto;
  });

  totalMatchParcialGlobal = computed(() => {
    const est = this.estrategia();
    const intMed = (est?.talentoInterno || []).filter(item => this.isMatchParcial(item.clasificacion)).length;
    const extMed = (est?.postulantesHistoricos || []).filter(post => this.isMatchParcial(post.clasificacion)).length;
    return intMed + extMed;
  });

  topCandidatoGlobal = computed(() => {
    const est = this.estrategia();
    const intList = (est?.talentoInterno || []).map(item => ({ codigo: item.codigo, match: item.matchPorcentaje || 0 }));
    const extList = (est?.postulantesHistoricos || []).map(post => ({ codigo: post.codigo, match: post.matchPorcentaje || 0 }));
    const combined = [...intList, ...extList].sort((a, b) => b.match - a.match);
    return combined[0]?.codigo || 'INT-0330';
  });

  plazoInternoDiasGlobal = computed(() => {
    const plan = this.planAccionOrdenado();
    if (!plan.length) return 7;
    let maxDia = 0;
    for (const paso of plan) {
      if (paso.plazo) {
        const matches = paso.plazo.match(/\d+/g);
        if (matches) {
          for (const m of matches) {
            const val = parseInt(m, 10);
            if (val > maxDia) maxDia = val;
          }
        }
      }
    }
    return maxDia > 0 ? maxDia : 7;
  });

  topCandidatoInterno = computed(() => {
    const list = this.estrategia()?.talentoInterno || [];
    if (!list.length) return 'INT-0285';
    const sorted = [...list].sort((a, b) => (b.matchPorcentaje || 0) - (a.matchPorcentaje || 0));
    return sorted[0]?.codigo || 'INT-0285';
  });

  planAccionOrdenado = computed(() => {
    const list = this.estrategia()?.estrategiaInterna?.planAccion || [];
    return [...list].sort((a, b) => (a.orden ?? 0) - (b.orden ?? 0));
  });

  planEvaluacionOrdenado = computed(() => {
    const list = this.estrategia()?.estrategiaInterna?.planEvaluacion || [];
    return [...list].sort((a, b) => (a.orden ?? 0) - (b.orden ?? 0));
  });

  // Computed: Canales y Plan de Acción Externa
  canalesSugeridosOrdenados = computed(() => {
    const list = this.estrategia()?.estrategiaExterna?.canalesSugeridos || [];
    return [...list].sort((a, b) => (a.prioridad ?? 0) - (b.prioridad ?? 0));
  });

  planAccionExternaOrdenado = computed(() => {
    const list = this.estrategia()?.estrategiaExterna?.planAccion || [];
    return [...list].sort((a, b) => (a.orden ?? 0) - (b.orden ?? 0));
  });

  private formatEnumText(val: string): string {
    if (!val) return '';
    const cleaned = val.replace(/_/g, ' ').toLowerCase().trim();
    return cleaned.charAt(0).toUpperCase() + cleaned.slice(1);
  }

  getCanalLabel(canal: string): string {
    if (!canal) return 'Canal sugerido';
    const norm = canal.toUpperCase().trim();
    switch (norm) {
      case 'LINKEDIN':
        return 'LinkedIn';
      case 'PORTALES_ESPECIALIZADOS':
        return 'Portales especializados';
      case 'BUSQUEDA_DIRECTA':
        return 'Búsqueda directa';
      case 'REFERIDOS_SECTORIALES':
        return 'Referidos sectoriales';
      case 'WEB_INSTITUCIONAL':
        return 'Web institucional';
      case 'REDES_SOCIALES':
        return 'Redes sociales';
      case 'BOLSA_TRABAJO':
        return 'Bolsa de trabajo';
      default:
        return this.formatEnumText(canal);
    }
  }

  getTipoAccionLabel(tipo: string): string {
    if (!tipo) return 'Acción';
    const norm = tipo.toUpperCase().trim();
    switch (norm) {
      case 'BUSQUEDA_Y_PUBLICACION':
        return 'Búsqueda y publicación';
      case 'BUSQUEDA_DIRECTA':
        return 'Búsqueda directa';
      case 'DIFUSION':
        return 'Difusión';
      case 'PUBLICACION':
        return 'Publicación';
      case 'CONVOCATORIA':
        return 'Convocatoria';
      default:
        return this.formatEnumText(tipo);
    }
  }

  getCriterioSeleccionLabel(criterio: string): string {
    if (!criterio) return 'Criterio de selección';
    const norm = criterio.toUpperCase().trim();
    switch (norm) {
      case 'PERFIL_ESPECIALIZADO':
        return 'Perfil especializado';
      case 'SEGMENTACION_SECTORIAL':
        return 'Segmentación sectorial';
      case 'PERFIL_DE_ALTA_DEMANDA':
        return 'Perfil de alta demanda';
      case 'RED_SECTORIAL':
        return 'Red sectorial';
      case 'MARCA_EMPLEADORA':
        return 'Marca empleadora';
      default:
        return this.formatEnumText(criterio);
    }
  }

  formatPlazoAccionExterna(plazo: string, index: number, list: any[]): string {
    if (!plazo) return '';
    if (plazo.includes('-')) return plazo;

    const match = plazo.match(/\d+/);
    if (!match) return plazo;

    const currentDay = parseInt(match[0], 10);
    const nextItem = list[index + 1];

    if (nextItem && nextItem.plazo) {
      const nextMatch = nextItem.plazo.match(/\d+/);
      if (nextMatch) {
        const nextDay = parseInt(nextMatch[0], 10);
        if (nextDay > currentDay + 1) {
          return `Día ${currentDay}-${nextDay - 1}`;
        }
      }
    }

    return plazo;
  }

  getClasificacionLabel(clasificacion?: string): string {
    if (!clasificacion) return 'Ajuste medio';
    const norm = clasificacion.toUpperCase().trim();
    if (norm === 'AJUSTE_MEDIO' || norm === 'MATCH_MEDIO' || norm === 'MEDIO' || norm === 'MATCH PARCIAL' || norm === 'AJUSTE MEDIO') {
      return 'Ajuste medio';
    }
    if (norm === 'AJUSTE_ALTO' || norm === 'MATCH_ALTO' || norm === 'ALTO' || norm === 'MATCH ALTO' || norm === 'AJUSTE ALTO') {
      return 'Match alto';
    }
    if (norm === 'AJUSTE_BAJO' || norm === 'MATCH_BAJO' || norm === 'BAJO' || norm === 'MATCH BAJO' || norm === 'AJUSTE BAJO') {
      return 'Match bajo';
    }
    if (norm === 'DESCARTADO') {
      return 'Descartado';
    }
    return clasificacion;
  }

  cleanEncontradoPrefix(text: string | null | undefined): string {
    if (!text) return '';
    return text.replace(/^Encontrado:\s*/i, '').trim();
  }

  getCriterioBadgeClass(crit: any): string {
    if (!crit) return 'criterio-badge badge-criterio-medio';
    let pct = crit.porcentajeCalculado;
    if (pct == null && crit.puntaje != null) {
      const match = String(crit.puntaje).match(/\d+/);
      if (match) pct = parseInt(match[0], 10);
    }
    if (pct == null) pct = 100;

    if (pct >= 80) {
      return 'criterio-badge badge-criterio-alto';
    } else if (pct >= 50) {
      return 'criterio-badge badge-criterio-medio';
    } else {
      return 'criterio-badge badge-criterio-bajo';
    }
  }

  isDescartado(clasificacion?: string): boolean {
    if (!clasificacion) return false;
    return clasificacion.toUpperCase().trim().includes('DESCARTAD');
  }

  isMatchAlto(clasificacion?: string): boolean {
    if (!clasificacion) return false;
    const norm = clasificacion.toUpperCase().trim();
    return (norm.includes('ALTO') || norm === 'MATCH ALTO' || norm === 'AJUSTE_ALTO') && !norm.includes('DESCARTAD');
  }

  isMatchParcial(clasificacion?: string): boolean {
    if (!clasificacion) return false;
    const norm = clasificacion.toUpperCase().trim();
    return (norm.includes('MEDIO') || norm.includes('PARCIAL') || norm === 'MATCH PARCIAL' || norm === 'AJUSTE_MEDIO') && !norm.includes('DESCARTAD');
  }

  isMatchBajo(clasificacion?: string, porcentaje?: number): boolean {
    if (this.isDescartado(clasificacion)) return false;
    if (clasificacion) {
      const norm = clasificacion.toUpperCase().trim();
      if (norm.includes('BAJO') || norm === 'MATCH BAJO' || norm === 'AJUSTE_BAJO') return true;
    }
    if (porcentaje != null && porcentaje < 60) return true;
    return !this.isMatchAlto(clasificacion) && !this.isMatchParcial(clasificacion);
  }

  getProgressBarClass(clasificacion?: string, porcentaje: number = 0): string {
    if (this.isDescartado(clasificacion)) {
      return 'fill-blue';
    }
    if (this.isMatchAlto(clasificacion) || (porcentaje >= 80 && !this.isMatchBajo(clasificacion, porcentaje) && !this.isMatchParcial(clasificacion))) {
      return 'fill-green';
    }
    if (this.isMatchParcial(clasificacion) || (porcentaje >= 60 && porcentaje < 80)) {
      return 'fill-orange';
    }
    return 'fill-red';
  }

  getEstadoFormateado(estado?: string): string {
    if (!estado) return 'EN REVISIÓN RRHH';
    const norm = estado.toUpperCase().replace(/[\u0300-\u036f]/g, '').trim();
    if (norm.includes('REVISION') || norm.includes('REVISIÓN') || norm.includes('RRHH')) {
      return 'EN REVISIÓN RRHH';
    }
    if (norm.includes('PENDIENTE')) {
      return 'PENDIENTE DE GENERACIÓN';
    }
    if (norm.includes('APROBAD')) {
      return 'APROBADA';
    }
    if (norm.includes('OBSERVAD')) {
      return 'OBSERVADA';
    }
    if (norm === 'COMPLETADO') {
      return 'EN REVISIÓN RRHH';
    }
    return estado.replace(/Estrategia$/i, '').replace(/([a-zñáéíóú])([A-ZÁÉÍÓÚ])/g, '$1 $2').trim();
  }

  getEstadoPillClass(estado?: string): string {
    if (!estado) return 'status-finalizado';
    const norm = estado.toUpperCase().trim();
    if (norm.includes('APROBADA')) return 'status-aprobado';
    if (norm.includes('OBSERVADA')) return 'status-observado';
    if (norm.includes('PENDIENTE')) return 'status-pendiente';
    return 'status-finalizado';
  }

  getPorcentajeNivelConfianza(): number {
    const val = this.estrategia()?.ejecucionMatching?.nivelConfianzaVal ?? 90;
    return Math.min(100, Math.max(0, Math.round(val)));
  }

  getPorcentajeCompatibilidad(): number {
    const val = this.estrategia()?.ejecucionMatching?.compatibilidadPromedio ?? 80;
    return Math.min(100, Math.max(0, Math.round(val)));
  }

  getImpactoBadgeClass(impacto: string): string {
    const norm = (impacto || '').toUpperCase();
    if (norm === 'ALTO') return 'badge-criterio-nocumplido';
    if (norm === 'MEDIO') return 'badge-criterio-parcial';
    return 'badge-codigo-postulante';
  }

  getEtapaNombreTraducido(etapa: string): string {
    if (!etapa) return 'Etapa de evaluación';
    const norm = etapa.toUpperCase().trim();
    switch (norm) {
      case 'VALIDACION_INICIAL':
        return 'Validación inicial';
      case 'VALIDACION_JEFATURA':
        return 'Validación con jefatura';
      case 'EVALUACION_TECNICA':
        return 'Evaluación técnica';
      case 'ENTREVISTA_COMPETENCIAS':
        return 'Entrevista por competencias';
      case 'DECISION':
        return 'Decisión final';
      default:
        return etapa;
    }
  }

  // Tabs & Modo Ampliado
  activeTab = signal<'INTERNA' | 'EXTERNA' | 'EVALUACION'>('INTERNA');
  modoAmpliado = signal<boolean>(false);

  toggleModoAmpliado(): void {
    this.modoAmpliado.update(val => !val);
  }

  // Modals & Forms
  showObservacionModal = signal<boolean>(false);
  comentarioText = signal<string>('');
  comentarioError = signal<boolean>(false);

  showAprobacionModal = signal<boolean>(false);
  isSubmitting = signal<boolean>(false);

  // Tabla 1: Postulante Interno (Filtros, Paginación)
  searchInterno = signal<string>('');
  pageSizeInterno = signal<number>(5); // 5, 10, 25
  pageInterno = signal<number>(1);

  // Tabla 2: Postulantes Históricos (Filtros, Paginación)
  searchHistorico = signal<string>('');
  pageSizeHistorico = signal<number>(5);
  pageHistorico = signal<number>(1);

  // Tabla 3: Evaluación de Postulantes (Pool Seleccionado Manualmente)
  searchEvaluados = signal<string>('');
  pageSizeEvaluados = signal<number>(5);
  pageEvaluados = signal<number>(1);

  // Datos mock iniciales para el 3er tab
  private getEvaluadosMockData(): PostulanteEvaluadoManualItem[] {
    return [
      {
        id: 101,
        codigo: 'WSP-0401',
        nombre: 'Juan Carlos Perez',
        email: 'jc.perez@nacionalseguros.com.bo',
        origen: 'WhatsApp',
        cargo: 'Analista Contable Junior',
        regional: 'Santa Cruz',
        pretensionSalarial: 7500,
        matchPorcentaje: 94,
        clasificacion: 'Match alto',
        fechaEvaluacion: '17/09/2026 12:37',
        detalleCriterios: {
          cumplidos: [],
          parciales: [],
          noCumplidos: [],
          fortalezas: [
            { texto: 'Experiencia previa comprobable de 3.5 años en contabilidad y auditoría financiera en sector seguros.', evidencia: '' },
            { texto: 'Conocimientos avanzados en conciliaciones bancarias, normativa ASFI y SAP Financials.', evidencia: '' },
            { texto: 'Formación académica concluida en Licenciatura en Contaduría Pública.', evidencia: '' }
          ],
          brechas: [
            { texto: 'Pretensión salarial (Bs. 7,500) ligeramente superior a la banda base sugerida (Bs. 7,000).', evidencia: '' }
          ],
          desglose: [
            { criterio: 'Formación en Contaduría / Auditoría', resultadoEncontrado: 'Título en Contaduría Pública con C.A.B. vigente.', puntaje: '30/30 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Experiencia contable en seguros', resultadoEncontrado: '3.5 años en gestión contable y conciliaciones en entidad financiera.', puntaje: '35/35 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Manejo de SAP y Normativa ASFI', resultadoEncontrado: 'Certificación en SAP FI y dominio de manual contable ASFI.', puntaje: '20/20 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Ajuste salarial y disponibilidad', resultadoEncontrado: 'Disponibilidad inmediata, pretensión dentro del rango aceptable.', puntaje: '9/15 pts (60%)', porcentajeCalculado: 60 }
          ]
        }
      },
      {
        id: 102,
        codigo: 'WSP-0402',
        nombre: 'Alejandra Siles',
        email: 'a.siles@nacionalseguros.com.bo',
        origen: 'WhatsApp',
        cargo: 'Asistente de Presupuestos',
        regional: 'La Paz',
        pretensionSalarial: 8000,
        matchPorcentaje: 88,
        clasificacion: 'Match alto',
        fechaEvaluacion: '17/09/2026 11:37',
        detalleCriterios: {
          cumplidos: [],
          parciales: [],
          noCumplidos: [],
          fortalezas: [
            { texto: 'Sólidos conocimientos en control presupuestario y análisis de desviaciones financieras.', evidencia: '' },
            { texto: 'Manejo fluido de modelos financieros en Excel y herramientas de BI.', evidencia: '' }
          ],
          brechas: [
            { texto: 'Requiere inducción en normativas técnicas específicas de ramos de vida.', evidencia: '' }
          ],
          desglose: [
            { criterio: 'Análisis presupuestario', resultadoEncontrado: '3 años de experiencia en presupuestos operacionales.', puntaje: '35/35 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Herramientas de BI y Analytics', resultadoEncontrado: 'Dominio de PowerBI y SQL intermedio.', puntaje: '25/25 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Conocimiento del sector seguros', resultadoEncontrado: 'Experiencia parcial en aseguradoras.', puntaje: '28/40 pts (70%)', porcentajeCalculado: 70 }
          ]
        }
      },
      {
        id: 103,
        codigo: 'POOL-0205',
        nombre: 'Ricardo Rojas',
        email: 'r.rojas@nacionalseguros.com.bo',
        origen: 'Pool Manual',
        cargo: 'Analista de Riesgos Junior',
        regional: 'Cochabamba',
        pretensionSalarial: 6800,
        matchPorcentaje: 76,
        clasificacion: 'Match parcial',
        fechaEvaluacion: '17/09/2026 10:37',
        detalleCriterios: {
          cumplidos: [],
          parciales: [],
          noCumplidos: [],
          fortalezas: [
            { texto: 'Pretensión salarial acorde a la posición (Bs. 6,800).', evidencia: '' },
            { texto: 'Disponibilidad para radicar en Santa Cruz o Cochabamba.', evidencia: '' }
          ],
          brechas: [
            { texto: 'Experiencia acumulada en seguros de 1.8 años (requerido mínimo 2.5 años).', evidencia: '' },
            { texto: 'Manejo intermedio de herramientas de modelado de riesgo actuarial.', evidencia: '' }
          ],
          desglose: [
            { criterio: 'Experiencia en Análisis de Riesgos', resultadoEncontrado: '1.8 años en evaluación de riesgos crediticios y operativos.', puntaje: '25/40 pts (62%)', porcentajeCalculado: 62 },
            { criterio: 'Pretensión salarial y regional', resultadoEncontrado: 'Totalmente alineado a presupuesto y residencia regional.', puntaje: '30/30 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Habilidades analíticas y cuantitativas', resultadoEncontrado: 'Evaluación psicotécnica y técnica con puntuación media-alta.', puntaje: '21/30 pts (70%)', porcentajeCalculado: 70 }
          ]
        }
      },
      {
        id: 104,
        codigo: 'WSP-0404',
        nombre: 'María Fernanda Quiroga',
        email: 'mf.quiroga@gmail.com',
        origen: 'WhatsApp',
        cargo: 'Auditor Interno SemiSenior',
        regional: 'La Paz',
        pretensionSalarial: 9000,
        matchPorcentaje: 98,
        clasificacion: 'Match alto',
        fechaEvaluacion: '16/09/2026 18:20',
        detalleCriterios: {
          cumplidos: [],
          parciales: [],
          noCumplidos: [],
          fortalezas: [
            { texto: 'Experiencia sólida en auditoría interna en el sector bancario y seguros.', evidencia: '' },
            { texto: 'Conocimientos técnicos completos en auditoría, control interno y gestión de riesgos.', evidencia: '' },
            { texto: 'Excelentes habilidades de comunicación y elaboración de dictámenes técnicos.', evidencia: '' },
            { texto: 'Disponibilidad inmediata para incorporación.', evidencia: '' }
          ],
          brechas: [
            { texto: 'Pretensión salarial en el tope superior del rango autorizado.', evidencia: '' }
          ],
          desglose: [
            { criterio: 'Experiencia en auditoría interna', resultadoEncontrado: '3.5 años de experiencia en auditoría interna en Banco Mercantil Santa Cruz.', puntaje: '38/38 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Conocimientos técnicos en auditoría y control', resultadoEncontrado: 'Conocimientos en auditoría interna, control interno, gestión de riesgos.', puntaje: '25/25 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Habilidades de comunicación', resultadoEncontrado: 'Comunicación efectiva demostrada en informes y hallazgos.', puntaje: '15/15 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Capacidad de análisis', resultadoEncontrado: 'Pensamiento analítico demostrado en identificación de debilidades.', puntaje: '15/15 pts (100%)', porcentajeCalculado: 100 },
            { criterio: 'Trabajo en equipo', resultadoEncontrado: 'Trabajo en equipo demostrado en colaboración interáreas.', puntaje: '15/15 pts (100%)', porcentajeCalculado: 100 }
          ]
        }
      }
    ];
  }

  // Computed: Pool de postulantes evaluados
  postulantesEvaluadosList = computed(() => {
    const list = this.estrategia()?.postulantesEvaluados;
    return list || [];
  });

  postulantesEvaluadosFiltrados = computed(() => {
    let list = this.postulantesEvaluadosList();
    const query = this.searchEvaluados().toLowerCase().trim();
    if (query) {
      list = list.filter(item =>
        item.nombre.toLowerCase().includes(query) ||
        item.codigo.toLowerCase().includes(query) ||
        item.cargo.toLowerCase().includes(query) ||
        item.regional.toLowerCase().includes(query) ||
        (item.email && item.email.toLowerCase().includes(query)) ||
        (item.origen && item.origen.toLowerCase().includes(query))
      );
    }
    return [...list].sort((a, b) => b.matchPorcentaje - a.matchPorcentaje);
  });

  postulantesEvaluadosPaginado = computed(() => {
    const list = this.postulantesEvaluadosFiltrados();
    const page = this.pageEvaluados();
    const size = this.pageSizeEvaluados();
    const start = (page - 1) * size;
    return list.slice(start, start + size);
  });

  totalEvaluadosPages = computed(() => {
    const total = this.postulantesEvaluadosFiltrados().length;
    const size = this.pageSizeEvaluados();
    return Math.ceil(total / size) || 1;
  });

  // Métricas del Pool Evaluado
  totalEvaluadosCount = computed(() => this.postulantesEvaluadosList().length);
  matchAltoEvaluadosCount = computed(() => this.postulantesEvaluadosList().filter(item => this.isMatchAlto(item.clasificacion)).length);
  matchParcialEvaluadosCount = computed(() => this.postulantesEvaluadosList().filter(item => this.isMatchParcial(item.clasificacion)).length);
  promedioMatchEvaluados = computed(() => {
    const list = this.postulantesEvaluadosList();
    if (!list.length) return 0;
    const total = list.reduce((acc, curr) => acc + (curr.matchPorcentaje || 0), 0);
    return Math.round(total / list.length);
  });
  topPostulanteEvaluado = computed(() => {
    const list = this.postulantesEvaluadosList();
    if (!list.length) return 'N/A';
    const sorted = [...list].sort((a, b) => (b.matchPorcentaje || 0) - (a.matchPorcentaje || 0));
    return sorted[0]?.codigo || 'N/A';
  });

  // Computed: Lista filtrada de talento interno
  talentoInternoFiltrado = computed(() => {
    let list = this.estrategia()?.talentoInterno || [];
    const query = this.searchInterno().toLowerCase().trim();
    if (query) {
      list = list.filter(item =>
        item.nombre.toLowerCase().includes(query) ||
        item.codigo.toLowerCase().includes(query) ||
        item.cargo.toLowerCase().includes(query) ||
        item.regional.toLowerCase().includes(query)
      );
    }
    return [...list].sort((a, b) => b.matchPorcentaje - a.matchPorcentaje);
  });

  // Computed: Paginación de talento interno
  talentoInternoPaginado = computed(() => {
    const list = this.talentoInternoFiltrado();
    const page = this.pageInterno();
    const size = this.pageSizeInterno();
    const start = (page - 1) * size;
    return list.slice(start, start + size);
  });

  totalInternoPages = computed(() => {
    const total = this.talentoInternoFiltrado().length;
    const size = this.pageSizeInterno();
    return Math.ceil(total / size) || 1;
  });

  // Computed: Lista filtrada de postulantes históricos
  postulantesHistoricosFiltrados = computed(() => {
    let list = this.estrategia()?.postulantesHistoricos || [];
    const query = this.searchHistorico().toLowerCase().trim();
    if (query) {
      list = list.filter(item =>
        item.nombre.toLowerCase().includes(query) ||
        item.codigo.toLowerCase().includes(query) ||
        item.ultimoCargo.toLowerCase().includes(query) ||
        item.regional.toLowerCase().includes(query)
      );
    }
    return [...list].sort((a, b) => b.matchPorcentaje - a.matchPorcentaje);
  });

  // Computed: Paginación de postulantes históricos
  postulantesHistoricosPaginado = computed(() => {
    const list = this.postulantesHistoricosFiltrados();
    const page = this.pageHistorico();
    const size = this.pageSizeHistorico();
    const start = (page - 1) * size;
    return list.slice(start, start + size);
  });

  totalHistoricoPages = computed(() => {
    const total = this.postulantesHistoricosFiltrados().length;
    const size = this.pageSizeHistorico();
    return Math.ceil(total / size) || 1;
  });

  cambiarPaginaInterno(delta: number): void {
    const next = this.pageInterno() + delta;
    if (next >= 1 && next <= this.totalInternoPages()) {
      this.pageInterno.set(next);
    }
  }

  cambiarPaginaHistorico(delta: number): void {
    const next = this.pageHistorico() + delta;
    if (next >= 1 && next <= this.totalHistoricoPages()) {
      this.pageHistorico.set(next);
    }
  }

  // Filas desplegables (Expandable Rows estilo React Table)
  expandedInternoIds = signal<Set<number>>(new Set());
  expandedHistoricoIds = signal<Set<number>>(new Set());
  expandedEvaluadoIds = signal<Set<number>>(new Set());

  toggleExpandInterno(id: number): void {
    const current = new Set(this.expandedInternoIds());
    if (current.has(id)) {
      current.delete(id);
    } else {
      current.add(id);
    }
    this.expandedInternoIds.set(current);
  }

  isInternoExpanded(id: number): boolean {
    return this.expandedInternoIds().has(id);
  }

  toggleExpandHistorico(id: number): void {
    const current = new Set(this.expandedHistoricoIds());
    if (current.has(id)) {
      current.delete(id);
    } else {
      current.add(id);
    }
    this.expandedHistoricoIds.set(current);
  }

  isHistoricoExpanded(id: number): boolean {
    return this.expandedHistoricoIds().has(id);
  }

  toggleExpandEvaluado(id: number): void {
    const current = new Set(this.expandedEvaluadoIds());
    if (current.has(id)) {
      current.delete(id);
    } else {
      current.add(id);
    }
    this.expandedEvaluadoIds.set(current);
  }

  isEvaluadoExpanded(id: number): boolean {
    return this.expandedEvaluadoIds().has(id);
  }

  cambiarPaginaEvaluados(delta: number): void {
    const next = this.pageEvaluados() + delta;
    if (next >= 1 && next <= this.totalEvaluadosPages()) {
      this.pageEvaluados.set(next);
    }
  }

  // Postulante Modal signals
  showPostulanteModal = signal<boolean>(false);
  selectedPostulante = signal<any | null>(null);

  abrirModalPostulante(candidato: any): void {
    this.selectedPostulante.set(candidato);
    this.showPostulanteModal.set(true);
  }

  cerrarModalPostulante(): void {
    this.showPostulanteModal.set(false);
    this.selectedPostulante.set(null);
  }

  getIniciales(nombre?: string): string {
    if (!nombre) return 'P';
    const parts = nombre.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  getAvatarColorClass(index: number): string {
    const classes = [
      'bg-blue-100 text-blue-700 border-blue-200',
      'bg-emerald-100 text-emerald-700 border-emerald-200',
      'bg-purple-100 text-purple-700 border-purple-200',
      'bg-amber-100 text-amber-700 border-amber-200',
      'bg-rose-100 text-rose-700 border-rose-200'
    ];
    return classes[index % classes.length];
  }

  // Toast feedback
  toastMessage = signal<{ type: 'success' | 'info' | 'error'; text: string } | null>(null);

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('estrategiaId');
    const id = idParam ? Number(idParam) : 2;
    this.cargarDetalle(id);
    this.iniciarPollingRealTime(id);
  }

  ngOnDestroy(): void {
    if (this.pollingSub) {
      this.pollingSub.unsubscribe();
    }
  }

  private iniciarPollingRealTime(id: number): void {
    this.pollingSub = timer(4000, 4000).subscribe(() => {
      if (!this.isLoading() && this.estrategia()) {
        this.estrategiasService.getMatchingResultadosByEjecucionId(id).subscribe({
          next: (res) => {
            if (res && res.postulantesEvaluados) {
              this.estrategia.update(prev => prev ? {
                ...prev,
                postulantesEvaluados: res.postulantesEvaluados
              } : null);
            }
          },
          error: () => {}
        });
      }
    });
  }

  toggleParametros(): void {
    this.isParametrosOpen.update(v => !v);
  }

  getPasosProceso(estado?: EstadoEstrategia): Array<{ numero: number; nombre: string; estado: 'completado' | 'actual' | 'pendiente' }> {
    if (!estado) {
      return [
        { numero: 1, nombre: 'Perfil aprobado', estado: 'completado' },
        { numero: 2, nombre: 'Pendiente de generación', estado: 'completado' },
        { numero: 3, nombre: 'En revisión RRHH', estado: 'actual' },
        { numero: 4, nombre: 'Aprobada', estado: 'pendiente' }
      ];
    }

    switch (estado) {
      case 'PendienteGeneracionEstrategia':
        return [
          { numero: 1, nombre: 'Perfil aprobado', estado: 'completado' },
          { numero: 2, nombre: 'Pendiente de generación', estado: 'actual' },
          { numero: 3, nombre: 'En revisión RRHH', estado: 'pendiente' },
          { numero: 4, nombre: 'Aprobada', estado: 'pendiente' }
        ];
      case 'EnRevisionRRHHEstrategia':
      case 'ObservadaEstrategia':
        return [
          { numero: 1, nombre: 'Perfil aprobado', estado: 'completado' },
          { numero: 2, nombre: 'Pendiente de generación', estado: 'completado' },
          { numero: 3, nombre: 'En revisión RRHH', estado: 'actual' },
          { numero: 4, nombre: 'Aprobada', estado: 'pendiente' }
        ];
      case 'AprobadaEstrategia':
        return [
          { numero: 1, nombre: 'Perfil aprobado', estado: 'completado' },
          { numero: 2, nombre: 'Pendiente de generación', estado: 'completado' },
          { numero: 3, nombre: 'En revisión RRHH', estado: 'completado' },
          { numero: 4, nombre: 'Aprobada', estado: 'completado' }
        ];
      default:
        return [
          { numero: 1, nombre: 'Perfil aprobado', estado: 'completado' },
          { numero: 2, nombre: 'Pendiente de generación', estado: 'completado' },
          { numero: 3, nombre: 'En revisión RRHH', estado: 'actual' },
          { numero: 4, nombre: 'Aprobada', estado: 'pendiente' }
        ];
    }
  }

  formatCargoText(cargo: string | null | undefined, area?: string | null | undefined): string {
    if (!cargo) return '';
    let formatted = cargo.replace(/([a-zñáéíóú])([A-ZÁÉÍÓÚ])/g, '$1 / $2').trim();
    if (area && area !== cargo && !formatted.toLowerCase().includes(area.toLowerCase())) {
      formatted += ` (${area})`;
    }
    return formatted;
  }

  cleanPrincipalBrecha(text?: string | null): string {
    if (!text) return 'Sin brechas críticas';
    let cleaned = text.trim();
    if (/^[A-Z0-9_]+\s*\((.*)\)$/i.test(cleaned)) {
      cleaned = cleaned.replace(/^[A-Z0-9_]+\s*\((.*)\)$/i, '$1').trim();
    } else {
      cleaned = cleaned.replace(/^[A-Z0-9_]+[:\s-]*/i, '').trim();
      cleaned = cleaned.replace(/^\((.*)\)$/, '$1').trim();
    }
    return cleaned || text;
  }

  getOrigenLabel(origen: string): string {
    if (!origen) return '';
    if (origen === 'BD_INTERNA') return 'Personal interno';
    if (origen === 'BD_EXTERNA_HISTORICA' || origen === 'BD_EXTERNA') return 'Postulantes históricos';
    const cleaned = origen.replace(/_/g, ' ').toLowerCase();
    return cleaned.charAt(0).toUpperCase() + cleaned.slice(1);
  }

  getTipoDescarteLabel(tipo: string): string {
    if (!tipo) return '';
    const mapa: Record<string, string> = {
      EXPERIENCIA_MINIMA: 'Experiencia mínima',
      DATOS_DESACTUALIZADOS: 'Datos desactualizados',
      PRETENSION_SALARIAL: 'Pretensión salarial',
      BRECHA_TECNICA: 'Brecha técnica',
      CRITERIO_EXCLUYENTE: 'Criterio excluyente'
    };
    if (mapa[tipo]) return mapa[tipo];
    const cleaned = tipo.replace(/_/g, ' ').toLowerCase();
    return cleaned.charAt(0).toUpperCase() + cleaned.slice(1);
  }

  getSourceBadgeClass(estado: string): string {
    const st = estado.toLowerCase();
    if (st === 'consultado' || st === 'aplicados' || st === 'valida' || st === 'aplicado' || st === 'validada') return 'source-badge-aplicado';
    if (st === 'parcial') return 'source-badge-parcial';
    if (st === 'no disponible' || st === 'no_disponible') return 'source-badge-nodisponible';
    if (st === 'error') return 'source-badge-error';
    return 'source-badge-default';
  }

  getSourceIcon(nombre: string): string {
    const n = nombre.toLowerCase();
    if (n.includes('perfil')) return 'assignment';
    if (n.includes('personal') || n.includes('interno')) return 'badge';
    if (n.includes('histórica') || n.includes('postulantes')) return 'history';
    if (n.includes('banda') || n.includes('salarial')) return 'payments';
    if (n.includes('criterio') || n.includes('búsqueda')) return 'tune';
    return 'folder_open';
  }

  cargarDetalle(id: number): void {
    this.estrategia.set(null);
    this.errorMessage.set(null);
    this.isLoading.set(true);
    this.estrategiasService.getDetalleEstrategiaById(id).subscribe({
      next: (item) => {
        this.estrategia.set(item);
        this.initBriefForm(item);
        if (item?.recomendacion?.estrategiaPrioritaria) {
          const tabMapped = (item.recomendacion.estrategiaPrioritaria || '').toUpperCase() === 'EXTERNA' ? 'EXTERNA' : 'INTERNA';
          this.activeTab.set(tabMapped);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error al cargar detalle:', err);
        this.errorMessage.set('No se pudo obtener la información de la ejecución de matching.');
        this.isLoading.set(false);
      }
    });
  }

  volver(): void {
    this.router.navigate(['/estrategias']);
  }

  isRefreshingEvaluados = signal<boolean>(false);

  refreshEvaluadosSilencioso(): void {
    const currentEst = this.estrategia();
    if (!currentEst) return;
    this.isRefreshingEvaluados.set(true);
    this.estrategiasService.getMatchingResultadosByEjecucionId(currentEst.estrategiaId).subscribe({
      next: (res) => {
        if (res && res.postulantesEvaluados) {
          this.estrategia.update(prev => prev ? {
            ...prev,
            postulantesEvaluados: res.postulantesEvaluados
          } : null);
        }
        this.isRefreshingEvaluados.set(false);
      },
      error: () => {
        this.isRefreshingEvaluados.set(false);
      }
    });
  }

  setTab(tab: 'INTERNA' | 'EXTERNA' | 'EVALUACION'): void {
    this.activeTab.set(tab);
    if (tab === 'EVALUACION') {
      this.refreshEvaluadosSilencioso();
    }
  }

  getEstadoConfig(estado?: EstadoEstrategia) {
    if (!estado) return { label: 'Sin estado', chipClass: 'chip-pendiente', icon: 'help_outline' };
    return ESTADO_ESTRATEGIA_CONFIG[estado] || { label: estado, chipClass: 'chip-pendiente', icon: 'help_outline' };
  }

  // --- Modal Observación ---
  abrirModalObservacion(): void {
    if (this.estrategia()?.estado === 'AprobadaEstrategia') return;
    this.comentarioText.set('');
    this.comentarioError.set(false);
    this.showObservacionModal.set(true);
  }

  cerrarModalObservacion(): void {
    this.showObservacionModal.set(false);
    this.comentarioText.set('');
    this.comentarioError.set(false);
  }

  guardarObservacion(): void {
    const text = this.comentarioText().trim();
    if (!text) {
      this.comentarioError.set(true);
      return;
    }

    const est = this.estrategia();
    if (!est) return;

    this.isSubmitting.set(true);
    this.estrategiasService.agregarObservacionMock(
      est.estrategiaId,
      text,
      'RRHH',
      'Analista de RRHH'
    ).subscribe({
      next: (updated: EstrategiaDetalle) => {
        this.estrategia.set(updated);
        this.isSubmitting.set(false);
        this.cerrarModalObservacion();
        this.mostrarToast('success', 'Observación registrada exitosamente.');
      },
      error: () => {
        this.isSubmitting.set(false);
      }
    });
  }

  // --- Modal Aprobación ---
  abrirModalAprobacion(): void {
    if (this.estrategia()?.estado === 'AprobadaEstrategia') return;
    this.showAprobacionModal.set(true);
  }

  cerrarModalAprobacion(): void {
    this.showAprobacionModal.set(false);
  }

  confirmarAprobacion(): void {
    const est = this.estrategia();
    if (!est) return;

    this.isSubmitting.set(true);
    this.estrategiasService.aprobarEstrategiaMock(est.estrategiaId).subscribe({
      next: (updated: EstrategiaDetalle) => {
        this.estrategia.set(updated);
        this.isSubmitting.set(false);
        this.cerrarModalAprobacion();
        this.mostrarToast('success', `Estrategia ${updated.codigoEstrategia} aprobada exitosamente.`);
      },
      error: () => {
        this.isSubmitting.set(false);
      }
    });
  }

  private mostrarToast(type: 'success' | 'info' | 'error', text: string): void {
    this.toastMessage.set({ type, text });
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 4000);
  }
}
