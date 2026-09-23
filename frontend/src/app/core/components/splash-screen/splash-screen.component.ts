import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GlobalLoadingService } from '../../services/global-loading.service';

@Component({
  selector: 'app-splash-screen',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  templateUrl: './splash-screen.component.html',
  styleUrls: ['./splash-screen.component.scss']
})
export class SplashScreenComponent {
  private globalLoadingService = inject(GlobalLoadingService);
  isLoading = this.globalLoadingService.isLoading();
}
