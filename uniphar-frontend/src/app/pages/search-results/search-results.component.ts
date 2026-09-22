import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { SearchService } from '../../services/search.service';
import { Page } from '../../models/page.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { FooterComponent } from '../../components/footer/footer.component';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent],
  templateUrl: './search-results.component.html'
})
export class SearchResultsComponent implements OnInit {
  results: Page[] = [];
  query: string = '';
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private searchService: SearchService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.query = params.get('q') || '';
      this.runSearch();
    });
  }

  runSearch(): void {
    if (!this.query) {
      this.results = [];
      this.loading = false;
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.searchService.search(this.query).subscribe({
      next: (data) => {
        this.results = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Search failed';
        this.loading = false;
        console.error(err);
        this.cdr.detectChanges();
      }
    });
  }                                         

  getResultRoute(result: Page): { commands: string[], queryParams?: { source: string } } {
    if (result.contentType === 'blogPost') {
        return { commands: ['/blog', result.slug], queryParams: { source: 'new' } };
    }

    if (result.contentType === 'migratedBlogPost') {
        return { commands: ['/blog', result.slug], queryParams: { source: 'migrated' } };
    }

    if (result.contentType === 'servicePage') {
        const brand = result.brandSlug || 'uniphar-medtech';
        return { commands: ['/', brand, 'services', result.slug] };
    }

    if (result.contentType === 'homePage') {
        return { commands: ['/', result.brandSlug || ''] };
    }

    if (result.contentType === 'standardPage') {
        const brand = result.brandSlug || 'uniphar-group';
        if (result.slug === 'services' || result.slug === 'products') {
        return { commands: ['/', brand, 'services'] };
        }
        return { commands: ['/', brand, result.slug] };
    }

    const brand = result.brandSlug || 'uniphar-group';
    return { commands: ['/', brand, result.slug] };
    }
}