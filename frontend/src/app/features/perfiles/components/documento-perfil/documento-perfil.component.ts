import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { DetallePerfil, PerfilDocumento } from '../../models/perfil.model';

@Component({
  selector: 'app-documento-perfil',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './documento-perfil.component.html',
  styleUrls: ['./documento-perfil.component.scss']
})
export class DocumentoPerfilComponent {
  @Input({ required: true }) perfil!: DetallePerfil;
  @Input() tipoDocumento: 'RESUMEN_EJECUTIVO_PDF' | 'PERFIL_ESTRUCTURADO_PDF' = 'PERFIL_ESTRUCTURADO_PDF';
  @Input() titulo: string = 'Documentación del perfil';
  @Input() descripcion: string = 'Consulte y descargue el profesiograma completo del cargo en formato oficial PDF.';

  @Output() abrirDocumento = new EventEmitter<PerfilDocumento | null>();

  get documento(): PerfilDocumento | undefined {
    if (!this.perfil || !this.perfil.documentos) return undefined;
    return this.perfil.documentos.find(d => d.tipoDocumento === this.tipoDocumento);
  }

  get tieneDocumento(): boolean {
    return !!this.documento;
  }

  get fileName(): string {
    const doc = this.documento;
    if (doc?.fileName) return doc.fileName;
    if (doc?.nombre) return doc.nombre;
    return this.tipoDocumento === 'RESUMEN_EJECUTIVO_PDF' 
      ? `Resumen_Ejecutivo_${this.perfil?.codigo || ''}.pdf`
      : `Profesiograma_${this.perfil?.codigo || ''}.pdf`;
  }

  get fechaGeneracion(): string | Date | null {
    const doc = this.documento;
    if (doc?.createdDate) return doc.createdDate;
    if (doc?.fechaCreacion) return doc.fechaCreacion;
    return null;
  }

  get estadoGeneracion(): string {
    const doc = this.documento;
    if (doc?.generadoPor) {
      return `Generado por ${doc.generadoPor}`;
    }
    if (doc) {
      return 'Generado automáticamente';
    }
    const est = (this.perfil?.estadoCodigo || this.perfil?.estado || '').toLowerCase();
    if (est.includes('rech')) return 'No generado por rechazo';
    if (est.includes('obs')) return 'Pausado por observación';
    if (est.includes('can')) return 'No generado por cancelación';
    return 'En proceso de validación...';
  }

  get colorEstado(): string {
    if (this.tieneDocumento) return '#047857';
    const est = (this.perfil?.estadoCodigo || this.perfil?.estado || '').toLowerCase();
    if (est.includes('rech')) return '#dc2626';
    if (est.includes('obs')) return '#d97706';
    if (est.includes('can')) return '#64748b';
    return '#d97706';
  }

  get fechaGeneracionTexto(): string {
    if (this.tieneDocumento) return '';
    const est = (this.perfil?.estadoCodigo || this.perfil?.estado || '').toLowerCase();
    if (est.includes('rech') || est.includes('can')) return 'No aplica';
    if (est.includes('obs')) return 'Pendiente de subsanación';
    return 'Pendiente';
  }

  get tooltipTexto(): string {
    if (this.tieneDocumento) return 'Abrir documento PDF';
    const est = (this.perfil?.estadoCodigo || this.perfil?.estado || '').toLowerCase();
    if (est.includes('rech')) return 'No se generó documento PDF debido a que el proceso fue rechazado';
    if (est.includes('obs')) return 'El documento estará disponible al subsanar y aprobar el perfil';
    if (est.includes('can')) return 'No se generó documento PDF debido a que el proceso fue cancelado';
    return 'Se generará automáticamente tras la aprobación';
  }

  onAbrirClick(event: Event): void {
    event.preventDefault();
    if (!this.tieneDocumento) return;
    this.abrirDocumento.emit(this.documento || null);
  }
}
