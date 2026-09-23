import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { EventoTrazabilidadPerfil } from '../../models/perfil.model';
import { EstadoPerfil } from '../../models/estado-perfil.enum';
import { PerfilesService } from '../../services/perfiles.service';
import { CONFIGURACION_ESTADOS_PERFIL } from '../../constants/configuracion-estados-perfil';

export interface EventoTrazabilidadProcesado {
  id: number;
  titulo: string;
  categoria: 'estado-rev-rrhh' | 'estado-rev-area' | 'estado-observado' | 'estado-corregido' | 'estado-aprobado';
  fechaHora: string;
  accionTexto: string;
  actorNombre: string;
  actorRol: string;
  iteracion: number;
  icono: string;
}

@Component({
  selector: 'app-trazabilidad-perfil',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './trazabilidad-perfil.component.html',
  styleUrls: ['./trazabilidad-perfil.component.scss']
})
export class TrazabilidadPerfilComponent implements OnInit, OnChanges {
  @Input({ required: true }) perfilId!: number;
  @Input() estado?: string;

  private perfilesService = inject(PerfilesService);

  eventos = signal<EventoTrazabilidadPerfil[]>([]);

  // Procesamiento visual de trazabilidad diferenciando aprobación del área vs aprobación final de RRHH
  eventosProcesados = computed<EventoTrazabilidadProcesado[]>(() => {
    const rawList = this.eventos();
    if (!rawList || rawList.length === 0) return [];

    // 1. Ocultar únicamente registros del Administrador (permitiendo acciones automáticas del sistema/agente)
    const sinAdmin = rawList.filter(evt => {
      const rol = (evt.actorRol || '').toLowerCase();
      const nombre = (evt.actorNombre || '').toLowerCase();
      if (rol.includes('system') || rol.includes('sistema') || rol.includes('n8n') || rol.includes('proceso') || rol.includes('agente')) {
        return true;
      }
      return !rol.includes('admin') && !nombre.includes('admin');
    });

    if (sinAdmin.length === 0) return [];

    // 2. Ordenamiento secuencial lógico: dentro de un rango de 2 minutos,
    // colocar los eventos del AGENTE antes que los eventos de envío al área
    const sorted = [...sinAdmin].sort((a, b) => {
      const dateA = new Date(a.fechaHora || '').getTime();
      const dateB = new Date(b.fechaHora || '').getTime();
      
      if (Math.abs(dateA - dateB) < 120000) { // dentro de 2 minutos
        const aIsAgent = (a.comentario || '').toLowerCase().includes('resumen ejecutivo generado por el agente');
        const bIsAgent = (b.comentario || '').toLowerCase().includes('resumen ejecutivo generado por el agente');
        if (aIsAgent && !bIsAgent) return -1;
        if (!aIsAgent && bIsAgent) return 1;
      }
      return dateA - dateB;
    });

    // Deduplicar transiciones duplicadas (guardadas bajo "Perfil" y "PerfilCargo" a la vez)
    // Conservamos la que tiene un comentario descriptivo y descartamos la genérica "Transición de estado..."
    const deduped: any[] = [];
    for (const evt of sorted) {
      const comLower = (evt.comentario || '').toLowerCase();
      if (comLower.includes('transición de estado registrada por el sistema') || comLower.includes('transicion de estado registrada por el sistema')) {
        const dateEvt = new Date(evt.fechaHora || '').getTime();
        const hasCustom = sorted.some(other => {
          if (other === evt) return false;
          const otherDate = new Date(other.fechaHora || '').getTime();
          const otherComLower = (other.comentario || '').toLowerCase();
          return other.estadoDestino === evt.estadoDestino && 
                 Math.abs(otherDate - dateEvt) < 60000 && 
                 !otherComLower.includes('transición de estado') && 
                 !otherComLower.includes('transicion de estado');
        });
        if (hasCustom) {
          continue;
        }
      }
      deduped.push(evt);
    }

    const resultado: EventoTrazabilidadProcesado[] = [];

    let iteracionActual = 1;
    let cicloEnCorreccion = false;

    for (let i = 0; i < deduped.length; i++) {
      const evt = deduped[i];
      const prevEvt = resultado.length > 0 ? resultado[resultado.length - 1] : null;

      const dest = String(evt.estadoDestino || '').toLowerCase();
      const orig = String(evt.estadoOrigen || '').toLowerCase();
      const com = (evt.comentario || '').trim();
      const comLower = com.toLowerCase();
      const rol = String(evt.actorRol || '').toLowerCase();
      const nombre = String(evt.actorNombre || '').toLowerCase();

      const esSolicitante = rol.includes('solicitan') || nombre.includes('solicitan') || nombre.includes('pelaez');

      if (dest.includes('res-gen') || dest.includes('resumen') || dest.includes('resumengenerado') || dest.includes('resumen generado')) {
        continue;
      }

      let titulo = 'En revisión RRHH';
      let categoria: 'estado-rev-rrhh' | 'estado-rev-area' | 'estado-observado' | 'estado-corregido' | 'estado-aprobado' = 'estado-rev-rrhh';
      let accionTexto = '';
      let icono = 'edit_note';

      // 0. Creación inicial (PERF-PEN-GEN o primer paso)
      if (i === 0 || dest.includes('pen-gen') || dest.includes('pendiente de generacion') || dest.includes('pendientegeneracionperfil') || comLower.includes('creación de perfil pendiente')) {
        titulo = 'Perfil Estructurado generado por el AGENTE';
        categoria = 'estado-rev-rrhh';
        accionTexto = 'Perfil estructurado y resumen ejecutivo generado de forma automática.';
        icono = 'smart_toy';
      }
      // 0.1 Resumen Ejecutivo generado por el AGENTE (Azul suave)
      else if (comLower.includes('resumen ejecutivo generado por el agente')) {
        titulo = 'Resumen Ejecutivo generado por el AGENTE';
        categoria = 'estado-rev-rrhh';
        accionTexto = 'Resumen ejecutivo del perfil generado/actualizado automáticamente por el Agente de IA.';
        icono = 'smart_toy';
      }
      // 1. Aprobación Final por RRHH (Verde suave)
      else if (dest.includes('apr-fin') || dest.includes('aprobado final') || dest.includes('aprobadofinal') || dest.includes('perfilaprobadofinal') || dest === 'perf-apr-fin' || (comLower.includes('aprobación final') || comLower.includes('aprobado final'))) {
        titulo = 'Aprobación Final RRHH';
        categoria = 'estado-aprobado';
        accionTexto = 'Aprobación final del perfil completada por RRHH.';
        icono = 'verified';
      }
      // 2. Aprobado por Área Solicitante (Verde suave)
      else if (dest.includes('apr-area') || dest.includes('aprobado por area') || dest.includes('aprobada') || dest.includes('aprobado') || dest === 'perf-apr-area' || (esSolicitante && (dest.includes('apr') || dest.includes('aprob') || comLower.includes('aprob')))) {
        titulo = 'Perfil aprobado por el Área Solicitante';
        categoria = 'estado-aprobado';
        accionTexto = 'El Área Solicitante aprobó el perfil.';
        icono = 'thumb_up';
      }
      // 3. Observado por Área Solicitante (Naranja suave)
      else if (dest.includes('observado') || dest.includes('observada') || dest.includes('obs-area') || dest === 'perf-obs-area') {
        titulo = 'Observado por Área Solicitante';
        categoria = 'estado-observado';
        icono = 'warning';

        if (com && !comLower.includes('transición') && !comLower.includes('creación')) {
          accionTexto = com;
        } else {
          accionTexto = 'Se registraron observaciones sobre el perfil.';
        }
      }
      // 4. Perfil corregido por RRHH (Turquesa / Cyan suave)
      else if (comLower.includes('subsanó')) {
        titulo = 'Perfil corregido por RRHH';
        categoria = 'estado-corregido';
        icono = 'task_alt';
        accionTexto = com;
      }
      // 5. Enviado al Área Solicitante / En revisión del Área Solicitante (Lila suave)
      else if (dest.includes('area solicitante') || dest.includes('revision area') || dest.includes('rev-area') || dest.includes('revisionarea') || dest.includes('enrevisionareasol') || comLower.includes('enviado al área') || comLower.includes('enviado para revisión')) {
        titulo = 'En revisión del Área Solicitante';
        categoria = 'estado-rev-area';
        icono = 'rocket_launch';
        accionTexto = 'Perfil estructurado enviado para revisión del Área Solicitante.';
      }
      // 6. Observaciones resueltas por RRHH (Turquesa / Cyan suave)
      else if (orig.includes('obs') || dest.includes('cor') || dest.includes('corregid') || dest.includes('corregida') || dest.includes('correccion') || dest.includes('cor-rrhh') || comLower.includes('atend') || comLower.includes('resuelt')) {
        titulo = 'Observaciones resueltas por RRHH';
        categoria = 'estado-corregido';
        icono = 'task_alt';

        if (com && !comLower.includes('transición') && !comLower.includes('creación')) {
          accionTexto = com.toLowerCase().startsWith('rrhh') ? com : `RRHH subsanó las observaciones: ${com}.`;
        } else {
          accionTexto = 'RRHH subsanó las observaciones recibidas.';
        }
      }
      // 6. Resumen Ejecutivo Generado
      else if (dest.includes('res-gen') || dest.includes('resumen') || dest.includes('resumengenerado')) {
        titulo = 'Resumen Ejecutivo Generado';
        categoria = 'estado-rev-rrhh';
        accionTexto = 'Resumen ejecutivo del perfil generado.';
        icono = 'smart_toy';
      }
      // 7. En revisión RRHH (Azul suave)
      else {
        titulo = 'En revisión RRHH';
        categoria = 'estado-rev-rrhh';
        icono = 'edit_note';

        if (comLower.includes('generación inicial') || comLower.includes('generacion de perfil') || comLower.includes('generación de perfil')) {
          accionTexto = 'En revisión por RRHH para ajustes y correcciones.';
        } else if (com && !comLower.includes('transición') && !comLower.includes('creación')) {
          accionTexto = com;
        } else {
          accionTexto = 'Ajustes en el perfil realizados por RRHH.';
        }
      }

      // Control Dinámico de Iteración
      if (categoria === 'estado-observado') {
        cicloEnCorreccion = true;
      } else if (categoria === 'estado-rev-area' && cicloEnCorreccion) {
        iteracionActual++;
        cicloEnCorreccion = false;
      }

      // Evitar duplicados idénticos consecutivos
      if (prevEvt && prevEvt.titulo === titulo && prevEvt.actorNombre === evt.actorNombre && (accionTexto.includes('Ajustes en el perfil') || accionTexto.includes('Revisión inicial') || prevEvt.accionTexto === accionTexto)) {
        prevEvt.fechaHora = evt.fechaHora;
        continue;
      }

      resultado.push({
        id: evt.id,
        titulo,
        categoria,
        fechaHora: evt.fechaHora,
        accionTexto,
        actorNombre: evt.actorNombre,
        actorRol: evt.actorRol,
        iteracion: iteracionActual,
        icono
      });

      // Si el evento actual es "Observado por Área Solicitante", agregar automáticamente "En revisión RRHH" a continuación
      if (categoria === 'estado-observado') {
        const tieneSiguienteRevision = (i + 1 < sorted.length) && 
          (String(sorted[i + 1].estadoDestino || '').toLowerCase().includes('cor') || 
           String(sorted[i + 1].estadoDestino || '').toLowerCase().includes('rev-rrhh'));
        
        if (!tieneSiguienteRevision) {
          resultado.push({
            id: evt.id * 1000, // ID virtual
            titulo: 'En revisión RRHH',
            categoria: 'estado-rev-rrhh',
            fechaHora: evt.fechaHora,
            accionTexto: 'En revisión por RRHH para subsanar observaciones.',
            actorNombre: 'Sistema',
            actorRol: 'Proceso Automático',
            iteracion: iteracionActual,
            icono: 'edit_note'
          });
        }
      }
    }

    return resultado;
  });

  ngOnInit(): void {
    this.cargarTrazabilidad();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['perfilId'] || changes['estado']) {
      this.cargarTrazabilidad();
    }
  }

  cargarTrazabilidad(): void {
    this.perfilesService.obtenerTrazabilidad(this.perfilId).subscribe({
      next: (list) => {
        this.eventos.set(list || []);
      },
      error: () => {}
    });
  }

  obtenerNombreEstado(estado: string): string {
    const config = CONFIGURACION_ESTADOS_PERFIL[estado as EstadoPerfil];
    return config ? config.etiqueta : estado;
  }
}
