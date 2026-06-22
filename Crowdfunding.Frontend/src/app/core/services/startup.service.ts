import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TeamMember {
  name: string;
  role: string;
  bio: string;
  avatarUrl?: string;
}

export interface Startup {
  id: string;
  founderId: string;
  founderName: string;
  name: string;
  tagline: string;
  industry: string;
  category: string;
  description: string;
  businessModel: string;
  financialOverview: string;
  logoUrl?: string;
  pitchDeckUrl?: string;
  websiteUrl?: string;
  videoUrl?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  healthScore: number;
  createdAt: string;
  teamMembers: TeamMember[];
}

@Injectable({
  providedIn: 'root'
})
export class StartupService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7165/api/startups';

  create(startup: any): Observable<Startup> {
    return this.http.post<Startup>(this.apiUrl, startup);
  }

  getAll(search?: string, category?: string, industry?: string, sortBy?: string): Observable<Startup[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (category) params = params.set('category', category);
    if (industry) params = params.set('industry', industry);
    if (sortBy) params = params.set('sortBy', sortBy);

    return this.http.get<Startup[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Startup> {
    return this.http.get<Startup>(`${this.apiUrl}/${id}`);
  }

  verify(id: string, status: 'Approved' | 'Rejected'): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/verify`, { status });
  }

  toggleSave(id: string): Observable<{ isSaved: boolean; message: string }> {
    return this.http.post<{ isSaved: boolean; message: string }>(`${this.apiUrl}/${id}/save`, {});
  }

  getSaved(): Observable<Startup[]> {
    return this.http.get<Startup[]>(`${this.apiUrl}/saved`);
  }
}
