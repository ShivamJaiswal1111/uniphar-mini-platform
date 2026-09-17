import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';

interface Goal {
  title: string;
  description: string;
  target: string;
  progress: number;
}

interface SustainabilityData {
  heroHeading: string;
  heroSubtext: string;
  overviewText: string;
  goals: Goal[];
  esgReportUrl: string;
}

@Component({
  selector: 'app-sustainability',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent],
  templateUrl: './sustainability.html',
  styleUrl: './sustainability.css'
})
export class Sustainability implements OnInit, OnDestroy {
  data: SustainabilityData | null = null;
  sanitizedOverview: SafeHtml | null = null;
  brandSlug: string = 'uniphar-group';
  loading: boolean = true;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
    private languageService: LanguageService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    combineLatest([
      this.route.params,
      this.languageService.currentLanguage$
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe(([params, culture]) => {
      this.brandSlug = params['brandSlug'] || 'uniphar-group';
      this.loadData(culture);
    });
  }

  loadData(culture: string): void {
    this.loading = true;
    this.error = null;
    this.api.get<SustainabilityData>(`${this.brandSlug}/sustainability`, culture).subscribe({
      next: data => {
        this.data = data;
        this.sanitizedOverview = data.overviewText
          ? this.sanitizer.bypassSecurityTrustHtml(data.overviewText)
          : null;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load sustainability information.';
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