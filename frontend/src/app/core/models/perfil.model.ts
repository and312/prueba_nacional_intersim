export interface PerfilListItem {
  perfilId: number;
  codigoPerfil: string;
  solicitudId: number;
  codigoSolicitud: string;
  cargo: string;
  areaSolicitante: string;
  estadoPerfil: string;
  estadoCodigo: string;
  ultimaActualizacion: string;
  accionRequerida: string;
  solicitante?: string;
  solicitanteId?: number;
}

export interface ResumenEjecutivo {
  resumen: string;
  objetivoCargo: string;
  funcionesPrincipales: string[];
  requisitosMinimos: string[];
  formacionExperiencia: string;
  hardSkills: string[];
  softSkills: string[];
  modalidad: string;
  ubicacion: string;
  bandaSalarial: string;
  criteriosEvaluacion: string;
  caracteristicasClave: string;
  valoracionPerfil: string;
}

export interface DatosGenerales {
  fechaSolicitud: string;
  area: string;
  solicitante: string;
  cargoSolicitante: string;
  cargo: string;
  regional: string;
  cantidadVacantes: number;
  tipoSolicitud: string;
  motivo: string;
}

export interface PerfilRequerido {
  objetivoCargo: string;
  formacionAcademica: string;
  experienciaMinima: string;
  experienciaIndispensable: string;
  conocimientosTecnicos: string;
  herramientasSistemas: string;
  competenciasClave: string;
  criteriosExcluyentes: string;
  criteriosDeseables: string;
  funciones: string;
}

export interface CondicionesVacante {
  modalidadTrabajo: string;
  disponibilidadRequerida: string;
  seniority: string;
  prioridad: string;
}

export interface PerfilEstructurado {
  datosGenerales: DatosGenerales;
  perfilRequerido: PerfilRequerido;
  condicionesVacante: CondicionesVacante;
}

export interface PerfilObservacion {
  id: number;
  perfilCargoId: number;
  tipoObservacionId: number;
  tipoObservacionNombre: string;
  comentario: string;
  usuarioSolicitanteId: number;
  usuarioSolicitanteNombre: string;
  numeroIteracion: number;
  estadoObservacion: string;
  createdDate: string;
  atendidaPorUsuarioId?: number;
  atendidaPorUsuarioNombre?: string;
  fechaAtencion?: string;
}

export interface PerfilDocumento {
  documentoId?: number;
  solicitudId?: number;
  tipoDocumento: string;
  fileName?: string;
  nombre?: string;
  storageProvider?: string;
  storagePath?: string;
  publicUrl?: string;
  url?: string;
  generadoPor?: string;
  createdDate?: string;
  fechaCreacion?: string;
}

export interface PerfilTrazabilidad {
  stateHistoryId: number;
  entidad: string;
  entidadId: number;
  estadoAnteriorId?: number;
  estadoAnteriorNombre?: string;
  estadoNuevoId: number;
  estadoNuevoNombre: string;
  comentario?: string;
  fecha: string;
  usuarioId?: number;
  usuarioNombre?: string;
}

export interface PerfilDetail {
  perfilId: number;
  codigoPerfil: string;
  codigoSolicitud: string;
  solicitudId: number;
  cargo: string;
  areaSolicitante: string;
  estadoPerfil: string;
  estadoCodigo: string;
  ultimaActualizacion: string;
  accionRequerida: string;
  solicitante: string;
  solicitanteId: number;
  resumenEjecutivo?: ResumenEjecutivo;
  perfilEstructurado: PerfilEstructurado;
  observaciones: PerfilObservacion[];
  documentos?: PerfilDocumento[];
  trazabilidad?: PerfilTrazabilidad[];
  profesiogramaJson?: any;
  version?: number;
  aprobadoPorArea?: boolean;
  salarioMinimo?: string;
  salarioMaximo?: string;
}

export interface PerfilFilters {
  search?: string;
  estado?: string;
}

export interface PerfilCounters {
  enRevisionCount: number;
  observadasCount: number;
  aprobadasCount: number;
}

export interface PerfilEstructuradoPersistido {
  perfilEstructuradoId: number;
  solicitudId: number;
  objetivoPrincipalCargo: string;
  perfilIdealCandidato: string;
  perfilTipoAltoAjuste: string;
  estadoGeneracion: string;
  datosGeneralesCargo: any;
  perfilRequerido: any;
  herramientasSistemas: any;
  filtrosClaveSeleccion: any;
  conocimientosTecnicosRequeridos: any[];
  funcionesPrincipalesCargo: any[];
  competenciasClave: any[];
  indicadoresExitoCargo: any[];
  matrizPonderacion: any[];
  fuentesUtilizadas: string[];
  alertas: string[];
  createdBy: string;
  createdDate: string;
  modifiedBy?: string;
  modifiedDate?: string;
}

