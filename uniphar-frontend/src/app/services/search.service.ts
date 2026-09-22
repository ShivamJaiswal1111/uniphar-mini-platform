import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Page } from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  constructor(private api: ApiService) {}

  search(query: string, culture: string = 'en-US'): Observable<Page[]> {
    return this.api.get<Page[]>('search', culture, { q: query });
  }
}