import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home.component').then(m => m.HomeComponent),
    pathMatch: 'full'
  },
  {
    path: 'investors',
    loadComponent: () =>
      import('./pages/investors/investors.component').then(m => m.InvestorsComponent),
    pathMatch: 'full'
  },
  {
    path: 'sustainability',
    loadComponent: () =>
      import('./pages/sustainability/sustainability').then(m => m.Sustainability),
    pathMatch: 'full'
  },
  {
    path: 'contact',
    loadComponent: () =>
      import('./pages/contact/contact.component').then(m => m.ContactComponent),
    pathMatch: 'full'
  },
  {
    path: 'about-us',
    loadComponent: () =>
      import('./pages/standard-page/standard-page.component').then(m => m.StandardPageComponent),
    pathMatch: 'full'
  },
  {
    path: 'blog',
    loadComponent: () =>
      import('./pages/blog-list/blog-list.component').then(m => m.BlogListComponent),
    pathMatch: 'full'
  },
  {
    path: 'search',
    loadComponent: () =>
      import('./pages/search-results/search-results.component').then(m => m.SearchResultsComponent)
  },
  {
    path: 'blog/:slug',
    loadComponent: () =>
      import('./pages/blog-detail/blog-detail.component').then(m => m.BlogDetailComponent)
  },
  {
    path: 'legacy/:pageSlug',
    loadComponent: () =>
      import('./pages/standard-page/standard-page.component').then(m => m.StandardPageComponent),
    data: { brandSlug: 'legacy' }
  },
  {
    path: ':brandSlug',
    loadComponent: () =>
      import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: ':brandSlug/services',
    loadComponent: () =>
      import('./pages/services/services.component').then(m => m.ServicesComponent)
  },
  {
    path: ':brandSlug/services/:serviceSlug',
    loadComponent: () =>
      import('./pages/service-detail/service-detail.component').then(m => m.ServiceDetailComponent)
  },
  {
    path: ':brandSlug/contact',
    loadComponent: () =>
      import('./pages/contact/contact.component').then(m => m.ContactComponent)
  },
  {
    path: ':brandSlug/sustainability',
    loadComponent: () =>
      import('./pages/sustainability/sustainability').then(m => m.Sustainability)
  },
  {
    path: ':brandSlug/:pageSlug',
    loadComponent: () =>
      import('./pages/standard-page/standard-page.component').then(m => m.StandardPageComponent)
  },
  { path: '**', redirectTo: '' }
];