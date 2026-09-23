import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileListSummary } from '../../models/profile.model';
import { ProfileMetricConfig } from '../../constants/profile-role.config';

@Component({
  selector: 'app-profile-summary-cards',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-summary-cards.component.html',
  styleUrls: ['./profile-summary-cards.component.scss']
})
export class ProfileSummaryCardsComponent {
  @Input({ required: true }) summary!: ProfileListSummary | null;
  @Input({ required: true }) metrics!: ProfileMetricConfig[];
  @Input() activeKey: string | null = null;
  @Output() cardSelect = new EventEmitter<string>();

  getMetricValue(key: string): number {
    if (!this.summary) return 0;
    return (this.summary as any)[key] || 0;
  }

  onCardClick(key: string): void {
    this.cardSelect.emit(key);
  }
}
