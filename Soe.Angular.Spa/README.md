## Project structure

core

- guards - Core guards like auth guard etc
- interceptors - Extensions to extend http and apply auth-token to all calls
- services - App specific services like translation service etc
- components - App specific components like header, footer etc
- constants - App specific constants
- (utils|functions) - App specific utils / functions
- core.module.ts

features

- feature_a
  -- pages - Visual pages, should be the ones used in the routing module
  -- components - Subparts of the pages
  -- constants - Feature related constants
  -- services - Feature related services
  -- models - Feature related models
  -- classes - Feature related classes
  -- feature_a.module.ts
  -- feature_a-routing.module.ts
- feature_b
  -- pages - Visual pages, should be the ones used in the routing module
  -- components - Subparts of the pages
  -- constants - Feature related constants
  -- services - Feature related services
  -- models - Feature related models
  -- classes - Feature related classes
  -- feature_b.module.ts
  -- feature_b-routing.module.ts

shared - Parts shared among multiple features (not app specific)

- ui - The component library of the app - should be tree-shakable
- classes - Shared classes for multiple features
- directives - Shared directives for multiple features
- components - Shared components for multiple features
- pipes - Shared pipes for multiple features
- services - Shared servies for multiple features
- constants - Shared constants for multiple features
- shared.module.ts

---

### Component library project structure

forms

- input
  -- input.component.ts
  -- input.component.spec.ts
  -- input.component.scss
  -- input.component.html
  -- input.module.ts
  shared
  forms.module.ts

table

- table.component.ts
- table.component.spec.ts
- table.component.scss
- table.component.html
- table.module.ts
