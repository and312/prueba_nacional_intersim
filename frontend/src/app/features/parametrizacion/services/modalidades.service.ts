import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { Modalidad } from '../models/modalidad.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ModalidadesService {
  private http = inject(HttpClient);

  listar(soloActivos = false): Observable<Modalidad[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/modalidades-trabajo`, {
      params: { soloActivos: soloActivos.toString() }
    }).pipe(
      map(list => list.map(item => ({
        idModalidad: item.id,
        nombre: item.nombre,
        activo: item.estado === 'Activo',
        puedeEliminar: !item.isDeleted,
        cantidadUsos: 0
      })))
    );
  }

  crear(datos: { nombre: string; activo: boolean }): Observable<Modalidad> {
    const codigo = 'MOD_' + datos.nombre.toUpperCase().replace(/[^A-Z0-9]/g, '_').substring(0, 10);
    return this.http.post<any>(`${environment.apiUrl}/modalidades-trabajo`, {
      nombre: datos.nombre,
      codigo: codigo,
      descripcion: datos.nombre,
      estado: datos.activo ? 'Activo' : 'Inactivo'
    }).pipe(
      map(item => ({
        idModalidad: item.id,
        nombre: item.nombre,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  actualizar(id: number, datos: { nombre: string }): Observable<Modalidad> {
    return this.http.get<any>(`${environment.apiUrl}/modalidades-trabajo/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/modalidades-trabajo/${id}`, {
          nombre: datos.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || datos.nombre,
          estado: current.estado
        });
      }),
      map(item => ({
        idModalidad: item.id,
        nombre: item.nombre,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  cambiarEstado(id: number, activo: boolean): Observable<Modalidad> {
    return this.http.get<any>(`${environment.apiUrl}/modalidades-trabajo/${id}`).pipe(
      switchMap(current => {
        return this.http.put<any>(`${environment.apiUrl}/modalidades-trabajo/${id}`, {
          nombre: current.nombre,
          codigo: current.codigo,
          descripcion: current.descripcion || current.nombre,
          estado: activo ? 'Activo' : 'Inactivo'
        });
      }),
      map(item => ({
        idModalidad: item.id,
        nombre: item.nombre,
        activo: item.estado === 'Activo',
        puedeEliminar: true,
        cantidadUsos: 0
      }))
    );
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/modalidades-trabajo/${id}`);
  }
}
