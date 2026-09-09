import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './hero.component.html'
})
export class HeroComponent {
  @Input() heading: string = '';
  @Input() subtext: string = '';
  @Input() imageUrl: string = '';
  @Input() ctaText: string = '';
  @Input() ctaLink: string = '';
}