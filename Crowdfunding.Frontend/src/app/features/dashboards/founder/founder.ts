import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { DashboardService, FounderDashboard, MetricCard } from '../../../core/services/dashboard.service';
import { CampaignService } from '../../../core/services/campaign.service';
import { AIService, AIPitchRefineResponse } from '../../../core/services/ai.service';
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
  private sanitizer = inject(DomSanitizer);

  dashboardData = signal<FounderDashboard | null>(null);
  myStartups = signal<Startup[]>([]);
  selectedStartup = signal<Startup | null>(null);

  // Tabs: metrics | startup | ai-pitch | launch
  activeTab = signal('metrics');

  // AI Pitch state variables
  draftPitch = '';
  refinedPitch = signal('');
  improvingPitch = signal(false);
  refinedPitchResponse = signal<AIPitchRefineResponse | null>(null);
  refinedPitchHtml = signal<SafeHtml>('');
  errorMessage = signal<string>('');
  generationStatus = signal<string>('');
  copied = signal<boolean>(false);

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
    this.errorMessage.set('');
    this.refinedPitchResponse.set(null);
    this.refinedPitchHtml.set('');
    this.generationStatus.set('Generating your investor-ready pitch...');

    this.aiService.improvePitch(startup.name, this.draftPitch).subscribe({
      next: (res) => {
        if (res.success) {
          this.refinedPitchResponse.set(res);
          this.refinedPitch.set(res.generatedPitch);
          const html = this.parseMarkdown(res.generatedPitch);
          this.refinedPitchHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
        } else {
          this.errorMessage.set(res.generatedPitch || 'AI Pitch refinement failed. Please try again.');
        }
        this.improvingPitch.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'AI Pitch refinement failed. Check your local Ollama connection.');
        this.improvingPitch.set(false);
      }
    });
  }

  copyToClipboard() {
    const pitch = this.refinedPitchResponse()?.generatedPitch;
    if (pitch) {
      navigator.clipboard.writeText(pitch).then(() => {
        this.copied.set(true);
        setTimeout(() => this.copied.set(false), 2000);
      }).catch(err => {
        console.error('Could not copy text: ', err);
      });
    }
  }

  downloadAsMarkdown() {
    const pitch = this.refinedPitchResponse()?.generatedPitch;
    if (pitch) {
      const blob = new Blob([pitch], { type: 'text/markdown;charset=utf-8;' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      const orgName = this.selectedStartup()?.name || 'startup';
      link.setAttribute('download', `${orgName.toLowerCase().replace(/\s+/g, '_')}_pitch_deck.md`);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  clearRefinement() {
    this.draftPitch = '';
    this.refinedPitchResponse.set(null);
    this.refinedPitchHtml.set('');
    this.errorMessage.set('');
    this.refinedPitch.set('');
  }

  regeneratePitch() {
    this.refinePitch();
  }

  parseMarkdown(md: string): string {
    if (!md) return '';
    
    // Simple HTML sanitization/escape to avoid custom XSS, but keeping simple
    let html = md
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    // Headings
    html = html.replace(/^### (.*?)$/gm, '<h3>$1</h3>');
    html = html.replace(/^## (.*?)$/gm, '<h2>$1</h2>');
    html = html.replace(/^# (.*?)$/gm, '<h1>$1</h1>');

    // Bold text
    html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

    // Tables
    const lines = html.split('\n');
    let inTable = false;
    let tableHeaders: string[] = [];
    let tableRows: string[][] = [];
    let newLines: string[] = [];

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      if (line.startsWith('|') && line.endsWith('|')) {
        const cells = line.split('|').map(c => c.trim()).filter((c, idx, arr) => idx > 0 && idx < arr.length - 1);
        if (!inTable) {
          inTable = true;
          tableHeaders = cells;
        } else if (line.includes('---')) {
          // Separator line
        } else {
          tableRows.push(cells);
        }
      } else {
        if (inTable) {
          let tableHtml = '<div class="table-container"><table class="markdown-table"><thead><tr>';
          tableHeaders.forEach(h => {
            tableHtml += `<th>${h}</th>`;
          });
          tableHtml += '</tr></thead><tbody>';
          tableRows.forEach(row => {
            tableHtml += '<tr>';
            row.forEach(cell => {
              tableHtml += `<td>${cell}</td>`;
            });
            tableHtml += '</tr>';
          });
          tableHtml += '</tbody></table></div>';
          newLines.push(tableHtml);

          inTable = false;
          tableHeaders = [];
          tableRows = [];
        }
        newLines.push(lines[i]);
      }
    }
    if (inTable) {
      let tableHtml = '<div class="table-container"><table class="markdown-table"><thead><tr>';
      tableHeaders.forEach(h => {
        tableHtml += `<th>${h}</th>`;
      });
      tableHtml += '</tr></thead><tbody>';
      tableRows.forEach(row => {
        tableHtml += '<tr>';
        row.forEach(cell => {
          tableHtml += `<td>${cell}</td>`;
        });
        tableHtml += '</tr>';
      });
      tableHtml += '</tbody></table></div>';
      newLines.push(tableHtml);
    }
    html = newLines.join('\n');

    // Bullet Lists
    const lines2 = html.split('\n');
    let inList = false;
    let newLines2: string[] = [];
    for (let i = 0; i < lines2.length; i++) {
      const line = lines2[i];
      const match = line.match(/^(\s*)([-*])\s+(.*?)$/);
      if (match) {
        if (!inList) {
          inList = true;
          newLines2.push('<ul>');
        }
        newLines2.push(`<li>${match[3]}</li>`);
      } else {
        if (inList) {
          inList = false;
          newLines2.push('</ul>');
        }
        newLines2.push(line);
      }
    }
    if (inList) {
      newLines2.push('</ul>');
    }
    html = newLines2.join('\n');

    // Numbered Lists
    const lines3 = html.split('\n');
    let inNumList = false;
    let newLines3: string[] = [];
    for (let i = 0; i < lines3.length; i++) {
      const line = lines3[i];
      const match = line.match(/^(\s*)(\d+)\.\s+(.*?)$/);
      if (match) {
        if (!inNumList) {
          inNumList = true;
          newLines3.push('<ol>');
        }
        newLines3.push(`<li>${match[3]}</li>`);
      } else {
        if (inNumList) {
          inNumList = false;
          newLines3.push('</ol>');
        }
        newLines3.push(line);
      }
    }
    if (inNumList) {
      newLines3.push('</ol>');
    }
    html = newLines3.join('\n');

    // Paragraphs
    const paragraphs = html.split('\n\n');
    html = paragraphs.map(p => {
      const trimmed = p.trim();
      if (!trimmed) return '';
      if (trimmed.startsWith('<h') || trimmed.startsWith('<ul') || trimmed.startsWith('<ol') || trimmed.startsWith('<div') || trimmed.startsWith('</') || trimmed.startsWith('<table')) {
        return trimmed;
      }
      return `<p>${trimmed.replace(/\n/g, '<br/>')}</p>`;
    }).join('\n');

    return html;
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
