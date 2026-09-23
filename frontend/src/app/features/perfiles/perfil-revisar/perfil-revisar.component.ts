import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ApiService } from '../../../core/services/api.service';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../../core/services/component-loading.service';
import { PerfilAprobarModalComponent } from '../perfil-aprobar-modal/perfil-aprobar-modal.component';
import { PerfilSeccionEditModalComponent } from '../perfil-seccion-edit-modal/perfil-seccion-edit-modal.component';

@Component({
  selector: 'app-perfil-revisar',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    DatePipe,
    MatDialogModule,
    LoaderComponent
  ],
  templateUrl: './perfil-revisar.component.html',
  styleUrls: ['./perfil-revisar.component.scss']
})
export class PerfilRevisarComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private dialog = inject(MatDialog);
  componentLoading = inject(ComponentLoadingService);

  perfilId = 0;
  perfil = signal<any | null>(null);
  parsedProfile = signal<any>({});
  trazabilidad = signal<any[]>([]);
  auditoria = signal<any[]>([]);
  activeTab = signal<'trazabilidad' | 'auditoria'>('trazabilidad');
  isRegeneratingPdf = signal(false);

  seccionesGrid = [
    { numero: 1, nombre: 'Objetivo del Cargo', icon: 'ads_click', class: 'col-span-2' },
    { numero: 2, nombre: 'Seniority', icon: 'military_tech', class: '' },
    { numero: 3, nombre: 'Condiciones Contractuales', icon: 'description', class: '' },
    { numero: 4, nombre: 'Formación Académica', icon: 'school', class: '' },
    { numero: 5, nombre: 'Experiencia Requerida', icon: 'work_history', class: '' },
    { numero: 6, nombre: 'Idiomas', icon: 'translate', class: '' },
    { numero: 7, nombre: 'Funciones Principales', icon: 'assignment', class: 'col-span-2' },
    { numero: 8, nombre: 'Responsabilidades Clave', icon: 'task', class: 'col-span-2' },
    { numero: 9, nombre: 'Competencias Técnicas', icon: 'handyman', class: '' },
    { numero: 10, nombre: 'Competencias Blandas', icon: 'psychology', class: '' },
    { numero: 11, nombre: 'Certificaciones Requeridas', icon: 'workspace_premium', class: '' },
    { numero: 12, nombre: 'Herramientas, KPIs y Riesgos', icon: 'analytics', class: 'col-span-2' }
  ];

  ngOnInit(): void {
    this.perfilId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.perfilId) {
      this.loadPerfil();
      this.loadTrazabilidad();
      this.loadAuditorias();
    }
  }

  loadPerfil(): void {
    this.componentLoading.show('perfil-revisar');
    this.apiService.getPerfilById(this.perfilId).subscribe({
      next: (res) => {
        this.perfil.set(res);
        this.parsedProfile.set(this.parseJson(res.profesiogramaJson));
        this.componentLoading.hide('perfil-revisar');
      },
      error: () => {
        this.componentLoading.hide('perfil-revisar');
      }
    });
  }

  loadTrazabilidad(): void {
    this.apiService.getTrazabilidadPerfil(this.perfilId).subscribe({
      next: (res) => {
        this.trazabilidad.set(res || []);
      },
      error: () => {}
    });
  }

  loadAuditorias(): void {
    this.apiService.getPerfilAuditoria(this.perfilId).subscribe({
      next: (res) => {
        this.auditoria.set(res || []);
      },
      error: () => {}
    });
  }

  parseJson(jsonStr: string): any {
    try {
      return JSON.parse(jsonStr);
    } catch {
      return {
        Cargo: this.perfil()?.cargoNombre || 'Desconocido',
        Objetivo: 'El profesiograma no tiene un formato JSON válido.',
        Funciones: [],
        Responsabilidades: [],
        Competencias: [],
        CompetenciasTecnicas: [],
        CompetenciasBlandas: [],
        Seniority: '',
        Formacion: '',
        Certificaciones: [],
        Experiencia: '',
        Idiomas: [],
        Herramientas: [],
        Kpis: [],
        Condiciones: '',
        Riesgos: [],
        ObservacionesIA: 'Sin observaciones parseables.'
      };
    }
  }

  getSeccionContenido(numeroSeccion: number): string {
    const profile = this.parsedProfile();
    switch (numeroSeccion) {
      case 1: return profile.Objetivo || '';
      case 2: return profile.Seniority || '';
      case 3: return profile.Condiciones || '';
      case 4: return profile.Formacion || '';
      case 5: return profile.Experiencia || '';
      case 6: return JSON.stringify(profile.Idiomas || []);
      case 7: return JSON.stringify(profile.Funciones || []);
      case 8: return JSON.stringify(profile.Responsabilidades || []);
      case 9: return JSON.stringify(profile.CompetenciasTecnicas || []);
      case 10: return JSON.stringify(profile.CompetenciasBlandas || []);
      case 11: return JSON.stringify(profile.Certificaciones || []);
      case 12: 
        return JSON.stringify({
          Herramientas: profile.Herramientas || [],
          Kpis: profile.Kpis || [],
          Riesgos: profile.Riesgos || [],
          ObservacionesIA: profile.ObservacionesIA || ''
        });
      default: return '';
    }
  }

  editarSeccion(numeroSeccion: number, nombreSeccion: string): void {
    const contenido = this.getSeccionContenido(numeroSeccion);

    const dialogRef = this.dialog.open(PerfilSeccionEditModalComponent, {
      width: '650px',
      disableClose: true,
      data: { numeroSeccion, nombreSeccion, contenido }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.componentLoading.show('perfil-revisar');
        this.apiService.actualizarPerfilSeccion(this.perfilId, numeroSeccion, result).subscribe({
          next: (res) => {
            this.perfil.set(res);
            this.parsedProfile.set(this.parseJson(res.profesiogramaJson));
            this.loadAuditorias();
            this.componentLoading.hide('perfil-revisar');
          },
          error: () => {
            this.componentLoading.hide('perfil-revisar');
          }
        });
      }
    });
  }

  regenerarPdf(): void {
    this.isRegeneratingPdf.set(true);
    this.apiService.regenerarPdf(this.perfilId).subscribe({
      next: () => {
        this.isRegeneratingPdf.set(false);
        this.loadPerfil();
      },
      error: () => {
        this.isRegeneratingPdf.set(false);
      }
    });
  }

  setTab(tab: 'trazabilidad' | 'auditoria'): void {
    this.activeTab.set(tab);
  }

  aprobarPerfil(): void {
    const dialogRef = this.dialog.open(PerfilAprobarModalComponent, {
      width: '480px',
      data: { cargoNombre: this.perfil()?.cargoNombre }
    });

    dialogRef.afterClosed().subscribe(comentario => {
      if (comentario !== undefined) {
        this.componentLoading.show('perfil-revisar');
        this.apiService.aprobarPerfilRRHH(this.perfilId, { comentario }).subscribe({
          next: () => {
            this.componentLoading.hide('perfil-revisar');
            this.router.navigate(['/perfiles']);
          },
          error: () => {
            this.componentLoading.hide('perfil-revisar');
          }
        });
      }
    });
  }
}
