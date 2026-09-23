import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CatalogValue, TipoObservacion } from '../models/catalogos.model';
import { SolicitudListItem, SolicitudDetail, SolicitudCreateRequest, SolicitudUpdateRequest, SolicitudFilters } from '../models/solicitud.model';
import { PerfilListItem, PerfilDetail, ResumenEjecutivo, PerfilObservacion, PerfilTrazabilidad, PerfilFilters } from '../models/perfil.model';

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // 1. Catálogos
  getCatalogos(nombre: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/config/catalogos`, {
      params: new HttpParams().set('catalogoNombre', nombre)
    });
  }

  // 2. Solicitudes
  getSolicitudes(page = 1, pageSize = 10, filters?: SolicitudFilters | string, soloMisSolicitudes?: boolean): Observable<PaginatedResponse<SolicitudListItem>> {
    let params = new HttpParams().set('pageNumber', page.toString()).set('pageSize', pageSize.toString());
    if (soloMisSolicitudes !== undefined) {
      params = params.set('soloMisSolicitudes', soloMisSolicitudes.toString());
    }
    if (filters) {
      if (typeof filters === 'string') {
        if (filters.trim()) {
          params = params.set('search', filters.trim());
        }
      } else {
        if (filters.search?.trim()) {
          params = params.set('search', filters.search.trim());
        }
        if (filters.estado?.trim()) {
          params = params.set('estado', filters.estado.trim());
        }
        if (filters.prioridad?.trim()) {
          params = params.set('prioridad', filters.prioridad.trim());
        }
        if (filters.fecha?.trim()) {
          params = params.set('fecha', filters.fecha.trim());
        }
      }
    }
    return this.http.get<PaginatedResponse<SolicitudListItem>>(`${this.baseUrl}/solicitudes`, { params });
  }

  getSolicitudById(id: number): Observable<SolicitudDetail> {
    return this.http.get<SolicitudDetail>(`${this.baseUrl}/solicitudes/${id}`);
  }

  crearSolicitud(solicitud: any): Observable<SolicitudDetail> {
    return this.http.post<SolicitudDetail>(`${this.baseUrl}/solicitudes`, solicitud);
  }

  actualizarSolicitud(id: number, solicitud: any): Observable<SolicitudDetail> {
    return this.http.put<SolicitudDetail>(`${this.baseUrl}/solicitudes/${id}`, solicitud);
  }

  transitarSolicitud(id: number, nuevoEstadoCodigo: string, comentario?: string, seMostroAdvertencia?: boolean, usuarioConfirmoEnvio?: boolean): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/solicitudes/${id}/transicion`, { 
      nuevoEstadoCodigo, 
      comentario, 
      seMostroAdvertencia, 
      usuarioConfirmoEnvio 
    });
  }

  checkWhatsAppCost(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/solicitudes/${id}/whatsapp-cost-check`);
  }

  enviarSolicitudARRhh(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/solicitudes/${id}/enviar`, {});
  }

  getHistorialSolicitud(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/solicitudes/${id}/historial`);
  }

  // 3. Perfiles
  getPerfiles(page = 1, pageSize = 10, filters?: PerfilFilters, soloMisPerfiles?: boolean): Observable<PaginatedResponse<PerfilListItem>> {
    let params = new HttpParams().set('pageNumber', page.toString()).set('pageSize', pageSize.toString());
    if (soloMisPerfiles !== undefined) {
      params = params.set('soloMisPerfiles', soloMisPerfiles.toString());
    }
    if (filters) {
      if (filters.search?.trim()) {
        params = params.set('search', filters.search.trim());
      }
      if (filters.estado?.trim()) {
        params = params.set('estado', filters.estado.trim());
      }
    }
    return this.http.get<any>(`${this.baseUrl}/perfiles`, { params }).pipe(
      map(res => {
        if (Array.isArray(res)) {
          return {
            items: res,
            totalCount: res.length
          };
        }
        return {
          items: res.items || [],
          totalCount: res.totalCount || 0
        };
      })
    );
  }

  getPerfilById(id: number): Observable<PerfilDetail> {
    return this.http.get<PerfilDetail>(`${this.baseUrl}/perfiles/${id}`);
  }

  guardarResumen(id: number, request: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/perfiles/${id}/resumen`, request);
  }

  registrarObservacion(id: number, tipoObservacionId: number, comentario: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/observaciones`, { tipoObservacionId, comentario });
  }

  enviarObservacionesSolicitante(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/enviar-observaciones`, {});
  }

  atenderObservaciones(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/atender-observaciones`, {});
  }

  enviarPerfilArea(id: number, payload?: any): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/enviar-area`, payload || {});
  }

  corregirPerfil(id: number, payload: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfiles/${id}/corregir`, payload);
  }

  aprobarPerfilRRHH(id: number, payload: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfiles/${id}/aprobar-rrhh`, payload);
  }

  aprobarSolicitante(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/aprobar-solicitante`, {});
  }

  aprobarFinal(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/${id}/aprobar-final`, {});
  }

  getTrazabilidadPerfil(id: number): Observable<PerfilTrazabilidad[]> {
    return this.http.get<PerfilTrazabilidad[]>(`${this.baseUrl}/perfiles/${id}/trazabilidad`);
  }

  actualizarPerfilSeccion(id: number, numeroSeccion: number, payload: { contenido: string, motivo?: string }): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfiles/${id}/secciones/${numeroSeccion}`, payload);
  }

  generarResumen(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/perfiles/${id}/generar-resumen`, {});
  }

  getPerfilAuditoria(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/perfiles/${id}/auditoria`);
  }

  regenerarPdf(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/perfiles/${id}/pdf/regenerar`, {});
  }

  descargarDocumentoPerfil(id: number, tipoDocumento: 'RESUMEN_EJECUTIVO_PDF' | 'PERFIL_ESTRUCTURADO_PDF'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/perfiles/${id}/pdf/descargar`, {
      params: new HttpParams().set('tipoDocumento', tipoDocumento),
      responseType: 'blob'
    });
  }

  descargarPdfEstructurado(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/perfiles/${id}/pdf-estructurado`, {
      responseType: 'blob'
    });
  }

  descargarPdfResumen(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/perfiles/${id}/pdf-resumen`, {
      responseType: 'blob'
    });
  }

  descargarPdfSolicitud(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/solicitudes/${id}/documentos/PERFIL_ESTRUCTURADO_PDF/descarga`, {
      responseType: 'blob'
    });
  }



  getObservacionesPerfil(perfilCargoId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/perfiles/${perfilCargoId}/observaciones`);
  }

  atenderObservacionId(observacionId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/observaciones/${observacionId}/atender`, {});
  }

  reabrirObservacionId(observacionId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/perfiles/observaciones/${observacionId}/reabrir`, {});
  }

  eliminarObservacionId(observacionId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/perfiles/observaciones/${observacionId}`);
  }

  // 3b. Catálogo de Tipos de Observación
  getTiposObservacion(soloActivos?: boolean): Observable<TipoObservacion[]> {
    let params = new HttpParams();
    if (soloActivos !== undefined) {
      params = params.set('soloActivos', soloActivos.toString());
    }
    return this.http.get<TipoObservacion[]>(`${this.baseUrl}/tipos-observacion`, { params });
  }

  crearTipoObservacion(dto: { codigo: string, nombre: string, descripcion?: string }): Observable<TipoObservacion> {
    return this.http.post<TipoObservacion>(`${this.baseUrl}/tipos-observacion`, dto);
  }

  actualizarTipoObservacion(id: number, dto: { nombre: string, descripcion?: string }): Observable<TipoObservacion> {
    return this.http.put<TipoObservacion>(`${this.baseUrl}/tipos-observacion/${id}`, dto);
  }

  activarTipoObservacion(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/tipos-observacion/${id}/activar`, {});
  }

  inactivarTipoObservacion(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/tipos-observacion/${id}/inactivar`, {});
  }

  // 4. Vacantes
  getVacantes(page = 1, pageSize = 10, codigoEstado?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (codigoEstado) {
      params = params.set('codigoEstado', codigoEstado);
    }
    return this.http.get<any>(`${this.baseUrl}/vacantes`, { params });
  }

  getVacanteById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/vacantes/${id}`);
  }

  crearVacante(vacante: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/vacantes`, vacante);
  }

  actualizarVacante(id: number, vacante: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/vacantes/${id}`, vacante);
  }

  publicarVacante(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/vacantes/${id}/publicar`, {});
  }

  cerrarVacante(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/vacantes/${id}/cerrar`, {});
  }

  pausarVacante(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/vacantes/${id}/pausar`, {});
  }

  reanudarVacante(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/vacantes/${id}/reanudar`, {});
  }

  // 5. Postulantes
  getPostulantes(page = 1, pageSize = 10, vacanteId?: number): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (vacanteId) {
      params = params.set('vacanteId', vacanteId.toString());
    }
    return this.http.get<any>(`${this.baseUrl}/postulantes`, { params });
  }

  getPostulanteById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/postulantes/${id}`);
  }

  registrarPostulante(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/postulantes/registro`, formData);
  }

  transitarEstadoPostulante(id: number, vacanteId: number, nuevoEstadoId: number, motivoDescarte?: string, justificacion?: string): Observable<any> {
    const url = `${this.baseUrl}/postulantes/${id}/estado?vacanteId=${vacanteId}`;
    return this.http.put<any>(url, { nuevoEstadoId, motivoDescarteCodigo: motivoDescarte, justificacionText: justificacion });
  }

  // 5.1 Postulantes Internos (CRUD)
  getPostulantesInternos(perfilCargoId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (perfilCargoId) params = params.set('perfilCargoId', perfilCargoId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/postulantes-internos`, { params });
  }

  getPostulanteInternoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/postulantes-internos/${id}`);
  }

  crearPostulanteInterno(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/postulantes-internos`, data);
  }

  actualizarPostulanteInterno(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/postulantes-internos/${id}`, data);
  }

  eliminarPostulanteInterno(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/postulantes-internos/${id}`);
  }

  // 5.2 Postulantes Externos (CRUD)
  getPostulantesExternos(perfilCargoId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (perfilCargoId) params = params.set('perfilCargoId', perfilCargoId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/postulantes-externos`, { params });
  }

  getPostulanteExternoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/postulantes-externos/${id}`);
  }

  crearPostulanteExterno(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/postulantes-externos`, data);
  }

  actualizarPostulanteExterno(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/postulantes-externos/${id}`, data);
  }

  eliminarPostulanteExterno(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/postulantes-externos/${id}`);
  }

  // 6. Matching IA
  getMatching(postulanteId: number, vacanteId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/matchings/${postulanteId}/${vacanteId}`);
  }

  evaluarMatching(postulanteId: number, vacanteId: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/matchings/evaluar`, { postulanteId, vacanteId });
  }

  // 6.1 Matching Ejecuciones (CRUD)
  getMatchingEjecuciones(perfilCargoId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (perfilCargoId) params = params.set('perfilCargoId', perfilCargoId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/matching-ejecuciones`, { params });
  }

  getMatchingEjecucionById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/matching-ejecuciones/${id}`);
  }

  crearMatchingEjecucion(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/matching-ejecuciones`, data);
  }

  actualizarMatchingEjecucion(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/matching-ejecuciones/${id}`, data);
  }

  eliminarMatchingEjecucion(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/matching-ejecuciones/${id}`);
  }

  // 6.2 Matching Resultados (CRUD)
  getMatchingResultados(matchingEjecucionId?: number, postulanteId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (matchingEjecucionId) params = params.set('matchingEjecucionId', matchingEjecucionId.toString());
    if (postulanteId) params = params.set('postulanteId', postulanteId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/matching-resultados`, { params });
  }

  getMatchingResultadoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/matching-resultados/${id}`);
  }

  crearMatchingResultado(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/matching-resultados`, data);
  }

  actualizarMatchingResultado(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/matching-resultados/${id}`, data);
  }

  eliminarMatchingResultado(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/matching-resultados/${id}`);
  }

  // 6.3 Estrategias Internas (CRUD)
  getEstrategiaInternas(matchingEjecucionId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (matchingEjecucionId) params = params.set('matchingEjecucionId', matchingEjecucionId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/estrategia-internas`, { params });
  }

  getEstrategiaInternaById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/estrategia-internas/${id}`);
  }

  crearEstrategiaInterna(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/estrategia-internas`, data);
  }

  actualizarEstrategiaInterna(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/estrategia-internas/${id}`, data);
  }

  eliminarEstrategiaInterna(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/estrategia-internas/${id}`);
  }

  // 6.4 Estrategias Externas (CRUD)
  getEstrategiaExternas(matchingEjecucionId?: number): Observable<any[]> {
    let params = new HttpParams();
    if (matchingEjecucionId) params = params.set('matchingEjecucionId', matchingEjecucionId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/estrategia-externas`, { params });
  }

  getEstrategiaExternaById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/estrategia-externas/${id}`);
  }

  crearEstrategiaExterna(data: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/estrategia-externas`, data);
  }

  actualizarEstrategiaExterna(id: number, data: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/estrategia-externas/${id}`, data);
  }

  eliminarEstrategiaExterna(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/estrategia-externas/${id}`);
  }

  // 7. Módulo 01 - Gestión de Usuarios y RBAC
  getUsuarios(page = 1, pageSize = 10, search?: string, areaId?: number, rolId?: number, estado?: string, cargo?: string, gerencia?: string): Observable<any> {
    let params = new HttpParams().set('pageNumber', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    if (areaId) params = params.set('areaId', areaId.toString());
    if (rolId) params = params.set('rolId', rolId.toString());
    if (estado) params = params.set('estado', estado);
    if (cargo) params = params.set('cargo', cargo);
    if (gerencia) params = params.set('gerencia', gerencia);
    return this.http.get<any>(`${this.baseUrl}/usuarios`, { params });
  }

  getUsuarioById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/usuarios/${id}`);
  }

  crearUsuario(usuario: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/usuarios`, usuario);
  }

  actualizarUsuario(id: number, usuario: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/usuarios/${id}`, usuario);
  }

  cambiarEstadoUsuario(id: number, estado: string): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/usuarios/${id}/estado`, { estado });
  }

  resetPasswordUsuario(id: number, nuevaClave: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/usuarios/${id}/reset-password`, { nuevaClave });
  }

  eliminarUsuario(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/usuarios/${id}`);
  }

  // Areas
  getAreas(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/areas`);
  }

  crearArea(area: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/areas`, area);
  }

  actualizarArea(id: number, area: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/areas/${id}`, area);
  }

  eliminarArea(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/areas/${id}`);
  }

  // Gerencias
  getGerencias(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/gerencias`);
  }

  // Roles
  getRoles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/roles`);
  }

  crearRol(rol: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/roles`, rol);
  }

  actualizarRol(id: number, rol: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/roles/${id}`, rol);
  }

  duplicarRol(id: number, nuevoNombre: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/roles/${id}/duplicate`, { nuevoNombre });
  }

  getPermisosList(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/roles/permisos`);
  }

  // 10. Recuperación de Contraseña
  forgotPassword(correo: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/forgot-password`, { correo });
  }

  validateResetToken(userId: number, token: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/validate-reset-token`, { userId, token });
  }

  resetPassword(userId: number, token: string, nuevaContrasena: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/reset-password`, { userId, token, nuevaContrasena });
  }

  // 11. Perfiles Estructurados (Backoffice CRUD)
  getPerfilesEstructurados(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/perfiles-estructurados`);
  }

  getPerfilEstructuradoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/perfiles-estructurados/${id}`);
  }

  getPerfilEstructuradoBySolicitudId(solicitudId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/perfiles-estructurados/solicitud/${solicitudId}`);
  }

  crearPerfilEstructurado(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/perfiles-estructurados`, payload);
  }

  actualizarPerfilEstructurado(id: number, payload: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/perfiles-estructurados/${id}`, payload);
  }

  getPerfilesHabilitadosEstrategia(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/matching-ejecuciones/perfiles-habilitados`);
  }
}
