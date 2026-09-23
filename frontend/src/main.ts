import { bootstrapApplication } from '@angular/platform-browser';
import { registerLocaleData } from '@angular/common';
import localeEsBo from '@angular/common/locales/es-BO';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

// Registrar el locale boliviano globalmente en la aplicacion
registerLocaleData(localeEsBo);

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
