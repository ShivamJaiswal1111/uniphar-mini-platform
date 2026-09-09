import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { Service } from '../../models/service.model';


@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, HeroComponent, FooterComponent],
  templateUrl: './services.component.html'
})
export class ServicesComponent implements OnInit, OnDestroy {
  services: Service[] = [];
  brandSlug: string = 'uniphar-medtech';
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
      this.brandSlug = params['brandSlug'] || 'uniphar-medtech';
      this.loadServices(culture);
    });
  }

  loadServices(culture: string): void {
    this.loading = true;
    this.error = null;
    this.api.get<Service[]>(`${this.brandSlug}/services`, culture).subscribe({
      next: data => {
        this.services = Array.isArray(data) ? data : [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load services.';
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