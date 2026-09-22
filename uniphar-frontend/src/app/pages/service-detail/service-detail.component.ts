import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { Service } from '../../models/service.model';
import { BreadcrumbComponent } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent, BreadcrumbComponent],
  templateUrl: './service-detail.component.html'
})
export class ServiceDetailComponent implements OnInit, OnDestroy {
  service: Service | null = null;
  brandSlug: string = '';
  serviceSlug: string = '';
  loading: boolean = true;
  error: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private api: ApiService,
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
      this.brandSlug = params['brandSlug'];
      this.serviceSlug = params['serviceSlug'];
      this.loadService(culture);
    });
  }

  loadService(culture: string): void {
    this.loading = true;
    this.error = null;
    this.api.get<Service>(`${this.brandSlug}/services/${this.serviceSlug}`, culture).subscribe({
      next: data => {
        this.service = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load service.';
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