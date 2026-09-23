export interface Observacion {
  idObservacion: number;
  nombre: string;
  activo: boolean;
  puedeEliminar?: boolean;
  cantidadUsos?: number;
}
