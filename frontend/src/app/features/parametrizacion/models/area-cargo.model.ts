export interface AreaCargo {
  idAreaCargo: number;
  nombre: string;
  codigo?: string;
  descripcion?: string;
  activo: boolean;
  puedeEliminar?: boolean;
  cantidadUsos?: number;
}
