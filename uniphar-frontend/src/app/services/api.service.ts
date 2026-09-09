import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, culture: string = 'en-US'): Observable<T> {
    const headers = new HttpHeaders({ 'Accept-Language': culture });
    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, { headers });
  }
}