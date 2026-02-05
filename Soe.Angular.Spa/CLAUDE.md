# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Essential Commands
- `npm start` - Start development server on https://localhost:44438 (with SSL certificates)
- `npm run build` - Production build (outputs to `../Soe.Angular/dist/spa`)
- `npm run build:dev` - Development build
- `npm test` - Run Jest tests with coverage
- `npm run lint` - Run ESLint on TypeScript and HTML files
- `npm run lint:fix` - Auto-fix ESLint issues
- `npm run format` - Format code with Prettier

### Testing
- Uses Jest with Angular testing utilities
- Test files: `*.spec.ts`
- Custom test setup in `setup-jest.ts` with `SoftOneTestBed`
- Coverage reports generated in `/coverage` directory

## Project Architecture

### High-Level Structure
This is an Angular 19 SPA following a feature-based architecture with a comprehensive UI component library.

**Core Architecture Pattern:**
- `core/` - App-specific services, interceptors, guards, and global components
- `features/` - Business domain modules (billing, economy, time, manage) 
- `shared/` - Reusable components, services, and utilities across features

### Key Features
- **Billing** - Product management, pricing, purchases, sales, inventory
- **Economy** - Accounting, suppliers, invoices, payments, currencies, budgets
- **Time** - Employee management, schedules, holidays, payroll
- **Manage** - System administration, field settings, export functionality

### UI Component Library (`shared/ui-components/`)
Comprehensive component library with:
- **Forms**: Input controls (textbox, numberbox, datepicker, select, etc.)
- **Grid**: AG-Grid wrapper with custom cell editors/renderers
- **Dialogs**: Reusable dialog and messagebox components
- **Navigation**: Tabs, toolbars, record navigation
- **Layout**: Split containers, expansion panels, resize containers

### Path Aliases (Jest & TypeScript)
- `@shared/*` → `src/app/shared/*`
- `@core/*` → `src/app/core/*` 
- `@features/*` → `src/app/features/*`
- `@ui/*` → `src/app/shared/ui-components/*`
- `@src/*` → `src/*`

## Technical Stack

### Frontend
- **Angular 19** with Angular Material and Bootstrap 5
- **AG-Grid Enterprise** for data grids
- **FontAwesome Pro** for icons
- **ngx-translate** for internationalization
- **RxJS** for reactive programming
- **Quill** for rich text editing
- **PDF handling** via ng2-pdf-viewer and pdfmake

### Development Tools
- **Jest** for testing (instead of Karma/Jasmine)
- **ESLint** with Angular and TypeScript rules
- **Prettier** for code formatting
- **SCSS** for styling

## Development Guidelines

### Module Organization
Each feature follows consistent structure:
```
feature-name/
├── components/          # UI components
├── pages/              # Routed pages  
├── services/           # Business logic
├── models/             # TypeScript interfaces/classes
├── feature-routing.module.ts
└── feature.module.ts
```

### UI Component Usage
- Use UI components from `@ui` instead of creating custom form controls
- Grid components should extend base directives (`grid-base.directive.ts`)
- Edit forms should extend `edit-base.directive.ts`

### HTTP & API
- All HTTP calls go through custom interceptors for authentication and data transformation
- Services extend base service classes for consistent patterns
- Use `@core/services` for app-wide services

### SSL Development
Development server runs on HTTPS with ASP.NET Core SSL certificates. The `prestart` script handles certificate setup via `aspnetcore-https.js`.

## Build Output
- Production builds output to `../Soe.Angular/dist/spa/` (integrated with .NET backend)
- Uses custom deploy URL: `/angular/dist/spa/`
- Large bundle size limits configured (20MB max for production)