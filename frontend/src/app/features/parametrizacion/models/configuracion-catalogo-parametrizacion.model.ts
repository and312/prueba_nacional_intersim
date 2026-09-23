export type ClaveCatalogoParametrizacion =
  | 'OBSERVACIONES'
  | 'REGIONALES'
  | 'TIPOS_SOLICITUD'
  | 'MODALIDADES'
  | 'AREAS_CARGO'
  | 'CARGOS';

export interface ConfiguracionCatalogoParametrizacion {
  clave: ClaveCatalogoParametrizacion;
  titulo: string;
  descripcion: string;
  icono: string;
}
