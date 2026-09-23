import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { CargoItem } from '../models/cargo.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CargosService {
  private http = inject(HttpClient);

  listar(areaCargoId?: number, soloActivos = false): Observable<CargoItem[]> {
    const params: any = { soloActivos: soloActivos.toString() };
    if (areaCargoId && areaCargoId > 0) {
      params.areaCargoId = areaCargoId.toString();
    }
    return this.http.get<any[]>(`${environment.apiUrl}/cargos`, { params }).pipe(
      map(list => list.map(item => ({
        idCargo: item.id,
        areaCargoId: item.areaCargoId,
        areaCargoNombre: item.areaCargoNombre,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: !item.isDeleted,
        cantidadUsos: 0
      })))
    );
  }

  crear(datos: { areaCargoId: number; nombre: string; activo: boolean; descripcion?: string }): Observable<CargoItem> {
    const codigo = 'CARGO_' + datos.nombre.toUpperCase().replace(/[^A-Z0-9]/g, '_').substring(0, 30);
    return this.http.post<any>(`${environment.apiUrl}/cargos`, {
      areaCargoId: datos.areaCargoId,
      nombre: datos.nombre,
      codigo: codigo,
      descripcion: datos.descripcion || datos.nombre,
      estado: datos.activo ? 'Activo' : 'Inactivo'
    }).pipe(
      map(item => ({
        idCargo: item.id,
        areaCargoId: item.areaCargoId,
        areaCargoNombre: item.areaCargoNombre,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  actualizar(id: number, datos: { areaCargoId: number; nombre: string }): Observable<CargoItem> {
    return this.http.get<any>(`${environment.apiUrl}/cargos/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/cargos/${id}`, {
          areaCargoId: datos.areaCargoId || current.areaCargoId,
          nombre: datos.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || datos.nombre,
          estado: current.estado
        });
      }),
      map(item => ({
        idCargo: item.id,
        areaCargoId: item.areaCargoId,
        areaCargoNombre: item.areaCargoNombre,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  cambiarEstado(id: number, activo: boolean): Observable<CargoItem> {
    return this.http.get<any>(`${environment.apiUrl}/cargos/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/cargos/${id}`, {
          areaCargoId: current.areaCargoId,
          nombre: current.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || current.nombre,
          estado: activo ? 'Activo' : 'Inactivo'
        });
      }),
      map(item => ({
        idCargo: item.id,
        areaCargoId: item.areaCargoId,
        areaCargoNombre: item.areaCargoNombre,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/cargos/${id}`);
  }
}
