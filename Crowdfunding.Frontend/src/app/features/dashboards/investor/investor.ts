import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, InvestorDashboard, MetricCard } from '../../../core/services/dashboard.service';
import { AIService } from '../../../core/services/ai.service';
import { StartupService, Startup } from '../../../core/services/startup.service';

@Component({
  selector: 'app-investor-dashboard',
  imports: [RouterLink, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './investor.html',
  styleUrl: '../founder/founder.scss' // Reuses vertical tab workspace layout
})
export class InvestorDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private aiService = inject(AIService);
  private startupService = inject(StartupService);

  dashboardData = signal<InvestorDashboard | null>(null);

  // Tabs: metrics | saved | history | ai-match
  activeTab = signal('metrics');

  // AI Recommendation Engine
  investorInterests = 'Clean energy tech, SaaS platforms';
  aiRecommendations = signal('');
  gettingRecommendations = signal(false);

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.dashboardService.getInvestor().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
      }
    });
  }

  switchTab(tab: string) {
    this.activeTab.set(tab);
    if (tab === 'saved') {
      // Reload watchlist
      this.dashboardService.getInvestor().subscribe({
        next: (data) => {
          if (this.dashboardData()) {
            this.dashboardData.set({
              ...this.dashboardData()!,
              savedStartups: data.savedStartups
            });
          }
        }
      });
    }
  }

  removeSaved(event: Event, startupId: string) {
    event.stopPropagation();
    event.preventDefault();
    this.startupService.toggleSave(startupId).subscribe({
      next: () => {
        this.switchTab('saved');
      }
    });
  }

  getRecommendations() {
    if (!this.investorInterests) return;

    this.gettingRecommendations.set(true);
    this.aiRecommendations.set('');

    this.aiService.getRecommendations(this.investorInterests).subscribe({
      next: (res) => {
        this.aiRecommendations.set(res.result);
        this.gettingRecommendations.set(false);
      },
      error: () => {
        this.aiRecommendations.set('Failed to generate investment recommendations. Ensure local Ollama is active.');
        this.gettingRecommendations.set(false);
      }
    });
  }
}
