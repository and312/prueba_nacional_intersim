import { EstadoGeneracionResumen } from './estado-generacion-resumen.enum';

export interface ValoracionPerfil {
  nivelCriticidad?: 'BAJA' | 'MEDIA' | 'ALTA' | null;
  nivelCoherencia?: number | null;
  riesgoCobertura?: 'BAJO' | 'MEDIO' | 'ALTO' | null;
  comentarioAgente?: string | null;
}

export interface InsumoResumenPerfil {
  id: number;
  nombre: string;
  tipo:
    | 'SOLICITUD_VACANTE'
    | 'PERFIL_ESTRUCTURADO'
    | 'MATRIZ_SKILLS'
    | 'DATO_HISTORICO'
    | 'MANUAL_FUNCIONES'
    | 'PERFIL_SIMILAR'
    | 'OTRO';
  utilizado: boolean;
  version?: string | null;
  fechaReferencia?: string | null;
}

export interface ResumenEjecutivoPerfil {
  perfilId: number;
  versionPerfil: number;
  versionResumen: number;
  estadoGeneracion: EstadoGeneracionResumen;

  fechaGeneracion?: string | null;
  fechaActualizacion?: string | null;

  resumenRol?: string | null;
  objetivoCargo?: string | null;

  funcionesPrincipales?: string[];
  requisitosMinimos?: string[];

  formacionAcademica?: string[];
  experienciaMinima?: string | null;
  experienciaIndispensable?: string[];
  experienciaDeseable?: string[];

  habilidadesTecnicas?: string[];
  habilidadesBlandas?: string[];

  modalidad?: string | null;
  ubicacion?: string | null;
  salario?: string | null;

  criteriosEvaluacion?: string[];
  caracteristicasClave?: string[];

  valoracion?: ValoracionPerfil | null;
  insumosUtilizados?: InsumoResumenPerfil[];
}
