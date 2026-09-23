import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject, throwError } from 'rxjs';
import { delay, map } from 'rxjs/operators';
import { EstadoPerfil } from '../models/estado-perfil.enum';
import { DetallePerfil, SeccionPerfil, ObservacionPerfil, EventoTrazabilidadPerfil, EstadoObservacionPerfil } from '../models/perfil.model';
import { ApiService } from '../../../core/services/api.service';
import { PerfilDetail } from '../../../core/models/perfil.model';
import { normalizeProfileList } from '../../../core/utils/profile-list-normalizer.utils';

@Injectable({
  providedIn: 'root'
})
export class PerfilesService {
  // TODO: Reemplazar por endpoint de observaciones.
  private observacionesSubject = new BehaviorSubject<ObservacionPerfil[]>([
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
      fechaRegistro: '2026-07-11T16:45:00Z',
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
      fechaRegistro: '2026-07-11T16:50:00Z',
      fechaResolucion: null,
      tipoObservacion: {
        idTipoObservacion: 3,
        nombre: 'Revisar criterio',
        activo: true
      }
    }
  ]);

  // Mapa de perfiles en memoria mock
  private perfilesMap = new Map<number, DetallePerfil>();
  private shouldFail = false;

  constructor(private apiService: ApiService) {
    this.inicializarPerfilesMock();
  }

  setShouldFail(fail: boolean): void {
    this.shouldFail = fail;
  }

  private inicializarPerfilesMock(): void {
    const cargosMock = [
      { id: 1, codigo: 'PER-001', solId: 101, solCod: 'SOL-101', cargo: 'Analista de Reclutamiento', area: 'Recursos Humanos', estado: EstadoPerfil.EnRevisionRRHHPE, version: 1 },
      { id: 2, codigo: 'PER-002', solId: 102, solCod: 'SOL-102', cargo: 'Jefe de Reclutamiento', area: 'Recursos Humanos', estado: EstadoPerfil.Corregida, version: 2 },
      { id: 3, codigo: 'PER-003', solId: 103, solCod: 'SOL-103', cargo: 'Ejecutivo Comercial', area: 'Comercial', estado: EstadoPerfil.Aprobada, version: 1 },
      { id: 4, codigo: 'PER-004', solId: 104, solCod: 'SOL-104', cargo: 'Analista de Finanzas', area: 'Finanzas', estado: EstadoPerfil.EnRevisionAreaSol, version: 1 },
      { id: 5, codigo: 'PER-005', solId: 105, solCod: 'SOL-105', cargo: 'Jefe de Cuentas', area: 'Comercial', estado: EstadoPerfil.Observada, version: 1 },
      { id: 6, codigo: 'PER-006', solId: 106, solCod: 'SOL-106', cargo: 'Ingeniero de Sistemas', area: 'Tecnología de Información', estado: EstadoPerfil.PerfilAprobadoFinal, version: 2 },
      { id: 7, codigo: 'PER-007', solId: 107, solCod: 'SOL-107', cargo: 'Analista de Datos', area: 'Tecnología de Información', estado: EstadoPerfil.SolicitudAprobada, version: 1 },
      { id: 8, codigo: 'PER-008', solId: 108, solCod: 'SOL-108', cargo: 'Jefe de Operaciones', area: 'Operaciones', estado: EstadoPerfil.PendienteGeneracionPerfil, version: 1 },
      { id: 9, codigo: 'PER-009', solId: 109, solCod: 'SOL-109', cargo: 'Gerente Comercial', area: 'Comercial', estado: EstadoPerfil.ResumenEjecutivoGenerado, version: 1 },
      { id: 10, codigo: 'PER-010', solId: 110, solCod: 'SOL-110', cargo: 'Jefe de Marketing', area: 'Marketing', estado: EstadoPerfil.Aprobada, version: 1 },
      { id: 11, codigo: 'PER-011', solId: 111, solCod: 'SOL-111', cargo: 'Ejecutivo de Atención', area: 'Operaciones', estado: EstadoPerfil.EnRevisionAreaSol, version: 2 }
    ];

    cargosMock.forEach(c => {
      this.perfilesMap.set(c.id, {
        id: c.id,
        codigo: c.codigo,
        solicitudId: c.solId,
        solicitudCodigo: c.solCod,
        cargo: c.cargo,
        areaId: c.id + 10,
        areaNombre: c.area,
        estado: c.estado,
        version: c.version,
        ultimaActualizacion: new Date().toISOString(),
        secciones: this.crearSeccionesMock(c.cargo, c.area),
        estadoCodigo: this.getMockEstadoCodigo(c.estado)
      });
    });
  }

  private getMockEstadoCodigo(estado: EstadoPerfil): string {
    const reverseMap: Record<EstadoPerfil, string> = {
      [EstadoPerfil.SolicitudAprobada]: 'PERF-PEN-GEN',
      [EstadoPerfil.PendienteGeneracionPerfil]: 'PERF-PEN-GEN',
      [EstadoPerfil.EnRevisionRRHHPE]: 'PERF-REV-RRHH',
      [EstadoPerfil.ResumenEjecutivoGenerado]: 'PERF-RES-GEN',
      [EstadoPerfil.EnRevisionAreaSol]: 'PERF-REV-AREA',
      [EstadoPerfil.Observada]: 'PERF-OBS-AREA',
      [EstadoPerfil.Corregida]: 'PERF-COR-RRHH',
      [EstadoPerfil.Aprobada]: 'PERF-APR-AREA',
      [EstadoPerfil.PerfilAprobadoFinal]: 'PERF-APR-FIN'
    };
    return reverseMap[estado] || 'PERF-PEN-GEN';
  }

  private crearSeccionesMock(cargo: string, area: string): SeccionPerfil[] {
    return [
      {
        id: 1,
        nombre: '1. Datos generales del cargo',
        icono: 'info',
        campos: [
          { id: '1a', etiqueta: 'Nombre del Cargo', valor: cargo, origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1b', etiqueta: 'Área Solicitante', valor: area, origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1c', etiqueta: 'Gerencia', valor: 'Operaciones y RRHH', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1d', etiqueta: 'Reporta a', valor: 'Gerente de División', origen: 'AGENTE_IA', editable: true, tipo: 'texto' },
          { id: '1e', etiqueta: 'Rango Salarial Estimado', valor: 'Bs. 8,000 - Bs. 11,000', origen: 'AGENTE_IA', editable: true, tipo: 'texto' }
        ]
      },
      {
        id: 2,
        nombre: '2. Objetivo principal del cargo',
        icono: 'ads_click',
        campos: [
          { 
            id: '2a', 
            etiqueta: 'Objetivo General', 
            valor: `Planificar, coordinar y ejecutar las actividades correspondientes al área de ${area} para garantizar la eficiencia de los procesos institucionales alineados con los objetivos de Nacional Seguros.`, 
            origen: 'AGENTE_IA', 
            editable: true, 
            tipo: 'textoLargo' 
          }
        ]
      },
      {
        id: 3,
        nombre: '3. Perfil requerido',
        icono: 'person',
        campos: [
          { id: '3a', etiqueta: 'Nivel Educativo Mínimo', valor: 'Licenciatura o Egresado Universitario', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '3b', etiqueta: 'Carreras Relacionadas', valor: 'Administración de Empresas, Ingeniería Comercial o ramas afines.', origen: 'AGENTE_IA', editable: true, tipo: 'texto' },
          { id: '3c', etiqueta: 'Años de Experiencia Requeridos', valor: 'Mínimo 2 años de experiencia en cargos similares.', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' }
        ]
      },
      {
        id: 4,
        nombre: '4. Conocimientos técnicos requeridos',
        icono: 'handyman',
        campos: [
          { id: '4a', etiqueta: 'Conocimientos Especializados', valor: ['Gestión por procesos', 'Planificación estratégica', 'Análisis de datos cuantitativos'], origen: 'AGENTE_IA', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 5,
        nombre: '5. Herramientas y sistemas',
        icono: 'computer',
        campos: [
          { id: '5a', etiqueta: 'Herramientas Obligatorias', valor: ['Microsoft Office 365 (Avanzado)', 'Power BI Intermedio', 'SAP ERP Basico'], origen: 'AGENTE_IA', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 6,
        nombre: '6. Funciones principales del cargo',
        icono: 'assignment',
        campos: [
          { 
            id: '6a', 
            etiqueta: 'Listado de Funciones', 
            valor: [
              'Supervisar las operaciones diarias del equipo a su cargo.',
              'Elaborar informes de gestión semanales y mensuales para la gerencia.',
              'Implementar mejoras continuas en los flujos de trabajo internos.',
              'Mantener una comunicación fluida y constante con proveedores externos clave.'
            ], 
            origen: 'AGENTE_IA', 
            editable: true, 
            tipo: 'lista' 
          }
        ]
      },
      {
        id: 7,
        nombre: '7. Competencias clave',
        icono: 'psychology',
        campos: [
          { id: '7a', etiqueta: 'Competencias Organizacionales', valor: ['Orientación a resultados', 'Trabajo colaborativo', 'Comunicación asertiva', 'Adaptabilidad al cambio'], origen: 'AGENTE_IA', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 8,
        nombre: '8. Indicadores de éxito del cargo',
        icono: 'analytics',
        campos: [
          { id: '8a', etiqueta: 'KPIs del Cargo', valor: ['Cumplimiento del plan operativo (%)', 'Tiempo promedio de resolución de incidentes (días)', 'Satisfacción del cliente interno (%)'], origen: 'AGENTE_IA', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 9,
        nombre: '9. Perfil ideal del candidato',
        icono: 'star',
        campos: [
          { id: '9a', etiqueta: 'Descripción de Ajuste Cultural', valor: 'Profesional proactivo, con alto sentido de ética, capacidad de autogestión y predisposición para liderar iniciativas de transformación digital en su equipo.', origen: 'AGENTE_IA', editable: true, tipo: 'textoLargo' }
        ]
      },
      {
        id: 10,
        nombre: '10. Filtros clave para selección',
        icono: 'filter_alt',
        campos: [
          { id: '10a', etiqueta: 'Preguntas Filtro Sugeridas', valor: ['¿Cuenta con experiencia liderando equipos en el sector asegurador?', '¿Tiene disponibilidad de incorporación inmediata?'], origen: 'AGENTE_IA', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 11,
        nombre: '11. Matriz sugerida de ponderación',
        icono: 'grid_on',
        campos: [
          { id: '11a', etiqueta: 'Matriz de Ponderación', valor: 'Experiencia Práctica: 40%, Conocimientos Técnicos: 30%, Competencias Blandas: 20%, Ajuste Cultural: 10%', origen: 'DATO_HISTORICO', editable: true, tipo: 'texto' }
        ]
      },
      {
        id: 12,
        nombre: '12. Perfil tipo de alto ajuste',
        icono: 'verified_user',
        campos: [
          { id: '12a', etiqueta: 'Descripción del Candidato Histórico Ideal', valor: 'Profesionales provenientes de consultoras multinacionales o bancos con experiencia acelerada de crecimiento y liderazgo de células ágiles.', origen: 'DATO_HISTORICO', editable: true, tipo: 'textoLargo' }
        ]
      }
    ];
  }

  private readonly stateMap: Record<string, EstadoPerfil> = {
    'PERF-PEN-GEN': EstadoPerfil.PendienteGeneracionPerfil,
    'PERF-REV-RRHH': EstadoPerfil.EnRevisionRRHHPE,
    'PERF-RES-GEN': EstadoPerfil.ResumenEjecutivoGenerado,
    'PERF-REV-AREA': EstadoPerfil.EnRevisionAreaSol,
    'PERF-OBS-AREA': EstadoPerfil.Observada,
    'PERF-COR-RRHH': EstadoPerfil.Corregida,
    'PERF-APR-AREA': EstadoPerfil.Aprobada,
    'PERF-APR-FIN': EstadoPerfil.PerfilAprobadoFinal,
  };

  private mapDetailToFrontend(p: PerfilDetail): DetallePerfil {
    const estado = this.stateMap[p.estadoCodigo] || EstadoPerfil.PendienteGeneracionPerfil;
    const secciones = this.mapEstructuradoToSecciones(p);

    return {
      id: p.perfilId,
      codigo: p.codigoPerfil,
      solicitudId: p.solicitudId,
      solicitudCodigo: p.codigoSolicitud,
      cargo: p.cargo,
      areaId: 0,
      areaNombre: p.areaSolicitante,
      estado: estado,
      version: p.version || 1,
      ultimaActualizacion: p.ultimaActualizacion,
      secciones: secciones,
      aprobadoPorArea: p.aprobadoPorArea,
      estadoCodigo: p.estadoCodigo,
      documentos: p.documentos ? p.documentos.map((d: any) => ({
        documentoId: d.documentoId,
        solicitudId: d.solicitudId,
        tipoDocumento: d.tipoDocumento,
        fileName: d.fileName || d.nombre,
        storageProvider: d.storageProvider,
        storagePath: d.storagePath,
        publicUrl: d.publicUrl || d.url,
        generadoPor: d.generadoPor,
        createdDate: d.createdDate || d.fechaCreacion
      })) : []
    };
  }

  private mapEstructuradoToSecciones(p: PerfilDetail): SeccionPerfil[] {
    const est = p.perfilEstructurado as any;
    const res = p.resumenEjecutivo as any;

    const splitTextToList = (text?: string | string[]): string[] => {
      return normalizeProfileList(text);
    };

    // Detect if we are using the raw/ungenerated structure or the consolidated/generated structure
    const isGenerated = est && (est.objetivoPrincipalCargo !== undefined || est.datosGeneralesCargo !== undefined);

    let cargo = p.cargo || '';
    let area = p.areaSolicitante || '';
    let regional = '';
    let reportaA = '';
    let tipoPosicion = '';
    let modalidad = '';
    let bandaSalarial = res?.bandaSalarial || '';
    let salario = p.salarioMinimo || p.salarioMaximo || bandaSalarial || '';
    if (salario.includes('-')) {
      const parts = salario.split('-');
      if (parts[0].trim() === parts[1].trim()) {
        salario = parts[0].trim();
      }
    }

    let objetivo = '';
    let formacion = '';
    let experiencia = '';
    let conocimientos: string[] = [];
    let herramientas: string[] = [];
    let funciones: string[] = [];
    let competencias: string[] = [];
    let indicadores: string[] = [];
    let perfilIdeal = '';
    let excluyentes: string[] = [];
    let deseables: string[] = [];
    let matriz: any = [];
    let altoAjuste = '';

    const parseStringOrArray = (val: any): string => {
      if (!val) return '';
      if (Array.isArray(val)) {
        return val.map((x: any) => String(x).trim()).filter((x: any) => x).join('\n');
      }
      return String(val).trim();
    };

    if (isGenerated) {
      // 1. Generated structure (from PerfilEstructurado table)
      cargo = est.datosGeneralesCargo?.cargo || cargo;
      area = est.datosGeneralesCargo?.area || area;
      regional = est.datosGeneralesCargo?.regionalCiudad || est.datosGeneralesCargo?.regional || '';
      reportaA = est.datosGeneralesCargo?.reportaA || est.datosGeneralesCargo?.reporta_a || 'Gerente de División';
      
      // Seniority / Tipo de posición
      tipoPosicion = est.datosGeneralesCargo?.seniority || est.condicionesVacante?.seniority || '';
      
      // Modalidad
      modalidad = est.datosGeneralesCargo?.modalidad || est.condicionesVacante?.modalidadTrabajo || est.condicionesVacante?.modalidad || '';

      objetivo = parseStringOrArray(est.objetivoPrincipalCargo);
      formacion = parseStringOrArray(est.perfilRequerido?.formacionAcademica || est.perfilRequerido?.estudios);
      
      // Experiencia
      const expMin = parseStringOrArray(est.perfilRequerido?.experienciaMinima);
      const expInd = parseStringOrArray(est.perfilRequerido?.experienciaIndispensable);
      const expDes = parseStringOrArray(est.perfilRequerido?.criteriosDeseables);
      const expParts = [];
      if (expMin) expParts.push(`Experiencia Mínima:\n${expMin}`);
      if (expInd) expParts.push(`Experiencia Indispensable:\n${expInd}`);
      if (expDes) expParts.push(`Experiencia Valorada/Deseable:\n${expDes}`);
      
      if (expParts.length > 0) {
        experiencia = expParts.join('\n\n');
      } else {
        const anosExp = est.perfilRequerido?.anosExperiencia;
        experiencia = anosExp ? `${anosExp} años de experiencia requeridos.` : '';
      }

      conocimientos = splitTextToList(est.conocimientosTecnicosRequeridos);

      // herramientasSistemas is an object (ide, versionamiento, ci_cd, requerido, deseable) or string array
      if (est.herramientasSistemas && typeof est.herramientasSistemas === 'object' && !Array.isArray(est.herramientasSistemas)) {
        const req = splitTextToList(est.herramientasSistemas.requerido);
        const des = splitTextToList(est.herramientasSistemas.deseable);
        herramientas = [];
        if (req.length > 0) herramientas.push(`Requerido: ${req.join(', ')}`);
        if (des.length > 0) herramientas.push(`Deseable: ${des.join(', ')}`);
        if (herramientas.length === 0) {
          herramientas = Object.entries(est.herramientasSistemas)
            .map(([key, val]) => `${key}: ${parseStringOrArray(val)}`)
            .filter((x: any) => x);
        }
      } else {
        herramientas = splitTextToList(est.herramientasSistemas);
      }

      funciones = splitTextToList(est.funcionesPrincipalesCargo);
      competencias = splitTextToList(est.competenciasClave);
      indicadores = splitTextToList(est.indicadoresExitoCargo);
      perfilIdeal = parseStringOrArray(est.perfilIdealCandidato);

      // filtrosClaveSeleccion is an object (ingles: Avanzado, criteriosExcluyentes, criteriosDeseables, etc.) or string array
      if (est.filtrosClaveSeleccion && typeof est.filtrosClaveSeleccion === 'object' && !Array.isArray(est.filtrosClaveSeleccion)) {
        const excText = parseStringOrArray(est.filtrosClaveSeleccion.criteriosExcluyentes || est.filtrosClaveSeleccion.excluyentes);
        const desText = parseStringOrArray(est.filtrosClaveSeleccion.criteriosDeseables || est.filtrosClaveSeleccion.deseables);
        
        excluyentes = excText ? [excText] : [];
        deseables = desText ? [desText] : [];
        
        if (excluyentes.length === 0 && deseables.length === 0) {
          excluyentes = Object.entries(est.filtrosClaveSeleccion)
            .map(([key, val]) => `${key}: ${parseStringOrArray(val)}`)
            .filter((x: any) => x);
          deseables = [];
        }
      } else {
        excluyentes = splitTextToList(est.filtrosClaveSeleccion);
        deseables = [];
      }

      // matrizPonderacion is an array of criteria and weights
      matriz = this.parseMatrizPonderacion(est.matrizPonderacion);

      altoAjuste = parseStringOrArray(est.perfilTipoAltoAjuste);
    } else {
      // 2. Raw / ungenerated structure (from Solicitud)
      const dg = est?.datosGenerales;
      const pr = est?.perfilRequerido;
      const cv = est?.condicionesVacante;

      cargo = dg?.cargo || cargo;
      area = dg?.area || area;
      regional = dg?.regional || '';
      reportaA = dg?.cargoSolicitante || 'Gerente de División';
      tipoPosicion = dg?.tipoSolicitud || '';
      modalidad = cv?.modalidadTrabajo || '';

      objetivo = parseStringOrArray(pr?.objetivoCargo);
      formacion = parseStringOrArray(pr?.formacionAcademica);

      const min = parseStringOrArray(pr?.experienciaMinima);
      const ind = parseStringOrArray(pr?.experienciaIndispensable);
      const des = parseStringOrArray(pr?.criteriosDeseables);
      const expParts = [];
      if (min) expParts.push(`Experiencia Mínima:\n${min}`);
      if (ind) expParts.push(`Experiencia Indispensable:\n${ind}`);
      if (des) expParts.push(`Experiencia Valorada/Deseable:\n${des}`);
      experiencia = expParts.join('\n\n');

      conocimientos = splitTextToList(pr?.conocimientosTecnicos);
      herramientas = splitTextToList(pr?.herramientasSistemas);
      funciones = splitTextToList(pr?.funciones);
      competencias = splitTextToList(pr?.competenciasClave);
      indicadores = splitTextToList(res?.criteriosEvaluacion);
      perfilIdeal = parseStringOrArray(res?.caracteristicasClave);

      excluyentes = splitTextToList(pr?.criteriosExcluyentes);
      deseables = splitTextToList(pr?.criteriosDeseables);
      
      matriz = this.parseMatrizPonderacion('Experiencia Práctica: 40%, Conocimientos Técnicos: 30%, Competencias Blandas: 20%, Ajuste Cultural: 10%');
      altoAjuste = parseStringOrArray(res?.valoracionPerfil);
    }

    return [
      {
        id: 1,
        nombre: '1. Datos generales del cargo',
        icono: 'info',
        campos: [
          { id: '1a', etiqueta: 'Nombre del cargo', valor: cargo || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1b', etiqueta: 'Área solicitante', valor: area || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1c', etiqueta: 'Regional / ciudad', valor: regional || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1d', etiqueta: 'Reporta a', valor: reportaA || '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'texto' },
          { id: '1e', etiqueta: 'Tipo de posición', valor: tipoPosicion || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1f', etiqueta: 'Modalidad', valor: modalidad || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'texto' },
          { id: '1g', etiqueta: 'Salario', valor: salario || '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'texto' }
        ]
      },
      {
        id: 2,
        nombre: '2. Objetivo principal del cargo',
        icono: 'ads_click',
        campos: [
          { id: '2a', etiqueta: 'Objetivo principal del cargo', valor: objetivo || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'textoLargo' }
        ]
      },
      {
        id: 3,
        nombre: '3. Perfil requerido',
        icono: 'person',
        campos: [
          { id: '3a', etiqueta: 'Formación académica', valor: formacion || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'textoLargo' },
          { id: '3b', etiqueta: 'Experiencia requerida', valor: experiencia || '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'textoLargo' }
        ]
      },
      {
        id: 4,
        nombre: '4. Conocimientos técnicos requeridos',
        icono: 'handyman',
        campos: [
          { id: '4a', etiqueta: 'Conocimientos técnicos requeridos', valor: conocimientos.length > 0 ? conocimientos : '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'lista' }
        ]
      },
      {
        id: 5,
        nombre: '5. Herramientas y sistemas',
        icono: 'computer',
        campos: [
          { id: '5a', etiqueta: 'Herramientas y sistemas', valor: herramientas.length > 0 ? herramientas : '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 6,
        nombre: '6. Funciones principales del cargo',
        icono: 'assignment',
        campos: [
          { id: '6a', etiqueta: 'Funciones principales del cargo', valor: funciones.length > 0 ? funciones : '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 7,
        nombre: '7. Competencias clave',
        icono: 'psychology',
        campos: [
          { id: '7a', etiqueta: 'Competencias clave', valor: competencias.length > 0 ? competencias : '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'lista' }
        ]
      },
      {
        id: 8,
        nombre: '8. Indicadores de éxito del cargo',
        icono: 'analytics',
        campos: [
          { id: '8a', etiqueta: 'Indicadores de éxito del cargo', valor: indicadores.length > 0 ? indicadores : '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'lista' }
        ]
      },
      {
        id: 9,
        nombre: '9. Perfil ideal del candidato',
        icono: 'star',
        campos: [
          { id: '9a', etiqueta: 'Descripción del perfil ideal del candidato', valor: perfilIdeal || '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'textoLargo' }
        ]
      },
      {
        id: 10,
        nombre: '10. Filtros clave para selección',
        icono: 'filter_alt',
        campos: [
          { id: '10a', etiqueta: 'Criterios excluyentes sugeridos', valor: excluyentes.length > 0 ? excluyentes : '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'lista' },
          { id: '10b', etiqueta: 'Criterios deseables', valor: deseables.length > 0 ? deseables : '—', origen: 'SOLICITUD_VACANTE', editable: false, tipo: 'lista' }
        ]
      },
      {
        id: 11,
        nombre: '11. Matriz sugerida de ponderación',
        icono: 'grid_on',
        campos: [
          { id: '11a', etiqueta: 'Matriz sugerida de ponderación', valor: matriz, origen: 'AJUSTADO_RRHH', editable: true, tipo: 'tabla' }
        ]
      },
      {
        id: 12,
        nombre: '12. Perfil tipo de alto ajuste',
        icono: 'verified_user',
        campos: [
          { id: '12a', etiqueta: 'Perfil tipo de alto ajuste', valor: altoAjuste || '—', origen: 'AJUSTADO_RRHH', editable: true, tipo: 'textoLargo' }
        ]
      }
    ];
  }

  obtenerPerfilPorId(id: number): Observable<DetallePerfil> {
    if (this.shouldFail) {
      return throwError(() => new Error('Error al conectar con el servidor para obtener el perfil.'));
    }

    return this.apiService.getPerfilById(id).pipe(
      map(perfil => this.mapDetailToFrontend(perfil))
    );
  }

  obtenerTrazabilidad(idPerfil: number): Observable<EventoTrazabilidadPerfil[]> {
    return this.apiService.getTrazabilidadPerfil(idPerfil).pipe(
      map((historyList: any[]) => {
        if (!historyList || historyList.length === 0) return [];

        // 1. Ordenar cronológicamente (ascendente)
        const sortedList = [...historyList].sort((a, b) => {
          return new Date(a.fechaCambio).getTime() - new Date(b.fechaCambio).getTime();
        });

        // 2. Derivar iteración de forma controlada a partir de los ciclos reales
        // PERF-REV-AREA -> PERF-OBS-AREA -> PERF-COR-RRHH -> PERF-REV-AREA
        let currentIteration = 1;

        return sortedList.map((item, index) => {
          const estadoAnt = this.mapNombreEstadoToEnum(item.estadoAnterior);
          const estadoNvo = this.mapNombreEstadoToEnum(item.estadoNuevo);

          // Si el estado de destino es en corrección de RRHH (o PERF-COR-RRHH),
          // incrementamos la iteración para el ciclo de corrección actual.
          if (estadoNvo === EstadoPerfil.Corregida || item.estadoNuevo === 'PERF-COR-RRHH') {
            currentIteration++;
          }

          // Determinar tipo de actor y rol
          let tipoActor: 'SISTEMA' | 'AGENTE_IA' | 'USUARIO' = 'USUARIO';
          let actorRol = item.rol || 'Usuario';
          let actorNombre = item.cambiadoPor || 'Usuario';

          const rolClean = (item.rol || '').toLowerCase();
          const cambiadoClean = (item.cambiadoPor || '').toLowerCase();

          if (rolClean === 'n8n' || rolClean === 'n8n_automation' || cambiadoClean.includes('bot') || cambiadoClean.includes('ia')) {
            tipoActor = 'AGENTE_IA';
            actorRol = 'Agente Inteligente';
            actorNombre = item.cambiadoPor || 'Bot RRHH v2.4';
          } else if (rolClean === 'system' || rolClean === 'sistema' || cambiadoClean === 'sistema' || cambiadoClean === 'system') {
            tipoActor = 'SISTEMA';
            actorRol = 'Proceso Automático';
            actorNombre = 'Sistema';
          }

          const comentario = item.justificacion || 'Cambio de estado registrado por el sistema.';

          return {
            id: item.historyId || index + 1,
            estadoOrigen: estadoAnt,
            estadoDestino: estadoNvo || EstadoPerfil.EnRevisionRRHHPE,
            fechaHora: item.fechaCambio,
            actorNombre: actorNombre,
            actorRol: actorRol,
            tipoActor: tipoActor,
            iteracion: item.iteracion || currentIteration,
            comentario: comentario
          } as EventoTrazabilidadPerfil;
        });
      })
    );
  }

  private mapNombreEstadoToEnum(nombre: string): EstadoPerfil | null {
    if (!nombre) return null;
    const clean = nombre.trim().toLowerCase();
    
    // Mapeo por Códigos Técnicos
    if (clean === 'perf-sol-apr' || clean === 'solicitudaprobada') return EstadoPerfil.SolicitudAprobada;
    if (clean === 'perf-pen-gen' || clean === 'pendientegeneracionperfil') return EstadoPerfil.PendienteGeneracionPerfil;
    if (clean === 'perf-rev-rrhh' || clean === 'enrevisionrrhhpe') return EstadoPerfil.EnRevisionRRHHPE;
    if (clean === 'perf-res-gen' || clean === 'resumenejecutivogenerado') return EstadoPerfil.ResumenEjecutivoGenerado;
    if (clean === 'perf-rev-area' || clean === 'enrevisionareasol') return EstadoPerfil.EnRevisionAreaSol;
    if (clean === 'perf-obs-area' || clean === 'observada') return EstadoPerfil.Observada;
    if (clean === 'perf-cor-rrhh' || clean === 'corregida') return EstadoPerfil.Corregida;
    if (clean === 'perf-apr-area' || clean === 'aprobada') return EstadoPerfil.Aprobada;
    if (clean === 'perf-apr-fin' || clean === 'perfilaprobadofinal') return EstadoPerfil.PerfilAprobadoFinal;

    // Normalizar quitando acentos
    const norm = clean.normalize('NFD').replace(/[\u0300-\u036f]/g, '');

    if (norm.includes('solicitud aprobada')) return EstadoPerfil.SolicitudAprobada;
    if (norm.includes('generando perfil') || norm.includes('pendiente de generacion')) return EstadoPerfil.PendienteGeneracionPerfil;
    if (norm.includes('revision rrhh')) return EstadoPerfil.EnRevisionRRHHPE;
    if (norm.includes('resumen ejecutivo generado')) return EstadoPerfil.ResumenEjecutivoGenerado;
    if (norm.includes('revision area') || norm.includes('revision solicitante') || norm.includes('revision del area')) return EstadoPerfil.EnRevisionAreaSol;
    if (norm.includes('observado') || norm.includes('observacion')) return EstadoPerfil.Observada;
    if (norm.includes('corregido') || norm.includes('correccion rrhh') || norm.includes('corregida')) return EstadoPerfil.Corregida;
    if (norm.includes('aprobado por') || norm.includes('aprobado area') || norm.includes('aprobada')) return EstadoPerfil.Aprobada;
    if (norm.includes('aprobado final')) return EstadoPerfil.PerfilAprobadoFinal;

    return null;
  }

  obtenerObservaciones(idPerfil: number): Observable<ObservacionPerfil[]> {
    return this.observacionesSubject.asObservable().pipe(
      delay(300),
      map(obs => obs.filter(o => o.idPerfil === idPerfil))
    );
  }

  registrarObservacion(obs: any): Observable<ObservacionPerfil> {
    const lista = this.observacionesSubject.value;
    const nuevaObs: ObservacionPerfil = {
      ...obs,
      idObservacionPerfil: lista.length + 1,
      fechaRegistro: new Date().toISOString(),
      estado: EstadoObservacionPerfil.Pendiente
    };
    this.observacionesSubject.next([...lista, nuevaObs]);
    return of(nuevaObs).pipe(delay(200));
  }

  eliminarObservacion(id: number): Observable<boolean> {
    const lista = this.observacionesSubject.value;
    const filtrada = lista.filter(o => o.idObservacionPerfil !== id);
    this.observacionesSubject.next(filtrada);
    return of(true).pipe(delay(200));
  }

  guardarAjustesPerfil(id: number, secciones: SeccionPerfil[]): Observable<boolean> {
    const perfil = this.perfilesMap.get(id);
    if (!perfil) return of(false);

    perfil.secciones = JSON.parse(JSON.stringify(secciones));
    perfil.ultimaActualizacion = new Date().toISOString();
    
    // Si estaba observada y RRHH guarda ajustes, simulamos que transita a "Corregida"
    if (perfil.estado === EstadoPerfil.Observada) {
      perfil.estado = EstadoPerfil.Corregida;
    }

    this.perfilesMap.set(id, perfil);
    return of(true).pipe(delay(400));
  }

  actualizarEstadoPerfil(id: number, nuevoEstado: EstadoPerfil): Observable<boolean> {
    const perfil = this.perfilesMap.get(id);
    if (!perfil) return of(false);

    perfil.estado = nuevoEstado;
    perfil.ultimaActualizacion = new Date().toISOString();
    this.perfilesMap.set(id, perfil);
    return of(true).pipe(delay(300));
  }

  private parseMatrizPonderacion(val: any): any[] {
    if (!val) return [];
    if (Array.isArray(val)) {
      return val.map((item: any) => {
        if (item && typeof item === 'object') {
          const crit = item.criterio || item.Criterio || '';
          const weight = item.ponderacion ?? item.Ponderacion ?? item.peso ?? item.Peso ?? 0;
          return { criterio: crit, ponderacion: Number(weight) };
        }
        const str = String(item).trim();
        const parts = str.split(':');
        const crit = parts[0] ? parts[0].trim() : str;
        let weight = 0;
        if (parts[1]) {
          weight = parseInt(parts[1].replace('%', '').trim(), 10) || 0;
        }
        return { criterio: crit, ponderacion: weight };
      });
    }
    if (typeof val === 'string') {
      const trimmed = val.trim();
      if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
        try {
          const parsed = JSON.parse(trimmed);
          if (Array.isArray(parsed)) {
            return this.parseMatrizPonderacion(parsed);
          }
        } catch {}
      }
      // If it's a comma-separated string, e.g. "Experiencia: 25%, Conocimientos: 20%"
      const items = val.split(',');
      return items.map(item => {
        const parts = item.split(':');
        const crit = parts[0] ? parts[0].trim() : item.trim();
        let weight = 0;
        if (parts[1]) {
          weight = parseInt(parts[1].replace('%', '').trim(), 10) || 0;
        }
        return { criterio: crit, ponderacion: weight };
      }).filter(x => x.criterio);
    }
    return [];
  }
}
