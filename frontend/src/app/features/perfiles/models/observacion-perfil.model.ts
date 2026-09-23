import { EstadoObservacionPerfil } from './estado-observacion-perfil.enum';
import { TipoObservacion } from './tipo-observacion.model';

export interface ObservacionPerfil {
  idObservacionPerfil: number;
  idTipoObservacion: number;
  idPerfil: number;
  comentario: string;

  estado: EstadoObservacionPerfil;

  idUsuarioSolicitante: number;
  nombreUsuarioSolicitante?: string | null;

  idUsuarioRRHH?: number | null;
  nombreUsuarioRRHH?: string | null;

  fechaRegistro: string;
  fechaResolucion?: string | null;

  tipoObservacion?: TipoObservacion;
}
