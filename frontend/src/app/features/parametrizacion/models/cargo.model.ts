export interface CargoItem {
  idCargo: number;
  areaCargoId: number;
  areaCargoNombre?: string;
  nombre: string;
  codigo?: string;
  descripcion?: string;
  activo: boolean;
  puedeEliminar?: boolean;
  cantidadUsos?: number;
}
