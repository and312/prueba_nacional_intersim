export interface CatalogValue {
  id: number;
  codigo: string;
  nombre: string;
  descripcion?: string;
  estado: string;
  isDeleted: boolean;
}

export interface TipoObservacion {
  tipoObservacionId: number;
  codigo: string;
  nombre: string;
  descripcion?: string;
  estado: string;
}
