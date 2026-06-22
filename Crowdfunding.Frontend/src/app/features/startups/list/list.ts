import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CampaignService, Campaign } from '../../../core/services/campaign.service';

@Component({
  selector: 'app-startup-list',
  imports: [RouterLink, CurrencyPipe, FormsModule],
  templateUrl: './list.html',
  styleUrl: './list.scss'
})
export class StartupList implements OnInit {
  private campaignService = inject(CampaignService);
  
  campaigns = signal<Campaign[]>([]);
  filteredCampaigns = signal<Campaign[]>([]);

  // Filter bindings
  searchQuery = '';
  selectedCategory = '';
  selectedIndustry = '';
  minGoal: number | null = null;
  maxGoal: number | null = null;
  selectedSort = 'newest';

  // Options
  categories = ['Energy', 'Aerospace', 'SaaS', 'Biotech', 'Fintech', 'AI & Robotics'];
  industries = ['HardTech', 'Logistics', 'Software', 'Healthcare', 'Finance', 'Automation'];

  ngOnInit() {
    this.loadCampaigns();
  }

  loadCampaigns() {
    this.campaignService.getAll().subscribe({
      next: (data) => {
        this.campaigns.set(data);
        this.applyFilters();
      }
    });
  }

  applyFilters() {
    let result = [...this.campaigns()];

    // Search query
    if (this.searchQuery) {
      const query = this.searchQuery.toLowerCase();
      result = result.filter(c => 
        c.startupName.toLowerCase().includes(query) || 
        c.startupTagline.toLowerCase().includes(query) ||
        c.pitch.toLowerCase().includes(query)
      );
    }

    // Category (mock mapping category from tagline/industry or standard filter)
    if (this.selectedCategory) {
      // Mock filter checking if matching or mock field
      result = result.filter(c => {
        if (this.selectedCategory === 'Energy') return c.startupName.toLowerCase().includes('fusion');
        if (this.selectedCategory === 'Aerospace') return c.startupName.toLowerCase().includes('drone') || c.startupName.toLowerCase().includes('logistics');
        return true;
      });
    }

    // Industry
    if (this.selectedIndustry) {
      result = result.filter(c => {
        if (this.selectedIndustry === 'HardTech') return c.startupName.toLowerCase().includes('fusion');
        if (this.selectedIndustry === 'Logistics') return c.startupName.toLowerCase().includes('drone') || c.startupName.toLowerCase().includes('logistics');
        return true;
      });
    }

    // Goal ranges
    if (this.minGoal !== null) {
      result = result.filter(c => c.fundingGoal >= (this.minGoal || 0));
    }
    if (this.maxGoal !== null) {
      result = result.filter(c => c.fundingGoal <= (this.maxGoal || Infinity));
    }

    // Sorting
    result = result.sort((a, b) => {
      if (this.selectedSort === 'newest') return b.id.localeCompare(a.id); // Simulating date sort via UUIDs
      if (this.selectedSort === 'fundingraised') return b.currentFunding - a.currentFunding;
      if (this.selectedSort === 'popularity') return b.investorCount - a.investorCount;
      if (this.selectedSort === 'progress') return b.progressPercentage - a.progressPercentage;
      return 0;
    });

    this.filteredCampaigns.set(result);
  }

  resetFilters() {
    this.searchQuery = '';
    this.selectedCategory = '';
    this.selectedIndustry = '';
    this.minGoal = null;
    this.maxGoal = null;
    this.selectedSort = 'newest';
    this.applyFilters();
  }
}
