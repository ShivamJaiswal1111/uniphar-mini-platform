import { Component, OnInit } from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs';
import { LanguageService } from '../../services/language.service';
import { FormsModule } from '@angular/forms';


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

  languages = [
    { code: 'en-US', label: 'English' },
    { code: 'fr-FR', label: 'French' },
    { code: 'de-DE', label: 'German' }
  ];

  constructor(
    private languageService: LanguageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.languageService.currentLanguage$.subscribe(lang => {
      this.currentLanguage = lang;
    });

    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe((e: any) => {
      const url: string = e.urlAfterRedirects;
      if (url.includes('uniphar-medtech')) this.activeBrand = 'uniphar-medtech';
      else if (url.includes('uniphar-pharma')) this.activeBrand = 'uniphar-pharma';
      else this.activeBrand = 'uniphar-group';
    });
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