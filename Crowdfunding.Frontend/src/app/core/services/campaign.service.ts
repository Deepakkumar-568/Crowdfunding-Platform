import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Campaign {
  id: string;
  startupId: string;
  startupName: string;
  startupTagline: string;
  startupLogoUrl: string;
  fundingGoal: number;
  currentFunding: number;
  investorCount: number;
  pitch: string;
  status: 'Draft' | 'Active' | 'Completed' | 'Cancelled';
  startsAt: string;
  endsAt: string;
  daysLeft: number;
  progressPercentage: number;
}

@Injectable({
  providedIn: 'root'
})
export class CampaignService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7165/api/campaigns';

  create(campaign: any): Observable<Campaign> {
    return this.http.post<Campaign>(this.apiUrl, campaign);
  }

  getAll(search?: string, minGoal?: number, maxGoal?: number, sortBy?: string): Observable<Campaign[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (minGoal) params = params.set('minGoal', minGoal.toString());
    if (maxGoal) params = params.set('maxGoal', maxGoal.toString());
    if (sortBy) params = params.set('sortBy', sortBy);

    return this.http.get<Campaign[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Campaign> {
    return this.http.get<Campaign>(`${this.apiUrl}/${id}`);
  }

  approve(id: string, status: 'Active' | 'Completed' | 'Cancelled'): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/approve`, { status });
  }

  getByStartup(startupId: string): Observable<Campaign[]> {
    return this.http.get<Campaign[]>(`${this.apiUrl}/startup/${startupId}`);
  }
}
