export type EstadoEstrategia =
  | 'PendienteGeneracionEstrategia'
  | 'EnRevisionRRHHEstrategia'
  | 'ObservadaEstrategia'
  | 'AprobadaEstrategia';

export interface EstrategiaItem {
  estrategiaId: number;
  codigoEstrategia: string;
  perfilId: number;
  codigoPerfil: string;
  cargo: string;
  areaId: number;
  area: string;
  estado: EstadoEstrategia;
  estrategiaRecomendada?: 'INTERNA' | 'EXTERNA' | string;
  ultimaActualizacion: string;
}

export interface EstrategiaSummary {
  total: number;
  pendientes: number;
  observadas: number;
  aprobadas: number;
  enRevision: number;
}

export interface EstrategiaFilterState {
  search: string;
  estado: string;
  area: string;
  cargo: string;
}

export const ESTADO_ESTRATEGIA_CONFIG: Record<
  EstadoEstrategia,
  { label: string; chipClass: string; icon: string }
> = {
  PendienteGeneracionEstrategia: {
    label: 'Pendiente de generación',
    chipClass: 'chip-pendiente',
    icon: 'hourglass_empty'
  },
  EnRevisionRRHHEstrategia: {
    label: 'En revisión RRHH',
    chipClass: 'chip-revision',
    icon: 'rate_review'
  },
  ObservadaEstrategia: {
    label: 'Observada',
    chipClass: 'chip-observada',
    icon: 'warning'
  },
  AprobadaEstrategia: {
    label: 'Aprobada',
    chipClass: 'chip-aprobada',
    icon: 'check_circle'
  }
};

/* DTO Backend de Ejecución de Matching */

export interface MatchingEjecucionFuenteDto {
  fuente?: string;
  nombre?: string;
  codigo?: string;
  estadoPerfil?: string;
  resultado?: string;
  detalle?: string;
  totalRecibidos?: number;
  totalPotenciales?: number;
  estado: string;
}

export interface MatchingEjecucionDto {
  id?: number;
  matchingEjecucionId?: number;
  codigoMatching: string;
  perfilCargoId: number;
  codigoPerfil?: string;
  cargo?: string;
  area?: string;
  perfilEstructuradoId: number;
  versionPerfil: number;
  estado: string;
  totalEvaluados: number;
  totalInternosEvaluados: number;
  totalHistoricosEvaluados: number;
  totalMatchAlto: number;
  totalMatchMedio: number;
  totalMatchBajo: number;
  totalDescartados: number;
  totalPotenciales: number;
  compatibilidadPromedio?: number;
  estrategiaRecomendada?: 'INTERNA' | 'EXTERNA' | string;
  nivelConfianza?: number;
  justificacion?: string;
  fuentesConsultadas?: MatchingEjecucionFuenteDto[] | any;
  resumenDescartes?: ResumenDescarte[] | any;
  parametrosMatching?: ParametrosMatching | Record<string, any>;
  fechaInicio: string;
  fechaFin?: string;
  createdDate?: string;
  mensajeError?: string;
}

/* Interfaces para el Detalle de Estrategia */

export interface RecomendacionMatching {
  estrategiaPrioritaria: 'INTERNA' | 'EXTERNA';
  nivelViabilidad: 'ALTA' | 'MEDIA' | 'BAJA';
  justificacion: string;
  internosViables: number;
  historicosViables: number;
  mejorMatchInterno: {
    porcentaje?: number;
    codigo?: string;
    cargado?: boolean;
  };
  mejorMatchHistorico: {
    porcentaje?: number;
    codigo?: string;
    cargado?: boolean;
  };
  principalBrecha: string;
  accionInmediata: string;
  estrategiaContingencia: string;
  nivelConfianzaPorcentaje?: number;
  nivelConfianzaEtiqueta?: string;
}

export interface AlertaSalarial {
  existe: boolean;
  mensaje: string;
}

export interface FuenteConsultada {
  nombre: string;
  detalle: string;
  resultado?: string;
  estado: string;
  totalRecibidos?: number;
  totalPotenciales?: number;
  fuenteRaw?: string;
}

export interface ResumenPersonalInterno {
  recibidos: number;
  evaluados: number;
  matchAlto?: number;
  matchParcial?: number;
  matchBajo?: number;
  descartados: number;
  viables: number;
  mejorMatch?: number;
  promedioMatching?: number;
  tieneResultados?: boolean;
}

export interface ResumenPostulantesHistoricos {
  recibidos: number;
  evaluados: number;
  matchAlto?: number;
  matchParcial?: number;
  matchBajo?: number;
  descartados: number;
  viables: number;
  mejorMatch?: number;
  promedioMatching?: number;
  requierenActualizarDatos: number;
  tieneResultados?: boolean;
}

export interface ResumenDescarte {
  origen: 'BD_INTERNA' | 'BD_EXTERNA_HISTORICA' | string;
  tipoDescarte: 'EXPERIENCIA_MINIMA' | 'DATOS_DESACTUALIZADOS' | 'PRETENSION_SALARIAL' | 'BRECHA_TECNICA' | string;
  cantidad: number;
  comentario?: string;
}

export interface ParametrosMatching {
  ajusteAltoDesde: number;
  ajusteMedioDesde: number;
  ajusteBajoDesde: number;
}

export interface EjecucionMatching {
  codigoMatching: string;
  estado: string;
  totalEvaluados: number;
  internosEvaluados: number;
  historicosEvaluados: number;
  matchAlto: number;
  matchMedio: number;
  matchBajo: number;
  potenciales: number;
  descartados: number;
  compatibilidadPromedio: number;
  nivelConfianza?: string;
  nivelConfianzaVal?: number;
  fechaInicio: string;
  fechaFin?: string;
  duracionTexto?: string;
}

export interface ObservacionEstrategiaItem {
  id: number;
  usuario: string;
  rol: string;
  fecha: string;
  comentario: string;
}

export interface CriterioEvaluacion {
  criterio: string;
  resultadoEncontrado: string;
  puntaje?: string | number;
  ponderacion?: number;
  puntajeObtenido?: number;
  porcentajeCalculado?: number;
}

export interface ItemEvidencia {
  texto: string;
  evidencia: string;
}

export interface DetalleEvaluacionCandidato {
  cumplidos: CriterioEvaluacion[];
  parciales: CriterioEvaluacion[];
  noCumplidos: CriterioEvaluacion[];
  fortalezas?: ItemEvidencia[];
  brechas?: ItemEvidencia[];
  desglose?: CriterioEvaluacion[];
}

export interface TalentoInternoItem {
  id: number;
  codigo: string;
  nombre: string;
  cargo: string;
  area: string;
  regional: string;
  matchPorcentaje: number;
  clasificacion: 'Match alto' | 'Match parcial' | string;
  brechaPrincipal?: string;
  disponibilidad: string;
  accionSugerida: string;
  detalleCriterios?: DetalleEvaluacionCandidato;
  tipoDescarte?: string | null;
  motivoExclusion?: string | null;
}

export interface PostulanteHistoricoItem {
  id: number;
  codigo: string;
  nombre: string;
  ultimoCargo: string;
  regional: string;
  matchPorcentaje: number;
  clasificacion: 'Match alto' | 'Match parcial' | string;
  vigencia: 'Vigente' | 'Requiere actualización' | string;
  brecha?: string;
  accion: string;
  detalleCriterios?: DetalleEvaluacionCandidato;
  tipoDescarte?: string | null;
  motivoExclusion?: string | null;
}

export interface PostulanteEvaluadoManualItem {
  id: number;
  codigo: string;
  nombre: string;
  email?: string;
  origen: 'WhatsApp' | 'Pool Manual' | 'Plataforma' | string;
  cargo: string;
  area?: string;
  regional: string;
  pretensionSalarial: number;
  matchPorcentaje: number;
  clasificacion: 'Match alto' | 'Match parcial' | 'Match bajo' | string;
  fechaEvaluacion: string;
  detalleCriterios?: DetalleEvaluacionCandidato;
}

export interface EstrategiaDetalle {
  estrategiaId: number;
  codigoEstrategia: string;
  codigoMatching?: string;
  perfilCargoId: number;
  perfilEstructuradoId: number;
  solicitudId: number;
  codigoPerfil: string;
  versionPerfil: number;
  versionEstrategia: number;
  cargo: string;
  area: string;
  regional: string;
  bandaSalarial: {
    minimo: number;
    maximo: number;
  };
  cantidadVacantes: number;
  prioridad: string;
  estado: EstadoEstrategia;
  estadoTecnicoEjecucion?: string;
  ultimaActualizacion?: string;
  mensajeError?: string;
  ejecucionMatching?: EjecucionMatching;
  parametrosMatching?: ParametrosMatching;
  resumenDescartes?: ResumenDescarte[];
  recomendacion: RecomendacionMatching;
  alertaSalarial: AlertaSalarial;
  fuentesConsultadas: FuenteConsultada[];
  resumenPersonalInterno: ResumenPersonalInterno;
  resumenPostulantesHistoricos: ResumenPostulantesHistoricos;
  postulantesEvaluados?: PostulanteEvaluadoManualItem[];
  observaciones: ObservacionEstrategiaItem[];
  talentoInterno?: TalentoInternoItem[];
  postulantesHistoricos?: PostulanteHistoricoItem[];
  estrategiaInterna?: EstrategiaInternaDetalle;
}

export interface PlanAccionItem {
  orden: number;
  plazo: string;
  accion: string;
  responsable: string;
  resultadoEsperado: string;
}

export interface PlanEvaluacionEtapa {
  orden: number;
  etapa: 'VALIDACION_INICIAL' | 'VALIDACION_JEFATURA' | 'EVALUACION_TECNICA' | 'ENTREVISTA_COMPETENCIAS' | 'DECISION' | string;
  objetivo: string;
  responsable: string;
  criterios: string[];
  resultadoEsperado: string;
}

export interface CriterioDificil {
  criterio: string;
  impacto: 'ALTO' | 'MEDIO' | 'BAJO' | string;
  evidencia: string;
}

export interface PublicoObjetivo {
  cargosSimilares?: string[];
  sectoresSugeridos?: string[];
  ubicacion?: string;
  modalidad?: string;
  experienciaMinima?: string;
  formacion?: string;
  conocimientosClave?: string[];
  herramientasClave?: string[];
  competenciasClave?: string[];
}

export interface CanalSugerido {
  prioridad: number;
  canal: 'LINKEDIN' | 'PORTALES_ESPECIALIZADOS' | 'BUSQUEDA_DIRECTA' | string;
  tipoAccion: 'BUSQUEDA_Y_PUBLICACION' | string;
  motivo: string;
  criterioSeleccion: 'PERFIL_ESPECIALIZADO' | 'SEGMENTACION_SECTORIAL' | 'PERFIL_DE_ALTA_DEMANDA' | string;
  seleccionado: boolean;
}

export interface PasoAccionExterna {
  orden: number;
  plazo: string;
  accion: string;
  motivo?: string;
  responsable: string;
  resultadoEsperado: string;
}

export interface BriefPublicacion {
  titulo?: string;
  ubicacion?: string;
  modalidad?: string;
  tipoContrato?: string;
  bandaSalarial?: string;
  introduccion?: string;
  desafioPrincipal?: string;
  funciones?: string[];
  formacion?: string;
  experiencia?: string;
  herramientas?: string[];
  conocimientos?: string[];
  competencias?: string[];
  propuestaValor?: string[];
  llamadoAccion?: string;
  enlacePostulacion?: string;
  contacto?: string;
  estadoEdicion?: 'BORRADOR' | 'PENDIENTE_APROBACION' | 'APROBADO' | string;
}

export interface EstrategiaExternaDetalle {
  estrategiaExternaId: number;
  matchingEjecucionId: number;
  prioridad: 'PRIORITARIA' | 'CONTINGENCIA_PREPARADA' | string;
  justificacion: string;
  criteriosDificiles: CriterioDificil[];
  publicoObjetivo: PublicoObjetivo;
  canalesSugeridos: CanalSugerido[];
  planAccion: PasoAccionExterna[];
  briefEditable: BriefPublicacion;
  conclusion: string;
  estadoId?: number | null;
  fechaCreacion?: string;
  fechaModificacion?: string;
}

export interface EstrategiaInternaDetalle {
  id?: number;
  matchingEjecucionId?: number;
  prioridad?: string;
  justificacion?: string;
  planAccion?: PlanAccionItem[];
  planEvaluacion?: PlanEvaluacionEtapa[];
  mensajeContingencia?: string;
  conclusion?: string;
}

export interface EstrategiaDetalle {
  estrategiaId: number;
  codigoEstrategia: string;
  codigoMatching?: string;
  perfilCargoId: number;
  perfilEstructuradoId: number;
  solicitudId: number;
  codigoPerfil: string;
  versionPerfil: number;
  versionEstrategia: number;
  cargo: string;
  area: string;
  regional: string;
  bandaSalarial: {
    minimo: number;
    maximo: number;
  };
  cantidadVacantes: number;
  prioridad: string;
  estado: EstadoEstrategia;
  estadoTecnicoEjecucion?: string;
  ultimaActualizacion?: string;
  mensajeError?: string;
  ejecucionMatching?: EjecucionMatching;
  parametrosMatching?: ParametrosMatching;
  resumenDescartes?: ResumenDescarte[];
  recomendacion: RecomendacionMatching;
  alertaSalarial: AlertaSalarial;
  fuentesConsultadas: FuenteConsultada[];
  resumenPersonalInterno: ResumenPersonalInterno;
  resumenPostulantesHistoricos: ResumenPostulantesHistoricos;
  observaciones: ObservacionEstrategiaItem[];
  talentoInterno?: TalentoInternoItem[];
  postulantesHistoricos?: PostulanteHistoricoItem[];
  estrategiaInterna?: EstrategiaInternaDetalle;
  estrategiaExterna?: EstrategiaExternaDetalle;
}
