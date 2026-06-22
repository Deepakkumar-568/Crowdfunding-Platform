import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { DashboardService, AdminDashboard } from '../../../core/services/dashboard.service';
import { StartupService } from '../../../core/services/startup.service';
import { CampaignService } from '../../../core/services/campaign.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './admin.html',
  styleUrl: '../founder/founder.scss'
})
export class AdminDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private startupService = inject(StartupService);
  private campaignService = inject(CampaignService);

  dashboardData = signal<AdminDashboard | null>(null);

  // Tabs: metrics | startups | campaigns | logs
  activeTab = signal('metrics');

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.dashboardService.getAdmin().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
      }
    });
  }

  switchTab(tab: string) {
    this.activeTab.set(tab);
  }

  verifyStartup(startupId: string, status: 'Approved' | 'Rejected') {
    this.startupService.verify(startupId, status).subscribe({
      next: () => {
        this.loadDashboard();
      }
    });
  }

  approveCampaign(campaignId: string, status: 'Active' | 'Cancelled') {
    this.campaignService.approve(campaignId, status).subscribe({
      next: () => {
        this.loadDashboard();
      }
    });
  }
}
