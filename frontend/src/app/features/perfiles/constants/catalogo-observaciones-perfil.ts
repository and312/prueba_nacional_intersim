export interface TipoObservacion {
  id: number;
  nombre: string;
}

export const CATALOGO_OBSERVACIONES_PERFIL: TipoObservacion[] = [
  { id: 1, nombre: 'Información incompleta' },
  { id: 2, nombre: 'Ajustar contenido' },
  { id: 3, nombre: 'Revisar criterio' },
  { id: 4, nombre: 'Contenido poco claro' }
];
