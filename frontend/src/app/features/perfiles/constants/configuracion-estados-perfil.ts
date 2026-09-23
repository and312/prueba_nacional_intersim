import { EstadoPerfil } from '../models/estado-perfil.enum';

export interface ConfiguracionEstadoPerfil {
  etiqueta: string;
  claseCss: string;
  tipoVisual: 'revision' | 'pendiente' | 'observado' | 'aprobado' | 'automatico';
  icono: string;
}

export const CONFIGURACION_ESTADOS_PERFIL: Record<EstadoPerfil, ConfiguracionEstadoPerfil> = {
  [EstadoPerfil.SolicitudAprobada]: {
    etiqueta: 'Solicitud aprobada',
    claseCss: 'badge-automatico',
    tipoVisual: 'automatico',
    icono: 'task_alt'
  },
  [EstadoPerfil.PendienteGeneracionPerfil]: {
    etiqueta: 'Generando perfil',
    claseCss: 'badge-pendiente',
    tipoVisual: 'pendiente',
    icono: 'hourglass_empty'
  },
  [EstadoPerfil.EnRevisionRRHHPE]: {
    etiqueta: 'En revisión RRHH',
    claseCss: 'badge-revision',
    tipoVisual: 'revision',
    icono: 'rate_review'
  },
  [EstadoPerfil.ResumenEjecutivoGenerado]: {
    etiqueta: 'En revisión RRHH',
    claseCss: 'badge-revision',
    tipoVisual: 'revision',
    icono: 'rate_review'
  },
  [EstadoPerfil.EnRevisionAreaSol]: {
    etiqueta: 'En revisión solicitante',
    claseCss: 'badge-revision',
    tipoVisual: 'revision',
    icono: 'forward_to_inbox'
  },
  [EstadoPerfil.Observada]: {
    etiqueta: 'Observado por solicitante',
    claseCss: 'badge-observado',
    tipoVisual: 'observado',
    icono: 'warning'
  },
  [EstadoPerfil.Corregida]: {
    etiqueta: 'Observado por solicitante',
    claseCss: 'badge-observado',
    tipoVisual: 'observado',
    icono: 'published_with_changes'
  },
  [EstadoPerfil.Aprobada]: {
    etiqueta: 'Aprobado por el Área Solicitante',
    claseCss: 'badge-aprobado',
    tipoVisual: 'aprobado',
    icono: 'check_circle'
  },
  [EstadoPerfil.PerfilAprobadoFinal]: {
    etiqueta: 'Aprobado final',
    claseCss: 'badge-aprobado',
    tipoVisual: 'aprobado',
    icono: 'verified'
  }
};
