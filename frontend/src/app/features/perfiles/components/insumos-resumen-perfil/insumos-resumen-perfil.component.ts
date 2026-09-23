import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InsumoResumenPerfil } from '../../models/resumen-ejecutivo-perfil.model';

@Component({
  selector: 'app-insumos-resumen-perfil',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './insumos-resumen-perfil.component.html',
  styleUrls: ['./insumos-resumen-perfil.component.scss']
})
export class InsumosResumenPerfilComponent {
  @Input({ required: true }) insumos: InsumoResumenPerfil[] = [];

  get insumosUtilizados(): InsumoResumenPerfil[] {
    return (this.insumos || []).filter(ins => ins.utilizado);
  }

  obtenerEtiquetaTipo(tipo: string): string {
    switch (tipo) {
      case 'SOLICITUD_VACANTE': return 'Solicitud de vacante';
      case 'PERFIL_ESTRUCTURADO': return 'Perfil estructurado previo';
      case 'MATRIZ_SKILLS': return 'Matriz de competencias';
      case 'DATO_HISTORICO': return 'Dato histórico';
      case 'MANUAL_FUNCIONES': return 'Manual de funciones';
      case 'PERFIL_SIMILAR': return 'Perfil similar';
      default: return 'Otros insumos';
    }
  }

  obtenerIconoTipo(tipo: string): string {
    switch (tipo) {
      case 'SOLICITUD_VACANTE': return 'description';
      case 'PERFIL_ESTRUCTURADO': return 'developer_board';
      case 'MATRIZ_SKILLS': return 'grid_on';
      case 'DATO_HISTORICO': return 'history';
      case 'MANUAL_FUNCIONES': return 'menu_book';
      case 'PERFIL_SIMILAR': return 'file_copy';
      default: return 'source';
    }
  }
}
