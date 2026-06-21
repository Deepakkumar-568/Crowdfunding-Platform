import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Investment {
  id: string;
  campaignId: string;
  campaignName: string;
  startupId: string;
  startupName: string;
  startupLogoUrl: string;
  amount: number;
  roiTracked: number;
  investedAt: string;
  status: 'Pending' | 'Success' | 'Refunded';
}

@Injectable({
  providedIn: 'root'
})
export class InvestmentService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7165/api/investments';

  create(campaignId: string, amount: number): Observable<Investment> {
    return this.http.post<Investment>(this.apiUrl, { campaignId, amount });
  }

  getMyInvestments(): Observable<Investment[]> {
    return this.http.get<Investment[]>(`${this.apiUrl}/my`);
  }

  getByCampaign(campaignId: string): Observable<Investment[]> {
    return this.http.get<Investment[]>(`${this.apiUrl}/campaign/${campaignId}`);
  }
}
