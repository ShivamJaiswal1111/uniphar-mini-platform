import { Component, OnInit } from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs';
import { LanguageService, LanguageOption } from '../../services/language.service';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './navbar.component.html'
})
export class NavbarComponent implements OnInit {
  currentLanguage: string = 'en-US';
  activeBrand: string = 'uniphar-group';
  showMedtech = false;
  showPharma = false;
  searchQuery: string = '';

  // Filled per brand from LanguageService, replaces the old hardcoded list.
  availableLanguages: LanguageOption[] = [];

  constructor(
    private languageService: LanguageService,
    private router: Router,
    public authService: AuthService
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  ngOnInit(): void {
    this.languageService.currentLanguage$.subscribe(lang => {
      this.currentLanguage = lang;
    });

    // Run once now (the navigation may already have finished before this
    // component was created), then again after every navigation.
    this.updateBrand(this.router.url);

    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e: any) => {
      this.updateBrand(e.urlAfterRedirects);
    });
  }

  private updateBrand(url: string): void {
    if (url.includes('uniphar-medtech')) this.activeBrand = 'uniphar-medtech';
    else if (url.includes('uniphar-pharma')) this.activeBrand = 'uniphar-pharma';
    else this.activeBrand = 'uniphar-group';

    this.availableLanguages = this.languageService.getLanguagesFor(this.activeBrand);
    this.languageService.ensureValidFor(this.activeBrand);
  }

  switchLanguage(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.languageService.setLanguage(select.value);
  }

  runSearch(): void {
    const term = this.searchQuery.trim();
    if (!term) return;
    this.router.navigate(['/search'], { queryParams: { q: term } });
    this.searchQuery = '';
  }
}