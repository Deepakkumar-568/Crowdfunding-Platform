import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Campaign } from './campaign.service';
import { Startup } from './startup.service';
import { Investment } from './investment.service';

export interface MetricCard {
  title: string;
  value: string;
  changeText: string;
  isPositive: boolean;
}

export interface NotificationDto {
  id: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface FounderDashboard {
  cards: MetricCard[];
  activeCampaigns: Campaign[];
  recentInvestments: Investment[];
  notifications: NotificationDto[];
  healthScore: number;
}

export interface InvestorDashboard {
  cards: MetricCard[];
  activeInvestments: Investment[];
  savedStartups: Startup[];
  recentHistory: Investment[];
  notifications: NotificationDto[];
}

export interface AdminDashboard {
  cards: MetricCard[];
  pendingStartups: Startup[];
  pendingCampaigns: Campaign[];
  recentActivityLog: NotificationDto[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7165/api/dashboard';

  getFounder(): Observable<FounderDashboard> {
    return this.http.get<FounderDashboard>(`${this.apiUrl}/founder`);
  }

  getInvestor(): Observable<InvestorDashboard> {
    return this.http.get<InvestorDashboard>(`${this.apiUrl}/investor`);
  }

  getAdmin(): Observable<AdminDashboard> {
    return this.http.get<AdminDashboard>(`${this.apiUrl}/admin`);
  }
}
