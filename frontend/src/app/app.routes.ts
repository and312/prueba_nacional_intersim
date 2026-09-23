import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layouts/layout/layout.component').then(m => m.LayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'catalogos',
        loadComponent: () => import('./features/catalogos/catalogos.component').then(m => m.CatalogosComponent)
      },
      {
        path: 'solicitudes',
        loadComponent: () => import('./features/solicitudes/solicitudes.component').then(m => m.SolicitudesComponent)
      },
      {
        path: 'solicitudes/nueva',
        loadComponent: () => import('./features/solicitudes/nueva-solicitud/nueva-solicitud.component').then(m => m.NuevaSolicitudComponent)
      },
      {
        path: 'solicitudes/editar/:id',
        loadComponent: () => import('./features/solicitudes/nueva-solicitud/nueva-solicitud.component').then(m => m.NuevaSolicitudComponent)
      },
      {
        path: 'solicitudes/:id',
        loadComponent: () => import('./features/solicitudes/solicitud-detalle/solicitud-detalle.component').then(m => m.SolicitudDetalleComponent)
      },
      {
        path: 'perfiles',
        loadComponent: () => import('./features/perfiles/perfiles.component').then(m => m.PerfilesComponent),
        children: [
          {
            path: '',
            loadComponent: () => import('./features/perfiles/perfiles-list/perfiles-list.component').then(m => m.PerfilesListComponent)
          },
          {
            path: ':id',
            loadComponent: () => import('./features/perfiles/detalle-perfil/detalle-perfil.component').then(m => m.DetallePerfilComponent)
          },
          {
            path: ':id/revisar',
            loadComponent: () => import('./features/perfiles/detalle-perfil/detalle-perfil.component').then(m => m.DetallePerfilComponent)
          },
          {
            path: ':id/corregir',
            loadComponent: () => import('./features/perfiles/detalle-perfil/detalle-perfil.component').then(m => m.DetallePerfilComponent)
          }
        ]
      },
      {
        path: 'estrategias',
        canActivate: [authGuard],
        data: { roles: ['RRHH', 'Administrador'] },
        children: [
          {
            path: '',
            loadComponent: () => import('./features/estrategias/estrategias-list/estrategias-list.component').then(m => m.EstrategiasListComponent)
          },
          {
            path: ':estrategiaId',
            loadComponent: () => import('./features/estrategias/detalle-estrategia/detalle-estrategia.component').then(m => m.DetalleEstrategiaComponent)
          }
        ]
      },
      {
        path: 'vacantes',
        loadComponent: () => import('./features/vacantes/vacantes.component').then(m => m.VacantesComponent)
      },
      {
        path: 'postulantes',
        loadComponent: () => import('./features/postulantes/postulantes.component').then(m => m.PostulantesComponent)
      },
      {
        path: 'usuarios',
        loadComponent: () => import('./features/usuarios/usuarios.component').then(m => m.UsuariosComponent)
      },
      {
        path: 'usuarios/roles',
        loadComponent: () => import('./features/usuarios/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'usuarios/areas',
        loadComponent: () => import('./features/usuarios/areas/areas.component').then(m => m.AreasComponent)
      },
      {
        path: 'integraciones',
        loadComponent: () => import('./features/integraciones/integraciones.component').then(m => m.IntegracionesComponent)
      },
      {
        path: 'configuracion/parametrizacion',
        loadComponent: () => import('./features/parametrizacion/paginas/inicio-parametrizacion/inicio-parametrizacion.component').then(m => m.InicioParametrizacionComponent),
        canActivate: [authGuard],
        data: { roles: ['Administrador'] }
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
