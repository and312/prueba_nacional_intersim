import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../../core/services/api.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './roles.component.html',
  styleUrls: ['./roles.component.scss']
})
export class RolesComponent implements OnInit {
  private apiService = inject(ApiService);

  roles = signal<any[]>([]);
  permisosDisponibles = signal<string[]>([]);
  isLoading = signal(false);
  isEditing = signal(false);
  selectedRolId = signal<number | null>(null);

  // Permisos seleccionados en el formulario
  permisosSeleccionados = new Set<string>();

  displayedColumns = ['nombre', 'descripcion', 'cantidadUsuarios', 'estado', 'acciones'];

  rolForm = new FormGroup({
    nombre: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    descripcion: new FormControl('', [Validators.maxLength(250)])
  });

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermisos();
  }

  loadRoles(): void {
    this.isLoading.set(true);
    this.apiService.getRoles().subscribe({
      next: (res) => {
        this.roles.set(res || []);
        this.isLoading.set(false);
      },
      error: () => {
        alert('Error al cargar los roles del sistema');
        this.isLoading.set(false);
      }
    });
  }

  loadPermisos(): void {
    this.apiService.getPermisosList().subscribe({
      next: (res) => {
        this.permisosDisponibles.set(res || []);
      },
      error: () => {
        console.error('Error al cargar la lista de permisos');
      }
    });
  }

  getGrupoPermiso(permiso: string): string {
    const parts = permiso.split('.');
    if (parts.length > 0) {
      const g = parts[0];
      return g.charAt(0).toUpperCase() + g.slice(1);
    }
    return 'General';
  }

  getNombrePermiso(permiso: string): string {
    const parts = permiso.split('.');
    if (parts.length > 1) {
      const n = parts[1];
      return n.charAt(0).toUpperCase() + n.slice(1);
    }
    return permiso;
  }

  getGruposPermisos(): string[] {
    const grupos = new Set<string>();
    this.permisosDisponibles().forEach(p => {
      grupos.add(this.getGrupoPermiso(p));
    });
    return Array.from(grupos);
  }

  getPermisosPorGrupo(grupo: string): string[] {
    return this.permisosDisponibles().filter(p => this.getGrupoPermiso(p) === grupo);
  }

  isPermisoSeleccionado(permiso: string): boolean {
    return this.permisosSeleccionados.has(permiso);
  }

  togglePermiso(permiso: string, checked: boolean): void {
    if (checked) {
      this.permisosSeleccionados.add(permiso);
    } else {
      this.permisosSeleccionados.delete(permiso);
    }
  }

  iniciarCrear(): void {
    this.isEditing.set(true);
    this.selectedRolId.set(null);
    this.rolForm.reset();
    this.permisosSeleccionados.clear();
  }

  iniciarEditar(rol: any): void {
    this.isEditing.set(true);
    this.selectedRolId.set(rol.rolId);
    this.rolForm.patchValue({
      nombre: rol.nombre,
      descripcion: rol.descripcion
    });
    this.permisosSeleccionados.clear();
    if (rol.permisos) {
      rol.permisos.forEach((p: string) => this.permisosSeleccionados.add(p));
    }
  }

  duplicar(rol: any): void {
    const nuevoNombre = prompt('Ingrese el nombre para el nuevo rol duplicado:', `${rol.nombre} - Copia`);
    if (nuevoNombre && nuevoNombre.trim() !== '') {
      this.isLoading.set(true);
      this.apiService.duplicarRol(rol.rolId, nuevoNombre.trim()).subscribe({
        next: () => {
          this.loadRoles();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al duplicar el rol');
          this.isLoading.set(false);
        }
      });
    }
  }

  cancelarEdicion(): void {
    this.isEditing.set(false);
    this.selectedRolId.set(null);
    this.rolForm.reset();
    this.permisosSeleccionados.clear();
  }

  guardar(): void {
    if (this.rolForm.invalid) return;

    const formVal = this.rolForm.value;
    const body = {
      nombre: formVal.nombre,
      descripcion: formVal.descripcion,
      permisoCodigos: Array.from(this.permisosSeleccionados)
    };

    this.isLoading.set(true);

    if (this.selectedRolId()) {
      // Editar
      this.apiService.actualizarRol(this.selectedRolId()!, body).subscribe({
        next: () => {
          this.isEditing.set(false);
          this.loadRoles();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al actualizar el rol');
          this.isLoading.set(false);
        }
      });
    } else {
      // Crear
      this.apiService.crearRol(body).subscribe({
        next: () => {
          this.isEditing.set(false);
          this.loadRoles();
        },
        error: (err) => {
          alert(err.error?.message || 'Error al crear el rol');
          this.isLoading.set(false);
        }
      });
    }
  }
}
