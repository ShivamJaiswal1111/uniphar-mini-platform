import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PageService } from '../../services/page.service';
import { Page } from '../../models/page.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';

@Component({
  selector: 'app-standard-page',
  standalone: true,
  imports: [CommonModule, NavbarComponent, HeroComponent, FooterComponent],
  templateUrl: './standard-page.component.html'
})
export class StandardPageComponent implements OnInit {
  page: Page | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private pageService: PageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const brandSlug = params.get('brandSlug') || 'uniphar-group';
      const pageSlug = params.get('pageSlug') || this.route.snapshot.url[0]?.path || '';

      this.loading = true;
      this.error = null;

      this.pageService.getPage(brandSlug, pageSlug).subscribe({
        next: (data) => {
          this.page = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = 'Failed to load page';
          this.loading = false;
          this.cdr.detectChanges();
          console.error(err);
        }
      });
    });
  }
}