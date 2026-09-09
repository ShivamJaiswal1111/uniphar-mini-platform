import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Brand } from '../models/brand.model';

@Injectable({ providedIn: 'root' })
export class BrandService {
  constructor(private api: ApiService) {}

  getAllBrands(culture: string = 'en-US'): Observable<Brand[]> {
    return this.api.get<Brand[]>('brands', culture);
  }

  getBrand(slug: string, culture: string = 'en-US'): Observable<Brand> {
    return this.api.get<Brand>(`brands/${slug}`, culture);
  }
}