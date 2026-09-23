import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/usuarios`;
  
  currentUser = signal<{ email: string; role: string } | null>(null);
  isSessionLocked = signal(false);

  private inactivityTimeout: any;
  private readonly TIMEOUT_MS = 1800000; // 30 minutos (1,800,000 ms)

  constructor(private http: HttpClient) {
    this.loadSession();
    this.startInactivityMonitoring();
  }

  private startInactivityMonitoring(): void {
    if (typeof window === 'undefined') return;

    this.resetInactivityTimer();

    const events = ['click', 'mousemove', 'keypress', 'touchstart', 'scroll'];
    events.forEach(event => {
      window.addEventListener(event, () => this.resetInactivityTimer());
    });
  }

  private resetInactivityTimer(): void {
    if (this.inactivityTimeout) {
      clearTimeout(this.inactivityTimeout);
    }

    if (!this.isLoggedIn()) return;

    this.inactivityTimeout = setTimeout(() => {
      this.lockSessionDueToInactivity();
    }, this.TIMEOUT_MS);
  }

  lockSessionDueToInactivity(): void {
    console.warn('[AuthService] Sesión bloqueada.');
    this.logout();
    this.isSessionLocked.set(true);
  }

  unlockSession(): void {
    this.isSessionLocked.set(false);
  }

  login(correo: string, contrasena: string): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/auth/login`, {
      correo,
      clave: contrasena,
      tipoAutenticacion: 'Local'
    }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user_email', res.usuario.correo);
        const role = res.usuario.roles[0] || 'Reclutador';
        localStorage.setItem('user_role', role);
        localStorage.setItem('user_nombres', res.usuario.nombres || '');
        localStorage.setItem('user_apellidos', res.usuario.apellidos || '');
        localStorage.setItem('user_area', res.usuario.areaNombre || '');
        localStorage.setItem('user_area_id', res.usuario.areaId?.toString() || '0');
        localStorage.setItem('user_cargo', res.usuario.cargo || '');
        localStorage.setItem('user_gerencia', res.usuario.gerencia || '');
        localStorage.setItem('user_id', res.usuario.usuarioId?.toString() || '');
        localStorage.setItem('user_permisos', JSON.stringify(res.usuario.permisos || []));
        this.currentUser.set({ email: res.usuario.correo, role: role });
        
        this.isSessionLocked.set(false);
        this.resetInactivityTimer();
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user_email');
    localStorage.removeItem('user_role');
    localStorage.removeItem('user_nombres');
    localStorage.removeItem('user_apellidos');
    localStorage.removeItem('user_area');
    localStorage.removeItem('user_area_id');
    localStorage.removeItem('user_cargo');
    localStorage.removeItem('user_gerencia');
    localStorage.removeItem('user_id');
    localStorage.removeItem('user_permisos');
    this.currentUser.set(null);
    if (this.inactivityTimeout) {
      clearTimeout(this.inactivityTimeout);
    }
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getUserRole(): string {
    return localStorage.getItem('user_role') || '';
  }

  getUserEmail(): string {
    return localStorage.getItem('user_email') || '';
  }

  getUserArea(): string {
    return localStorage.getItem('user_area') || '';
  }

  getUserAreaId(): number {
    const id = localStorage.getItem('user_area_id');
    return id ? parseInt(id, 10) : 0;
  }

  getUserCargo(): string {
    return localStorage.getItem('user_cargo') || '';
  }

  getUserGerencia(): string {
    return localStorage.getItem('user_gerencia') || '';
  }

  getUserFullName(): string {
    const nom = localStorage.getItem('user_nombres') || '';
    const ape = localStorage.getItem('user_apellidos') || '';
    return `${nom} ${ape}`.trim() || this.getUserEmail();
  }

  getUserId(): number {
    const id = localStorage.getItem('user_id');
    return id ? parseInt(id, 10) : 1;
  }


  hasPermission(codigo: string): boolean {
    try {
      const permisos: string[] = JSON.parse(localStorage.getItem('user_permisos') || '[]');
      return permisos.includes(codigo);
    } catch {
      return false;
    }
  }

  getToken(): string {
    return localStorage.getItem('token') || '';
  }

  private loadSession(): void {
    const token = localStorage.getItem('token');
    const email = localStorage.getItem('user_email');
    const role = localStorage.getItem('user_role');

    if (token && email && role) {
      this.currentUser.set({ email, role });
      this.resetInactivityTimer();
    }
  }
}
