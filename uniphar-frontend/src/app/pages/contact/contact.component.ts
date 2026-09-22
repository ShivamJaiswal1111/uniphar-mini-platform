import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { LanguageService } from '../../services/language.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { MapComponent } from '../../components/map/map';
import { ContactFormComponent } from '../../components/contact-form/contact-form.component';
import { BreadcrumbComponent } from '../../components/breadcrumb/breadcrumb.component';

interface ContactData {
  address: string;
  phone: string;
  email: string;
  mapEmbed: string;
  officeImageUrl: string;
  latitude: number | null;
  longitude: number | null;
  heroHeading: string | null;
  heroSubtext: string | null;
  heroImageUrl: string | null;
  breadcrumbs: { title: string; url: string | null }[];
}

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent, MapComponent, ContactFormComponent, BreadcrumbComponent],
  templateUrl: './contact.component.html'
})
export class ContactComponent implements OnInit, OnDestroy {
  contact: ContactData | null = null;
  brandSlug: string = 'uniphar-group';
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
      this.brandSlug = params['brandSlug'] || 'uniphar-group';
      this.loadContact(culture);
    });
  }

  loadContact(culture: string): void {
    this.loading = true;
    this.error = null;
    this.api.get<ContactData>(`${this.brandSlug}/contact`, culture).subscribe({
      next: data => {
        this.contact = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = 'Failed to load contact details.';
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