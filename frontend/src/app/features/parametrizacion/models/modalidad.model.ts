export interface Modalidad {
  idModalidad: number;
  nombre: string;
  activo: boolean;
  puedeEliminar?: boolean;
  cantidadUsos?: number;
}
