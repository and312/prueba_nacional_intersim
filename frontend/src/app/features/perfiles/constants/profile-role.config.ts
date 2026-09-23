export interface ProfileMetricConfig {
  key: string;
  label: string;
  icon: string;
  class?: string;
}

export interface ProfileRoleConfig {
  title: string;
  description: string;
  showAreaColumn: boolean;
  showAreaFilter: boolean;
  metrics: ProfileMetricConfig[];
}

export const PROFILE_ROLE_CONFIGS: Record<'RRHH' | 'AreaSol' | 'Administrador', ProfileRoleConfig> = {
  RRHH: {
    title: 'Gestión de perfiles de vacante',
    description: 'Revisa los perfiles generados por el agente, gestiona observaciones y continúa el flujo de validación con el área solicitante.',
    showAreaColumn: true,
    showAreaFilter: true,
    metrics: [
      { key: 'totalEnProceso', label: 'Total en proceso', icon: 'description', class: 'metric-registered' },
      { key: 'enRevisionRRHH', label: 'En revisión RRHH', icon: 'manage_search', class: 'metric-review' },
      { key: 'observadosPorCorregir', label: 'Observados por corregir', icon: 'rate_review', class: 'metric-observed' },
      { key: 'pendientesAprobacionFinal', label: 'Pendientes de aprobación final', icon: 'task_alt', class: 'metric-approved' }
    ]
  },
  Administrador: {
    title: 'Gestión de perfiles de vacante (Administración)',
    description: 'Panel de administración global para supervisar el estado de todos los perfiles de vacantes en el sistema.',
    showAreaColumn: true,
    showAreaFilter: true,
    metrics: [
      { key: 'totalEnProceso', label: 'Total en proceso', icon: 'description', class: 'metric-registered' },
      { key: 'enRevisionRRHH', label: 'En revisión RRHH', icon: 'manage_search', class: 'metric-review' },
      { key: 'observadosPorCorregir', label: 'Observados por corregir', icon: 'rate_review', class: 'metric-observed' },
      { key: 'pendientesAprobacionFinal', label: 'Pendientes de aprobación final', icon: 'task_alt', class: 'metric-approved' }
    ]
  },
  AreaSol: {
    title: 'Gestión de perfiles de vacante',
    description: 'Revisa y valida los perfiles enviados por RRHH para las vacantes de tu área.',
    showAreaColumn: false,
    showAreaFilter: false,
    metrics: [
      { key: 'pendientesRevisionArea', label: 'Pendientes de revisión', icon: 'hourglass_empty', class: 'metric-pending' },
      { key: 'observadosPorCorregir', label: 'Observados por el área', icon: 'rate_review', class: 'metric-observed' },
      { key: 'aprobadosPorArea', label: 'Aprobados por el área', icon: 'task_alt', class: 'metric-approved' },
      { key: 'perfilesFinalizados', label: 'Perfiles finalizados', icon: 'verified', class: 'metric-registered' }
    ]
  }
};
