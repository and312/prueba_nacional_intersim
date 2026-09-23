import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormGroup, FormControl, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-integraciones',
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
    MatTabsModule,
    MatCheckboxModule
  ],
  templateUrl: './integraciones.component.html',
  styleUrls: ['./integraciones.component.scss']
})
export class IntegracionesComponent implements OnInit {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/integraciones';

  // ── Pestañas y Vista ─────────────────────────────────────────
  activeTab = signal<number>(0);
  viewMode = signal<'list' | 'form' | 'detail'>('list');
  isEditMode = signal(false);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  // ── Datos de Integraciones ────────────────────────────────────
  integraciones = signal<any[]>([]);
  totalIntegraciones = signal(0);
  pageSizeIntegraciones = signal(10);
  pageIndexIntegraciones = signal(0);
  searchIntegraciones = signal('');
  selectedIntegracion = signal<any | null>(null);

  // ── Datos de API Keys ────────────────────────────────────────
  apiKeys = signal<any[]>([]);
  selectedKey = signal<any | null>(null);
  plainKeyGenerated = signal<string | null>(null);
  mostrarModalKey = signal(false);
  permisosDisponibles = signal<any[]>([]);
  permisosSeleccionados = signal<number[]>([]);
  historialCambios = signal<any[]>([]);

  // ── Datos de Auditoría de Uso ────────────────────────────────
  auditorias = signal<any[]>([]);
  totalAuditorias = signal(0);
  pageSizeAuditorias = signal(20);
  pageIndexAuditorias = signal(0);
  searchAuditorias = signal('');

  // ── Formularios ──────────────────────────────────────────────
  integracionForm = new FormGroup({
    nombre: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    codigo: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    descripcion: new FormControl('', [Validators.maxLength(500)]),
    tipo: new FormControl('Workflow', [Validators.required]),
    responsable: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    correoResponsable: new FormControl('', [Validators.required, Validators.email, Validators.maxLength(100)]),
    observaciones: new FormControl('', [Validators.maxLength(1000)])
  });

  apiKeyForm = new FormGroup({
    nombre: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    descripcion: new FormControl('', [Validators.maxLength(250)]),
    workflow: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    fechaExpiracion: new FormControl(''),
    observaciones: new FormControl('', [Validators.maxLength(1000)])
  });

  rotarForm = new FormGroup({
    fechaExpiracion: new FormControl('')
  });

  ngOnInit(): void {
    this.cargarIntegraciones();
    this.cargarPermisosCatalog();
    this.cargarAuditorias();
  }

  // ==========================================
  // 1. GESTIÓN DE INTEGRACIONES
  // ==========================================

  cargarIntegraciones(): void {
    this.isLoading.set(true);
    const params: any = {
      pageNumber: this.pageIndexIntegraciones() + 1,
      pageSize: this.pageSizeIntegraciones()
    };
    if (this.searchIntegraciones()) {
      params.search = this.searchIntegraciones();
    }

    this.http.get<any>(this.apiUrl, { params }).subscribe({
      next: (res) => {
        this.integraciones.set(res.items || []);
        this.totalIntegraciones.set(res.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.showError('Error al cargar integraciones.');
        this.isLoading.set(false);
      }
    });
  }

  buscarIntegraciones(): void {
    this.pageIndexIntegraciones.set(0);
    this.cargarIntegraciones();
  }

  onPageChangeIntegraciones(event: PageEvent): void {
    this.pageIndexIntegraciones.set(event.pageIndex);
    this.pageSizeIntegraciones.set(event.pageSize);
    this.cargarIntegraciones();
  }

  nuevaIntegracion(): void {
    this.isEditMode.set(false);
    this.integracionForm.reset({
      tipo: 'Workflow',
      nombre: '',
      codigo: '',
      descripcion: '',
      responsable: '',
      correoResponsable: '',
      observaciones: ''
    });
    this.integracionForm.get('codigo')?.enable();
    this.viewMode.set('form');
  }

  editarIntegracion(integracion: any): void {
    this.isEditMode.set(true);
    this.selectedIntegracion.set(integracion);
    this.integracionForm.reset({
      nombre: integracion.nombre,
      codigo: integracion.codigo,
      descripcion: integracion.descripcion,
      tipo: integracion.tipo,
      responsable: integracion.responsable,
      correoResponsable: integracion.correoResponsable,
      observaciones: integracion.observaciones
    });
    this.integracionForm.get('codigo')?.disable();
    this.viewMode.set('form');
  }

  verDetalleIntegracion(integracion: any): void {
    this.selectedIntegracion.set(integracion);
    this.cargarApiKeys(integracion.id);
    this.viewMode.set('detail');
  }

  cancelarForm(): void {
    this.viewMode.set('list');
  }

  guardarIntegracion(): void {
    if (this.integracionForm.invalid) {
      this.integracionForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const body = this.integracionForm.getRawValue();

    if (this.isEditMode()) {
      const id = this.selectedIntegracion().id;
      this.http.put<any>(`${this.apiUrl}/${id}`, body).subscribe({
        next: (res) => {
          this.isLoading.set(false);
          this.viewMode.set('list');
          this.cargarIntegraciones();
        },
        error: (err) => {
          this.showError('Error al guardar la integración.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.http.post<any>(this.apiUrl, body).subscribe({
        next: (res) => {
          this.isLoading.set(false);
          this.viewMode.set('list');
          this.cargarIntegraciones();
        },
        error: (err) => {
          this.showError(err.error?.message || 'Error al crear la integración.');
          this.isLoading.set(false);
        }
      });
    }
  }

  cambiarEstadoIntegracion(integracion: any, estado: string): void {
    this.isLoading.set(true);
    this.http.put<any>(`${this.apiUrl}/${integracion.id}/estado`, { estado }).subscribe({
      next: () => {
        this.cargarIntegraciones();
        if (this.selectedIntegracion()?.id === integracion.id) {
          this.selectedIntegracion.update(i => i ? { ...i, estado } : null);
        }
      },
      error: () => this.showError('Error al cambiar el estado.'),
      complete: () => this.isLoading.set(false)
    });
  }

  eliminarIntegracion(integracion: any): void {
    if (!confirm(`¿Está seguro de eliminar la integración "${integracion.nombre}"? Esto revocará de forma lógica todas sus API Keys.`)) {
      return;
    }
    this.isLoading.set(true);
    this.http.delete<any>(`${this.apiUrl}/${integracion.id}`).subscribe({
      next: () => {
        this.cargarIntegraciones();
        this.viewMode.set('list');
      },
      error: () => this.showError('Error al eliminar la integración.'),
      complete: () => this.isLoading.set(false)
    });
  }

  // ==========================================
  // 2. GESTIÓN DE API KEYS
  // ==========================================

  cargarApiKeys(integracionId: number): void {
    this.http.get<any[]>(`${this.apiUrl}/${integracionId}/keys`).subscribe({
      next: (res) => this.apiKeys.set(res),
      error: () => this.showError('Error al cargar API Keys.')
    });
  }

  cargarPermisosCatalog(): void {
    this.http.get<any[]>(`${this.apiUrl}/permisos`).subscribe({
      next: (res) => this.permisosDisponibles.set(res),
      error: () => this.showError('Error al cargar el catálogo de permisos.')
    });
  }

  cambiarPestana(index: number): void {
    this.activeTab.set(index);
    if (index === 0) {
      this.cargarIntegraciones();
    } else if (index === 2) {
      this.cargarAuditorias();
    }
  }

  seleccionarPermiso(id: number, event: any): void {
    const arr = [...this.permisosSeleccionados()];
    if (event.checked) {
      arr.push(id);
    } else {
      const idx = arr.indexOf(id);
      if (idx > -1) arr.splice(idx, 1);
    }
    this.permisosSeleccionados.set(arr);
  }

  crearApiKey(): void {
    this.apiKeyForm.reset({
      nombre: '',
      descripcion: '',
      workflow: '',
      fechaExpiracion: '',
      observaciones: ''
    });
    this.permisosSeleccionados.set([]);
    this.selectedKey.set(null);
  }

  guardarApiKey(): void {
    if (this.apiKeyForm.invalid) {
      this.apiKeyForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const values = this.apiKeyForm.getRawValue();
    const body = {
      nombre: values.nombre,
      descripcion: values.descripcion,
      workflow: values.workflow,
      fechaExpiracion: values.fechaExpiracion ? new Date(values.fechaExpiracion).toISOString() : null,
      permisoIds: this.permisosSeleccionados(),
      observaciones: values.observaciones
    };

    const integracionId = this.selectedIntegracion().id;

    this.http.post<any>(`${this.apiUrl}/${integracionId}/keys`, body).subscribe({
      next: (res) => {
        this.plainKeyGenerated.set(res.apiKeyPlana);
        this.mostrarModalKey.set(true);
        this.cargarApiKeys(integracionId);
        // Cerrar formulario de inserción
        this.crearApiKey();
      },
      error: (err) => this.showError('Error al generar API Key.'),
      complete: () => this.isLoading.set(false)
    });
  }

  copiarKeyPortapapeles(): void {
    if (this.plainKeyGenerated()) {
      navigator.clipboard.writeText(this.plainKeyGenerated()!);
      alert('API Key copiada al portapapeles con éxito.');
    }
  }

  cerrarModalKey(): void {
    this.mostrarModalKey.set(false);
    this.plainKeyGenerated.set(null);
  }

  abrirRotarApiKey(key: any): void {
    this.selectedKey.set(key);
    this.rotarForm.reset({
      fechaExpiracion: key.fechaExpiracion ? new Date(key.fechaExpiracion).toISOString().substring(0,16) : ''
    });
  }

  ejecutarRotarApiKey(): void {
    this.isLoading.set(true);
    const val = this.rotarForm.getRawValue();
    const body = {
      fechaExpiracion: val.fechaExpiracion ? new Date(val.fechaExpiracion).toISOString() : null
    };

    const integracionId = this.selectedIntegracion().id;
    const keyId = this.selectedKey().id;

    this.http.post<any>(`${this.apiUrl}/${integracionId}/keys/${keyId}/rotar`, body).subscribe({
      next: (res) => {
        this.plainKeyGenerated.set(res.apiKeyPlana);
        this.mostrarModalKey.set(true);
        this.cargarApiKeys(integracionId);
        this.selectedKey.set(null);
      },
      error: () => this.showError('Error al rotar API Key.'),
      complete: () => this.isLoading.set(false)
    });
  }

  revocarApiKey(key: any): void {
    if (!confirm(`¿Está seguro de revocar permanentemente la API Key "${key.nombre}"? Esta acción no se puede deshacer.`)) {
      return;
    }
    this.isLoading.set(true);
    const integracionId = this.selectedIntegracion().id;
    this.http.post<any>(`${this.apiUrl}/${integracionId}/keys/${key.id}/revocar`, {}).subscribe({
      next: () => this.cargarApiKeys(integracionId),
      error: () => this.showError('Error al revocar la API Key.'),
      complete: () => this.isLoading.set(false)
    });
  }

  cambiarEstadoApiKey(key: any, estado: string): void {
    this.isLoading.set(true);
    const integracionId = this.selectedIntegracion().id;
    this.http.put<any>(`${this.apiUrl}/${integracionId}/keys/${key.id}/estado`, { estado }).subscribe({
      next: () => this.cargarApiKeys(integracionId),
      error: () => this.showError('Error al cambiar el estado.'),
      complete: () => this.isLoading.set(false)
    });
  }

  eliminarApiKey(key: any): void {
    if (!confirm(`¿Está seguro de eliminar lógicamente la API Key "${key.nombre}"?`)) {
      return;
    }
    this.isLoading.set(true);
    const integracionId = this.selectedIntegracion().id;
    this.http.delete<any>(`${this.apiUrl}/${integracionId}/keys/${key.id}`).subscribe({
      next: () => this.cargarApiKeys(integracionId),
      error: () => this.showError('Error al eliminar la API Key.'),
      complete: () => this.isLoading.set(false)
    });
  }

  verHistorialCambios(key: any): void {
    this.selectedKey.set(key);
    this.http.get<any[]>(`${this.apiUrl}/keys/${key.id}/historial`).subscribe({
      next: (res) => this.historialCambios.set(res),
      error: () => this.showError('Error al obtener el historial de la API Key.')
    });
  }

  // ==========================================
  // 3. AUDITORÍA DE USO
  // ==========================================

  cargarAuditorias(): void {
    const params: any = {
      pageNumber: this.pageIndexAuditorias() + 1,
      pageSize: this.pageSizeAuditorias()
    };
    if (this.searchAuditorias()) {
      params.search = this.searchAuditorias();
    }

    this.http.get<any>(`${this.apiUrl}/auditoria`, { params }).subscribe({
      next: (res) => {
        this.auditorias.set(res.items || []);
        this.totalAuditorias.set(res.totalCount || 0);
      },
      error: () => this.showError('Error al cargar la bitácora de uso.')
    });
  }

  buscarAuditorias(): void {
    this.pageIndexAuditorias.set(0);
    this.cargarAuditorias();
  }

  onPageChangeAuditorias(event: PageEvent): void {
    this.pageIndexAuditorias.set(event.pageIndex);
    this.pageSizeAuditorias.set(event.pageSize);
    this.cargarAuditorias();
  }

  // ==========================================
  // HELPERS
  // ==========================================

  private showError(msg: string): void {
    this.errorMessage.set(msg);
    setTimeout(() => this.errorMessage.set(null), 5000);
  }
}
