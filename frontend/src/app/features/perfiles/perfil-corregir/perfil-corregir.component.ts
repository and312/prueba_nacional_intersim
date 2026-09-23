import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { LoaderComponent } from '../../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../../core/services/component-loading.service';

@Component({
  selector: 'app-perfil-corregir',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    LoaderComponent
  ],
  templateUrl: './perfil-corregir.component.html',
  styleUrls: ['./perfil-corregir.component.scss']
})
export class PerfilCorregirComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private fb = inject(FormBuilder);
  componentLoading = inject(ComponentLoadingService);

  perfilId = 0;
  perfil = signal<any | null>(null);
  editForm!: FormGroup;
  isSaved = signal(false);
  newPerfilId = signal<number | null>(null);

  ngOnInit(): void {
    this.perfilId = Number(this.route.snapshot.paramMap.get('id'));
    this.initForm();
    if (this.perfilId) {
      this.loadPerfil();
    }
  }

  initForm(): void {
    this.editForm = this.fb.group({
      cargo: ['', Validators.required],
      objetivo: ['', Validators.required],
      seniority: [''],
      condiciones: [''],
      formacion: [''],
      experiencia: [''],
      funciones: [''],
      responsabilidades: [''],
      competenciasTecnicas: [''],
      competenciasBlandas: [''],
      certificaciones: [''],
      herramientas: [''],
      kpis: [''],
      riesgos: [''],
      idiomas: ['']
    });
  }

  loadPerfil(): void {
    this.componentLoading.show('perfil-corregir');
    this.apiService.getPerfilById(this.perfilId).subscribe({
      next: (res) => {
        this.perfil.set(res);
        const parsed = this.parseJson(res.profesiogramaJson);
        this.populateForm(parsed);
        this.componentLoading.hide('perfil-corregir');
      },
      error: () => {
        this.componentLoading.hide('perfil-corregir');
      }
    });
  }

  parseJson(jsonStr: string): any {
    try {
      return JSON.parse(jsonStr);
    } catch {
      return {};
    }
  }

  populateForm(data: any): void {
    this.editForm.patchValue({
      cargo: data.Cargo || this.perfil()?.cargoNombre || '',
      objetivo: data.Objetivo || '',
      seniority: data.Seniority || '',
      condiciones: data.Condiciones || '',
      formacion: data.Formacion || '',
      experiencia: data.Experiencia || '',
      funciones: this.listToText(data.Funciones),
      responsabilidades: this.listToText(data.Responsabilidades),
      competenciasTecnicas: this.listToText(data.CompetenciasTecnicas),
      competenciasBlandas: this.listToText(data.CompetenciasBlandas),
      certificaciones: this.listToText(data.Certificaciones),
      herramientas: this.listToText(data.Herramientas),
      kpis: this.listToText(data.Kpis),
      riesgos: this.listToText(data.Riesgos),
      idiomas: this.listToText(data.Idiomas)
    });
  }

  listToText(list: any): string {
    if (!list || !Array.isArray(list)) return '';
    return list.join('\n');
  }

  textToList(text: string): string[] {
    if (!text) return [];
    return text.split('\n')
      .map(line => line.trim())
      .filter(line => line !== '');
  }

  onSave(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const formVal = this.editForm.value;
    const profileJsonObj = {
      Cargo: formVal.cargo,
      Objetivo: formVal.objetivo,
      Seniority: formVal.seniority,
      Condiciones: formVal.condiciones,
      Formacion: formVal.formacion,
      Experiencia: formVal.experiencia,
      Funciones: this.textToList(formVal.funciones),
      Responsabilidades: this.textToList(formVal.responsabilidades),
      CompetenciasTecnicas: this.textToList(formVal.competenciasTecnicas),
      CompetenciasBlandas: this.textToList(formVal.competenciasBlandas),
      Certificaciones: this.textToList(formVal.certificaciones),
      Herramientas: this.textToList(formVal.herramientas),
      Kpis: this.textToList(formVal.kpis),
      Riesgos: this.textToList(formVal.riesgos),
      Idiomas: this.textToList(formVal.idiomas),
      ObservacionesIA: this.perfil()?.observacionesIa || ''
    };

    const payload = {
      Cargo: formVal.cargo,
      Descripcion: JSON.stringify(profileJsonObj)
    };

    this.componentLoading.show('perfil-corregir');
    this.apiService.corregirPerfil(this.perfilId, payload).subscribe({
      next: (res) => {
        this.componentLoading.hide('perfil-corregir');
        this.isSaved.set(true);
        this.newPerfilId.set(res.perfilId);
        // Actualizar el perfil actual en memoria
        this.perfil.set(res);
      },
      error: () => {
        this.componentLoading.hide('perfil-corregir');
      }
    });
  }

  enviarArea(): void {
    const targetId = this.newPerfilId() || this.perfilId;
    this.componentLoading.show('perfil-corregir');
    this.apiService.enviarPerfilArea(targetId, { comentario: 'Perfil revisado y corregido enviado al Área Solicitante.' }).subscribe({
      next: () => {
        this.componentLoading.hide('perfil-corregir');
        this.router.navigate(['/perfiles']);
      },
      error: () => {
        this.componentLoading.hide('perfil-corregir');
      }
    });
  }
}
