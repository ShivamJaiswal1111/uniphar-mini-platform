import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { InvestorsComponent } from './pages/investors/investors.component';
import { ServicesComponent } from './pages/services/services.component';
import { ContactComponent } from './pages/contact/contact.component';
import { ServiceDetailComponent } from './pages/service-detail/service-detail.component';
import { StandardPageComponent } from './pages/standard-page/standard-page.component';
import { Sustainability } from './pages/sustainability/sustainability';
import { BlogListComponent } from './pages/blog-list/blog-list.component';
import { BlogDetailComponent } from './pages/blog-detail/blog-detail.component';
import { SearchResultsComponent } from './pages/search-results/search-results.component';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'investors', component: InvestorsComponent, pathMatch: 'full' },
  { path: 'sustainability', component: Sustainability, pathMatch: 'full' },
  { path: 'contact', component: ContactComponent, pathMatch: 'full' },
  { path: 'about-us', component: StandardPageComponent, pathMatch: 'full' },
  { path: 'blog', component: BlogListComponent, pathMatch: 'full' },
  { path: 'search', component: SearchResultsComponent },
  { path: 'blog/:slug', component: BlogDetailComponent },
  { path: 'legacy/:pageSlug', component: StandardPageComponent, data: { brandSlug: 'legacy' } },
  { path: ':brandSlug', component: HomeComponent },
  { path: ':brandSlug/services', component: ServicesComponent },
  { path: ':brandSlug/services/:serviceSlug', component: ServiceDetailComponent },
  { path: ':brandSlug/contact', component: ContactComponent },
  { path: ':brandSlug/sustainability', component: Sustainability },
  { path: ':brandSlug/:pageSlug', component: StandardPageComponent },
  { path: '**', redirectTo: '' }
];