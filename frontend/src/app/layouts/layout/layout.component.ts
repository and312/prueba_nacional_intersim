import { Component, inject, signal, HostListener } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  authService = inject(AuthService);
  router = inject(Router);

  isCollapsed = signal(false);
  isMobileOpen = signal(false);
  isHeaderMenuOpen = signal(false);

  toggleHeaderMenu(): void {
    this.isHeaderMenuOpen.update(v => !v);
  }

  closeHeaderMenu(): void {
    if (this.isHeaderMenuOpen()) {
      this.isHeaderMenuOpen.set(false);
    }
  }

  getUserDisplayRole(): string {
    const role = this.authService.getUserRole();
    if (role === 'RRHH' || role === 'Recursos Humanos') {
      return 'Recursos Humanos';
    }
    if (role === 'Solicitante' || role === 'Area Solicitante') {
      return 'Área Solicitante';
    }
    if (role === 'Administrador' || role === 'Admin') {
      return 'Administrador';
    }
    return role || 'Usuario';
  }

  getUserShortName(): string {
    const role = this.authService.getUserRole();
    if (role === 'RRHH' || role === 'Recursos Humanos') {
      return 'RRHH';
    }
    const nom = localStorage.getItem('user_nombres') || '';
    if (nom.trim()) {
      return nom.trim().split(' ')[0];
    }
    const email = this.authService.getUserEmail();
    if (email) {
      const prefix = email.split('@')[0];
      return prefix.charAt(0).toUpperCase() + prefix.slice(1);
    }
    return role || 'Usuario';
  }

  getUserFullName(): string {
    const nom = localStorage.getItem('user_nombres') || '';
    const ape = localStorage.getItem('user_apellidos') || '';
    const fullName = `${nom} ${ape}`.trim();
    if (fullName) {
      return fullName;
    }
    const role = this.getUserDisplayRole();
    if (role === 'Recursos Humanos') {
      return 'Usuario RRHH';
    }
    return role;
  }

  getUserEmail(): string {
    return this.authService.getUserEmail();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  hasRole(roles: string[]): boolean {
    return roles.includes(this.authService.getUserRole());
  }

  toggleSidebar(): void {
    if (window.innerWidth < 1024) {
      this.isMobileOpen.set(!this.isMobileOpen());
    } else {
      this.isCollapsed.set(!this.isCollapsed());
    }
  }

  closeMobileSidebar(): void {
    if (this.isMobileOpen()) {
      this.isMobileOpen.set(false);
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth >= 1024) {
      if (this.isMobileOpen()) {
        this.isMobileOpen.set(false);
      }
      if (this.isHeaderMenuOpen()) {
        this.isHeaderMenuOpen.set(false);
      }
    }
  }
}
