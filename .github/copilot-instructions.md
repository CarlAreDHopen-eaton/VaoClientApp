# Copilot Instructions

## Project Overview
- This is a .NET 8 Avalonia UI desktop application (with optional Android target) for controlling HERNIS FLEX VAO video surveillance systems.
- The solution contains two projects:
  - **VaoClient** (`Vao.Client` namespace) — a library that wraps the FLEX REST API using RestSharp, manages cameras, alarms, monitors, playback, and downloads.
  - **VaoClientApp** (`Vao.Sample` namespace) — an Avalonia UI front-end with pages, controls, theming (Brightlayer UI design system), and a custom navigation service.
- UI follows the Brightlayer UI design system. Refer to `VaoClientApp/DESIGN.MD` for color tokens, typography, spacing, and component specifications.
- Theming and layout definitions are JSON-driven (see `Themes/` and `Layouts/` folders).

## General Guidelines
- When writing or modifying C# code in this repository, follow the naming conventions and formatting rules outlined below.
- Target .NET 8. Do not use APIs unavailable in `net8.0`.
- Use file-scoped namespaces for new files.
- Prefer explicit types over `var`.
- Within each region or member group, order members alphabetically by name.

## Code Style

### Naming Conventions

| Symbol Kind | Convention | Example |
|---|---|---|
| Private fields (including `readonly`) | `m` prefix + PascalCase | `mVariableName`, `mApiClient`, `mCurrentUser` |
| Parameters | camelCase | `variableName` |
| Local variables | camelCase | `itemCount` |
| Public / internal / protected fields | PascalCase | `CurrentUser` |
| Constants (`const`) | `C_` prefix + UPPER_SNAKE_CASE | `C_MAX_RETRIES` |
| Static readonly fields | PascalCase | `DefaultTimeout` |
| Classes, structs, records, enums, delegates | PascalCase | `FlexApiClient` |
| Interfaces | `I` prefix + PascalCase | `IRequestHandler` |
| Methods and local functions | PascalCase | `GetStatusMessages` |
| Properties | PascalCase | `IsConnected` |
| Events | PascalCase | `OnMessage` |
| Enum members | PascalCase | `AlarmActive` |
| Namespaces | PascalCase | `Vao.Client.Utility` |
| Type parameters | `T` prefix + PascalCase | `TItem`, `TResult` |

### Additional Rules
- Boolean members, properties, and variables should use clear prefixes when appropriate: `Is`, `Has`, `Can`, `Should`.
- Async methods must end with the `Async` suffix.
- Avoid abbreviations unless they are well-known and clear (for example `Http`, `Uri`, `Ptz`).
- Use `nameof` instead of string literals when referring to member names (e.g., `nameof(PropertyName)` in argument exceptions, `OnPropertyChanged`, etc.).
- When writing C# property setters (or any block with multiple statements), always expand them onto separate lines. Never collapse multiple statements into a single line within braces.
- Use `#region` / `#endregion` blocks consistent with the existing codebase. Order class members as follows:

  1. `Private Members` — private and private readonly fields
  2. `Public Events` — event declarations
  3. `Constructors`
  4. `Public Properties`
  5. `Public Methods`
  6. `Internal Methods`
  7. `Private Methods`
- Write clear and concise XML documentation comments (`/// <summary>`) for every public and internal method. Include `<param>` and `<returns>` tags when applicable. Keep descriptions brief — one or two sentences is sufficient.

## Avalonia UI Guidelines
- AXAML files use `DynamicResource` for all theme-sensitive values (colors, brushes, font sizes).
- Do not hard-code colors or font sizes in AXAML or code-behind; reference design tokens defined in `DESIGN.MD`.
- Pages implement `INavigableView` and inherit from `NavigableViewBase`.

## Dependencies
- **RestSharp** for REST API communication in VaoClient.
- **Newtonsoft.Json** / **Json.Net** for JSON serialization in VaoClient.
- **FluentFTP** for FTP-based downloads in VaoClient.
- Do not add new NuGet packages without justification; prefer existing dependencies.

## Compatibility
- Do not use underscore-prefixed private fields (for example `_variableName`) unless existing code in the same file already requires it for compatibility.
