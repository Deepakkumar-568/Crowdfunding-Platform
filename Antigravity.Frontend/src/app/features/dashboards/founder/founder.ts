import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService, FounderDashboard, MetricCard } from '../../../core/services/dashboard.service';
import { CampaignService } from '../../../core/services/campaign.service';
import { AIService } from '../../../core/services/ai.service';
import { StartupService, Startup } from '../../../core/services/startup.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-founder-dashboard',
  imports: [RouterLink, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './founder.html',
  styleUrl: './founder.scss'
})
export class FounderDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private campaignService = inject(CampaignService);
  private aiService = inject(AIService);
  private startupService = inject(StartupService);
  private authService = inject(AuthService);

  dashboardData = signal<FounderDashboard | null>(null);
  myStartups = signal<Startup[]>([]);
  selectedStartup = signal<Startup | null>(null);

  // Tabs: metrics | startup | ai-pitch | launch
  activeTab = signal('metrics');

  // AI Pitch state variables
  draftPitch = '';
  refinedPitch = signal('');
  improvingPitch = signal(false);

  // Launch Campaign state variables
  fundingGoal = 100000;
  pitchContent = '';
  startsAt = '';
  endsAt = '';
  launchSuccess = signal(false);
  launchError = signal('');

  // Create Startup state variables
  showCreateStartupForm = signal(false);
  startupName = '';
  startupTagline = '';
  startupIndustry = 'Software';
  startupCategory = 'SaaS';
  startupDesc = '';
  startupModel = '';
  startupFinancials = '';
  createSuccess = signal(false);

  ngOnInit() {
    this.loadDashboard();
    this.loadMyStartups();
  }

  loadDashboard() {
    this.dashboardService.getFounder().subscribe({
      next: (data) => {
        this.dashboardData.set(data);
      }
    });
  }

  loadMyStartups() {
    const userId = this.authService.currentUser()?.id;
    if (userId) {
      this.startupService.getAll().subscribe({
        next: (startups) => {
          const mine = startups.filter(s => s.founderId === userId);
          this.myStartups.set(mine);
          if (mine.length > 0) {
            this.selectedStartup.set(mine[0]);
          }
        }
      });
    }
  }

  switchTab(tab: string) {
    this.activeTab.set(tab);
    if (tab === 'ai-pitch' && this.selectedStartup() && !this.draftPitch) {
      this.draftPitch = this.selectedStartup()?.description || '';
    }
  }

  refinePitch() {
    const startup = this.selectedStartup();
    if (!startup || !this.draftPitch) return;

    this.improvingPitch.set(true);
    this.refinedPitch.set('');

    this.aiService.improvePitch(startup.name, this.draftPitch).subscribe({
      next: (res) => {
        this.refinedPitch.set(res.result);
        this.improvingPitch.set(false);
      },
      error: () => {
        this.refinedPitch.set('AI Pitch refinement failed. Check your local Ollama connection.');
        this.improvingPitch.set(false);
      }
    });
  }

  submitCampaign(event: Event) {
    event.preventDefault();
    const startup = this.selectedStartup();
    if (!startup || !this.startsAt || !this.endsAt || !this.pitchContent) return;

    const payload = {
      startupId: startup.id,
      fundingGoal: this.fundingGoal,
      pitch: this.pitchContent,
      startsAt: new Date(this.startsAt).toISOString(),
      endsAt: new Date(this.endsAt).toISOString()
    };

    this.launchError.set('');
    this.campaignService.create(payload).subscribe({
      next: () => {
        this.launchSuccess.set(true);
        this.pitchContent = '';
        this.startsAt = '';
        this.endsAt = '';
        this.loadDashboard();
        setTimeout(() => this.launchSuccess.set(false), 5000);
      },
      error: (err) => {
        this.launchError.set(err.error?.message || 'Failed to submit campaign for approval.');
      }
    });
  }

  submitCreateStartup(event: Event) {
    event.preventDefault();
    if (!this.startupName || !this.startupTagline || !this.startupDesc) return;

    const payload = {
      name: this.startupName,
      tagline: this.startupTagline,
      industry: this.startupIndustry,
      category: this.startupCategory,
      description: this.startupDesc,
      businessModel: this.startupModel,
      financialOverview: this.startupFinancials,
      teamMembers: []
    };

    this.startupService.create(payload).subscribe({
      next: (res) => {
        this.createSuccess.set(true);
        this.startupName = '';
        this.startupTagline = '';
        this.startupDesc = '';
        this.startupModel = '';
        this.startupFinancials = '';
        this.loadMyStartups();
        this.loadDashboard();
        setTimeout(() => {
          this.createSuccess.set(false);
          this.showCreateStartupForm.set(false);
          this.activeTab.set('metrics');
        }, 3000);
      }
    });
  }
}
