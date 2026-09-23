import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  FormGroup, FormControl, Validators,
  ReactiveFormsModule, FormsModule
} from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    LoaderComponent
  ],
  templateUrl: './usuarios.component.html',
  styleUrls: ['./usuarios.component.scss']
})
export class UsuariosComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private router     = inject(Router);
  componentLoading = inject(ComponentLoadingService);

  // ── Vista ────────────────────────────────────────────────────
  viewMode       = signal<'list' | 'form' | 'detail'>('list');
  isEditMode     = signal(false);
  isLoading      = signal(false);
  errorMessage   = signal<string | null>(null);

  // ── Datos ────────────────────────────────────────────────────
  usuarios       = signal<any[]>([]);
  areas          = signal<any[]>([]);
  rolesList      = signal<any[]>([]);
  selectedUsuario = signal<any | null>(null);
  cargosList     = signal<string[]>([
    'Administrador de Sistemas',
    'Jefe de Recursos Humanos',
    'Jefe de Reclutamiento',
    'Jefe de Finanzas',
    'Analista de Reclutamiento',
    'Analista de Recursos Humanos',
    'Analista Actuarial',
    'Ejecutivo Comercial',
    'Asistente Administrativo',
    'Auditor Interno',
    'Decisor de Solicitudes',
    'Solicitante de Personal'
  ]);

  // ── Paginación ───────────────────────────────────────────────
  totalCount     = signal(0);
  pageSize       = signal(10);
  pageIndex      = signal(0);
  searchQuery    = signal('');

  // ── Filtros (propiedades normales para [(ngModel)]) ──────────
  filterArea:    number | null = null;
  filterRol:     number | null = null;
  filterEstado:  string = '';
  filterCargo:   string = '';
  filterGerencia: string = '';

  displayedColumns = [
    'fotografia', 'nombreCompleto', 'correo',
    'area', 'cargo', 'rol', 'estado', 'fechaCreacion', 'acciones'
  ];

  // ── Formulario ───────────────────────────────────────────────
  usuarioForm = new FormGroup({
    nombres:           new FormControl('',   [Validators.required, Validators.maxLength(100)]),
    apellidos:         new FormControl('',   [Validators.required, Validators.maxLength(100)]),
    correo:            new FormControl('',   [Validators.required, Validators.email, Validators.maxLength(100)]),
    tipoAutenticacion: new FormControl('Local', [Validators.required]),
    clave:             new FormControl(''),
    areaId:            new FormControl<number | null>(null, [Validators.required]),
    cargo:             new FormControl('',   [Validators.required, Validators.maxLength(100)]),
    gerencia:          new FormControl('',   [Validators.maxLength(100)]),
    telefono:          new FormControl('',   [Validators.maxLength(20)]),
    extension:         new FormControl('',   [Validators.maxLength(10)]),
    estado:            new FormControl('Activo', [Validators.required]),
    observaciones:     new FormControl('',   [Validators.maxLength(500)]),
    fotografiaUrl:     new FormControl(''),
    rolIds:            new FormControl<number[]>([], [Validators.required])
  });

  // ── Lifecycle ────────────────────────────────────────────────
  ngOnInit(): void {
    const rol = this.authService.getUserRole();
    console.log('[UsuariosComponent] Rol del usuario actual:', rol);
    console.log('[UsuariosComponent] Token en localStorage:', !!localStorage.getItem('token'));
    if (rol !== 'Administrador') {
      this.errorMessage.set(
        `Acceso denegado. Tu rol actual es "${rol || 'Sin rol'}". ` +
        `Se requiere el rol Administrador para gestionar usuarios. ` +
        `Inicia sesión con una cuenta de Administrador.`
      );
      return;
    }
    this.loadUsuarios();
    this.loadAreas();
    this.loadRoles();

    // Autocargar gerencia al seleccionar el área
    this.usuarioForm.get('areaId')?.valueChanges.subscribe(areaId => {
      if (areaId) {
        const areaObj = this.areas().find(a => a.areaId === areaId);
        if (areaObj) {
          this.usuarioForm.get('gerencia')?.setValue(areaObj.gerencia || '');
        }
      } else {
        this.usuarioForm.get('gerencia')?.setValue('');
      }
    });
  }

  // ── Carga de datos ───────────────────────────────────────────
  loadUsuarios(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.apiService.getUsuarios(
      this.pageIndex() + 1,
      this.pageSize(),
      this.searchQuery(),
      this.filterArea  || undefined,
      this.filterRol   || undefined,
      this.filterEstado || undefined,
      this.filterCargo  || undefined,
      this.filterGerencia || undefined
    ).subscribe({
      next: (res) => {
        this.usuarios.set(res.items   || []);
        this.totalCount.set(res.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 401) {
          this.authService.logout();
          this.router.navigate(['/login']);
        } else if (err.status === 403) {
          this.errorMessage.set(
            'No tienes permisos para ver la lista de usuarios. Se requiere el rol Administrador.'
          );
        } else {
          this.errorMessage.set(
            `Error ${err.status || ''} al cargar usuarios. Verifica la conexión con el servidor.`
          );
        }
      }
    });
  }

  loadAreas(): void {
    this.apiService.getAreas().subscribe({
      next: (res) => this.areas.set(res || []),
      error: () => { /* no critical */ }
    });
  }

  loadRoles(): void {
    this.apiService.getRoles().subscribe({
      next: (res) => this.rolesList.set(res || []),
      error: () => { /* no critical */ }
    });
  }

  // ── Filtros ──────────────────────────────────────────────────
  onSearch(event: Event): void {
    this.searchQuery.set((event.target as HTMLInputElement).value);
    this.pageIndex.set(0);
    this.loadUsuarios();
  }

  aplicarFiltros(): void {
    this.pageIndex.set(0);
    this.loadUsuarios();
  }

  limpiarFiltros(): void {
    this.filterArea    = null;
    this.filterRol     = null;
    this.filterEstado  = '';
    this.filterCargo   = '';
    this.filterGerencia = '';
    this.searchQuery.set('');
    this.pageIndex.set(0);
    this.loadUsuarios();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.loadUsuarios();
  }

  // ── Navegación ───────────────────────────────────────────────
  irAlLogin(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // ── Formulario: Crear ────────────────────────────────────────
  iniciarCrear(): void {
    this.isEditMode.set(false);
    this.viewMode.set('form');
    this.usuarioForm.reset({ tipoAutenticacion: 'Local', estado: 'Activo', rolIds: [] });
    this.usuarioForm.get('correo')?.enable();
    this.usuarioForm.get('clave')?.setValidators([Validators.required, Validators.minLength(4)]);
    this.usuarioForm.get('clave')?.updateValueAndValidity();
  }

  // ── Formulario: Editar ───────────────────────────────────────
  iniciarEditar(user: any): void {
    this.isEditMode.set(true);
    this.viewMode.set('form');
    this.selectedUsuario.set(user);

    const userRolIds = this.rolesList()
      .filter(r => user.roles?.includes(r.nombre))
      .map(r => r.rolId);

    this.usuarioForm.patchValue({
      nombres: user.nombres, apellidos: user.apellidos, correo: user.correo,
      tipoAutenticacion: user.tipoAutenticacion, areaId: user.areaId,
      cargo: user.cargo, gerencia: user.gerencia, telefono: user.telefono,
      extension: user.extension, estado: user.estado,
      observaciones: user.observaciones, fotografiaUrl: user.fotografiaUrl,
      rolIds: userRolIds
    });

    this.usuarioForm.get('correo')?.enable();
    this.usuarioForm.get('clave')?.clearValidators();
    this.usuarioForm.get('clave')?.updateValueAndValidity();
  }

  // ── Rol helpers ──────────────────────────────────────────────
  isRolSelected(rolId: number): boolean {
    return (this.usuarioForm.get('rolIds')?.value as number[] || []).includes(rolId);
  }

  toggleRol(rolId: number): void {
    const current = (this.usuarioForm.get('rolIds')?.value as number[] || []);
    const updated = current.includes(rolId) ? [] : [rolId];
    this.usuarioForm.patchValue({ rolIds: updated });
  }

  // ── Detalle ──────────────────────────────────────────────────
  verDetalle(user: any): void {
    this.isLoading.set(true);
    this.apiService.getUsuarioById(user.usuarioId).subscribe({
      next: (res) => {
        this.selectedUsuario.set(res);
        this.viewMode.set('detail');
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        alert('Error al obtener el detalle del usuario.');
      }
    });
  }

  cancelar(): void {
    this.viewMode.set('list');
    this.selectedUsuario.set(null);
    this.usuarioForm.reset();
  }

  // ── Guardar ──────────────────────────────────────────────────
  guardar(): void {
    if (this.usuarioForm.invalid) {
      this.usuarioForm.markAllAsTouched();
      return;
    }
    const formVal = this.usuarioForm.getRawValue();
    if (formVal.correo) formVal.correo = formVal.correo.trim();
    if (formVal.nombres) formVal.nombres = formVal.nombres.trim();
    if (formVal.apellidos) formVal.apellidos = formVal.apellidos.trim();
    
    this.isLoading.set(true);

    const req$ = this.isEditMode()
      ? this.apiService.actualizarUsuario(this.selectedUsuario()!.usuarioId, formVal)
      : this.apiService.crearUsuario(formVal);

    req$.subscribe({
      next: () => { this.viewMode.set('list'); this.loadUsuarios(); },
      error: (err) => {
        this.isLoading.set(false);
        alert(err.error?.message || 'Error al guardar el usuario.');
      }
    });
  }

  // ── Cambiar Estado ───────────────────────────────────────────
  cambiarEstado(user: any, nuevoEstado: string): void {
    const accion = nuevoEstado === 'Activo' ? 'activar' : nuevoEstado === 'Inactivo' ? 'desactivar' : 'bloquear';
    if (!confirm(`¿Desea ${accion} la cuenta de ${user.nombres}?`)) return;

    this.isLoading.set(true);
    this.apiService.cambiarEstadoUsuario(user.usuarioId, nuevoEstado).subscribe({
      next: () => this.loadUsuarios(),
      error: (err) => {
        this.isLoading.set(false);
        alert(err.error?.message || 'Error al cambiar el estado.');
      }
    });
  }

  // ── Reset Password ───────────────────────────────────────────
  resetPassword(user: any): void {
    const nuevaClave = prompt(`Nueva contraseña para ${user.nombres} ${user.apellidos}:`);
    if (!nuevaClave?.trim()) return;

    this.isLoading.set(true);
    this.apiService.resetPasswordUsuario(user.usuarioId, nuevaClave.trim()).subscribe({
      next: () => { this.isLoading.set(false); alert('Contraseña restablecida exitosamente.'); },
      error: (err) => {
        this.isLoading.set(false);
        alert(err.error?.message || 'Error al restablecer la contraseña.');
      }
    });
  }

  // ── Image error fallback ─────────────────────────────────────
  onImgError(event: Event): void {
    (event.target as HTMLImageElement).src =
      'https://ui-avatars.com/api/?name=NS&background=004370&color=fff&bold=true';
  }
}
