import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';
import { map, delay, concatMap, tap, catchError } from 'rxjs/operators';
import { ProfileListItem, ProfileListSummary, ProfileListQuery, PaginatedProfileResponse } from '../models/profile.model';
import { ProfileStatus } from '../models/profile-status';
import { ApiService } from '../../../core/services/api.service';

@Injectable({
  providedIn: 'root'
})
export class ProfilesListService {
  private profilesSubject = new BehaviorSubject<ProfileListItem[]>([]);
  private shouldFail = false;

  private readonly stateMap: Record<string, ProfileStatus> = {
    'PERF-PEN-GEN': ProfileStatus.PendienteGeneracionPerfil,
    'PERF-REV-RRHH': ProfileStatus.EnRevisionRRHHPE,
    'PERF-RES-GEN': ProfileStatus.ResumenEjecutivoGenerado,
    'PERF-REV-AREA': ProfileStatus.EnRevisionAreaSol,
    'PERF-OBS-AREA': ProfileStatus.Observada,
    'PERF-COR-RRHH': ProfileStatus.Corregida,
    'PERF-APR-AREA': ProfileStatus.Aprobada,
    'PERF-APR-FIN': ProfileStatus.PerfilAprobadoFinal,
  };

  private areasMap = new Map<string, number>();
  private areaIdCounter = { val: 1 };
  private areasLoaded = false;

  constructor(private apiService: ApiService) {}

  setShouldFail(fail: boolean): void {
    this.shouldFail = fail;
  }

  private getOrCreateAreaId(areaNombre: string, areasMap: Map<string, number>, counter: { val: number }): number {
    if (!areaNombre) return 0;
    const key = areaNombre.trim().toLowerCase();
    if (!areasMap.has(key)) {
      areasMap.set(key, counter.val++);
    }
    return areasMap.get(key)!;
  }

  private parseFechaValida(val: any): string {
    if (!val) return '';
    const str = String(val);
    if (str.startsWith('0001-01-01') || str.startsWith('1970-01-01')) return '';
    const d = new Date(str);
    return (!isNaN(d.getTime()) && d.getFullYear() > 2000) ? str : '';
  }

  private mapBackendToFrontend(item: any): ProfileListItem {
    const estado = this.stateMap[item.estadoCodigo] || ProfileStatus.PendienteGeneracionPerfil;
    const areaNombre = item.areaSolicitante || 'Sin Área';
    const areaId = this.getOrCreateAreaId(areaNombre, this.areasMap, this.areaIdCounter);

    const fechaSolicitudFinal = this.parseFechaValida(item.fechaSolicitud) ||
      this.parseFechaValida(item.createdDate) ||
      this.parseFechaValida(item.fechaCreacion) ||
      this.parseFechaValida(item.ultimaActualizacion) ||
      new Date().toISOString();

    const ultimaActualizacionFinal = this.parseFechaValida(item.ultimaActualizacion) || fechaSolicitudFinal;

    return {
      id: item.perfilId,
      codigo: item.codigoPerfil,
      solicitudId: item.solicitudId || 0,
      solicitudCodigo: item.codigoSolicitud,
      cargo: item.cargo,
      areaId: areaId,
      areaNombre: areaNombre,
      estado: estado,
      version: 1,
      ultimaActualizacion: ultimaActualizacionFinal,
      estadoCodigo: item.estadoCodigo,
      solicitante: item.solicitante || 'Sin Asignar',
      origen: item.canal || item.canalOrigen || 'Web',
      prioridad: item.prioridad || 'MEDIA',
      fechaSolicitud: fechaSolicitudFinal
    };
  }

  private loadRealAreas(): Observable<any> {
    if (this.areasLoaded) return of(null);
    return this.apiService.getAreas().pipe(
      tap(areas => {
        if (areas) {
          areas.forEach(a => {
            this.areasMap.set(a.nombre.trim().toLowerCase(), a.areaId);
          });
          this.areasLoaded = true;
        }
      }),
      catchError(() => of(null))
    );
  }

  getProfiles(query: ProfileListQuery, userRole: 'RRHH' | 'AreaSol' | 'Administrador', userArea: string): Observable<PaginatedProfileResponse> {
    if (this.shouldFail) {
      return new Observable<PaginatedProfileResponse>(observer => {
        setTimeout(() => {
          observer.error(new Error('Error de conexión con el servidor.'));
        }, 600);
      });
    }

    return this.loadRealAreas().pipe(
      concatMap(() => this.apiService.getPerfiles(1, 100, undefined, query.soloMisPerfiles)),
      map(res => {
        const mappedList = (res.items || []).map((item: any) => this.mapBackendToFrontend(item));
        
        // Ordenar desde el último que ingresa (más reciente) hasta el primero
        mappedList.sort((a, b) => new Date(b.ultimaActualizacion).getTime() - new Date(a.ultimaActualizacion).getTime());

        this.profilesSubject.next(mappedList);

        let filtered = [...mappedList];
        if (userRole === 'AreaSol') {
          filtered = filtered.filter(p => p.areaNombre.toLowerCase() === userArea.toLowerCase());
        }

        if (query.search) {
          const searchLower = query.search.toLowerCase().trim();
          filtered = filtered.filter(p =>
            p.codigo.toLowerCase().includes(searchLower) ||
            p.solicitudCodigo.toLowerCase().includes(searchLower) ||
            p.cargo.toLowerCase().includes(searchLower) ||
            p.areaNombre.toLowerCase().includes(searchLower)
          );
        }

        if (query.estado) {
          if (query.estado === ProfileStatus.EnRevisionAreaSol) {
            filtered = filtered.filter(p => p.estado === ProfileStatus.EnRevisionAreaSol || p.estado === ProfileStatus.ResumenEjecutivoGenerado);
          } else if (query.estado === ProfileStatus.Observada) {
            filtered = filtered.filter(p => p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida);
          } else {
            filtered = filtered.filter(p => p.estado === query.estado);
          }
        }

        if (query.areaId) {
          filtered = filtered.filter(p => p.areaId === Number(query.areaId));
        }

        if (query.accionRequerida) {
          filtered = filtered.filter(p => {
            if (userRole === 'RRHH' || userRole === 'Administrador') {
              if (query.accionRequerida === 'revisar') {
                return p.estado === ProfileStatus.EnRevisionRRHHPE;
              } else if (query.accionRequerida === 'corregir') {
                return p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida;
              } else if (query.accionRequerida === 'aprobar_final') {
                return p.estado === ProfileStatus.Aprobada;
              }
            } else {
              if (query.accionRequerida === 'revisar') {
                return p.estado === ProfileStatus.EnRevisionAreaSol || p.estado === ProfileStatus.ResumenEjecutivoGenerado;
              } else if (query.accionRequerida === 'observada') {
                return p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida;
              }
            }
            return true;
          });
        }

        if (query.fechaDesde) {
          const desde = new Date(query.fechaDesde);
          filtered = filtered.filter(p => new Date(p.ultimaActualizacion) >= desde);
        }
        if (query.fechaHasta) {
          const hasta = new Date(query.fechaHasta);
          hasta.setHours(23, 59, 59, 999);
          filtered = filtered.filter(p => new Date(p.ultimaActualizacion) <= hasta);
        }

        const total = filtered.length;
        const page = query.page || 1;
        const pageSize = query.pageSize || 10;
        const startIndex = (page - 1) * pageSize;
        const items = filtered.slice(startIndex, startIndex + pageSize);

        return {
          items,
          total,
          page,
          pageSize
        };
      })
    );
  }

  getSummary(userRole: 'RRHH' | 'AreaSol' | 'Administrador', userArea: string): Observable<ProfileListSummary> {
    return this.profilesSubject.asObservable().pipe(
      map(profiles => {
        let list = [...profiles];
        if (userRole === 'AreaSol') {
          list = list.filter(p => p.areaNombre.toLowerCase() === userArea.toLowerCase());
        }

        const totalEnProceso = list.filter(p => p.estado !== ProfileStatus.PerfilAprobadoFinal).length;
        const enRevisionRRHH = list.filter(p => p.estado === ProfileStatus.EnRevisionRRHHPE).length;
        const observadosPorCorregir = list.filter(p => p.estado === ProfileStatus.Observada || p.estado === ProfileStatus.Corregida).length;
        const pendientesAprobacionFinal = list.filter(p => p.estado === ProfileStatus.Aprobada).length;
        const pendientesRevisionArea = list.filter(p => p.estado === ProfileStatus.EnRevisionAreaSol || p.estado === ProfileStatus.ResumenEjecutivoGenerado).length;
        const aprobadosPorArea = list.filter(p => p.estado === ProfileStatus.Aprobada).length;
        const perfilesFinalizados = list.filter(p => p.estado === ProfileStatus.PerfilAprobadoFinal).length;

        return {
          totalEnProceso,
          enRevisionRRHH,
          observadosPorCorregir,
          pendientesAprobacionFinal,
          pendientesRevisionArea,
          aprobadosPorArea,
          perfilesFinalizados
        };
      })
    );
  }

  aprobarPerfilFinal(id: number): Observable<boolean> {
    const profiles = this.profilesSubject.value;
    const index = profiles.findIndex(p => p.id === id);
    if (index !== -1 && profiles[index].estado === ProfileStatus.Aprobada) {
      const updated = [...profiles];
      updated[index] = {
        ...updated[index],
        estado: ProfileStatus.PerfilAprobadoFinal,
        ultimaActualizacion: new Date().toISOString()
      };
      this.profilesSubject.next(updated);
      return of(true).pipe(delay(200));
    }
    return of(false).pipe(delay(200));
  }

  getAreasList(): Observable<{ id: number; nombre: string }[]> {
    return this.profilesSubject.asObservable().pipe(
      map(profiles => {
        const uniqueAreas = new Map<number, string>();
        profiles.forEach(p => {
          if (p.areaNombre && p.areaId) {
            uniqueAreas.set(p.areaId, p.areaNombre);
          }
        });
        const list: { id: number; nombre: string }[] = [];
        uniqueAreas.forEach((nombre, id) => {
          list.push({ id, nombre });
        });
        return list.sort((a, b) => a.nombre.localeCompare(b.nombre));
      })
    );
  }
}
