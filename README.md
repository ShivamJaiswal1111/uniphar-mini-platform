# Uniphar Mini Platform

A full-stack multisite platform built with Umbraco v17, .NET 10, and Angular 18, demonstrating a real-world three-tier architecture connecting a headless CMS to a middleware API to a modern frontend SPA.

## Architecture

Editor publishes content in Umbraco
↓
Umbraco Content Delivery API (JSON)
↓
.NET 10 Web API (middleware, caching, business logic)
↓
Angular 18 SPA (rendering, routing, language switching)


## The Three Brands

| Brand | Languages | URL |
|---|---|---|
| Uniphar Group | English | uniphargroup.localhost:44335 |
| Uniphar Medtech | English, French | unimedtech.localhost:44335 |
| Uniphar Pharma | English, German | unipharma.localhost:44335 |

## Projects

### UniPharGroup — Umbraco v17 CMS
- Three root nodes, one per brand
- Seven document types with shared SEO, Hero and Page Settings compositions
- Block List editors for Key Stats, Features, Navigation Cards, Documents and Goals
- Culture variants for English, French and German
- Three user groups with restricted start nodes per brand
- Content Delivery API enabled with CORS for .NET consumption

### UniPharApi — .NET 10 Web API
- BrandController, PageController, InvestorController, ServiceController, MediaController, WebhookController
- UmbracoService with per-brand Host header override for multisite resolution
- IMemoryCache across all controllers with cache key pattern `brand:slug:culture`
- WebhookController clears cache on Umbraco content publish
- Swagger UI for endpoint testing

### uniphar-frontend — Angular 18 SPA
- Standalone component architecture (no NgModule)
- Navbar with hover dropdowns per brand, language switcher
- Hero, Footer shared components
- Home, Investors, Services, Service Detail, Contact page components
- LanguageService with BehaviorSubject driving Accept-Language headers
- combineLatest + takeUntil pattern for clean subscription management
- ChangeDetectorRef for zone-aware change detection

## Running Locally

Start all three servers in separate terminals:

```bash
# Terminal 1 — Umbraco CMS
cd UniPharGroup
dotnet run

# Terminal 2 — .NET API
cd UniPharApi
dotnet run

# Terminal 3 — Angular
cd uniphar-frontend
ng serve
```

Then open `http://localhost:4200`.

## Tech Stack

- **CMS** — Umbraco v17, SQL Server
- **API** — .NET 10, ASP.NET Core, IMemoryCache, Swagger
- **Frontend** — Angular 18, TypeScript, RxJS
- **Dev tools** — Visual Studio 2022, VS Code, Swagger UI