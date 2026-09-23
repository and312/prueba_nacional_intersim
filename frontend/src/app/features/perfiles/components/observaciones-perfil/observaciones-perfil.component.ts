import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ObservacionPerfil, TipoObservacion, EstadoObservacionPerfil } from '../../models/perfil.model';
import { ObservacionesPerfilService } from '../../services/observaciones-perfil.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-observaciones-perfil',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './observaciones-perfil.component.html',
  styleUrls: ['./observaciones-perfil.component.scss']
})
export class ObservacionesPerfilComponent implements OnInit, OnChanges {
  @Input({ required: true }) perfilId!: number;
  @Output() observacionesCambio = new EventEmitter<void>();

  private observacionesService = inject(ObservacionesPerfilService);
  private authService = inject(AuthService);

  observaciones = signal<ObservacionPerfil[]>([]);
  tiposObservacion = signal<TipoObservacion[]>([]);

  isSubmitting = signal(false);

  formularioObs = new FormGroup({
    tipoObservacionId: new FormControl<number | ''>('', [Validators.required]),
    comentario: new FormControl('', [Validators.required, Validators.minLength(5)])
  });

  ngOnInit(): void {
    this.cargarTiposObservacion();
    this.cargarObservaciones();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['perfilId']) {
      this.cargarObservaciones();
    }
  }

  cargarTiposObservacion(): void {
    this.observacionesService.obtenerTiposObservacionActivos().subscribe(list => {
      this.tiposObservacion.set(list || []);
    });
  }

  cargarObservaciones(): void {
    this.observacionesService.obtenerObservacionesPerfil(this.perfilId).subscribe(list => {
      this.observaciones.set(list || []);
    });
  }

  agregarObservacion(): void {
    if (this.formularioObs.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    const tipoObsId = Number(this.formularioObs.value.tipoObservacionId);
    const comentario = this.formularioObs.value.comentario || '';

    this.observacionesService.agregarObservacionPerfil({
      idPerfil: this.perfilId,
      idTipoObservacion: tipoObsId,
      comentario,
      estado: EstadoObservacionPerfil.Pendiente,
      idUsuarioSolicitante: this.authService.getUserId(),
      nombreUsuarioSolicitante: this.authService.getUserFullName()
    }).subscribe({
      next: () => {
        this.formularioObs.reset({
          tipoObservacionId: '',
          comentario: ''
        });
        this.cargarObservaciones();
        this.observacionesCambio.emit();
        this.isSubmitting.set(false);
      },
      error: (err) => {
        alert(err.error?.message || 'Error al agregar la observación.');
        this.isSubmitting.set(false);
      }
    });
  }

  eliminarObservacion(idObservacionPerfil: number): void {
    this.observacionesService.eliminarObservacionPerfil(idObservacionPerfil).subscribe(() => {
      this.cargarObservaciones();
      this.observacionesCambio.emit();
    });
  }
}
