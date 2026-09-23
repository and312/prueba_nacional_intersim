import { Injectable, inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';
import { EstadoGeneracionResumen } from '../models/estado-generacion-resumen.enum';
import { ResumenEjecutivoPerfil, InsumoResumenPerfil } from '../models/resumen-ejecutivo-perfil.model';
import { ApiService } from '../../../core/services/api.service';
import { normalizeProfileList } from '../../../core/utils/profile-list-normalizer.utils';

@Injectable({
  providedIn: 'root'
})
export class DatosResumenEjecutivoMockService {
  private apiService = inject(ApiService);
  private errorFijo = false;

  setErrorFijo(error: boolean): void {
    this.errorFijo = error;
  }

  obtenerResumenPorPerfilId(perfilId: number): Observable<ResumenEjecutivoPerfil> {
    if (this.errorFijo) {
      return throwError(() => new Error('Error de conexión simulado.'));
    }

    return this.apiService.getPerfilById(perfilId).pipe(
      map(perfil => {
        const res = perfil.resumenEjecutivo;
        
        // Determinar si el resumen ejecutivo está pendiente
        // Está pendiente si no viene el objeto resumenEjecutivo o el campo resumen está vacío
        const isPending = !res || !res.resumen || res.resumen.trim() === '';
        
        if (isPending) {
          return {
            perfilId,
            versionPerfil: perfil.version || 1,
            versionResumen: 1,
            estadoGeneracion: EstadoGeneracionResumen.PendienteGeneracion,
            fechaGeneracion: perfil.ultimaActualizacion,
            fechaActualizacion: perfil.ultimaActualizacion,
            insumosUtilizados: this.getRealInsumos(perfil.cargo, perfil.codigoSolicitud)
          };
        }

        let valoracion = null;
        if (res.valoracionPerfil) {
          try {
            valoracion = JSON.parse(res.valoracionPerfil);
          } catch (e) {
            valoracion = { comentarioAgente: res.valoracionPerfil };
          }
        }

        const splitTextToList = (text?: string | string[]): string[] => {
          return normalizeProfileList(text);
        };

        const mappedResumen: ResumenEjecutivoPerfil = {
          perfilId,
          versionPerfil: perfil.version || 1,
          versionResumen: 1,
          estadoGeneracion: EstadoGeneracionResumen.Generado,
          fechaGeneracion: perfil.ultimaActualizacion,
          fechaActualizacion: perfil.ultimaActualizacion,
          resumenRol: res.resumen,
          objetivoCargo: res.objetivoCargo,
          funcionesPrincipales: res.funcionesPrincipales || [],
          requisitosMinimos: res.requisitosMinimos || [],
          formacionAcademica: splitTextToList(res.formacionExperiencia),
          experienciaMinima: perfil.perfilEstructurado?.perfilRequerido?.experienciaMinima || 'No especificada',
          experienciaIndispensable: splitTextToList(perfil.perfilEstructurado?.perfilRequerido?.experienciaIndispensable),
          experienciaDeseable: splitTextToList(perfil.perfilEstructurado?.perfilRequerido?.criteriosDeseables),
          habilidadesTecnicas: res.hardSkills || [],
          habilidadesBlandas: res.softSkills || [],
          modalidad: res.modalidad,
          ubicacion: res.ubicacion,
          salario: res.bandaSalarial,
          criteriosEvaluacion: splitTextToList(res.criteriosEvaluacion),
          caracteristicasClave: splitTextToList(res.caracteristicasClave),
          valoracion: valoracion,
          insumosUtilizados: this.getRealInsumos(perfil.cargo, perfil.codigoSolicitud)
        };

        return mappedResumen;
      })
    );
  }

  private getRealInsumos(cargo: string, codigoSolicitud: string): InsumoResumenPerfil[] {
    return [
      {
        id: 1,
        nombre: `Solicitud de Vacante - ${cargo} (${codigoSolicitud})`,
        tipo: 'SOLICITUD_VACANTE',
        utilizado: true
      }
    ];
  }
}
