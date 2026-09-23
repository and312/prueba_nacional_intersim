import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { TipoObservacion } from '../models/tipo-observacion.model';
import { ObservacionPerfil } from '../models/observacion-perfil.model';
import { ApiService } from '../../../core/services/api.service';
import { EstadoObservacionPerfil } from '../models/estado-observacion-perfil.enum';

@Injectable({
  providedIn: 'root'
})
export class ObservacionesPerfilService {
  private apiService = inject(ApiService);

  /**
   * Obtiene todos los tipos de observación activos del catálogo.
   */
  obtenerTiposObservacionActivos(): Observable<TipoObservacion[]> {
    return this.apiService.getTiposObservacion(true).pipe(
      map(tipos => tipos
        .filter(t => t.estado === 'Activo')
        .map(t => ({
          idTipoObservacion: t.tipoObservacionId || (t as any).id,
          nombre: t.nombre,
          activo: true
        }))
      )
    );
  }

  /**
   * Obtiene la lista completa de observaciones registradas para un perfil en particular.
   */
  obtenerObservacionesPerfil(idPerfil: number): Observable<ObservacionPerfil[]> {
    return this.apiService.getObservacionesPerfil(idPerfil).pipe(
      map(obsList => {
        return obsList.map(o => ({
          idObservacionPerfil: o.id,
          idTipoObservacion: o.tipoObservacionId,
          idPerfil: o.perfilCargoId,
          comentario: o.comentario,
          estado: o.estadoObservacion === 'Atendida' ? EstadoObservacionPerfil.Resuelta : EstadoObservacionPerfil.Pendiente,
          idUsuarioSolicitante: o.usuarioSolicitanteId,
          nombreUsuarioSolicitante: o.usuarioSolicitanteNombre,
          idUsuarioRRHH: o.atendidaPorUsuarioId,
          nombreUsuarioRRHH: o.atendidaPorUsuarioNombre,
          fechaRegistro: o.createdDate,
          fechaResolucion: o.fechaAtencion,
          tipoObservacion: {
            idTipoObservacion: o.tipoObservacionId,
            nombre: o.tipoObservacionNombre || 'Observación',
            activo: true
          }
        }));
      })
    );
  }

  /**
   * Registra una nueva observación para el perfil globalmente.
   */
  agregarObservacionPerfil(
    observacion: Omit<ObservacionPerfil, 'idObservacionPerfil' | 'fechaRegistro'>
  ): Observable<any> {
    return this.apiService.registrarObservacion(
      observacion.idPerfil,
      observacion.idTipoObservacion,
      observacion.comentario
    );
  }

  /**
   * Elimina una observación del perfil por su identificador.
   */
  eliminarObservacionPerfil(idObservacionPerfil: number): Observable<any> {
    return this.apiService.eliminarObservacionId(idObservacionPerfil);
  }

  /**
   * Marca una observación como resuelta.
   */
  resolverObservacionPerfil(idObservacionPerfil: number): Observable<any> {
    return this.apiService.atenderObservacionId(idObservacionPerfil);
  }

  /**
   * Reabre una observación marcada como resuelta.
   */
  reabrirObservacionPerfil(idObservacionPerfil: number): Observable<any> {
    return this.apiService.reabrirObservacionId(idObservacionPerfil);
  }
}
