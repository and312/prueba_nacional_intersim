import { Component, Input, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ProfileListQuery } from '../../models/profile.model';
import { ProfileStatus } from '../../models/profile-status';
import { PROFILE_STATUS_CONFIGS } from '../../constants/profile-status.config';
import { ProfilesListService } from '../../services/profiles-list.service';

@Component({
  selector: 'app-profile-filters',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-filters.component.html',
  styleUrls: ['./profile-filters.component.scss']
})
export class ProfileFiltersComponent implements OnInit {
  @Input({ required: true }) userRole!: 'RRHH' | 'AreaSol' | 'Administrador';
  @Input({ required: true }) showAreaFilter!: boolean;
  @Output() filterChange = new EventEmitter<Partial<ProfileListQuery>>();

  private profilesService = inject(ProfilesListService);

  filterForm = new FormGroup({
    search: new FormControl(''),
    estado: new FormControl<ProfileStatus | ''>(''),
    areaId: new FormControl<number | ''>(''),
    fechaDesde: new FormControl(''),
    fechaHasta: new FormControl('')
  });

  statusOptions = [
    ProfileStatus.EnRevisionRRHHPE,
    ProfileStatus.EnRevisionAreaSol,
    ProfileStatus.Observada,
    ProfileStatus.Aprobada,
    ProfileStatus.PerfilAprobadoFinal
  ].map(status => ({
    value: status,
    label: PROFILE_STATUS_CONFIGS[status].label
  }));

  areas: { id: number; nombre: string }[] = [];

  ngOnInit(): void {
    this.profilesService.getAreasList().subscribe(list => {
      this.areas = list;
    });

    this.filterForm.valueChanges.subscribe(() => {
      this.emitFilters();
    });
  }

  emitFilters(): void {
    const raw = this.filterForm.value;
    const query: Partial<ProfileListQuery> = {};
    
    if (raw.search) query.search = raw.search;
    if (raw.estado) query.estado = raw.estado as ProfileStatus;
    if (raw.areaId) query.areaId = Number(raw.areaId);
    if (raw.fechaDesde) query.fechaDesde = raw.fechaDesde;
    if (raw.fechaHasta) query.fechaHasta = raw.fechaHasta;

    this.filterChange.emit(query);
  }

  clearFilters(): void {
    this.filterForm.setValue({
      search: '',
      estado: '',
      areaId: '',
      fechaDesde: '',
      fechaHasta: ''
    });
  }
}
