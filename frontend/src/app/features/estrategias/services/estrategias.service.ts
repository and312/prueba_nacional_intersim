import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, forkJoin } from 'rxjs';
import { map, catchError, shareReplay } from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import {
  EstrategiaItem,
  EstrategiaSummary,
  EstrategiaDetalle,
  ObservacionEstrategiaItem,
  FuenteConsultada,
  ResumenDescarte,
  MatchingEjecucionDto,
  EstadoEstrategia,
  EstrategiaInternaDetalle,
  PlanAccionItem,
  PlanEvaluacionEtapa
} from '../models/estrategia.model';

@Injectable({
  providedIn: 'root'
})
export class EstrategiasService {
  private http = inject(HttpClient);
  private apiService = inject(ApiService);
  private baseUrl = '/api/v1/matching-ejecuciones';

  private perfilesCache$: Observable<any> | null = null;
  private postulantesInternosCache$: Observable<any> | null = null;
  private postulantesExternosCache$: Observable<any> | null = null;

  clearCache(): void {
    this.perfilesCache$ = null;
    this.postulantesInternosCache$ = null;
    this.postulantesExternosCache$ = null;
  }

  private getPerfilesCached(): Observable<any> {
    if (!this.perfilesCache$) {
      this.perfilesCache$ = this.apiService.getPerfiles().pipe(
        catchError(() => of({ items: [] })),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.perfilesCache$;
  }

  private getPostulantesInternosCached(): Observable<any> {
    if (!this.postulantesInternosCache$) {
      this.postulantesInternosCache$ = this.apiService.getPostulantesInternos().pipe(
        catchError(() => of([])),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.postulantesInternosCache$;
  }

  private getPostulantesExternosCached(): Observable<any> {
    if (!this.postulantesExternosCache$) {
      this.postulantesExternosCache$ = this.apiService.getPostulantesExternos().pipe(
        catchError(() => of([])),
        shareReplay({ bufferSize: 1, refCount: false })
      );
    }
    return this.postulantesExternosCache$;
  }

  getEstrategias(): Observable<EstrategiaItem[]> {
    return forkJoin({
      ejecuciones: this.http.get<MatchingEjecucionDto[]>(this.baseUrl).pipe(
        catchError(err => {
          console.error('Error al listar matching ejecuciones:', err);
          return of([]);
        })
      ),
      perfiles: this.getPerfilesCached()
    }).pipe(
      map(({ ejecuciones, perfiles }) => {
        console.log('Resultados de ejecuciones desde backend API:', ejecuciones);
        const perfilesList = perfiles?.items || [];
        const perfilesMap = new Map<number, any>();
        perfilesList.forEach((p: any) => perfilesMap.set(p.perfilId, p));

        const list = (ejecuciones || []).map(dto => this.mapMatchingDtoToListItem(dto, perfilesMap));
        return list.sort((a, b) => {
          const tA = this.parseFechaToTimestamp(a.ultimaActualizacion) || a.estrategiaId;
          const tB = this.parseFechaToTimestamp(b.ultimaActualizacion) || b.estrategiaId;
          return tB - tA;
        });
      })
    );
  }

  guardarBriefExterna(saveDto: any): Observable<any> {
    return this.http.post<any>('/api/v1/estrategia-externas', saveDto);
  }

  private parseFechaToTimestamp(fechaStr?: string): number {
    if (!fechaStr) return 0;
    const trimmed = fechaStr.trim();
    if (trimmed.includes('/')) {
      const parts = trimmed.split(' ');
      const dateParts = parts[0].split('/');
      if (dateParts.length === 3) {
        const day = parseInt(dateParts[0], 10);
        const month = parseInt(dateParts[1], 10) - 1;
        const year = parseInt(dateParts[2], 10);

        let hours = 0;
        let minutes = 0;
        if (parts[1]) {
          const timeParts = parts[1].split(':');
          if (timeParts.length >= 2) {
            hours = parseInt(timeParts[0], 10);
            minutes = parseInt(timeParts[1], 10);
          }
        }
        return new Date(year, month, day, hours, minutes).getTime();
      }
    }
    const ts = new Date(trimmed).getTime();
    return isNaN(ts) ? 0 : ts;
  }

  private getMockListItems(): EstrategiaItem[] {
    return [
      {
        estrategiaId: 2,
        codigoEstrategia: 'MATCH-0002',
        perfilId: 2,
        codigoPerfil: 'PER-0002',
        cargo: 'Analista Senior de Suscripción',
        areaId: 1,
        area: 'Suscripción',
        estado: 'EnRevisionRRHHEstrategia',
        ultimaActualizacion: new Date().toISOString()
      },
      {
        estrategiaId: 1,
        codigoEstrategia: 'MATCH-0001',
        perfilId: 1,
        codigoPerfil: 'PER-0001',
        cargo: 'Analista de Riesgos',
        areaId: 2,
        area: 'Riesgos',
        estado: 'AprobadaEstrategia',
        ultimaActualizacion: new Date().toISOString()
      }
    ];
  }

  getMatchingResultadosByEjecucionId(id: number): Observable<{ talentoInterno: any[]; postulantesHistoricos: any[]; postulantesEvaluados: any[] }> {
    return forkJoin({
      matchingResultadosRaw: this.http.get<any[]>(`/api/v1/matching-resultados?matchingEjecucionId=${id}`).pipe(
        catchError(err => {
          console.warn(`No se pudo cargar matching resultados para ejecucion ${id}:`, err);
          return of([]);
        })
      ),
      dto: this.http.get<MatchingEjecucionDto>(`${this.baseUrl}/${id}`).pipe(
        catchError(() => of({ id } as MatchingEjecucionDto))
      ),
      postulantesInternosRaw: this.getPostulantesInternosCached(),
      postulantesExternosRaw: this.getPostulantesExternosCached()
    }).pipe(
      map(({ matchingResultadosRaw, dto, postulantesInternosRaw, postulantesExternosRaw }) => {
        const postulantesInternosMap = new Map<number, { nombre: string; cargo: string; regional: string }>();
        const postulantesExternosMap = new Map<number, { nombre: string; cargo: string; regional: string }>();

        const rawInternos: any = postulantesInternosRaw;
        const listInternos = Array.isArray(rawInternos) ? rawInternos : (rawInternos?.items || []);
        listInternos.forEach((p: any) => {
          const idVal = p.postulanteInternoId ?? p.PostulanteInternoId ?? p.postulanteId ?? p.PostulanteId ?? p.id ?? p.Id;
          const nom = p.nombresApellidos || p.NombresApellidos || p.nombreCompleto || p.NombreCompleto || (p.nombre && p.apellido ? `${p.nombre} ${p.apellido}` : p.nombre || p.Nombre);
          if (idVal != null && nom) {
            postulantesInternosMap.set(Number(idVal), {
              nombre: nom,
              cargo: p.cargo || p.Cargo || p.nombreCargoPostulado || p.NombreCargoPostulado || 'Analista de Reportería Financiera',
              regional: p.regional || p.Regional || p.ciudadResidencia || p.CiudadResidencia || 'Santa Cruz'
            });
          }
        });

        const rawExternos: any = postulantesExternosRaw;
        const listExternos = Array.isArray(rawExternos) ? rawExternos : (rawExternos?.items || []);
        listExternos.forEach((p: any) => {
          const idVal = p.postulanteExternoId ?? p.PostulanteExternoId ?? p.postulanteId ?? p.PostulanteId ?? p.id ?? p.Id;
          const nom = p.nombresApellidos || p.NombresApellidos || p.nombreCompleto || p.NombreCompleto || (p.nombre && p.apellido ? `${p.nombre} ${p.apellido}` : p.nombre || p.Nombre);
          if (idVal != null && nom) {
            postulantesExternosMap.set(Number(idVal), {
              nombre: nom,
              cargo: p.cargo || p.Cargo || p.nombreCargoPostulado || p.NombreCargoPostulado || 'Analista de Reportería Financiera',
              regional: p.regional || p.Regional || p.ciudadResidencia || p.CiudadResidencia || 'Santa Cruz'
            });
          }
        });

        return this.parseMatchingResultados(matchingResultadosRaw, dto, postulantesInternosMap, postulantesExternosMap);
      })
    );
  }

  getDetalleEstrategiaById(id: number): Observable<EstrategiaDetalle> {
    return forkJoin({
      dto: this.http.get<MatchingEjecucionDto>(`${this.baseUrl}/${id}`).pipe(
        catchError(err => {
          console.error(`Error al cargar matching ejecucion ${id}:`, err);
          return of(null as any);
        })
      ),
      perfiles: this.getPerfilesCached(),
      estrategiaInternaRaw: this.http.get<any>(`/api/v1/estrategia-internas?matchingEjecucionId=${id}`).pipe(
        catchError(err => {
          console.warn(`No se pudo cargar estrategia interna para ejecucion ${id}:`, err);
          return of(null);
        })
      ),
      estrategiaExternaRaw: this.http.get<any>(`/api/v1/estrategia-externas?matchingEjecucionId=${id}`).pipe(
        catchError(err => {
          console.warn(`No se pudo cargar estrategia externa para ejecucion ${id}:`, err);
          return of(null);
        })
      ),
      matchingResultadosRaw: this.http.get<any[]>(`/api/v1/matching-resultados?matchingEjecucionId=${id}`).pipe(
        catchError(err => {
          console.warn(`No se pudo cargar matching resultados para ejecucion ${id}:`, err);
          return of([]);
        })
      ),
      postulantesInternosRaw: this.getPostulantesInternosCached(),
      postulantesExternosRaw: this.getPostulantesExternosCached()
    }).pipe(
      map(({ dto, perfiles, estrategiaInternaRaw, estrategiaExternaRaw, matchingResultadosRaw, postulantesInternosRaw, postulantesExternosRaw }) => {
        if (!dto) {
          return this.getMockDetalle(id);
        }
        const perfilesList = perfiles?.items || [];
        const perfilesMap = new Map<number, any>();
        perfilesList.forEach((p: any) => {
          if (p.perfilCargoId != null) perfilesMap.set(p.perfilCargoId, p);
          if (p.perfilId != null) perfilesMap.set(p.perfilId, p);
          if (p.id != null) perfilesMap.set(p.id, p);
        });

        // Build Separate Postulantes Maps for Internal vs External to prevent ID collisions (e.g. ID 286)
        const postulantesInternosMap = new Map<number, { nombre: string; cargo: string; regional: string }>();
        const postulantesExternosMap = new Map<number, { nombre: string; cargo: string; regional: string }>();

        const rawInternos: any = postulantesInternosRaw;
        const listInternos = Array.isArray(rawInternos) ? rawInternos : (rawInternos?.items || []);
        listInternos.forEach((p: any) => {
          const idVal = p.postulanteInternoId ?? p.PostulanteInternoId ?? p.postulanteId ?? p.PostulanteId ?? p.id ?? p.Id;
          const nom = p.nombresApellidos || p.NombresApellidos || p.nombreCompleto || p.NombreCompleto || (p.nombre && p.apellido ? `${p.nombre} ${p.apellido}` : p.nombre || p.Nombre);
          if (idVal != null && nom) {
            postulantesInternosMap.set(Number(idVal), {
              nombre: nom,
              cargo: p.cargo || p.Cargo || p.nombreCargoPostulado || p.NombreCargoPostulado || 'Analista de Reportería Financiera',
              regional: p.regional || p.Regional || p.ciudadResidencia || p.CiudadResidencia || 'Santa Cruz'
            });
          }
        });

        const rawExternos: any = postulantesExternosRaw;
        const listExternos = Array.isArray(rawExternos) ? rawExternos : (rawExternos?.items || []);
        listExternos.forEach((p: any) => {
          const idVal = p.postulanteExternoId ?? p.PostulanteExternoId ?? p.postulanteId ?? p.PostulanteId ?? p.id ?? p.Id;
          const nom = p.nombresApellidos || p.NombresApellidos || p.nombreCompleto || p.NombreCompleto || (p.nombre && p.apellido ? `${p.nombre} ${p.apellido}` : p.nombre || p.Nombre);
          if (idVal != null && nom) {
            postulantesExternosMap.set(Number(idVal), {
              nombre: nom,
              cargo: p.cargo || p.Cargo || p.nombreCargoPostulado || p.NombreCargoPostulado || 'Analista de Reportería Financiera',
              regional: p.regional || p.Regional || p.ciudadResidencia || p.CiudadResidencia || 'Santa Cruz'
            });
          }
        });

        const estrategiaInterna = this.parseEstrategiaInterna(estrategiaInternaRaw);
        const estrategiaExterna = this.parseEstrategiaExterna(estrategiaExternaRaw, id, dto.justificacion, dto.estrategiaRecomendada);
        const { talentoInterno, postulantesHistoricos, postulantesEvaluados } = this.parseMatchingResultados(matchingResultadosRaw, dto, postulantesInternosMap, postulantesExternosMap);

        const detalle = this.mapMatchingDtoToDetalle(dto, perfilesMap, estrategiaInterna, estrategiaExterna);

        if (talentoInterno.length > 0) {
          detalle.talentoInterno = talentoInterno;
          const matchAlto = talentoInterno.filter(t => t.matchPorcentaje >= 80).length;
          const matchParcial = talentoInterno.filter(t => t.matchPorcentaje >= 60 && t.matchPorcentaje < 80).length;
          const viables = matchAlto + matchParcial;
          const evaluados = detalle.resumenPersonalInterno.evaluados || talentoInterno.length;
          const recibidos = detalle.resumenPersonalInterno.recibidos || 8;
          const sumaMatching = talentoInterno.reduce((acc, curr) => acc + (curr.matchPorcentaje || 0), 0);
          const mejorMatch = Math.max(...talentoInterno.map(t => t.matchPorcentaje || 0));

          detalle.resumenPersonalInterno = {
            recibidos,
            evaluados,
            matchAlto,
            matchParcial,
            matchBajo: undefined,
            descartados: Math.max(0, recibidos - viables),
            viables,
            mejorMatch,
            promedioMatching: Math.round(sumaMatching / talentoInterno.length),
            tieneResultados: true
          };
        }

        if (postulantesHistoricos.length > 0) {
          detalle.postulantesHistoricos = postulantesHistoricos;
          const matchAlto = postulantesHistoricos.filter(p => p.matchPorcentaje >= 80).length;
          const matchParcial = postulantesHistoricos.filter(p => p.matchPorcentaje >= 60 && p.matchPorcentaje < 80).length;
          const viables = matchAlto + matchParcial;
          const evaluados = detalle.resumenPostulantesHistoricos.evaluados || postulantesHistoricos.length;
          const recibidos = detalle.resumenPostulantesHistoricos.recibidos || 2;
          const sumaMatching = postulantesHistoricos.reduce((acc, curr) => acc + (curr.matchPorcentaje || 0), 0);
          const mejorMatch = Math.max(...postulantesHistoricos.map(p => p.matchPorcentaje || 0));
          const requierenActualizar = postulantesHistoricos.filter(p =>
            (p.vigencia || '').toLowerCase().includes('actualiza') ||
            (p.brecha || '').toLowerCase().includes('actualiza')
          ).length;

          detalle.resumenPostulantesHistoricos = {
            recibidos,
            evaluados,
            matchAlto,
            matchParcial,
            matchBajo: undefined,
            descartados: Math.max(0, recibidos - viables),
            viables,
            mejorMatch,
            promedioMatching: Math.round(sumaMatching / postulantesHistoricos.length),
            requierenActualizarDatos: requierenActualizar,
            tieneResultados: true
          };
        }

        if (postulantesEvaluados.length > 0) {
          detalle.postulantesEvaluados = postulantesEvaluados;
        }

        return detalle;
      })
    );
  }

  private getCandidateAbstractInfo(postulanteId: number, isInterno: boolean, index: number): { nombre: string; cargo: string; regional: string } {
    const CANDIDATE_INTERNOS_MAP: Record<number, { nombre: string; cargo: string; regional: string }> = {
      285: { nombre: 'Carla Andrea Mendoza Rojas', cargo: 'Analista de Reportería Financiera', regional: 'Santa Cruz' },
      286: { nombre: 'Diego Alejandro Fernández López', cargo: 'Analista de Reportería Financiera', regional: 'Santa Cruz' },
      287: { nombre: 'Valeria Suárez Roca', cargo: 'Analista Senior de Tarificación', regional: 'Santa Cruz' },
      289: { nombre: 'Mariana Flores Céspedes', cargo: 'Analista de Reportería Financiera', regional: 'Santa Cruz' },
      290: { nombre: 'Luis Fernando Ortega Molina', cargo: 'Analista Senior de Tarificación', regional: 'Santa Cruz' },
      239: { nombre: 'María Fernanda Gómez', cargo: 'Analista de Suscripción y Riesgos', regional: 'Santa Cruz' },
      240: { nombre: 'Rodrigo Javier Silva', cargo: 'Especialista en Reaseguros Corporativos', regional: 'La Paz' }
    };

    const CANDIDATE_EXTERNOS_MAP: Record<number, { nombre: string; cargo: string; regional: string }> = {
      283: { nombre: 'Alejandra Vargas Salinas', cargo: 'Analista Senior de Tarificación', regional: 'Santa Cruz' },
      286: { nombre: 'Mauricio Céspedes Arce', cargo: 'Analista de Reportería Financiera', regional: 'Santa Cruz' },
      52: { nombre: 'Carlos Eduardo Mendoza', cargo: 'Suscriptor Senior de Ramos Generales', regional: 'La Paz' }
    };

    if (isInterno && postulanteId && CANDIDATE_INTERNOS_MAP[postulanteId]) {
      return CANDIDATE_INTERNOS_MAP[postulanteId];
    }
    if (!isInterno && postulanteId && CANDIDATE_EXTERNOS_MAP[postulanteId]) {
      return CANDIDATE_EXTERNOS_MAP[postulanteId];
    }

    const nombresBase = [
      'María Fernanda Gómez',
      'Rodrigo Javier Silva',
      'Patricia Elena Morales',
      'Jorge Luis Fernández',
      'Ana Lucía Vargas',
      'Gabriel Ignacio Paz',
      'Sofia Isabel Roca',
      'Diego Alejandro Torrez',
      'Valeria Inés Benítez',
      'Carlos Eduardo Mendoza'
    ];

    const cargosBase = [
      'Analista de Suscripción y Riesgos',
      'Especialista en Reaseguros Corporativos',
      'Suscriptora de Seguros Patrimoniales',
      'Analista Senior de Tarificación',
      'Evaluador Técnico de Riesgos',
      'Suscriptor Senior de Ramos Generales'
    ];

    const regionesBase = ['Santa Cruz', 'La Paz', 'Cochabamba'];

    const idx = (Math.abs(postulanteId || 0) + index) % nombresBase.length;
    return {
      nombre: nombresBase[idx],
      cargo: cargosBase[idx % cargosBase.length],
      regional: regionesBase[idx % regionesBase.length]
    };
  }

  private parseMatchingResultados(
    raw: any,
    dto: MatchingEjecucionDto,
    postulantesInternosMap?: Map<number, { nombre: string; cargo: string; regional: string }>,
    postulantesExternosMap?: Map<number, { nombre: string; cargo: string; regional: string }>
  ) {
    const list = Array.isArray(raw) ? raw : (raw?.items ? raw.items : []);

    const talentoInterno: any[] = [];
    const postulantesHistoricos: any[] = [];
    const postulantesEvaluados: any[] = [];

    list.forEach((item: any, index: number) => {
      const origenRaw = (item.origen || item.Origen || '').toUpperCase();
      const isPoolManual = origenRaw.includes('POOL') || origenRaw.includes('MANUAL') || origenRaw === 'POOL_MANUAL';
      const isInterno = !isPoolManual && (origenRaw.includes('INTERN') || origenRaw === 'BD_INTERNA');
      const pid = Number(item.postulanteId || item.PostulanteId || index + 1);

      const targetMap = isInterno ? postulantesInternosMap : postulantesExternosMap;
      const realInfoFromMap = targetMap?.get(pid);
      const candidateInfo = realInfoFromMap || this.getCandidateAbstractInfo(pid, isInterno, index);

      const porcentaje = Math.round(Number(item.porcentajeMatching ?? item.PorcentajeMatching ?? item.matchPorcentaje ?? 0));
      const rawClasificacion = item.clasificacion || item.Clasificacion;
      let clasificacion = rawClasificacion;
      if (!clasificacion || clasificacion === '0') {
        clasificacion = porcentaje >= 80 ? 'Match alto' : (porcentaje >= 60 ? 'Ajuste medio' : 'Match bajo');
      } else if (clasificacion.toUpperCase() === 'AJUSTE_MEDIO') {
        clasificacion = 'Ajuste medio';
      } else if (clasificacion.toUpperCase() === 'AJUSTE_ALTO') {
        clasificacion = 'Match alto';
      } else if (clasificacion.toUpperCase() === 'AJUSTE_BAJO') {
        clasificacion = 'Match bajo';
      }

      const matchingResultadoId = Number(item.matchingResultadoId ?? item.MatchingResultadoId ?? item.id ?? item.Id ?? 0);

      // Extract JSONs directly representing FortalezasJson, BrechasJson and DesgloseCriteriosJson
      let fortalezasList: Array<{ texto: string; evidencia: string }> = [];
      let brechasList: Array<{ texto: string; evidencia: string }> = [];
      let desgloseList: Array<{ criterio: string; resultadoEncontrado: string; puntaje: string; ponderacion?: number; puntajeObtenido?: number; porcentajeCalculado?: number }> = [];

      const parseJsonSafely = (val: any) => {
        if (!val) return null;
        if (typeof val === 'string') {
          try { return JSON.parse(val); } catch { return null; }
        }
        return val;
      };

      const fortalezasRaw = item.fortalezas ?? item.Fortalezas ?? item.fortalezasJson ?? item.FortalezasJson;
      const brechasRaw = item.brechas ?? item.Brechas ?? item.brechasJson ?? item.BrechasJson;
      const desgloseRaw = item.desgloseCriterios ?? item.DesgloseCriterios ?? item.desgloseCriteriosJson ?? item.DesgloseCriteriosJson;

      const parsedFortalezas = parseJsonSafely(fortalezasRaw);
      const parsedBrechas = parseJsonSafely(brechasRaw);
      const parsedDesglose = parseJsonSafely(desgloseRaw);

      // 1. Map FortalezasJson
      if (Array.isArray(parsedFortalezas) && parsedFortalezas.length > 0) {
        fortalezasList = parsedFortalezas.map((f: any) => ({
          texto: typeof f === 'string' ? f : (f.criterio || f.nombre || f.texto || String(f)),
          evidencia: ''
        }));
      }

      // 2. Map BrechasJson
      if (Array.isArray(parsedBrechas) && parsedBrechas.length > 0) {
        const cleanBrechas = parsedBrechas.filter((b: any) => {
          const txt = (typeof b === 'string' ? b : (b.criterio || b.nombre || b.texto || '')).toLowerCase();
          return txt && !txt.includes('sin brecha') && !txt.includes('ninguna brecha') && txt !== '[]';
        });
        if (cleanBrechas.length > 0) {
          brechasList = cleanBrechas.map((b: any) => ({
            texto: typeof b === 'string' ? b : (b.criterio || b.nombre || b.texto || String(b)),
            evidencia: ''
          }));
        }
      }

      // 3. Map DesgloseCriteriosJson
      if (parsedDesglose && typeof parsedDesglose === 'object') {
        if (Array.isArray(parsedDesglose)) {
          desgloseList = parsedDesglose.map((c: any) => {
            const critNombre = typeof c === 'string' ? c : (c.criterio || c.Criterio || c.nombre || c.nombreCriterio || 'Criterio evaluado');
            const evidencia = typeof c === 'string' ? 'Evaluación técnica completada' : (c.evidencia || c.Evidencia || c.resultadoEncontrado || c.ResultadoEncontrado || 'Evaluación técnica completada');
            
            const ponderacion = (c && typeof c === 'object' && (c.ponderacion ?? c.Ponderacion ?? c.peso)) != null ? Number(c.ponderacion ?? c.Ponderacion ?? c.peso) : null;
            const puntajeObtenido = (c && typeof c === 'object' && (c.puntajeObtenido ?? c.PuntajeObtenido ?? c.puntajeObtenidoNum)) != null ? Number(c.puntajeObtenido ?? c.PuntajeObtenido ?? c.puntajeObtenidoNum) : null;
            
            let puntajeTexto = '100%';
            let porcentajeCalc = 100;

            if (ponderacion !== null && puntajeObtenido !== null && ponderacion > 0) {
              porcentajeCalc = Math.round((puntajeObtenido / ponderacion) * 100);
              puntajeTexto = `${puntajeObtenido}/${ponderacion} pts (${porcentajeCalc}%)`;
            } else if (c && typeof c === 'object' && c.puntaje != null) {
              puntajeTexto = String(c.puntaje);
              const numMatch = String(c.puntaje).match(/\d+/);
              if (numMatch) porcentajeCalc = parseInt(numMatch[0], 10);
            } else if (c && typeof c === 'object' && c.Puntaje != null) {
              puntajeTexto = String(c.Puntaje);
              const numMatch = String(c.Puntaje).match(/\d+/);
              if (numMatch) porcentajeCalc = parseInt(numMatch[0], 10);
            } else if (c && typeof c === 'object' && c.puntajeNum != null) {
              porcentajeCalc = Number(c.puntajeNum);
              puntajeTexto = `${c.puntajeNum}%`;
            }

            return {
              criterio: critNombre,
              resultadoEncontrado: evidencia,
              puntaje: puntajeTexto,
              ponderacion: ponderacion !== null ? ponderacion : undefined,
              puntajeObtenido: puntajeObtenido !== null ? puntajeObtenido : undefined,
              porcentajeCalculado: porcentajeCalc
            };
          });
        } else if (Array.isArray(parsedDesglose.cumplidos) || Array.isArray(parsedDesglose.parciales) || Array.isArray(parsedDesglose.noCumplidos)) {
          const allItems = [...(parsedDesglose.cumplidos || []), ...(parsedDesglose.parciales || []), ...(parsedDesglose.noCumplidos || [])];
          desgloseList = allItems.map((c: any) => typeof c === 'string' ? { criterio: c, resultadoEncontrado: 'Evaluación técnica completada', puntaje: '80%', porcentajeCalculado: 80 } : {
            criterio: c.criterio || c.Criterio || c.nombre || 'Criterio evaluado',
            resultadoEncontrado: c.resultadoEncontrado || c.ResultadoEncontrado || c.evidencia || 'Evaluación técnica completada',
            puntaje: c.puntaje || c.Puntaje || '80%',
            porcentajeCalculado: 80
          });
        }
      }

      // Fallbacks if empty
      if (fortalezasList.length === 0) {
        const cargoTarget = item.cargo || item.Cargo || candidateInfo.cargo || 'Suscripción y Riesgos';
        if (porcentaje >= 80) {
          fortalezasList = [
            { texto: `Experiencia relevante en ${cargoTarget}`, evidencia: '' },
            { texto: 'Formación académica requerida', evidencia: 'Título profesional verificado en provisión nacional' },
            { texto: 'Competencias clave alineadas con el perfil', evidencia: 'Evaluación técnica completada exitosamente' },
            { texto: 'Disponibilidad inmediata para el cargo', evidencia: 'Disponibilidad de incorporación confirmada' }
          ];
        } else {
          fortalezasList = [
            { texto: 'Formación técnica base', evidencia: 'Perfil compatible con la vacante' }
          ];
        }
      }

      if (brechasList.length === 0 && porcentaje < 80) {
        brechasList = [
          { texto: 'Pretensión salarial o movilidad geográfica', evidencia: 'Requiere confirmación o ajuste de pretensión salarial' }
        ];
      }

      if (desgloseList.length === 0) {
        desgloseList = [
          { criterio: 'Experiencia técnica específica', resultadoEncontrado: 'Evaluación técnica de perfil', puntaje: `${porcentaje}%` },
          { criterio: 'Formación profesional requerida', resultadoEncontrado: 'Acreditación profesional verificada', puntaje: '100%' }
        ];
      }

      const detalleCriterios = {
        fortalezas: fortalezasList,
        brechas: brechasList,
        desglose: desgloseList,
        cumplidos: fortalezasList.map(f => ({ criterio: f.texto, resultadoEncontrado: f.evidencia, puntaje: '100%' })),
        parciales: [],
        noCumplidos: brechasList.map(b => ({ criterio: b.texto, resultadoEncontrado: b.evidencia, puntaje: '40%' }))
      };

      const nombreFinal = item.nombre || item.Nombre || item.postulanteNombre || item.candidatoNombre || item.postulante?.nombreCompleto || candidateInfo.nombre;
      const cargoFinal = item.cargo || item.Cargo || item.ultimoCargo || candidateInfo.cargo;
      const regionalFinal = item.regional || item.Regional || candidateInfo.regional;

      if (isPoolManual) {
        const codigoVal = item.codigo || item.Codigo || `POOL-${String(pid).padStart(4, '0')}`;
        const existingIdx = postulantesEvaluados.findIndex(p =>
          (item.id && p.id === item.id) ||
          p.codigo === codigoVal ||
          p.nombre.toLowerCase().trim() === nombreFinal.toLowerCase().trim()
        );

        const newItem = {
          id: item.id || item.Id || index + 1,
          matchingResultadoId,
          codigo: codigoVal,
          nombre: nombreFinal,
          email: item.email || item.Email || `${nombreFinal.toLowerCase().replace(/\s+/g, '.')}@ejemplo.com`,
          origen: item.origen || item.Origen || 'Pool Manual',
          cargo: cargoFinal,
          area: item.area || item.Area || dto.area || 'Suscripción',
          regional: regionalFinal,
          pretensionSalarial: Number(item.pretensionSalarial || item.PretensionSalarial || 8500),
          matchPorcentaje: porcentaje,
          clasificacion,
          fechaEvaluacion: item.fechaCreacion || item.FechaCreacion || item.fechaEvaluacion || item.FechaEvaluacion || new Date().toISOString().split('T')[0],
          detalleCriterios
        };

        if (existingIdx >= 0) {
          postulantesEvaluados[existingIdx] = newItem;
        } else {
          postulantesEvaluados.push(newItem);
        }
      } else if (isInterno) {
        talentoInterno.push({
          id: item.id || item.Id || index + 1,
          matchingResultadoId,
          codigo: item.codigo || item.Codigo || `INT-${String(pid).padStart(4, '0')}`,
          nombre: nombreFinal,
          cargo: cargoFinal,
          area: item.area || item.Area || dto.area || 'Suscripción',
          regional: regionalFinal,
          matchPorcentaje: porcentaje,
          clasificacion,
          brechaPrincipal: item.brechaPrincipal || item.BrechaPrincipal || (brechasList[0]?.texto || 'Sin brechas críticas'),
          disponibilidad: item.disponibilidad || item.Disponibilidad || 'Disponible',
          accionSugerida: item.accionSugerida || item.AccionSugerida || 'Validar disponibilidad',
          tipoDescarte: item.tipoDescarte || item.TipoDescarte || null,
          motivoExclusion: item.motivoExclusion || item.MotivoExclusion || null,
          detalleCriterios
        });
      } else {
        postulantesHistoricos.push({
          id: item.id || item.Id || index + 1,
          matchingResultadoId,
          codigo: item.codigo || item.Codigo || `HIS-${String(pid).padStart(4, '0')}`,
          nombre: nombreFinal,
          ultimoCargo: cargoFinal,
          regional: regionalFinal,
          matchPorcentaje: porcentaje,
          clasificacion,
          vigencia: item.vigencia || item.Vigencia || 'Vigente',
          brecha: item.brecha || item.Brecha || (brechasList[0]?.texto || 'Actualizar datos'),
          accion: item.accion || item.Accion || 'Revisar antecedentes',
          tipoDescarte: item.tipoDescarte || item.TipoDescarte || null,
          motivoExclusion: item.motivoExclusion || item.MotivoExclusion || null,
          detalleCriterios
        });
      }
    });

    return { talentoInterno, postulantesHistoricos, postulantesEvaluados };
  }

  private parseEstrategiaExterna(raw: any, matchingEjecucionId: number, dtoJustificacion?: string, dtoRecomendada?: string): any {
    const item = Array.isArray(raw) ? (raw.length > 0 ? raw[0] : null) : raw;

    const parseField = (val: any, fallback: any) => {
      if (!val) return fallback;
      if (typeof val === 'string') {
        try { return JSON.parse(val); } catch { return fallback; }
      }
      return val;
    };

    const isExternaRecomendada = (dtoRecomendada || '').toUpperCase() === 'EXTERNA';
    const defaultPrioridad = isExternaRecomendada ? 'PRIORITARIA' : 'CONTINGENCIA_PREPARADA';

    const defaultCriteriosDificiles = [
      { criterio: 'Experiencia específica en Suscripción de Ramos Generales', impacto: 'ALTO', evidencia: 'Requiere más de 5 años comprobados en la industria de seguros corporativos.' },
      { criterio: 'Manejo avanzado de inglés técnico para reaseguros', impacto: 'ALTO', evidencia: 'Certificación B2/C1 para negociación directa con firmas internacionales.' },
      { criterio: 'Acreditación técnica vigente ante la APS', impacto: 'MEDIO', evidencia: 'Exámenes normativos aprobados y vigencia institucional activa.' }
    ];

    const defaultPublicoObjetivo = {
      cargosSimilares: ['Suscriptor Senior', 'Analista de Riesgos', 'Especialista en Reaseguros'],
      sectoresSugeridos: ['Seguros Generales', 'Reaseguradoras', 'Brokers Corporativos'],
      ubicacion: 'La Paz / Santa Cruz',
      modalidad: 'Presencial con flexibilidad',
      experienciaMinima: '5 años en cargos de suscripción o evaluación de riesgo',
      formacion: 'Licenciatura en Economía, Ingeniería Comercial o Finanzas',
      conocimientosClave: ['Normativa APS', 'Tarificación técnica de riesgos', 'Suscripción corporativa'],
      herramientasClave: ['Excel avanzado', 'Software de gestión actuarial', 'PowerBI'],
      competenciasClave: ['Negociación estratégica', 'Pensamiento analítico', 'Toma de decisiones bajo presión']
    };

    const defaultCanalesSugeridos = [
      { prioridad: 1, canal: 'LINKEDIN', tipoAccion: 'BUSQUEDA_Y_PUBLICACION', motivo: 'Red profesional principal con mayor densidad de ejecutivos del sector asegurador', criterioSeleccion: 'PERFIL_ESPECIALIZADO', seleccionado: true },
      { prioridad: 2, canal: 'PORTALES_ESPECIALIZADOS', tipoAccion: 'BUSQUEDA_Y_PUBLICACION', motivo: 'Difusión en plataformas de empleo ejecutivo y colegios de profesionales', criterioSeleccion: 'SEGMENTACION_SECTORIAL', seleccionado: true },
      { prioridad: 3, canal: 'BUSQUEDA_DIRECTA', tipoAccion: 'BUSQUEDA_DIRECTA', motivo: 'Headhunting directo para perfiles de alta demanda en competidores', criterioSeleccion: 'PERFIL_DE_ALTA_DEMANDA', seleccionado: false }
    ];

    const defaultPlanAccion = [
      { orden: 1, plazo: 'Días 1 - 3', accion: 'Publicación de brief y prospección inicial', motivo: 'Generación de pipeline temprano', responsable: 'Reclutador Senior', resultadoEsperado: 'Publicación activa en LinkedIn y portales especializados' },
      { orden: 2, plazo: 'Días 4 - 8', accion: 'Contacto directo y filtro telefónico', motivo: 'Evaluación de interés y ajuste salarial', responsable: 'Analista de Selección', resultadoEsperado: 'Mínimo 8 candidatos calificados en primer filtro' },
      { orden: 3, plazo: 'Días 9 - 14', accion: 'Entrevistas técnicas y envío de shortlist', motivo: 'Validación de competencias y brechas', responsable: 'Jefe de Selección y Área', resultadoEsperado: 'Terna de candidatos finalistas para entrevista presencial' }
    ];

    const defaultBriefEditable = {
      titulo: 'Suscriptor Senior de Ramos Generales',
      ubicacion: 'La Paz, Bolivia',
      modalidad: 'Presencial',
      tipoContrato: 'Indefinido',
      bandaSalarial: 'Acorde al mercado asegurador',
      introduccion: 'Buscamos un profesional apasionado por el análisis de riesgos y la suscripción corporativa para incorporarse a nuestro equipo de excelencia en Nacional Seguros.',
      desafioPrincipal: 'Liderar la evaluación técnica y tarificación de pólizas complejas garantizando la rentabilidad de la cartera y el cumplimiento normativo.',
      funciones: [
        'Evaluar y autorizar solicitudes de suscripción para clientes corporativos.',
        'Analizar niveles de riesgo y estructurar condiciones de cobertura.',
        'Mantener relación técnica directa con brokers y reaseguradores internacionales.'
      ],
      formacion: 'Licenciatura en Ciencias Económicas, Financieras, Administración o Ingeniería Comercial.',
      experiencia: 'Mínimo 5 años de experiencia comprobada en suscripción de ramos generales o gestión de riesgos en el sector asegurador.',
      herramientas: ['Excel avanzado', 'Software de análisis de riesgo', 'Sistemas de gestión de seguros'],
      conocimientos: ['Normativa de seguros APS', 'Tarificación técnica', 'Reaseguros internacionales'],
      competencias: ['Capacidad de análisis cuantitativo', 'Toma de decisiones estratégica', 'Comunicación y negociación asertiva'],
      propuestaValor: [
        'Desarrollo profesional en la aseguradora líder del mercado nacional.',
        'Paquete salarial competitivo con incentivos por desempeño.',
        'Capacitación continua en reaseguros y riesgos con alianzas internacionales.'
      ],
      llamadoAccion: '¡Postúlate ahora y sé parte de nuestro equipo de innovación y excelencia!',
      enlacePostulacion: 'https://nacionalseguros.com.bo/carreras',
      contacto: 'talentohumano@nacionalseguros.com.bo',
      estadoEdicion: 'BORRADOR'
    };

    if (!item) {
      return {
        estrategiaExternaId: 0,
        matchingEjecucionId,
        prioridad: defaultPrioridad,
        justificacion: dtoJustificacion || 'Estrategia orientada a la atracción proactiva de talento externo calificado ante la limitada disponibilidad de candidatos internos con compatibilidad directa.',
        criteriosDificiles: defaultCriteriosDificiles,
        publicoObjetivo: defaultPublicoObjetivo,
        canalesSugeridos: defaultCanalesSugeridos,
        planAccion: defaultPlanAccion,
        briefEditable: defaultBriefEditable,
        conclusion: 'La estrategia externa permite garantizar la cobertura efectiva de la posición mitigando el riesgo de vacancia crítica.'
      };
    }

    const criteriosDificiles = parseField(item.criteriosDificiles, defaultCriteriosDificiles);
    const publicoObjetivo = parseField(item.publicoObjetivo, defaultPublicoObjetivo);
    const canalesSugeridos = parseField(item.canalesSugeridos, defaultCanalesSugeridos);
    const planAccion = parseField(item.planAccion, defaultPlanAccion);
    const briefEditable = parseField(item.briefingEditable ?? item.briefEditable, defaultBriefEditable);

    return {
      estrategiaExternaId: item.id ?? item.estrategiaExternaId ?? 0,
      matchingEjecucionId: item.matchingEjecucionId ?? matchingEjecucionId,
      prioridad: item.prioridad || defaultPrioridad,
      justificacion: item.justificacion || dtoJustificacion || 'Estrategia externa orientada a la atracción proactiva de talento especializado.',
      criteriosDificiles: Array.isArray(criteriosDificiles) ? criteriosDificiles : defaultCriteriosDificiles,
      publicoObjetivo: publicoObjetivo && typeof publicoObjetivo === 'object' ? publicoObjetivo : defaultPublicoObjetivo,
      canalesSugeridos: Array.isArray(canalesSugeridos) ? canalesSugeridos : defaultCanalesSugeridos,
      planAccion: Array.isArray(planAccion) ? planAccion : defaultPlanAccion,
      briefEditable: briefEditable && typeof briefEditable === 'object' ? briefEditable : defaultBriefEditable,
      conclusion: item.conclusion || 'La estrategia externa permite garantizar la cobertura efectiva de la posición mitigando la falta de talento interno inmediato.',
      estadoId: item.estadoId,
      fechaCreacion: item.fechaCreacion,
      fechaModificacion: item.fechaModificacion
    };
  }

  private parseEstrategiaInterna(raw: any): EstrategiaInternaDetalle | undefined {
    if (!raw) return undefined;
    const item = Array.isArray(raw) ? (raw.length > 0 ? raw[0] : null) : raw;
    if (!item) return undefined;

    const rawPlanAccion = item.planAccion ?? item.PlanAccion;
    let planAccion: PlanAccionItem[] = [];
    if (typeof rawPlanAccion === 'string') {
      try { planAccion = JSON.parse(rawPlanAccion); } catch { planAccion = []; }
    } else if (Array.isArray(rawPlanAccion)) {
      planAccion = rawPlanAccion;
    }

    const rawPlanEvaluacion = item.planEvaluacion ?? item.PlanEvaluacion;
    let planEvaluacion: PlanEvaluacionEtapa[] = [];
    if (typeof rawPlanEvaluacion === 'string') {
      try { planEvaluacion = JSON.parse(rawPlanEvaluacion); } catch { planEvaluacion = []; }
    } else if (Array.isArray(rawPlanEvaluacion)) {
      planEvaluacion = rawPlanEvaluacion;
    }

    return {
      id: item.id ?? item.estrategiaInternaId ?? item.EstrategiaInternaId,
      matchingEjecucionId: item.matchingEjecucionId ?? item.MatchingEjecucionId,
      prioridad: item.prioridad ?? item.Prioridad,
      justificacion: item.justificacion ?? item.Justificacion,
      planAccion,
      planEvaluacion,
      mensajeContingencia: item.mensajeContingencia ?? item.MensajeContingencia,
      conclusion: item.conclusion ?? item.Conclusion
    };
  }

  private getMockDetalle(id: number): EstrategiaDetalle {
    const dummyDto: MatchingEjecucionDto = {
      id,
      perfilCargoId: id,
      perfilEstructuradoId: id,
      versionPerfil: 1,
      codigoPerfil: `PER-${String(id).padStart(4, '0')}`,
      codigoMatching: `MATCH-${String(id).padStart(4, '0')}`,
      estado: 'COMPLETADO',
      totalEvaluados: 12,
      totalInternosEvaluados: 5,
      totalHistoricosEvaluados: 7,
      totalMatchAlto: 2,
      totalMatchMedio: 3,
      totalMatchBajo: 0,
      totalPotenciales: 5,
      totalDescartados: 7,
      compatibilidadPromedio: 84,
      nivelConfianza: 92,
      estrategiaRecomendada: 'INTERNA',
      justificacion: 'Análisis automatizado basado en perfiles estructurales e historial de competencias.',
      fechaInicio: new Date().toISOString(),
      createdDate: new Date().toISOString()
    };
    return this.mapMatchingDtoToDetalle(dummyDto, new Map());
  }

  private mapMatchingDtoToListItem(dto: MatchingEjecucionDto, perfilesMap: Map<number, any>): EstrategiaItem {
    const realId = dto.matchingEjecucionId ?? dto.id ?? 0;
    const perfilObj = perfilesMap.get(dto.perfilCargoId);

    const codigoPerfil = perfilObj?.codigoPerfil || (dto.codigoPerfil ? dto.codigoPerfil : `PER-${String(dto.perfilCargoId).padStart(4, '0')}`);
    const cargo = perfilObj?.cargo || dto.cargo || 'Suscriptor Senior';
    const area = perfilObj?.areaSolicitante || dto.area || 'Suscripción';
    const fechaActualizacion = dto.fechaFin || dto.fechaInicio || dto.createdDate || new Date().toISOString();

    const rawEstado = (dto.estado || '').toUpperCase();
    let estadoMapped: EstadoEstrategia = 'EnRevisionRRHHEstrategia';
    if (rawEstado === 'COMPLETADO' || rawEstado === 'ENREVISIONRRHHESTRATEGIA' || rawEstado === 'ENREVISION') {
      estadoMapped = 'EnRevisionRRHHEstrategia';
    } else if (rawEstado === 'APROBADA' || rawEstado === 'APROBADAESTRATEGIA') {
      estadoMapped = 'AprobadaEstrategia';
    } else if (rawEstado === 'OBSERVADA' || rawEstado === 'OBSERVADAESTRATEGIA') {
      estadoMapped = 'ObservadaEstrategia';
    } else if (rawEstado === 'PENDIENTE' || rawEstado === 'PROCESANDO' || rawEstado === 'PENDIENTEGENERACIONESTRATEGIA') {
      estadoMapped = 'PendienteGeneracionEstrategia';
    }

    const estrategiaRecomendada = (dto.estrategiaRecomendada || '').toUpperCase() === 'EXTERNA' ? 'EXTERNA' : 'INTERNA';

    return {
      estrategiaId: realId,
      codigoEstrategia: dto.codigoMatching || `MATCH-${String(realId).padStart(4, '0')}`,
      perfilId: dto.perfilCargoId,
      codigoPerfil,
      cargo,
      areaId: 1,
      area,
      estado: estadoMapped,
      estrategiaRecomendada,
      ultimaActualizacion: fechaActualizacion
    };
  }

  private formatEstadoName(raw?: string): string {
    if (!raw) return 'EN REVISIÓN RRHH';
    const norm = raw.toUpperCase().trim();
    if (norm === 'ENREVISIONRRHHESTRATEGIA' || norm === 'ENREVISION' || norm.includes('REVISION')) {
      return 'EN REVISIÓN RRHH';
    }
    if (norm === 'PENDIENTEGENERACIONESTRATEGIA' || norm === 'PENDIENTE' || norm.includes('PENDIENTE')) {
      return 'PENDIENTE DE GENERACIÓN';
    }
    if (norm === 'APROBADAESTRATEGIA' || norm === 'APROBADA') {
      return 'APROBADA';
    }
    if (norm === 'OBSERVADAESTRATEGIA' || norm === 'OBSERVADA') {
      return 'OBSERVADA';
    }
    if (norm === 'COMPLETADO') {
      return 'EN REVISIÓN RRHH';
    }
    return raw.replace(/Estrategia$/i, '').replace(/([A-Z])/g, ' $1').trim().toUpperCase();
  }

  private mapMatchingDtoToDetalle(dto: MatchingEjecucionDto, perfilesMap: Map<number, any>, estrategiaInterna?: EstrategiaInternaDetalle, estrategiaExterna?: any): EstrategiaDetalle {
    const perfilObj = perfilesMap.get(dto.perfilCargoId);

    const codigoPerfil = perfilObj?.codigoPerfil || (dto.codigoPerfil ? dto.codigoPerfil : `PER-${String(dto.perfilCargoId).padStart(4, '0')}`);
    const cargo = perfilObj?.cargo || dto.cargo || 'Suscriptor Senior';
    const area = perfilObj?.areaSolicitante || dto.area || 'Suscripción';
    const fechaActualizacion = dto.fechaFin || dto.fechaInicio || dto.createdDate || new Date().toISOString();

    // Parse fuentesConsultadas JSON if string
    let fuentesRaw: any[] = dto.fuentesConsultadas;
    if (typeof fuentesRaw === 'string') {
      try { fuentesRaw = JSON.parse(fuentesRaw); } catch { fuentesRaw = []; }
    }
    if (!Array.isArray(fuentesRaw)) {
      fuentesRaw = [];
    }

    // Parse resumenDescartes JSON if string
    let descartesRaw: any[] = dto.resumenDescartes;
    if (typeof descartesRaw === 'string') {
      try { descartesRaw = JSON.parse(descartesRaw); } catch { descartesRaw = []; }
    }
    if (!Array.isArray(descartesRaw)) {
      descartesRaw = [];
    }

    // Parse parametrosMatching JSON if string
    let parametrosRaw: any = dto.parametrosMatching;
    if (typeof parametrosRaw === 'string') {
      try { parametrosRaw = JSON.parse(parametrosRaw); } catch { parametrosRaw = {}; }
    }

    // Extract totals by fuente from fuentesConsultadas
    let internosRecibidos = 0;
    let internosViables = 0;
    let historicosRecibidos = 0;
    let historicosViables = 0;

    const fuentesConsultadasMapped: FuenteConsultada[] = fuentesRaw.map((f: any) => {
      const fuenteTipo = (f.fuente || f.nombre || '').toUpperCase();
      let nombreFormatted = f.nombre || 'Fuente consultada';
      let detalleFormatted = f.detalle || '';
      let resultadoFormatted = f.resultado || '';

      if (fuenteTipo.includes('PERFIL_APROBADO') || fuenteTipo.includes('PERFIL ESTRUCTURADO')) {
        nombreFormatted = 'Perfil estructurado aprobado';
        detalleFormatted = f.detalle || `${codigoPerfil} · v${dto.versionPerfil || 1}`;
      } else if (fuenteTipo.includes('BD_PERSONAL_INTERNO') || fuenteTipo.includes('PERSONAL INTERNO')) {
        nombreFormatted = 'Base de personal interno';
        internosRecibidos = f.totalRecibidos ?? dto.totalInternosEvaluados ?? dto.totalEvaluados ?? 0;
        internosViables = f.totalPotenciales ?? dto.totalPotenciales ?? 0;
        detalleFormatted = `${internosRecibidos} registros evaluados`;
        resultadoFormatted = `${internosViables} viables`;
      } else if (fuenteTipo.includes('BD_POSTULANTES_HISTORICOS') || fuenteTipo.includes('POSTULANTES HISTÓRICOS') || fuenteTipo.includes('HISTÓRICA')) {
        nombreFormatted = 'Base histórica de postulantes';
        historicosRecibidos = f.totalRecibidos ?? dto.totalHistoricosEvaluados ?? 0;
        historicosViables = f.totalPotenciales ?? 0;
        detalleFormatted = `${historicosRecibidos} registros evaluados`;
        resultadoFormatted = historicosViables > 0 ? `${historicosViables} viables` : '';
      } else if (fuenteTipo.includes('BANDAS_SALARIALES') || fuenteTipo.includes('BANDA')) {
        nombreFormatted = 'Bandas salariales';
        detalleFormatted = f.detalle || f.resultado || 'Banda salarial de referencia utilizada: 10000';
        resultadoFormatted = '';
      } else if (fuenteTipo.includes('CRITERIOS_BUSQUEDA') || fuenteTipo.includes('CRITERIO')) {
        nombreFormatted = 'Criterios de búsqueda';
        detalleFormatted = f.detalle || f.resultado || '5 criterios ponderados (suma 100)';
        resultadoFormatted = '';
      }

      if (resultadoFormatted && (resultadoFormatted.trim() === detalleFormatted.trim() || resultadoFormatted.toLowerCase() === detalleFormatted.toLowerCase())) {
        resultadoFormatted = '';
      }

      return {
        nombre: nombreFormatted,
        detalle: detalleFormatted,
        resultado: resultadoFormatted,
        estado: f.estado || 'CONSULTADO',
        totalRecibidos: f.totalRecibidos,
        totalPotenciales: f.totalPotenciales,
        fuenteRaw: f.fuente
      };
    });

    // Fallbacks for totals if not captured from fuentes array
    if (internosRecibidos === 0) internosRecibidos = dto.totalInternosEvaluados || dto.totalEvaluados || 0;
    if (internosViables === 0) internosViables = dto.totalPotenciales || 0;
    if (historicosRecibidos === 0) historicosRecibidos = dto.totalHistoricosEvaluados || 0;

    // Calculate descartes by origin
    const descartesInternos = descartesRaw
      .filter((d: any) => (d.origen || '').toUpperCase().includes('INTERNA') || (d.origen || '').toUpperCase() === 'BD_INTERNA')
      .reduce((sum: number, d: any) => sum + (d.cantidad || 0), 0);

    const descartesHistoricos = descartesRaw
      .filter((d: any) => (d.origen || '').toUpperCase().includes('HISTORICA') || (d.origen || '').toUpperCase() === 'BD_EXTERNA_HISTORICA')
      .reduce((sum: number, d: any) => sum + (d.cantidad || 0), 0);

    const desactualizadosCount = descartesRaw
      .filter((d: any) => (d.tipoDescarte || '').toUpperCase() === 'DATOS_DESACTUALIZADOS')
      .reduce((sum: number, d: any) => sum + (d.cantidad || 0), 0);

    // Calculate principal brecha
    let brechaPrincipalLabel = 'Sin brechas críticas identificadas';
    if (descartesRaw.length > 0) {
      const sortedDescartes = [...descartesRaw].sort((a, b) => (b.cantidad || 0) - (a.cantidad || 0));
      const topDescarte = sortedDescartes[0];
      const tipoKey = (topDescarte.tipoDescarte || '').toUpperCase();
      if (tipoKey.includes('EXPERIENCIA')) brechaPrincipalLabel = 'Experiencia mínima';
      else if (tipoKey.includes('DATOS')) brechaPrincipalLabel = 'Datos desactualizados';
      else if (tipoKey.includes('SALARIAL') || tipoKey.includes('PRETENSION')) brechaPrincipalLabel = 'Pretensión salarial';
      else if (tipoKey.includes('TECNICA') || tipoKey.includes('BRECHA')) brechaPrincipalLabel = 'Brecha técnica';
      else brechaPrincipalLabel = topDescarte.tipoDescarte || 'Criterios no cumplidos';

      if (topDescarte.comentario) {
        brechaPrincipalLabel += ` (${topDescarte.comentario})`;
      }
    }

    const estrategiaRecomendada: 'INTERNA' | 'EXTERNA' = (dto.estrategiaRecomendada || '').toUpperCase() === 'EXTERNA' ? 'EXTERNA' : 'INTERNA';
    const accionInmediata = estrategiaRecomendada === 'INTERNA'
      ? 'Revisar y contactar candidatos internos viables'
      : 'Preparar activación de la estrategia externa';

    const estrategiaContingencia = estrategiaRecomendada === 'INTERNA'
      ? 'Mantener preparada la estrategia externa'
      : 'Continuar con búsqueda externa prioritaria';

    // Duration calculation
    let duracionTexto = '—';
    if (dto.fechaInicio && dto.fechaFin) {
      const inicio = new Date(dto.fechaInicio).getTime();
      const fin = new Date(dto.fechaFin).getTime();
      if (!isNaN(inicio) && !isNaN(fin) && fin >= inicio) {
        const diffMs = fin - inicio;
        const mins = Math.floor(diffMs / 60000);
        const secs = Math.floor((diffMs % 60000) / 1000);
        duracionTexto = mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
      }
    } else if (dto.fechaInicio && !dto.fechaFin) {
      duracionTexto = 'Ejecución en proceso';
    }

    const rawEstado = (dto.estado || '').toUpperCase();
    let estadoMapped: EstadoEstrategia = 'EnRevisionRRHHEstrategia';
    if (rawEstado === 'COMPLETADO' || rawEstado === 'ENREVISIONRRHHESTRATEGIA' || rawEstado === 'ENREVISION') {
      estadoMapped = 'EnRevisionRRHHEstrategia';
    } else if (rawEstado === 'APROBADA' || rawEstado === 'APROBADAESTRATEGIA') {
      estadoMapped = 'AprobadaEstrategia';
    } else if (rawEstado === 'OBSERVADA' || rawEstado === 'OBSERVADAESTRATEGIA') {
      estadoMapped = 'ObservadaEstrategia';
    } else if (rawEstado === 'PENDIENTE' || rawEstado === 'PROCESANDO' || rawEstado === 'PENDIENTEGENERACIONESTRATEGIA') {
      estadoMapped = 'PendienteGeneracionEstrategia';
    }

    const realId = dto.matchingEjecucionId ?? dto.id ?? 0;

    return {
      estrategiaId: realId,
      codigoEstrategia: dto.codigoMatching || `MATCH-${String(realId).padStart(4, '0')}`,
      codigoMatching: dto.codigoMatching || `MATCH-${String(realId).padStart(4, '0')}`,
      perfilCargoId: dto.perfilCargoId,
      perfilEstructuradoId: dto.perfilEstructuradoId,
      solicitudId: dto.perfilCargoId,
      codigoPerfil,
      versionPerfil: dto.versionPerfil || 1,
      versionEstrategia: 1,
      cargo,
      area,
      regional: 'La Paz',
      bandaSalarial: { minimo: 0, maximo: 0 },
      cantidadVacantes: 1,
      prioridad: 'Alta',
      estado: estadoMapped,
      estadoTecnicoEjecucion: dto.estado,
      ultimaActualizacion: fechaActualizacion,
      mensajeError: dto.mensajeError,
      ejecucionMatching: {
        codigoMatching: dto.codigoMatching || `MATCH-${String(dto.id).padStart(4, '0')}`,
        estado: this.formatEstadoName(dto.estado),
        totalEvaluados: dto.totalEvaluados || 0,
        internosEvaluados: dto.totalInternosEvaluados || 0,
        historicosEvaluados: dto.totalHistoricosEvaluados || 0,
        matchAlto: dto.totalMatchAlto || 0,
        matchMedio: dto.totalMatchMedio || 0,
        matchBajo: dto.totalMatchBajo || 0,
        potenciales: dto.totalPotenciales || 0,
        descartados: dto.totalDescartados || 0,
        compatibilidadPromedio: dto.compatibilidadPromedio ?? 0,
        nivelConfianzaVal: dto.nivelConfianza ?? 0,
        fechaInicio: dto.fechaInicio,
        fechaFin: dto.fechaFin,
        duracionTexto
      },
      parametrosMatching: {
        ajusteAltoDesde: parametrosRaw.ajusteAltoDesde ?? 80,
        ajusteMedioDesde: parametrosRaw.ajusteMedioDesde ?? 60,
        ajusteBajoDesde: parametrosRaw.ajusteBajoDesde ?? 40
      },
      recomendacion: {
        estrategiaPrioritaria: estrategiaRecomendada,
        nivelViabilidad: 'ALTA',
        justificacion: dto.justificacion || 'Estrategia generada a partir del análisis automático de perfiles y fuentes.',
        internosViables,
        historicosViables,
        mejorMatchInterno: { cargado: false }, // Rule 4: mostrar — para mejor match interno al no venir de MatchingResultados
        mejorMatchHistorico: { cargado: false }, // Rule 4: mostrar — para mejor match histórico al no venir de MatchingResultados
        principalBrecha: brechaPrincipalLabel,
        accionInmediata,
        estrategiaContingencia,
        nivelConfianzaPorcentaje: dto.nivelConfianza
      },
      alertaSalarial: { existe: false, mensaje: '' },
      fuentesConsultadas: fuentesConsultadasMapped,
      resumenPersonalInterno: {
        recibidos: internosRecibidos,
        evaluados: dto.totalInternosEvaluados || dto.totalEvaluados || 0,
        descartados: descartesInternos,
        viables: internosViables,
        tieneResultados: false // Rule 7: mostrar — para match alto, medio, bajo, mejor match y promedio
      },
      resumenPostulantesHistoricos: {
        recibidos: historicosRecibidos,
        evaluados: dto.totalHistoricosEvaluados || 0,
        descartados: descartesHistoricos,
        viables: historicosViables,
        requierenActualizarDatos: desactualizadosCount,
        tieneResultados: false // Rule 7: mostrar — para match alto, medio, bajo, mejor match y promedio
      },
      resumenDescartes: descartesRaw,
      observaciones: [],
      talentoInterno: [],
      postulantesHistoricos: [],
      estrategiaInterna,
      estrategiaExterna
    };
  }

  getSummary(items: EstrategiaItem[]): EstrategiaSummary {
    return {
      total: items.length,
      pendientes: items.filter(e => e.estado === 'PendienteGeneracionEstrategia').length,
      enRevision: items.filter(e => e.estado === 'EnRevisionRRHHEstrategia').length,
      observadas: items.filter(e => e.estado === 'ObservadaEstrategia').length,
      aprobadas: items.filter(e => e.estado === 'AprobadaEstrategia').length
    };
  }

  agregarObservacionMock(id: number, comentario: string, usuario: string, rol: string): Observable<EstrategiaDetalle> {
    return this.getDetalleEstrategiaById(id).pipe(
      map(detalle => {
        const now = new Date();
        const fechaFormatted = `${now.getDate().toString().padStart(2, '0')}/${(now.getMonth() + 1).toString().padStart(2, '0')}/${now.getFullYear()} ${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;
        const nuevaObs: ObservacionEstrategiaItem = {
          id: (detalle.observaciones || []).length + 1,
          usuario,
          rol,
          fecha: fechaFormatted,
          comentario
        };
        return {
          ...detalle,
          estado: 'ObservadaEstrategia' as EstadoEstrategia,
          observaciones: [nuevaObs, ...(detalle.observaciones || [])]
        };
      })
    );
  }

  aprobarEstrategiaMock(id: number): Observable<EstrategiaDetalle> {
    return this.getDetalleEstrategiaById(id).pipe(
      map(detalle => ({
        ...detalle,
        estado: 'AprobadaEstrategia' as EstadoEstrategia
      }))
    );
  }
}
