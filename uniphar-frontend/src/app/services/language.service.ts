import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private currentLanguage = new BehaviorSubject<string>('en-US');
  currentLanguage$ = this.currentLanguage.asObservable();

  setLanguage(culture: string): void {
    this.currentLanguage.next(culture);
  }

  getLanguage(): string {
    return this.currentLanguage.getValue();
  }
}