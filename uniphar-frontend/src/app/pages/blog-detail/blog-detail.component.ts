import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PageService } from '../../services/page.service';
import { Page } from '../../models/page.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { HeroComponent } from '../../components/hero/hero.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { BreadcrumbComponent } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-blog-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, HeroComponent, FooterComponent, BreadcrumbComponent],
  templateUrl: './blog-detail.component.html'
})
export class BlogDetailComponent implements OnInit {
  post: Page | null = null;
  loading = true;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private pageService: PageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') || '';
    const source = this.route.snapshot.queryParamMap.get('source') || 'new';

    this.pageService.getBlogPost(slug, source).subscribe({
      next: (data) => {
        this.post = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load post';
        this.loading = false;
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }
  
}
