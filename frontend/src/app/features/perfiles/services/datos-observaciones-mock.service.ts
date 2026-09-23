import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay, map } from 'rxjs/operators';
import { ObservacionPerfil } from '../models/observacion-perfil.model';
import { TipoObservacion } from '../models/tipo-observacion.model';
import { EstadoObservacionPerfil } from '../models/estado-observacion-perfil.enum';

@Injectable({
  providedIn: 'root'
})
export class DatosObservacionesMockService {
  private tiposObservacion = new BehaviorSubject<TipoObservacion[]>([
    { idTipoObservacion: 1, nombre: 'Información incompleta', activo: true },
    { idTipoObservacion: 2, nombre: 'Ajustar contenido', activo: true },
    { idTipoObservacion: 3, nombre: 'Revisar criterio', activo: true },
    { idTipoObservacion: 4, nombre: 'Contenido poco claro', activo: true },
    { idTipoObservacion: 5, nombre: 'Inactivo de prueba', activo: false }
  ]);

  private observaciones = new BehaviorSubject<ObservacionPerfil[]>([
    {
      idObservacionPerfil: 1,
      idTipoObservacion: 1,
      idPerfil: 5,
      comentario: 'Falta incluir la herramienta Salesforce que es indispensable para el cargo.',
      estado: EstadoObservacionPerfil.Pendiente,
      idUsuarioSolicitante: 20,
      nombreUsuarioSolicitante: 'Jefe Comercial',
      idUsuarioRRHH: null,
      nombreUsuarioRRHH: null,
      fechaRegistro: '2026-07-11T12:45:00',
      fechaResolucion: null,
      tipoObservacion: {
        idTipoObservacion: 1,
        nombre: 'Información incompleta',
        activo: true
      }
    },
    {
      idObservacionPerfil: 2,
      idTipoObservacion: 3,
      idPerfil: 5,
      comentario: 'El filtro de experiencia previa debe ser de mínimo 2 años en el sector financiero, no en general.',
      estado: EstadoObservacionPerfil.Pendiente,
      idUsuarioSolicitante: 20,
      nombreUsuarioSolicitante: 'Jefe Comercial',
      idUsuarioRRHH: null,
      nombreUsuarioRRHH: null,
      fechaRegistro: '2026-07-11T12:50:00',
      fechaResolucion: null,
      tipoObservacion: {
        idTipoObservacion: 3,
        nombre: 'Revisar criterio',
        activo: true
      }
    }
  ]);

  obtenerTiposObservacion(): Observable<TipoObservacion[]> {
    return this.tiposObservacion.asObservable();
  }

  obtenerObservaciones(): Observable<ObservacionPerfil[]> {
    return this.observaciones.asObservable();
  }

  agregarObservacion(obs: Omit<ObservacionPerfil, 'idObservacionPerfil' | 'fechaRegistro'>): Observable<ObservacionPerfil> {
    const lista = this.observaciones.value;
    const tipos = this.tiposObservacion.value;
    const tipo = tipos.find(t => t.idTipoObservacion === obs.idTipoObservacion);

    const nuevaObs: ObservacionPerfil = {
      ...obs,
      idObservacionPerfil: lista.length > 0 ? Math.max(...lista.map(o => o.idObservacionPerfil)) + 1 : 1,
      fechaRegistro: new Date().toISOString(),
      tipoObservacion: tipo
    };

    this.observaciones.next([...lista, nuevaObs]);
    return of(nuevaObs).pipe(delay(200));
  }

  eliminarObservacion(idObservacionPerfil: number): Observable<boolean> {
    const lista = this.observaciones.value;
    const filtrada = lista.filter(o => o.idObservacionPerfil !== idObservacionPerfil);
    this.observaciones.next(filtrada);
    return of(true).pipe(delay(200));
  }

  resolverObservacion(idObservacionPerfil: number): Observable<ObservacionPerfil> {
    const lista = this.observaciones.value;
    const index = lista.findIndex(o => o.idObservacionPerfil === idObservacionPerfil);
    if (index === -1) {
      throw new Error('Observación no encontrada.');
    }

    const obsActualizada = {
      ...lista[index],
      estado: EstadoObservacionPerfil.Resuelta,
      idUsuarioRRHH: 99,
      nombreUsuarioRRHH: 'Analista de RRHH (Mock)',
      fechaResolucion: new Date().toISOString()
    };

    const nuevaLista = [...lista];
    nuevaLista[index] = obsActualizada;
    this.observaciones.next(nuevaLista);

    return of(obsActualizada).pipe(delay(200));
  }
}
