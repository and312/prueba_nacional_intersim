import { CatalogValue } from './catalogos.model';

export interface SolicitudListItem {
  solicitudId: number;
  codigo: string;
  cargo: string;
  area: string;
  estadoNombre: string;
  estadoCodigo: string;
  createdDate: string;
  prioridad: string;
  seniority: string;
  solicitanteNombre?: string;
  tipoSolicitud?: {
    id: number;
    codigo: string;
    nombre: string;
  };
  modalidadTrabajo?: {
    id: number;
    codigo: string;
    nombre: string;
  };
  lastModifiedDate?: string;
}

export interface SolicitudDetail {
  solicitudId: number;
  codigo: string;
  cargo: string;
  area: string;
  solicitanteId: number;
  solicitanteNombre?: string;
  solicitanteEmail?: string;
  solicitanteCargo?: string;
  decisorId?: number;
  decisorNombre?: string;
  estadoNombre: string;
  estadoCodigo: string;
  createdDate: string;
  prioridad: string;
  seniority: string;
  funciones: any;
  motivo?: string;
  cantidadVacantes?: number;
  observaciones?: string;
  regional?: CatalogValue;
  tipoSolicitud?: any;
  modalidadTrabajo?: CatalogValue;
  
  // Perfil Requerido
  objetivoCargo?: string;
  formacionAcademica?: string;
  experienciaMinima?: string;
  experienciaIndispensable?: string;
  conocimientosTecnicos?: string;
  herramientasSistemas?: string;
  competenciasClave?: string;
  disponibilidadRequerida?: string;
  criteriosExcluyentes?: string;
  criteriosDeseables?: string;
  
  completitudPorcentaje?: number;
  justificacionRechazo?: string;
  pdfDocumentUrl?: string;

  // Legacy/Temp compatibility properties
  jornada?: string;
  skills?: string;
  skillsList?: string[];
  educacion?: string;
  experiencia?: string;
  jefatura?: string;
  modalidad?: string;
  ubicacion?: string;
  fechaIdeal?: string;
  sede?: string;
  createdBy?: string;
}

export interface SolicitudCreateRequest {
  cargo: string;
  regionalId: number;
  cantidadVacantes: number;
  tipoSolicitudId: number;
  motivo?: string;
  objetivoCargo?: string;
  formacionAcademica?: string;
  experienciaMinima?: string;
  experienciaIndispensable?: string;
  conocimientosTecnicos?: string;
  herramientasSistemas?: string;
  competenciasClave?: string;
  criteriosExcluyentes?: string;
  criteriosDeseables?: string;
  funciones: string;
  modalidadTrabajoId: number;
  disponibilidadRequerida?: string;
  seniority: string;
  prioridad: string;
  observaciones?: string;
  usuarioId?: number;
}

export interface SolicitudUpdateRequest extends SolicitudCreateRequest {}

export interface SolicitudFilters {
  search?: string;
  estado?: string;
  prioridad?: string;
  fecha?: string;
}
