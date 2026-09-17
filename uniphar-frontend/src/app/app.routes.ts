import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { InvestorsComponent } from './pages/investors/investors.component';
import { ServicesComponent } from './pages/services/services.component';
import { ContactComponent } from './pages/contact/contact.component';
import { ServiceDetailComponent } from './pages/service-detail/service-detail.component';
import { StandardPageComponent } from './pages/standard-page/standard-page.component';
import { Sustainability } from './pages/sustainability/sustainability';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'investors', component: InvestorsComponent },
  { path: 'sustainability', component: Sustainability },
  { path: 'contact', component: ContactComponent },
  { path: 'about-us', component: StandardPageComponent },
  { path: ':brandSlug', component: HomeComponent },
  { path: ':brandSlug/services', component: ServicesComponent },
  { path: ':brandSlug/services/:serviceSlug', component: ServiceDetailComponent },
  { path: ':brandSlug/contact', component: ContactComponent },
  { path: ':brandSlug/sustainability', component: Sustainability },
  { path: ':brandSlug/:pageSlug', component: StandardPageComponent },
  { path: '**', redirectTo: '' }
];