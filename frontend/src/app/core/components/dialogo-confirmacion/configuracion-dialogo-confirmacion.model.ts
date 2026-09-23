import { Observable } from 'rxjs';
import { VarianteDialogoConfirmacion } from './variante-dialogo-confirmacion.type';

export interface ResumenCompactoConfirmacion {
  codigo: string;
  cargo: string;
  solicitudOrigen: string;
  area: string;
  version: string;
}

export interface ConfiguracionDialogoConfirmacion {
  titulo: string;
  descripcion: string;
  icono?: string;
  variante: VarianteDialogoConfirmacion;
  
  // Elementos de resumen compacto
  resumen?: ResumenCompactoConfirmacion;

  // Checkbox opcional
  requiereCheckbox?: boolean;
  textoCheckbox?: string;

  // Acciones
  textoBotonPrincipal?: string;
  textoBotonCancelar?: string;

  // Callback asincrono para confirmacion con control de carga
  onConfirmar?: () => Observable<any> | Promise<any>;
}
