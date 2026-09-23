import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-areas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './areas.component.html',
  styleUrls: ['./areas.component.scss']
})
export class AreasComponent implements OnInit {
  private apiService = inject(ApiService);

  areas = signal<any[]>([]);
  gerencias = signal<any[]>([]);
  isLoading = signal(false);
  
  // Modos de vista: 'list' | 'form' | 'detail'
  viewMode = signal<'list' | 'form' | 'detail'>('list');
  selectedAreaId = signal<number | null>(null);
  selectedArea = signal<any | null>(null);

  // Filtros locales
  filterGerencia = signal<string>('');
  filterEstado = signal<string>('');
  searchQuery = signal<string>('');

  // Señal computada para filtrar las áreas de forma reactiva
  filteredAreas = computed(() => {
    return this.areas().filter(a => {
      const matchesSearch = !this.searchQuery() || 
        (a.nombre || '').toLowerCase().includes(this.searchQuery().toLowerCase()) ||
        (a.codigo || '').toLowerCase().includes(this.searchQuery().toLowerCase()) ||
        (a.responsable || '').toLowerCase().includes(this.searchQuery().toLowerCase());
      
      const matchesGerencia = !this.filterGerencia() || 
        (a.gerencia === this.filterGerencia());

      const matchesEstado = !this.filterEstado() || 
        (a.estado === this.filterEstado());

      return matchesSearch && matchesGerencia && matchesEstado;
    });
  });

  areaForm = new FormGroup({
    codigo: new FormControl('', [Validators.required, Validators.maxLength(20)]),
    nombre: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    gerenciaId: new FormControl<number | null>(null, [Validators.required]),
    responsable: new FormControl('', [Validators.email, Validators.maxLength(100)]),
    estado: new FormControl('Activo', [Validators.required])
  });

  ngOnInit(): void {
    this.loadGerencias();
    this.loadAreas();
  }

  loadGerencias(): void {
    this.apiService.getGerencias().subscribe({
      next: (res) => {
        this.gerencias.set(res || []);
      },
      error: () => {
        console.error('Error al cargar catálogo de gerencias.');
      }
    });
  }

  loadAreas(): void {
    this.isLoading.set(true);
    this.apiService.getAreas().subscribe({
      next: (res) => {
        this.areas.set(res || []);
        this.isLoading.set(false);
      },
      error: () => {
        alert('Error al cargar las áreas organizacionales');
        this.isLoading.set(false);
      }
    });
  }

  iniciarCrear(): void {
    this.viewMode.set('form');
    this.selectedAreaId.set(null);
    this.areaForm.reset({ estado: 'Activo', gerenciaId: null });
    this.areaForm.get('codigo')?.enable();
  }

  iniciarEditar(area: any): void {
    this.viewMode.set('form');
    this.selectedAreaId.set(area.areaId);
    this.areaForm.patchValue({
      codigo: area.codigo,
      nombre: area.nombre,
      gerenciaId: area.gerenciaId,
      responsable: area.responsable,
      estado: area.estado
    });
    this.areaForm.get('codigo')?.disable(); // El código no se puede editar
  }

  verDetalle(area: any): void {
    this.selectedArea.set(area);
    this.viewMode.set('detail');
  }

  cancelarEdicion(): void {
    this.viewMode.set('list');
    this.selectedAreaId.set(null);
    this.areaForm.reset();
  }

  limpiarFiltros(): void {
    this.filterGerencia.set('');
    this.filterEstado.set('');
    this.searchQuery.set('');
  }

  guardar(): void {
    if (this.areaForm.invalid) return;

    const formVal = this.areaForm.getRawValue();
    this.isLoading.set(true);

    if (this.selectedAreaId()) {
      // Editar
      this.apiService.actualizarArea(this.selectedAreaId()!, formVal).subscribe({
        next: () => {
          this.viewMode.set('list');
          this.loadAreas();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al actualizar el área');
          this.isLoading.set(false);
        }
      });
    } else {
      // Crear
      this.apiService.crearArea(formVal).subscribe({
        next: () => {
          this.viewMode.set('list');
          this.loadAreas();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al crear el área');
          this.isLoading.set(false);
        }
      });
    }
  }

  eliminar(areaId: number): void {
    if (confirm('¿Está seguro de que desea eliminar esta área?')) {
      this.isLoading.set(true);
      this.apiService.eliminarArea(areaId).subscribe({
        next: () => {
          this.loadAreas();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al eliminar el área. Verifique si tiene usuarios asociados.');
          this.isLoading.set(false);
        }
      });
    }
  }
}
