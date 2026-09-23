import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebhookService {
  private http = inject(HttpClient);

  generateExecutiveSummaryPdf(payload: any): Observable<string> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/A4_Genera_PDF_resumen_ejecutivo_del_rol';
    return this.http.post(url, payload, { responseType: 'text' });
  }

  generateStructuredProfilePdf(payload: any): Observable<string> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/ares-generador-pdf-perfil-estructurado';
    return this.http.post(url, payload, { responseType: 'text' });
  }

  generarResumen(payload: any): Observable<string> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/perfil_vacante_resumen';
    return this.http.post(url, payload, { responseType: 'text' });
  }

  notifySolicitanteProfileUnderReview(payload: any): Observable<string> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/Notificador2_Solicitante_observa_perfil';
    return this.http.post(url, payload, { responseType: 'text' });
  }

  triggerMotorMatching(payload: any): Observable<string> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/motor_matching';
    return this.http.post(url, payload, { responseType: 'text' });
  }

  triggerMotorMatchingModificadoMVP(payload: any): Observable<any> {
    const url = 'https://nacional-seguros-dev.isia.cloud/webhook/motor_Matching_Modificado_MVP';
    return this.http.post(url, payload);
  }
}
