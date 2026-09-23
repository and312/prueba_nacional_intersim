import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AreaCargo } from '../models/area-cargo.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AreasCargoService {
  private http = inject(HttpClient);

  listar(soloActivos = false): Observable<AreaCargo[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/areas-cargo`, {
      params: { soloActivos: soloActivos.toString() }
    }).pipe(
      map(list => list.map(item => ({
        idAreaCargo: item.id,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: !item.isDeleted,
        cantidadUsos: 0
      })))
    );
  }

  crear(datos: { nombre: string; activo: boolean; descripcion?: string }): Observable<AreaCargo> {
    const codigo = 'AREA_' + datos.nombre.toUpperCase().replace(/[^A-Z0-9]/g, '_').substring(0, 30);
    return this.http.post<any>(`${environment.apiUrl}/areas-cargo`, {
      nombre: datos.nombre,
      codigo: codigo,
      descripcion: datos.descripcion || datos.nombre,
      estado: datos.activo ? 'Activo' : 'Inactivo'
    }).pipe(
      map(item => ({
        idAreaCargo: item.id,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  actualizar(id: number, datos: { nombre: string }): Observable<AreaCargo> {
    return this.http.get<any>(`${environment.apiUrl}/areas-cargo/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/areas-cargo/${id}`, {
          nombre: datos.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || datos.nombre,
          estado: current.estado
        });
      }),
      map(item => ({
        idAreaCargo: item.id,
        nombre: item.nombre,
        codigo: item.codigo,
        descripcion: item.descripcion,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  cambiarEstado(id: number, activo: boolean): Observable<AreaCargo> {
    return this.http.get<any>(`${environment.apiUrl}/areas-cargo/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/areas-cargo/${id}`, {
          nombre: current.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || current.nombre,
          estado: activo ? 'Activo' : 'Inactivo'
        });
      }),
      map(item => ({
        idAreaCargo: item.id,
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
    return this.http.delete<void>(`${environment.apiUrl}/areas-cargo/${id}`);
  }
}
