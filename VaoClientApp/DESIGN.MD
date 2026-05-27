# HERNIS FLEX VAO Client - UI Design Specification

> **Strictly follows [Brightlayer UI](https://brightlayer-ui.github.io/) Design System by Eaton**

## Table of Contents

1. [Design Philosophy](#design-philosophy)
2. [Color System](#color-system)
3. [Typography](#typography)
4. [Spacing & Layout](#spacing--layout)
5. [Component Specifications](#component-specifications)
6. [Page Layouts](#page-layouts)
7. [Accessibility](#accessibility)
8. [Implementation Classes](#implementation-classes)

---

## Design Philosophy

This application **strictly adheres** to the Brightlayer UI design system. All components, colors, spacing, and interactions must follow these guidelines without deviation.

### Core Principles

| Principle | Description |
|-----------|-------------|
| **Consistency** | All UI elements use predefined tokens and classes |
| **Hierarchy** | Visual importance conveyed through color, size, and emphasis |
| **Accessibility** | WCAG 2.1 AA compliant - no exceptions |
| **Theme Support** | Full light/dark theme support using dynamic resources |

---

## Color System

> ⚠️ **Never use hardcoded colors.** Always use `{DynamicResource TokenName}`.
>
> Actual color values are defined in the theme JSON files under `VaoClientApp/Themes/`.
> See `VaoClientApp/Themes/THEMES.MD` for theme file documentation.

### Accent Colors

| Token | Usage |
|-------|-------|
| `Primary` | Primary actions, app bar, active states, links |
| `PrimaryLight` | Hover states only |
| `PrimaryDark` | Pressed states, user avatars |
| `PrimaryHeaderFg` | App bar text (kept readable across themes) |

### Semantic/Status Colors

| Token | Usage |
|-------|-------|
| `Success` | Success states, live indicators, confirmations |
| `Warning` | Warnings, caution states |
| `Error` | Errors, destructive actions, playback mode |
| `Info` | Informational messages |

### Background & Surface Colors

| Token | Usage |
|-------|-------|
| `AppBg` | Application background |
| `Surface1` | Elevation level 1 (content sections, cards) |
| `Surface2` | Elevation level 2 |
| `Surface3` | Elevation level 3 |

### Text Colors (Opacity-Based)

| Token | Usage |
|-------|-------|
| `TextPrimary` | High emphasis text (list item names, primary content) |
| `SidebarHeaderFg` | Section header text |
| `LabelFg` | Medium emphasis text (labels, secondary content) |
| `TextSecondary` | Disabled/hint text |
| `ListItemIconFg` | List item icons (camera type, preset, split icons) |
| `ListItemNumberFg` | List item number/ID text (#0001, #001) |

### Border Colors

| Token | Usage |
|-------|-------|
| `Divider` | General dividers and separators |
| `SidebarBorder` | Sidebar border |

### Sidebar Colors

| Token | Usage |
|-------|-------|
| `SidebarBg` | Navigation drawer background |
| `SidebarCardBg` | Sidebar section card background |
| `SidebarBorder` | Sidebar border |
| `SidebarHeaderFg` | Sidebar header text |

### Video Panel Colors

| Token | Usage |
|-------|-------|
| `VideoBg` | Video player background |
| `VideoHeaderBg` | Video panel header background |
| `VideoHeaderNeutral` | Header idle/neutral state |
| `VideoHeaderLive` | Header live streaming state |
| `VideoHeaderPlayback` | Header playback state |

### Message Log Colors

| Token | Usage |
|-------|-------|
| `MessageLogBackground` | Log panel background |
| `MessageErrorForeground` | Error messages |
| `MessageWarningForeground` | Warning messages |
| `MessageDebugForeground` | Debug messages |
| `MessageDefaultForeground` | Default messages |

### User Profile Colors

| Token | Usage |
|-------|-------|
| `UserProfileConnectedBackground` | Connected user avatar |
| `UserProfileDisconnectedBackground` | Disconnected user avatar |

### Camera Lock Colors

| Token | Usage |
|-------|-------|
| `CameraLockAlarmForeground` | Alarm-locked camera indicator |
| `CameraLockManualForeground` | User-locked camera indicator |

### Camera Selection Colors

| Token | Usage |
|-------|-------|
| `CameraSelectedBackground` | Selected camera button background |
| `CameraSelectedHoverBackground` | Selected camera button hover state |

### Alarm Status Colors

| Token | Usage |
|-------|-------|
| `AlarmStatusActive` | Active alarm |
| `AlarmStatusInactive` | Inactive alarm |
| `AlarmStatusAcknowledged` | Acknowledged alarm |
| `AlarmStatusTampered` | Tampered alarm |
| `AlarmStatusDefault` | Default alarm state |

---

## Typography

### Font Family (Brightlayer UI Specification)

**Primary Font: Open Sans (Embedded)**

Brightlayer UI specifies **Open Sans** as the primary typeface. This application **bundles Open Sans fonts** as embedded resources to ensure consistent typography across all platforms.

**Embedded Font Files:**
- `OpenSans-Regular.ttf` (400 weight)
- `OpenSans-Medium.ttf` (500 weight)
- `OpenSans-SemiBold.ttf` (600 weight)
- `OpenSans-Bold.ttf` (700 weight)
- `MaterialSymbolsOutlined.ttf` (Icon font - Material Symbols from Brightlayer UI)
- `BrightlayerUIIcons.ttf` (Icon font - Brightlayer UI custom icons)

**Location:** `VaoClientApp/Assets/Fonts/`

**Font URI:** `avares://VaoClientApp/Assets/Fonts#Open Sans`

The application uses the following font stack with system fallbacks:

```
avares://VaoClientApp/Assets/Fonts#Open Sans, Segoe UI, San Francisco, Roboto, Helvetica Neue, Arial, sans-serif
```

> **Theme Font Override:** The font family can be overridden per theme by setting the optional `FontFamily` field in the theme JSON file. When specified, it replaces the default Open Sans font stack for the entire application. See [`Themes/THEMES.MD`](Themes/THEMES.MD) for details.

**Monospace Font: Consolas**

For code, logs, and technical data:

```
Consolas, Monaco, Courier New, monospace
```

### Font Resources

| Resource Key | Font Stack |
|--------------|------------|
| `BrightlayerFont` | avares://VaoClientApp/Assets/Fonts#Open Sans, Segoe UI, San Francisco, Roboto, Helvetica Neue, Arial, sans-serif |
| `MonospaceFont` | Consolas, Monaco, Courier New, monospace |
| `MaterialSymbolsFont` | avares://VaoClientApp/Assets/Fonts#Material Symbols Outlined |
| `BrightlayerIconFont` | avares://VaoClientApp/Assets/Fonts#BrightlayerUIIcons |

### Type Scale (Brightlayer UI)

> Font sizes are defined in theme JSON files under `Sizing.Typography`. The values below describe the **role** of each style class.

| Class | Weight | Usage |
|-------|--------|-------|
| App Title | SemiBold (600) | App bar title (`PageHeaderTitleFontSize`) |
| Subtitle | Regular (400) | App bar subtitle, opacity 0.7 (`PageHeaderSubtitleFontSize`) |
| `.section-header` | SemiBold (600) | Section headings (`SectionHeaderFontSize`) |
| `.subtitle1` | SemiBold (600) | Emphasized subtitles |
| `.subtitle2` | SemiBold (600) | Secondary subtitles |
| `.body1` | Regular (400) | Primary body text |
| `.body2` | Regular (400) | Secondary body text |
| `.label` | Regular (400) | Form labels |
| `.caption` | Regular (400) | Captions, hints |
| `.overline` | SemiBold (600) | Overline text, uppercase |
| `.monospace` | Regular (400) | Code, logs |
| Button | SemiBold (600) | Button text, all button classes |

### Font Weights

| Weight Name | Value | Usage |
|-------------|-------|-------|
| Regular | 400 | Body text, labels |
| Medium | 500 | App titles |
| SemiBold | 600 | Buttons, headers, emphasis |
| Bold | 700 | Strong emphasis (rare) |

### Font Embedding Implementation

The Open Sans fonts are embedded as AvaloniaResource in the project file to ensure they are always available:

```xml
<!-- VaoClientApp.csproj -->
<ItemGroup>
  <AvaloniaResource Include="Assets\Fonts\*.ttf" />
</ItemGroup>
```

All UI elements reference the embedded fonts via the `avares://` URI scheme:

```xml
<!-- App.axaml -->
<Style Selector="Window">
  <Setter Property="FontFamily" Value="avares://VaoClientApp/Assets/Fonts#Open Sans, Segoe UI, ..."/>
</Style>
```

**Benefits:**
- ✅ Consistent typography across all platforms (Windows, macOS, Linux)
- ✅ No dependency on system-installed fonts
- ✅ Guaranteed Brightlayer UI compliance
- ✅ Fonts bundled with the application executable

### Iconography (Brightlayer UI)

This application uses **font-based icons** from the Brightlayer UI icon packages, eliminating the need for PNG bitmap icons and ensuring crisp rendering at all sizes and DPI scales.

**Icon Fonts:**

| Font | Usage | Style Class |
|------|-------|-------------|
| Material Symbols Outlined | General UI icons (menu, arrows, zoom, settings, etc.) | `.ms-icon` |
| Brightlayer UI Icons | Industry-specific icons (camera, video streaming, lock levels, etc.) | `.blui-icon` |

**Usage in AXAML:**

```xml
<!-- Material Symbols icon (e.g., menu hamburger) -->
<TextBlock Text="&#xE5D2;" Classes="ms-icon" FontSize="24"/>

<!-- Brightlayer UI icon (e.g., camera) -->
<TextBlock Text="&#xF299;" Classes="blui-icon" FontSize="24"/>
```

**Icon Mapping:**

| Icon | Font | Codepoint | Usage |
|------|------|-----------|-------|
| menu | Material Symbols | `&#xE5D2;` | Hamburger menu |
| gamepad | Material Symbols | `&#xE30F;` | Camera control section |
| photo_camera | Material Symbols | `&#xE412;` | Camera selection section |
| pin_drop | Material Symbols | `&#xE55E;` | Preset selection section |
| notifications | Material Symbols | `&#xE7F5;` | Alarms section |
| play_arrow | Material Symbols | `&#xE037;` | Playback section |
| download | Material Symbols | `&#xF090;` | Download section |
| arrow_upward | Material Symbols | `&#xE5D8;` | Tilt up |
| arrow_downward | Material Symbols | `&#xE5DB;` | Tilt down |
| arrow_back | Material Symbols | `&#xE5C4;` | Pan left |
| arrow_forward | Material Symbols | `&#xE5C8;` | Pan right |
| zoom_in | Material Symbols | `&#xE8FF;` | Zoom in |
| zoom_out | Material Symbols | `&#xE900;` | Zoom out |
| flip_to_front | Material Symbols | `&#xE883;` | Focus near |
| flip_to_back | Material Symbols | `&#xE882;` | Focus far |
| control_camera | Material Symbols | `&#xE074;` | Camera center / absolute position |
| lock_open | Material Symbols | `&#xE898;` | Camera unlocked |
| lock | Material Symbols | `&#xE899;` | Camera locked |
| settings | Material Symbols | `&#xE8B8;` | Settings menu item |
| person | Material Symbols | `&#xF0D3;` | User profile |
| key | Material Symbols | `&#xE73C;` | Login |
| logout | Material Symbols | `&#xE9BA;` | Logout |
| calendar_today | Material Symbols | `&#xE935;` | Date picker |
| schedule | Material Symbols | `&#xE8B5;` | Time picker |

> ⚠️ **Never use emoji characters for icons.** Always use the embedded icon fonts with the `ms-icon` or `blui-icon` style classes.

---

## Spacing & Layout

### Layout Resource Tokens

> All dimension values are defined in theme JSON files under `Sizing` and `Components`.
> Use `{DynamicResource TokenName}` to reference them.

| Token | Usage |
|-------|-------|
| `AppBarHeight` | App bar height |
| `SidebarWidth` | Sidebar expanded width |
| `ButtonMinHeight` | Minimum button height |
| `ButtonPadding` | Button padding (Thickness) |
| `InputMinHeight` | Minimum input field height |
| `InputPadding` | Input field padding (Thickness) |
| `ListItemMinHeight` | Minimum list item height |
| `AvatarSize` | User avatar diameter |
| `AvatarCornerRadius` | Avatar corner radius (circular) |
| `IconSize` | Default icon size |
| `IconSizeSmall` | Small icon size |
| `PageMargin` | Page-level margin (Thickness) |
| `PageMarginNoBottom` | Page margin without bottom (Thickness) |
| `SidebarMenuHeaderFontSize` | Sidebar menu header font size |
| `SidebarMenuHeaderMinHeight` | Sidebar menu header minimum height |
| `SidebarMenuHeaderPadding` | Sidebar menu header padding (Thickness) |
| `SidebarMenuItemHeight` | Sidebar menu item height |
| `SidebarMenuSearchHeight` | Sidebar search field height |
| `SectionHeaderFontSize` | Section header font size |
| `PageHeaderTitleFontSize` | Page/app bar title font size |
| `PageHeaderSubtitleFontSize` | Page/app bar subtitle font size |

### Layout Structure

```
┌─────────────────────────────────────────────────────────┐
│                 APP BAR (Primary)                       │
│  [☰]  Title                                       [👤]  │
│       Subtitle (opacity: 0.7)                           │
├──────────────┬──────────────────────────────────────────┤
│              │                                          │
│   SIDEBAR    │           CONTENT AREA                   │
│              │                                          │
│              │  ┌────────────────────────────────────┐  │
│  [Expander]  │  │    VIDEO PLAYER                    │  │
│  [Expander]  │  │    Header: VideoHeaderBg           │  │
│  [Expander]  │  │    Body: VideoBg                   │  │
│              │  └────────────────────────────────────┘  │
│              │  ─────────── GridSplitter ─────────────  │
│              │  ┌────────────────────────────────────┐  │
│              │  │    MESSAGES (Surface2 header)      │  │
│              │  └────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## Component Specifications

### App Bar

| Property | Value |
|----------|-------|
| Height | `{DynamicResource AppBarHeight}` |
| Background | `{DynamicResource Primary}` |
| Title Font | `PageHeaderTitleFontSize`, Medium, `PrimaryHeaderFg` |
| Subtitle Font | `PageHeaderSubtitleFontSize`, Regular, `PrimaryHeaderFg`, opacity 0.7 |

### Buttons (Strict Hierarchy)

**Use the correct button style based on action importance:**

| Class | Emphasis | Usage | Visual |
|-------|----------|-------|--------|
| `.primary` | High | Primary actions (Save, Submit, Go) | Filled blue |
| `.outlined` | Medium | Secondary actions (Cancel, Stop) | Blue border |
| `.text` | Low | Tertiary actions | Blue text only |
| `.icon` | - | Icon-only buttons, toolbar | Transparent, circular hover |
| `.fab` | High | Floating action buttons | Circular, filled |

#### Button Specifications

```
MinHeight: {DynamicResource ButtonMinHeight}
Padding: {DynamicResource ButtonPadding}
CornerRadius: 4px
FontWeight: SemiBold (600)
```

#### Button States

| State | `.primary` | `.outlined` |
|-------|------------|-------------|
| Default | Primary bg, white text | Transparent, Primary border |
| Hover | PrimaryLight bg | Primary bg, white text |
| Pressed | PrimaryDark bg | PrimaryDark bg |
| Disabled | 38% opacity | 38% opacity |

#### Dialog Button Placement (Mandatory)

```
                              [Cancel]  [Save]
                              outlined  primary
```

- **Alignment**: Right
- **Order**: Secondary ? Primary (left to right)
- **Spacing**: 12px between buttons
- **Min Width**: 100px

### Expander Sections

| State | Background | Text Color |
|-------|------------|------------|
| Collapsed | Transparent | `SidebarHeaderFg` |
| Expanded | `Primary` | White |

**Header Structure:**
```
┌─────────────────────────────────────────┐
│ [Icon]  Section Title              [▾]  │
├─────────────────────────────────────────┤
│ Content Area                            │
│                                         │
└─────────────────────────────────────────┘
```

### Form Controls

| Control | MinHeight | Padding | CornerRadius |
|---------|-----------|---------|--------------|
| TextBox | `InputMinHeight` | `InputPadding` | 4px |
| ComboBox | `InputMinHeight` | `InputPadding` | 4px |
| CheckBox | auto | default | - |

### Cards/Sections

```
Background: SidebarCardBg
BorderBottom: 1px Divider
CornerRadius: 0
Margin: 0 (flush layout)
```

### Dialog Windows

```
┌─────────────────────────────────────────┐
│ ┌─────────── HEADER ─────────────────┐ │
│ │  Title (PageHeaderTitleFontSize)    │ │
│ │  Subtitle (PageHeaderSubtitleFontSize) │ │
├─────────────────────────────────────────┤
│                                         │
│   Content                               │
│                                         │
│   ─────────────────────────             │  Dividers between sections
│                                         │
│   More content...                       │
│                                         │
├─────────────────────────────────────────┤
│              [Cancel] [Save]            │  Footer (Surface1)
└─────────────────────────────────────────┘
```

### User Avatar

| Property | Value |
|----------|-------|
| Size | `{DynamicResource AvatarSize}` |
| CornerRadius | `{DynamicResource AvatarCornerRadius}` (circular) |
| Background | `PrimaryDark` (connected) / `UserProfileDisconnectedBackground` |
| Text | SemiBold, White |
| Content | First letter of username |

---

## Page Layouts

### Main Window

| Region | Width | Background |
|--------|-------|------------|
| Sidebar (expanded) | `SidebarWidth` | `SidebarBg` |
| Sidebar (collapsed) | - | `Surface1` |
| Content | Flexible | `AppBg` |
| Video Panel | Flexible | `Surface1` |
| Messages Panel | Flexible | `Surface1` |

### Settings Dialog

| Property | Value |
|----------|-------|
| Width | 500px |
| Height | 550px |
| MinWidth | 450px |
| MinHeight | 500px |
| Resizable | No |

---

## Accessibility

### Color Contrast Requirements (WCAG 2.1 AA)

| Element | Required Ratio | Achieved |
|---------|----------------|----------|
| Body text | 4.5:1 | ✅ 7:1+ |
| Large text (18px+) | 3:1 | ✅ 7:1+ |
| UI components | 3:1 | ✅ 4.5:1+ |

### Focus Indicators

- All interactive elements must have visible focus states
- Focus ring color: `Primary` or system default
- Tab order follows visual layout

### Keyboard Navigation

| Key | Action |
|-----|--------|
| Tab | Move focus forward |
| Shift+Tab | Move focus backward |
| Enter/Space | Activate button/control |
| Escape | Close dialog/cancel |
| F9 | Toggle sidebar |

---

## Implementation Classes

### CSS/Style Classes

| Class | Usage |
|-------|-------|
| `.sidebar-shell` | Main sidebar container |
| `.sidebar-card` | Sidebar section wrapper |
| `.sidebar-section` | Expander with blue active state |
| `.primary` | Primary (filled) button |
| `.outlined` | Outlined button |
| `.text` | Text button |
| `.icon` | Icon button (circular hover) |
| `.fab` | Floating action button |
| `.section-header` | Section title text |
| `.label` | Form label text |
| `.caption` | Caption/hint text |
| `.theme-icon` | Icon with theme opacity |

### Dynamic Resources

All visual properties must use `DynamicResource`:

```xml
<!-- ✅ Correct -->
<Border Background="{DynamicResource Primary}"/>

<!-- ❌ Wrong - Never hardcode colors -->
<Border Background="#007BC1"/>
```

### Theme Switching

```csharp
// Switch to dark theme
Application.Current.RequestedThemeVariant = ThemeVariant.Dark;

// Switch to light theme
Application.Current.RequestedThemeVariant = ThemeVariant.Light;
```

---

## References

- [Brightlayer UI Documentation](https://brightlayer-ui.github.io/)
- [Brightlayer UI Design Patterns](https://brightlayer-ui.github.io/patterns)
- [Brightlayer UI Style Guide](https://brightlayer-ui.github.io/style)
- [Brightlayer UI Components](https://brightlayer-ui.github.io/development/components)
- [Material Design 3](https://m3.material.io/) (underlying foundation)
