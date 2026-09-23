import { ProfileStatus } from '../models/profile-status';

export interface ProfileStatusConfig {
  label: string;
  cssClass: string;
  visualType: 'review' | 'pending' | 'observed' | 'approved' | 'automatic';
  icon: string;
}

export const PROFILE_STATUS_CONFIGS: Record<ProfileStatus, ProfileStatusConfig> = {
  [ProfileStatus.SolicitudAprobada]: {
    label: 'Estado no identificado',
    cssClass: 'badge-automatic',
    visualType: 'automatic',
    icon: 'help_outline'
  },
  [ProfileStatus.PendienteGeneracionPerfil]: {
    label: 'Generando perfil',
    cssClass: 'badge-pending',
    visualType: 'pending',
    icon: 'hourglass_empty'
  },
  [ProfileStatus.EnRevisionRRHHPE]: {
    label: 'En revisión RRHH',
    cssClass: 'badge-review',
    visualType: 'review',
    icon: 'rate_review'
  },
  [ProfileStatus.ResumenEjecutivoGenerado]: {
    label: 'En revisión RRHH',
    cssClass: 'badge-review',
    visualType: 'review',
    icon: 'rate_review'
  },
  [ProfileStatus.EnRevisionAreaSol]: {
    label: 'En revisión solicitante',
    cssClass: 'badge-solicitante',
    visualType: 'review',
    icon: 'forward_to_inbox'
  },
  [ProfileStatus.Observada]: {
    label: 'Observado por solicitante',
    cssClass: 'badge-observed',
    visualType: 'observed',
    icon: 'warning'
  },
  [ProfileStatus.Corregida]: {
    label: 'Observado por solicitante',
    cssClass: 'badge-observed',
    visualType: 'observed',
    icon: 'warning'
  },
  [ProfileStatus.Aprobada]: {
    label: 'Aprobado por el Área Solicitante',
    cssClass: 'badge-approved',
    visualType: 'approved',
    icon: 'check_circle'
  },
  [ProfileStatus.PerfilAprobadoFinal]: {
    label: 'Aprobado final',
    cssClass: 'badge-approved',
    visualType: 'approved',
    icon: 'verified'
  }
};
