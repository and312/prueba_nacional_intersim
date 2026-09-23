import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { Observacion } from '../models/observacion.model';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ObservacionesService {
  private http = inject(HttpClient);

  listar(): Observable<Observacion[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/tipos-observacion`).pipe(
      map(list => list.map(item => ({
        idObservacion: item.id,
        nombre: item.nombre,
        activo: item.estado === 'Activo',
        puedeEliminar: false, // El backend no soporta delete para tipos-observacion
        cantidadUsos: 0
      })))
    );
  }

  crear(datos: { nombre: string; activo: boolean }): Observable<Observacion> {
    const codigo = 'OBS_' + datos.nombre.toUpperCase().replace(/[^A-Z0-9]/g, '_').substring(0, 10);
    return this.http.post<any>(`${environment.apiUrl}/tipos-observacion`, {
      nombre: datos.nombre,
      codigo: codigo,
      descripcion: datos.nombre
    }).pipe(
      switchMap(() => this.listar()),
      map(list => list.find(item => item.nombre === datos.nombre)!)
    );
  }

  actualizar(id: number, datos: { nombre: string }): Observable<Observacion> {
    return this.http.put<any>(`${environment.apiUrl}/tipos-observacion/${id}`, {
      nombre: datos.nombre,
      descripcion: datos.nombre
    }).pipe(
      switchMap(() => this.listar()),
      map(list => list.find(item => item.idObservacion === id)!)
    );
  }

  cambiarEstado(id: number, activo: boolean): Observable<Observacion> {
    const endpoint = activo ? 'activar' : 'inactivar';
    return this.http.patch<any>(`${environment.apiUrl}/tipos-observacion/${id}/${endpoint}`, {}).pipe(
      switchMap(() => this.listar()),
      map(list => list.find(item => item.idObservacion === id)!)
    );
  }

  eliminar(id: number): Observable<void> {
    return throwError(() => new Error('La eliminación física no está permitida para las Observaciones en el servidor. Por favor, desactive el elemento usando el interruptor de estado.'));
  }
}
