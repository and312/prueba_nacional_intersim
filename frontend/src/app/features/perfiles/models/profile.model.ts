import { ProfileStatus } from './profile-status';

export interface ProfileListItem {
  id: number;
  codigo: string;
  solicitudId: number;
  solicitudCodigo: string;
  cargo: string;
  areaId: number;
  areaNombre: string;
  estado: ProfileStatus;
  version: number;
  ultimaActualizacion: string;
  estadoCodigo: string;
  solicitante: string;
  origen: string;
  prioridad: string;
  fechaSolicitud: string;
}

export interface ProfileListSummary {
  totalEnProceso: number;
  enRevisionRRHH: number;
  observadosPorCorregir: number;
  pendientesAprobacionFinal: number;
  pendientesRevisionArea: number;
  aprobadosPorArea: number;
  perfilesFinalizados: number;
}

export interface ProfileListQuery {
  page: number;
  pageSize: number;
  search?: string;
  estado?: ProfileStatus | null;
  areaId?: number | null;
  accionRequerida?: string | null;
  fechaDesde?: string | null;
  fechaHasta?: string | null;
  soloMisPerfiles?: boolean;
}

export interface PaginatedProfileResponse {
  items: ProfileListItem[];
  total: number;
  page: number;
  pageSize: number;
}
