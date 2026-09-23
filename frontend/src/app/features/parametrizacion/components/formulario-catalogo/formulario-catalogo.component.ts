import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AreaCargo } from '../../models/area-cargo.model';

@Component({
  selector: 'app-formulario-catalogo',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './formulario-catalogo.component.html',
  styleUrls: ['./formulario-catalogo.component.scss']
})
export class FormularioCatalogoComponent implements OnInit {
  @Input() nombreInicial = '';
  @Input() esEdicion = false;
  @Input() nombresExistentes: string[] = [];
  @Input() esCargo = false;
  @Input() areaCargoIdInicial: number | null = null;
  @Input() listadoAreasCargo: AreaCargo[] = [];

  @Output() guardar = new EventEmitter<{ nombre: string; areaCargoId?: number }>();
  @Output() cancelar = new EventEmitter<void>();

  formulario = new FormGroup({
    nombre: new FormControl('', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(100)
    ]),
    areaCargoId: new FormControl<number | null>(null)
  });

  ngOnInit(): void {
    if (this.esCargo) {
      this.formulario.controls.areaCargoId.setValidators([Validators.required]);
      const defaultAreaId = this.areaCargoIdInicial || (this.listadoAreasCargo.length > 0 ? this.listadoAreasCargo[0].idAreaCargo : null);
      this.formulario.patchValue({
        nombre: this.nombreInicial || '',
        areaCargoId: defaultAreaId
      });
    } else {
      this.formulario.patchValue({ nombre: this.nombreInicial || '' });
    }
  }

  get errorDuplicado(): boolean {
    const valor = (this.formulario.value.nombre || '').trim().toLowerCase();
    if (!valor) return false;

    // Si estamos editando y el valor no ha cambiado, no es error de duplicado
    if (this.esEdicion && valor === this.nombreInicial.trim().toLowerCase()) {
      return false;
    }

    return this.nombresExistentes.some(n => n.trim().toLowerCase() === valor);
  }

  get esFormularioInvalido(): boolean {
    if (this.esCargo && !this.formulario.value.areaCargoId) {
      return true;
    }
    return this.formulario.invalid || this.errorDuplicado || !(this.formulario.value.nombre || '').trim();
  }

  onSubmit(): void {
    if (this.esFormularioInvalido) return;
    const valorLimpio = (this.formulario.value.nombre || '').trim();
    const areaId = this.esCargo ? Number(this.formulario.value.areaCargoId) : undefined;
    this.guardar.emit({ nombre: valorLimpio, areaCargoId: areaId });
  }

  onCancelar(): void {
    this.cancelar.emit();
  }
}

