import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AITextResponse {
  result: string;
}

export interface AIPitchRefineResponse {
  success: boolean;
  generatedPitch: string;
  score: number;
  fundingStage: string;
  estimatedValuation: string;
  suggestedFunding: string;
  strengths: string[];
  weaknesses: string[];
  risks: string[];
  investorQuestions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AIService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7165/api/ai';

  enhanceDescription(name: string, tagline: string, description: string): Observable<AITextResponse> {
    return this.http.post<AITextResponse>(`${this.apiUrl}/enhance-description`, { name, tagline, description });
  }

  improvePitch(name: string, pitch: string): Observable<AIPitchRefineResponse> {
    return this.http.post<AIPitchRefineResponse>(`${this.apiUrl}/improve-pitch`, { name, pitch });
  }

  generateSummary(name: string, description: string, businessModel: string, financials: string): Observable<AITextResponse> {
    return this.http.post<AITextResponse>(`${this.apiUrl}/generate-summary`, { name, description, businessModel, financials });
  }

  getRecommendations(investorInterests: string): Observable<AITextResponse> {
    return this.http.post<AITextResponse>(`${this.apiUrl}/recommendations`, { investorInterests });
  }

  getHealthScore(name: string, goal: number, funding: number, teamCount: number): Observable<{ healthScore: number }> {
    const params = new HttpParams()
      .set('name', name)
      .set('goal', goal.toString())
      .set('funding', funding.toString())
      .set('teamCount', teamCount.toString());
      
    return this.http.get<{ healthScore: number }>(`${this.apiUrl}/health-score`, { params });
  }
}
