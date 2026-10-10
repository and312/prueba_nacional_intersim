import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class WebhookService {
  private http = inject(HttpClient);
  private webhooks = environment.webhooks;

  generateExecutiveSummaryPdf(payload: any): Observable<string> {
    return this.http.post(this.webhooks.generateExecutiveSummaryPdf, payload, { responseType: 'text' });
  }

  generateStructuredProfilePdf(payload: any): Observable<string> {
    return this.http.post(this.webhooks.generateStructuredProfilePdf, payload, { responseType: 'text' });
  }

  generarResumen(payload: any): Observable<string> {
    return this.http.post(this.webhooks.generarResumen, payload, { responseType: 'text' });
  }

  notifySolicitanteProfileUnderReview(payload: any): Observable<string> {
    return this.http.post(this.webhooks.notifySolicitanteProfileUnderReview, payload, { responseType: 'text' });
  }

  triggerMotorMatching(payload: any): Observable<string> {
    return this.http.post(this.webhooks.triggerMotorMatching, payload, { responseType: 'text' });
  }

  triggerMotorMatchingModificadoMVP(payload: any): Observable<any> {
    return this.http.post(this.webhooks.triggerMotorMatchingModificadoMVP, payload);
  }
}
