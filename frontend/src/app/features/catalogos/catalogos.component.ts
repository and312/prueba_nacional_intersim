import { Component, OnInit, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { ApiService } from '../../core/services/api.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';

@Component({
  selector: 'app-catalogos',
  standalone: true,
  imports: [
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    LoaderComponent
  ],
  templateUrl: './catalogos.component.html',
  styleUrls: ['./catalogos.component.scss']
})
export class CatalogosComponent implements OnInit {
  private apiService = inject(ApiService);
  componentLoading = inject(ComponentLoadingService);

  catalogos = ['Estados', 'Areas', 'MotivosDescarte'];
  selectedCatalogo = signal('Estados');
  dataSource = signal<any[]>([]);
  displayedColumns: string[] = ['id', 'codigo', 'nombre'];

  ngOnInit(): void {
    this.loadCatalogData(this.selectedCatalogo());
  }

  onCatalogChange(catalog: string): void {
    this.selectedCatalogo.set(catalog);
    this.loadCatalogData(catalog);
  }

  loadCatalogData(catalog: string): void {
    this.apiService.getCatalogos(catalog).subscribe({
      next: (data) => {
        this.dataSource.set(data);
      },
      error: () => {
        // En caso de error de conexión en el MVP, inyectamos datos semilla para simulación
        if (catalog === 'Estados') {
          this.dataSource.set([
            { id: 1, codigo: 'SOL-REG', nombre: 'Registrada' },
            { id: 2, codigo: 'SOL-APR', nombre: 'Aprobada' },
            { id: 3, codigo: 'VAC-PUB', nombre: 'Publicada' },
            { id: 4, codigo: 'POS-REG', nombre: 'Registrado' }
          ]);
        } else if (catalog === 'Areas') {
          this.dataSource.set([
            { id: 1, codigo: 'TI', nombre: 'Tecnología de la Información' },
            { id: 2, codigo: 'RRHH', nombre: 'Recursos Humanos' },
            { id: 3, codigo: 'FIN', nombre: 'Finanzas' }
          ]);
        } else {
          this.dataSource.set([
            { id: 1, codigo: 'DESC-EXP', nombre: 'Falta de Experiencia' },
            { id: 2, codigo: 'DESC-SAL', nombre: 'Pretensión Salarial Excedida' }
          ]);
        }
      }
    });
  }
}
