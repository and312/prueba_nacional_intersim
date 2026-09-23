import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ComponentLoadingService {
  private loadingStates = new Map<string, any>();

  show(key: string): void {
    if (!this.loadingStates.has(key)) {
      this.loadingStates.set(key, signal<boolean>(false));
    }
    this.loadingStates.get(key).set(true);
  }

  hide(key: string): void {
    if (this.loadingStates.has(key)) {
      this.loadingStates.get(key).set(false);
    }
  }

  isLoading(key: string) {
    if (!this.loadingStates.has(key)) {
      this.loadingStates.set(key, signal<boolean>(false));
    }
    return this.loadingStates.get(key);
  }
}
