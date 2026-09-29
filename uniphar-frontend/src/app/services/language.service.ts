import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface LanguageOption {
  code: string;
  label: string;
}

const ENGLISH: LanguageOption = { code: 'en-US', label: 'English' };
const FRENCH: LanguageOption = { code: 'fr-FR', label: 'French' };
const GERMAN: LanguageOption = { code: 'de-DE', label: 'German' };

// Single source of truth: which cultures each brand has in Umbraco.
// Keys match the brand slugs used in routes and in the API.
const BRAND_LANGUAGES: Record<string, LanguageOption[]> = {
  'uniphar-group': [ENGLISH],
  'uniphar-medtech': [ENGLISH, FRENCH],
  'uniphar-pharma': [ENGLISH, GERMAN]
};

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

  /** Languages a brand offers. Unknown brands get English only. */
  getLanguagesFor(brandSlug: string): LanguageOption[] {
    return BRAND_LANGUAGES[brandSlug] ?? [ENGLISH];
  }

  /** If the current language isn't offered by this brand, fall back to English. */
  ensureValidFor(brandSlug: string): void {
    const supported = this.getLanguagesFor(brandSlug).map(l => l.code);
    if (!supported.includes(this.getLanguage())) {
      this.setLanguage(ENGLISH.code);
    }
  }
}