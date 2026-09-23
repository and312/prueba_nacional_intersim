export interface TipoSolicitud {
  idTipoSolicitud: number;
  nombre: string;
  activo: boolean;
  puedeEliminar?: boolean;
  cantidadUsos?: number;
}
