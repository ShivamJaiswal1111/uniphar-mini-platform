import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { InvestorsComponent } from './pages/investors/investors.component';
import { ServicesComponent } from './pages/services/services.component';
import { ContactComponent } from './pages/contact/contact.component';
import { ServiceDetailComponent } from './pages/service-detail/service-detail.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'investors', component: InvestorsComponent },
  { path: 'contact', component: ContactComponent },
  { path: ':brandSlug', component: HomeComponent },
  { path: ':brandSlug/services', component: ServicesComponent },
  { path: ':brandSlug/services/:serviceSlug', component: ServiceDetailComponent },
  { path: ':brandSlug/contact', component: ContactComponent },
  { path: '**', redirectTo: '' }
];