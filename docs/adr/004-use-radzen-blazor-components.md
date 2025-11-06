# ADR 004: Use Radzen Blazor for UI Components

## Status
Accepted (with future migration consideration to FluentUI)

## Context
The Blazor WebAssembly application requires a comprehensive UI component library for:
- Responsive layouts and grids
- Forms and input controls
- Data visualization (charts, leaderboards)
- Navigation components
- Loading and error states

We evaluated several Blazor component libraries:
- **Microsoft FluentUI**: Official Microsoft design system, modern, actively developed
- **Radzen Blazor**: Mature, extensive component library, free and open source
- **MudBlazor**: Material Design, good community, MIT licensed
- **Telerik UI for Blazor**: Commercial, comprehensive, expensive

## Decision
We will use **Radzen Blazor** as our primary UI component library for the initial release.

## Rationale
1. **Comprehensive Components**: Over 90+ components covering all common UI patterns
2. **Free & Open Source**: MIT licensed, no cost constraints
3. **Mature & Stable**: Well-tested in production applications
4. **Good Documentation**: Clear examples and API documentation
5. **Active Maintenance**: Regular updates and bug fixes
6. **Theme Support**: Multiple built-in themes (Material, Fluent, Bootstrap)
7. **Data Components**: Excellent DataGrid, Charts, and Scheduler components

## Consequences

### Positive
- Rapid UI development with pre-built, tested components
- Consistent look and feel across the application
- Saves development time (no need to build custom components)
- Responsive design built-in
- Accessible components (WCAG compliance)
- No licensing costs

### Negative
- Additional bundle size (~300-500KB gzipped)
- Learning curve for Radzen-specific APIs
- Potential vendor lock-in (component-specific code)
- Not the "official" Microsoft design system
- May require future migration if FluentUI becomes mandatory

## Implementation Notes

### Installation
```powershell
dotnet add package Radzen.Blazor
```

### Registration
```csharp
// Program.cs (Client)
builder.Services.AddRadzenComponents();
```

### Imports
```razor
@using Radzen
@using Radzen.Blazor
```

### Theme Selection
Use the **Material** theme for a modern, clean aesthetic:
```html
<link href="_content/Radzen.Blazor/css/material-base.css" rel="stylesheet" />
```

## Future Considerations

### Migration to FluentUI
The architectural guidelines specify **Microsoft.FluentUI.AspNetCore.Components** as the preferred UI library. We should consider migrating when:
1. FluentUI reaches feature parity with Radzen for our use cases
2. FluentUI's component maturity and stability improve
3. We have dedicated time for UI refactoring

**Migration Strategy**:
- Component-by-component replacement (gradual migration)
- Start with simpler components (buttons, inputs)
- Keep both libraries during transition period
- Update styling to match Fluent design system

### Evaluation Criteria for Migration
- [ ] FluentUI has DataGrid with sorting/filtering
- [ ] FluentUI has Chart components
- [ ] FluentUI documentation is comprehensive
- [ ] FluentUI has mobile-responsive components
- [ ] Migration effort is justified by business value

## Alternatives Considered

### Microsoft FluentUI
- **Pros**: Official Microsoft design system, modern, growing ecosystem
- **Cons**: Fewer components, less mature, limited documentation
- **Why not chosen (now)**: Not feature-complete for our requirements, will reconsider in future

### MudBlazor
- **Pros**: Beautiful Material Design, active community, MIT licensed
- **Cons**: Material Design may not align with Microsoft ecosystem
- **Why not chosen**: Radzen offers more components and themes

### Telerik UI for Blazor
- **Pros**: Enterprise-grade, extensive components, excellent support
- **Cons**: Expensive (~$1,000/developer/year), commercial license
- **Why not chosen**: Cost-prohibitive for demonstration project

## References
- [Radzen Blazor Components](https://blazor.radzen.com/)
- [Radzen GitHub Repository](https://github.com/radzenhq/radzen-blazor)
- [Microsoft FluentUI Blazor](https://www.fluentui-blazor.net/)
- [Blazor Component Library Comparison](https://github.com/AdrienTorris/awesome-blazor#component-libraries)
