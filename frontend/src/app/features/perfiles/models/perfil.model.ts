import { EstadoPerfil } from './estado-perfil.enum';

export type OrigenCampoPerfil =
  | 'SOLICITUD_VACANTE'
  | 'AGENTE_IA'
  | 'DATO_HISTORICO'
  | 'AJUSTADO_RRHH';

export interface CampoPerfil {
  id: string;
  etiqueta: string;
  valor: any; // Can be string, string[], number or null
  origen: OrigenCampoPerfil;
  editable: boolean;
  tipo: 'texto' | 'textoLargo' | 'lista' | 'porcentaje' | 'tabla';
  obligatorio?: boolean;
}

export interface SeccionPerfil {
  id: number;
  nombre: string;
  icono: string;
  campos: CampoPerfil[];
}

export { EstadoObservacionPerfil } from './estado-observacion-perfil.enum';
export type { TipoObservacion } from './tipo-observacion.model';
export type { ObservacionPerfil } from './observacion-perfil.model';

export interface EventoTrazabilidadPerfil {
  id: number;
  estadoOrigen: EstadoPerfil | null;
  estadoDestino: EstadoPerfil;
  fechaHora: string;
  actorNombre: string;
  actorRol: string;
  tipoActor: 'SISTEMA' | 'AGENTE_IA' | 'USUARIO';
  iteracion: number;
  comentario: string;
}

export interface PermisosPerfil {
  puedeEditar: boolean;
  puedeGuardar: boolean;
  puedeEnviarAreaSolicitante: boolean;
  puedeEnviarCorreccionesArea: boolean;
  puedeAprobarArea: boolean;
  puedeRegistrarObservaciones: boolean;
  puedeAprobarFinal: boolean;
  puedeVerPdf: boolean;
}

import { PerfilDocumento } from '../../../core/models/perfil.model';
export type { PerfilDocumento };

export interface DetallePerfil {
  id: number;
  codigo: string;
  solicitudId: number;
  solicitudCodigo: string;
  cargo: string;
  areaId: number;
  areaNombre: string;
  estado: EstadoPerfil;
  version: number;
  ultimaActualizacion: string;
  secciones: SeccionPerfil[];
  aprobadoPorArea?: boolean;
  estadoCodigo: string;
  documentos?: PerfilDocumento[];
}

export interface AccionCabeceraPerfil {
  id: string;
  etiqueta: string;
  icono: string;
  claseCss: 'primario' | 'secundario' | 'peligro' | 'advertencia' | 'exito';
  deshabilitado: boolean;
  tooltip?: string;
}
