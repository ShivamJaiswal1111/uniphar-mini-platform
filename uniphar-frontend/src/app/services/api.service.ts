import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, culture: string = 'en-US', extraParams: Record<string, string> = {}): Observable<T> {
    const headers = new HttpHeaders({ 'Accept-Language': culture });
    let params = `culture=${culture}`;
    for (const [key, value] of Object.entries(extraParams)) {
      params += `&${key}=${value}`;
    }
    return this.http.get<T>(`${this.baseUrl}/${endpoint}?${params}`, { headers });
  }
  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body);
  }
}