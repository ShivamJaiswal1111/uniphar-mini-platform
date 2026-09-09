import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { PageService } from '../../services/page.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { Page } from '../../models/page.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent],
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit, OnDestroy {
  page: Page | null = null;
  brandSlug: string = 'uniphar-group';
  currentCulture: string = 'en-US';
  loading: boolean = true;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private pageService: PageService,
    private languageService: LanguageService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    combineLatest([
      this.route.params,
      this.languageService.currentLanguage$
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe(([params, culture]) => {
      this.brandSlug = params['brandSlug'] || 'uniphar-group';
      this.currentCulture = culture;
      this.loadPage();
    });
  }

  loadPage(): void {
    this.loading = true;
    this.error = null;
    this.pageService.getHome(this.brandSlug, this.currentCulture).subscribe({
      next: data => {
        this.page = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load page content.';
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