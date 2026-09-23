import { Component, OnInit, inject, signal } from '@angular/core';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ApiService } from '../../core/services/api.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';

@Component({
  selector: 'app-vacantes',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    LoaderComponent
  ],
  templateUrl: './vacantes.component.html',
  styleUrls: ['./vacantes.component.scss']
})
export class VacantesComponent implements OnInit {
  private apiService = inject(ApiService);
  componentLoading = inject(ComponentLoadingService);

  vacantes = signal<any[]>([]);
  perfiles = signal<any[]>([]);
  isCreating = signal(false);
  isEditing = signal(false);
  selectedId = signal<number | null>(null);

  displayedColumns = ['id', 'perfilCargoId', 'cantidadPlazas', 'estado', 'acciones'];

  vacanteForm = new FormGroup({
    perfilCargoId: new FormControl<number | null>(null, [Validators.required]),
    cantidadPlazas: new FormControl<number | null>(null, [Validators.required, Validators.min(1)]),
    bandaSalarialMin: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    bandaSalarialMax: new FormControl<number | null>(null, [Validators.required, Validators.min(0)])
  });

  ngOnInit(): void {
    this.loadVacantes();
    this.loadPerfiles();
  }

  loadVacantes(): void {
    this.apiService.getVacantes(1, 20).subscribe({
      next: (res) => {
        this.vacantes.set(res.items || []);
      },
      error: () => {
        this.vacantes.set([
          { id: 1, perfilCargoId: 1, cantidadPlazas: 3, estadoNombre: 'Publicada', bandaSalarialMin: 5000, bandaSalarialMax: 8000 },
          { id: 2, perfilCargoId: 2, cantidadPlazas: 1, estadoNombre: 'Creada', bandaSalarialMin: 4000, bandaSalarialMax: 6000 }
        ]);
      }
    });
  }

  loadPerfiles(): void {
    this.apiService.getPerfiles(1, 100).subscribe({
      next: (res) => {
        this.perfiles.set(res.items || []);
      },
      error: () => {
        this.perfiles.set([
          { id: 1, solicitudId: 1, estadoNombre: 'Aprobado' },
          { id: 2, solicitudId: 2, estadoNombre: 'Creado' }
        ]);
      }
    });
  }

  startCreate(): void {
    this.vacanteForm.reset();
    this.isCreating.set(true);
    this.isEditing.set(false);
  }

  cancel(): void {
    this.isCreating.set(false);
    this.isEditing.set(false);
    this.selectedId.set(null);
  }

  onSubmit(): void {
    if (this.vacanteForm.invalid) return;

    const payload = this.vacanteForm.value;

    if (this.isEditing() && this.selectedId() !== null) {
      this.apiService.actualizarVacante(this.selectedId()!, payload).subscribe(() => {
        this.loadVacantes();
        this.cancel();
      });
    } else {
      this.apiService.crearVacante(payload).subscribe(() => {
        this.loadVacantes();
        this.cancel();
      });
    }
  }

  edit(element: any): void {
    this.selectedId.set(element.id);
    this.vacanteForm.patchValue({
      perfilCargoId: element.perfilCargoId,
      cantidadPlazas: element.cantidadPlazas,
      bandaSalarialMin: element.bandaSalarialMin,
      bandaSalarialMax: element.bandaSalarialMax
    });
    this.isEditing.set(true);
    this.isCreating.set(false);
  }

  publicar(element: any): void {
    this.apiService.publicarVacante(element.id).subscribe(() => {
      this.loadVacantes();
    });
  }

  cerrar(element: any): void {
    this.apiService.cerrarVacante(element.id).subscribe(() => {
      this.loadVacantes();
    });
  }

  pausar(element: any): void {
    this.apiService.pausarVacante(element.id).subscribe(() => {
      this.loadVacantes();
    });
  }

  reanudar(element: any): void {
    this.apiService.reanudarVacante(element.id).subscribe(() => {
      this.loadVacantes();
    });
  }
}
