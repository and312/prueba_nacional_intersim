import { ConfiguracionCatalogoParametrizacion } from '../models/configuracion-catalogo-parametrizacion.model';

export const CATALOGOS_PARAMETRIZACION: ConfiguracionCatalogoParametrizacion[] = [
  {
    clave: 'OBSERVACIONES',
    titulo: 'Observaciones',
    descripcion: 'Administra los tipos parametrizables para registrar observaciones dentro de un perfil.',
    icono: 'rate_review'
  },
  {
    clave: 'REGIONALES',
    titulo: 'Regionales',
    descripcion: 'Administra las sedes físicas de cobertura a nivel nacional.',
    icono: 'location_on'
  },
  {
    clave: 'TIPOS_SOLICITUD',
    titulo: 'Tipos de solicitud',
    descripcion: 'Administra los tipos de solicitud para contrataciones.',
    icono: 'assignment_turned_in'
  },
  {
    clave: 'MODALIDADES',
    titulo: 'Modalidades',
    descripcion: 'Administra las modalidades de trabajo permitidas.',
    icono: 'work'
  },
  {
    clave: 'AREAS_CARGO',
    titulo: 'Áreas de Cargo',
    descripcion: 'Administra las áreas organizacionales específicas para la clasificación de cargos.',
    icono: 'domain'
  },
  {
    clave: 'CARGOS',
    titulo: 'Cargos',
    descripcion: 'Administra el catálogo parametrizado de cargos asignados a cada área.',
    icono: 'badge'
  }
];
