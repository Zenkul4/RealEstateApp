---
name: realestate-frontend-design
description: Guidance for distinctive, intentional visual design when building new UI or reshaping existing views in the RealEstateApp (ASP.NET Core MVC / Razor Stack).
license: Apache License 2.0
---

# RealEstateApp Frontend Design

Approach this as the design lead building a premium, modern real estate platform (`RealEstateApp`). The application must reject standard bootstrap/generic admin-template aesthetics. Make deliberate, opinionated choices about palette, typography, and layout that reflect architectural precision, space, and structure, specific to property browsing and management.

## Ground it in the subject: Real Estate & Architecture

The single job of this interface is to make property discovery, analysis, and management feel seamless, trustworthy, and high-end. 
- **Audience:** Homebuyers, renters, and professional real estate agents.
- **Vernacular:** Use design elements inspired by architectural blueprints, spatial design, framing, and clean layouts. Every view must prioritize high-quality property photography, clear pricing metrics, and structural grid layouts.

## Design principles for Razor Views (.cshtml)

### 1. The Hero and Property Showcase
Open with the single most critical element of the view:
- On public browsing pages: A high-impact, intentional property search/filter mechanism or a beautifully framed featured listing.
- On management dashboards: Clear, concise data visualizations of active listings or agent performance.
- Avoid generic hero sections with overused stock gradients and centered text.

### 2. Typography & Type Scale
Typography must carry the personality of a premium real estate firm:
- Define a strict type scale with intentional weights and line heights using native CSS variables.
- Ensure high contrast between display elements (property titles, prices) and utility text (square footage, address, features).
- The type treatment must look like a curated architectural magazine, not a default UI vehicle.

### 3. Structure as Information
- Avoid generic numbered sequences (01 / 02 / 03) unless the user is performing a literal multi-step sequence (e.g., a property wizard or wizard-based mortgage calculation).
- Use structural dividers, clean borders, and spatial grid systems (CSS Grid/Flexbox) to encode relationships between property metadata (Price, Beds, Baths, Size).

### 4. Technical Stack Constraints (ASP.NET Core MVC)
- **Output Target:** Generate exclusively functional, production-grade ASP.NET Core Razor Views (`.cshtml`). Do not output React, Vue, or Angular components.
- **Tag Helpers:** Integrate native .NET Tag Helpers (`asp-for`, `asp-action`, `asp-controller`, `asp-validation-for`) tightly into the HTML structure for seamless data binding with the Application ViewModels.
- **Model Integration:** Ensure form structures and data displays map cleanly to C# strongly-typed models (e.g., `@model PropertyViewModel`).

## Process: Token System & Implementation

Work in two distinct mental passes:

1. **Design Tokens:** Define an explicit, compact system before rendering HTML:
   - **Color:** A palette of 4–6 named hex values representing deep architectural darks, crisp clean surfaces, and single high-end accent tones (e.g., deep slate, muted brass, off-white surfaces).
   - **Layout:** Rely on pure CSS Grid and Flexbox layouts. Match implementation complexity to the vision (precision in spacing, minimal clean borders).
2. **Execution:** Write standard, semantic HTML5 elements alongside Razor syntax. Avoid class selector collision by cleanly separating layout-level containers from reusable component classes (like property cards).

## Interface Copy & Validation

- **Active Voice:** Action elements must use clear, conversational commands based on what the user controls: Use "Guardar cambios", "Publicar propiedad", or "Eliminar listado" instead of generic verbs like "Submit".
- **Validation and State:** Error messages (via `asp-validation-summary`) must be direct, instructive, and system-agnostic. Empty states (e.g., "No se encontraron propiedades que coincidan con sus filtros") must act as an explicit invitation to reset or broaden the search criteria.