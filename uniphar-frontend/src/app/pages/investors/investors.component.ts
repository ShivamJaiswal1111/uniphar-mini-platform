import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Subject, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { InvestorOverview } from '../../models/investor.model';
import { BreadcrumbComponent } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-investors',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent, BreadcrumbComponent],
  templateUrl: './investors.component.html'
})
export class InvestorsComponent implements OnInit, OnDestroy {
  overview: InvestorOverview | null = null;
  sanitizedTicker: SafeHtml | null = null;
  loading: boolean = true;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private languageService: LanguageService,
    private sanitizer: DomSanitizer,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.languageService.currentLanguage$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(culture => {
      this.loadOverview(culture);
    });
  }

  loadOverview(culture: string): void {
    this.loading = true;
    this.error = null;
    this.api.get<InvestorOverview>('investors/overview', culture).subscribe({
      next: data => {
        this.overview = data;
        this.sanitizedTicker = data.stockTickerEmbed
          ? this.sanitizer.bypassSecurityTrustHtml(data.stockTickerEmbed)
          : null;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load investor data.';
        this.loading = false;
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}