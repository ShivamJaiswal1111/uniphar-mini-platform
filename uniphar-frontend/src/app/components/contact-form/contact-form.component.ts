import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-contact-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact-form.component.html'
})
export class ContactFormComponent {
  @Input() brandSlug: string = '';

  form: FormGroup;
  submitting = false;
  successMessage = '';
  errorMessage = '';

  constructor(private fb: FormBuilder, private api: ApiService) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      message: ['', Validators.required]
    });
  }

  onSubmit(): void {
    this.successMessage = '';
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;

    this.api.post(`${this.brandSlug}/contact/submit`, this.form.value)
      .subscribe({
        next: () => {
          this.successMessage = 'Thank you — your message has been received.';
          this.form.reset();
          this.submitting = false;
        },
        error: () => {
          this.errorMessage = 'Something went wrong. Please try again.';
          this.submitting = false;
        }
      });
  }
}