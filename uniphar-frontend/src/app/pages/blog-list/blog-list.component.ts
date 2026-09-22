import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PageService } from '../../services/page.service';
import { Page } from '../../models/page.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { FooterComponent } from '../../components/footer/footer.component';
import { BreadcrumbComponent } from '../../components/breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-blog-list',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent, FooterComponent, BreadcrumbComponent],
  templateUrl: './blog-list.component.html'
})
export class BlogListComponent implements OnInit {
  posts: Page[] = [];
  loading = true;
  error: string | null = null;

  constructor(
    private pageService: PageService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    console.log('BlogList ngOnInit fired');
    this.pageService.getBlogPosts().subscribe({
      next: (data) => {
        console.log('Got data:', data);
        this.posts = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = 'Failed to load blog posts';
        this.loading = false;
        this.cdr.detectChanges();
        console.error(err);
      }
    });
  }

  getSource(post: Page): string {
    return post.contentType === 'migratedBlogPost' ? 'migrated' : 'new';
  }
}