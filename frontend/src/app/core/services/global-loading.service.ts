import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class GlobalLoadingService {
  private activeRequests = 0;
  private loadingSignal = signal<boolean>(false);
  private timerId: any = null;

  show(forceImmediate = false): void {
    this.activeRequests++;
    
    if (forceImmediate) {
      if (this.timerId) {
        clearTimeout(this.timerId);
        this.timerId = null;
      }
      this.loadingSignal.set(true);
    } else if (this.activeRequests === 1 && !this.loadingSignal()) {
      if (this.timerId) {
        clearTimeout(this.timerId);
      }
      this.timerId = setTimeout(() => {
        if (this.activeRequests > 0) {
          this.loadingSignal.set(true);
        }
      }, 500);
    }
  }

  hide(): void {
    if (this.activeRequests > 0) {
      this.activeRequests--;
    }
    
    if (this.activeRequests === 0) {
      if (this.timerId) {
        clearTimeout(this.timerId);
        this.timerId = null;
      }
      this.loadingSignal.set(false);
    }
  }

  isLoading() {
    return this.loadingSignal;
  }
}
