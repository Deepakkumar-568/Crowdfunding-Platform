import { Component, OnInit, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CampaignService, Campaign } from '../../core/services/campaign.service';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, CurrencyPipe, DecimalPipe, FormsModule],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements OnInit {
  private campaignService = inject(CampaignService);
  featuredCampaigns = signal<Campaign[]>([]);
  
  // Platform statistics
  stats = signal({
    totalRaised: 12450000,
    totalInvestors: 4850,
    successRate: 94,
    activeCampaignsCount: 18
  });

  faqItems = signal([
    {
      q: 'How does crowdfunding work on Antigravity?',
      a: 'Founders build their startup profiles and launch vetted seed rounds. Approved campaigns accept capital from eligible retail and accredited investors.',
      open: false
    },
    {
      q: 'What is the AI Health Score?',
      a: 'Our AI model evaluates operational traction, funding momentum, and roster completeness to issue a health score helping investors gauge campaign quality.',
      open: false
    },
    {
      q: 'How do I track my returns?',
      a: 'Investors use their custom dashboard to audit portfolio growth, estimated yields, and receive direct updates from founders.',
      open: false
    }
  ]);

  contactName = '';
  contactEmail = '';
  contactMessage = '';
  contactSuccess = signal(false);

  ngOnInit() {
    this.campaignService.getAll().subscribe({
      next: (campaigns) => {
        // Display top 3 campaigns as featured
        this.featuredCampaigns.set(campaigns.slice(0, 3));
      }
    });
  }

  toggleFaq(index: number) {
    const items = this.faqItems();
    items[index].open = !items[index].open;
    this.faqItems.set([...items]);
  }

  submitContact(event: Event) {
    event.preventDefault();
    // Simulate contact form submission
    this.contactSuccess.set(true);
    this.contactName = '';
    this.contactEmail = '';
    this.contactMessage = '';
    setTimeout(() => this.contactSuccess.set(false), 5000);
  }
}
