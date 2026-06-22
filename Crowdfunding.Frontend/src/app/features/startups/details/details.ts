import { Component, OnInit, signal, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StartupService, Startup } from '../../../core/services/startup.service';
import { CampaignService, Campaign } from '../../../core/services/campaign.service';
import { InvestmentService } from '../../../core/services/investment.service';
import { AIService } from '../../../core/services/ai.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-startup-details',
  imports: [RouterLink, CurrencyPipe, FormsModule],
  templateUrl: './details.html',
  styleUrl: './details.scss'
})
export class StartupDetails implements OnInit {
  private route = inject(ActivatedRoute);
  private startupService = inject(StartupService);
  private campaignService = inject(CampaignService);
  private investmentService = inject(InvestmentService);
  private aiService = inject(AIService);
  authService = inject(AuthService);

  startup = signal<Startup | null>(null);
  campaign = signal<Campaign | null>(null);
  
  // AI summary states
  aiSummary = signal('Loading AI Synthesis...');
  generatingSummary = signal(false);

  // Watchlist states
  isSaved = signal(false);

  // Investment states
  investAmount: number | null = null;
  investSuccess = signal(false);
  investError = signal('');

  activeTab = signal('pitch'); // pitch | model | financials | team

  ngOnInit() {
    const startupId = this.route.snapshot.paramMap.get('id');
    if (startupId) {
      this.loadStartupDetails(startupId);
      this.checkSavedState(startupId);
    }
  }

  loadStartupDetails(id: string) {
    this.startupService.getById(id).subscribe({
      next: (data) => {
        this.startup.set(data);
        this.loadCampaign(data.id);
        this.generateAISummary(data);
      }
    });
  }

  loadCampaign(startupId: string) {
    this.campaignService.getByStartup(startupId).subscribe({
      next: (campaigns) => {
        if (campaigns && campaigns.length > 0) {
          // Display the first active or draft campaign
          const activeCampaign = campaigns.find(c => c.status === 'Active') || campaigns[0];
          this.campaign.set(activeCampaign);
        }
      }
    });
  }

  generateAISummary(startup: Startup) {
    this.generatingSummary.set(true);
    this.aiService.generateSummary(startup.name, startup.description, startup.businessModel, startup.financialOverview)
      .subscribe({
        next: (res) => {
          this.aiSummary.set(res.result);
          this.generatingSummary.set(false);
        },
        error: () => {
          this.aiSummary.set('Failed to synthesize AI executive summary. Ensure local services are online.');
          this.generatingSummary.set(false);
        }
      });
  }

  checkSavedState(startupId: string) {
    if (this.authService.isLoggedIn() && this.authService.userRole() === 'Investor') {
      this.startupService.getSaved().subscribe({
        next: (saved) => {
          const exists = saved.some(s => s.id === startupId);
          this.isSaved.set(exists);
        }
      });
    }
  }

  toggleSave() {
    if (!this.authService.isLoggedIn()) return;
    const s = this.startup();
    if (!s) return;

    this.startupService.toggleSave(s.id).subscribe({
      next: (res) => {
        this.isSaved.set(res.isSaved);
      }
    });
  }

  submitInvestment(event: Event) {
    event.preventDefault();
    if (!this.authService.isLoggedIn()) return;
    const c = this.campaign();
    if (!c || !this.investAmount) return;

    this.investError.set('');
    this.investmentService.create(c.id, this.investAmount).subscribe({
      next: () => {
        this.investSuccess.set(true);
        this.investAmount = null;
        if (this.startup()) {
          this.loadStartupDetails(this.startup()!.id);
        }
        setTimeout(() => this.investSuccess.set(false), 5000);
      },
      error: (err) => {
        this.investError.set(err.error?.message || 'Transaction could not be completed.');
      }
    });
  }

  switchTab(tab: string) {
    this.activeTab.set(tab);
  }
}
