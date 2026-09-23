import { Component, OnInit, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { LoaderComponent } from '../../core/components/loader/loader.component';
import { ComponentLoadingService } from '../../core/services/component-loading.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    LoaderComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  componentLoading = inject(ComponentLoadingService);

  userArea = signal('');
  solicitudesCount = signal(0);
  perfilesCount = signal(0);
  vacantesCount = signal(0);
  postulantesCount = signal(0);

  ngOnInit(): void {
    this.userArea.set(this.authService.getUserArea());
    this.loadMetrics();
  }

  loadMetrics(): void {
    // Cargar conteos de los diferentes módulos del backend filtrados por el área del usuario
    this.apiService.getSolicitudes(1, 1, this.userArea()).subscribe({
      next: res => this.solicitudesCount.set(res.totalCount || 0),
      error: () => this.solicitudesCount.set(0)
    });

    this.apiService.getPerfiles(1, 1).subscribe({
      next: res => this.perfilesCount.set(res.totalCount || 0),
      error: () => this.perfilesCount.set(0)
    });

    this.apiService.getVacantes(1, 1).subscribe({
      next: res => this.vacantesCount.set(res.totalCount || 0),
      error: () => this.vacantesCount.set(0)
    });

    this.apiService.getPostulantes(1, 1).subscribe({
      next: res => this.postulantesCount.set(res.totalCount || 0),
      error: () => this.postulantesCount.set(0)
    });
  }
}
