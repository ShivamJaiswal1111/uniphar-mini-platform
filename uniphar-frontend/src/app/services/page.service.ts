import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Page } from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class PageService {
  constructor(private api: ApiService) {}

  getPage(brandSlug: string, pageSlug: string, culture: string = 'en-US'): Observable<Page> {
    return this.api.get<Page>(`${brandSlug}/page/${pageSlug}`, culture);
  }

  getHome(brandSlug: string, culture: string = 'en-US'): Observable<Page> {
    return this.api.get<Page>(`${brandSlug}/home`, culture);
  }

  getLegacyPage(slug: string): Observable<Page> {
    return this.api.get<Page>(`legacy/${slug}`);
  }
  getBlogPosts(): Observable<Page[]> {
    return this.api.get<Page[]>('blog');
  }

  getBlogPost(slug: string, source: string = 'new'): Observable<Page> {
    return this.api.get<Page>(`blog/${slug}`, 'en-US', { source });
  }

  
}