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

### Brand Colors (Eaton Blue)

| Token | Value | Usage |
|-------|-------|-------|
| `Primary` | `#007BC1` | Primary actions, app bar, active states, links |
| `PrimaryLight` | `#0088D0` | Hover states only |
| `PrimaryDark` | `#006BA1` | Pressed states, user avatars |

> ?? **Never use hardcoded colors.** Always use `{DynamicResource TokenName}`.

### Semantic Colors

| Token | Value | Usage |
|-------|-------|-------|
| `Success` | `#39B620` | Success states, live indicators, confirmations |
| `Warning` | `#F0AA1F` | Warnings, caution states |
| `Error` | `#CA3C3D` | Errors, destructive actions, playback mode |
| `Info` | `#007BC1` | Informational messages (same as Primary) |

### Background Colors

#### Dark Theme

| Token | Value | Description |
|-------|-------|-------------|
| `AppBg` | `#121212` | Application background |
| `SectionBg` | `#1E1E1E` | Content sections, cards |
| `SidebarBg` | `#121212` | Navigation drawer |
| `SidebarCardBg` | `#1E1E1E` | Sidebar sections |
| `VideoBg` | `#000000` | Video player |
| `VideoHeaderBg` | `#1E1E1E` | Video panel header |
| `Surface1` | `#1E1E1E` | Elevation level 1 |
| `Surface2` | `#2C2C2C` | Elevation level 2 |
| `Surface3` | `#3D3D3D` | Elevation level 3 |

#### Light Theme

| Token | Value | Description |
|-------|-------|-------------|
| `AppBg` | `#F4F4F4` | Application background |
| `SectionBg` | `#FFFFFF` | Content sections, cards |
| `SidebarBg` | `#FFFFFF` | Navigation drawer |
| `SidebarCardBg` | `#FFFFFF` | Sidebar sections |
| `VideoBg` | `#E0E0E0` | Video player |
| `VideoHeaderBg` | `#007BC1` | Video panel header (Primary) |
| `Surface1` | `#FFFFFF` | Elevation level 1 |
| `Surface2` | `#F4F4F4` | Elevation level 2 |
| `Surface3` | `#EBEBEB` | Elevation level 3 |

### Text Colors (Opacity-Based)

| Token | Dark Theme | Light Theme | Usage |
|-------|------------|-------------|-------|
| `SidebarHeaderFg` | `rgba(255,255,255,0.87)` | `rgba(0,0,0,0.87)` | High emphasis |
| `LabelFg` | `rgba(255,255,255,0.60)` | `rgba(0,0,0,0.60)` | Medium emphasis |
| `TextSecondary` | `rgba(255,255,255,0.38)` | `rgba(0,0,0,0.38)` | Disabled/hint |

### Border Colors

| Token | Dark Theme | Light Theme |
|-------|------------|-------------|
| `Divider` | `rgba(255,255,255,0.12)` | `rgba(0,0,0,0.12)` |
| `SidebarBorder` | `rgba(255,255,255,0.12)` | `#E0E0E0` |

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
| `BrightlayerIconFont` | avares://VaoClientApp/Assets/Fonts#Brightlayer UI Icons |

### Type Scale (Brightlayer UI)

| Class | Size | Weight | Usage |
|-------|------|--------|-------|
| App Title | 20px | Medium (500) | App bar title |
| Subtitle | 12px | Regular (400) | App bar subtitle, opacity 0.7 |
| `.section-header` | 16px | SemiBold (600) | Section headings |
| `.subtitle1` | 16px | SemiBold (600) | Emphasized subtitles |
| `.subtitle2` | 14px | SemiBold (600) | Secondary subtitles |
| `.body1` | 16px | Regular (400) | Primary body text |
| `.body2` | 14px | Regular (400) | Secondary body text |
| `.label` | 12px | Regular (400) | Form labels |
| `.caption` | 11px | Regular (400) | Captions, hints |
| `.overline` | 10px | SemiBold (600) | Overline text, uppercase |
| `.monospace` | inherit | Regular (400) | Code, logs |
| Button | 14px | SemiBold (600) | Button text |

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

### Spacing Scale (8px Grid - Mandatory)

| Token | Value | Usage |
|-------|-------|-------|
| `Spacing-xs` | 4px | Icon gaps, tight spacing |
| `Spacing-sm` | 8px | Default element spacing |
| `Spacing-md` | 16px | Section padding, margins |
| `Spacing-lg` | 24px | Large section spacing, dialog padding |
| `Spacing-xl` | 32px | Page-level spacing |

### Fixed Dimensions

| Element | Size |
|---------|------|
| `AppBarHeight` | 64px |
| `SidebarWidth` | 280px |
| `SidebarCollapsedWidth` | 56px |
| `ButtonMinHeight` | 36px |
| `InputMinHeight` | 36px |
| `ListItemMinHeight` | 48px |
| `AvatarSize` | 40px |
| `IconSize` | 24px |
| `IconSizeSmall` | 20px |

### Layout Structure

```
???????????????????????????????????????????????????????????
?                 APP BAR (64px, Primary)                 ?
?  [?]  Title                                       [??]  ?
?       Subtitle (opacity: 0.7)                           ?
???????????????????????????????????????????????????????????
?              ?                                          ?
?   SIDEBAR    ?           CONTENT AREA                   ?
?   (280px)    ?                                          ?
?              ?  ??????????????????????????????????????  ?
?  [Expander]  ?  ?    VIDEO PLAYER                    ?  ?
?  [Expander]  ?  ?    Header: VideoHeaderBg           ?  ?
?  [Expander]  ?  ?    Body: VideoBg                   ?  ?
?              ?  ??????????????????????????????????????  ?
?              ?  ?????????? GridSplitter ??????????????? ?
?              ?  ??????????????????????????????????????  ?
?              ?  ?    MESSAGES (Surface2 header)      ?  ?
?              ?  ??????????????????????????????????????  ?
???????????????????????????????????????????????????????????
```

---

## Component Specifications

### App Bar

| Property | Value |
|----------|-------|
| Height | 64px (fixed) |
| Background | `{DynamicResource Primary}` |
| Shadow | `0 2 4 0 #40000000` |
| Left Padding | 8px |
| Right Padding | 16px |
| Title Font | 20px, Medium, White |
| Subtitle Font | 12px, Regular, White, opacity 0.7 |

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
MinHeight: 36px
Padding: 16px horizontal, 8px vertical
CornerRadius: 4px
FontWeight: Medium (500)
FontSize: 14px
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
???????????????????????????????????????????
? [Icon]  Section Title              [?]  ?  48px height
???????????????????????????????????????????
? Content Area                            ?  16px padding
? (Margin: 16,8,16,16)                    ?
???????????????????????????????????????????
```

### Form Controls

| Control | MinHeight | Padding | CornerRadius |
|---------|-----------|---------|--------------|
| TextBox | 36px | 12,8 | 4px |
| ComboBox | 36px | 12,8 | 4px |
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
???????????????????????????????????????????
? ???????????? HEADER (64px) ???????????? ?
? ??  Title (20px, Medium)              ???
? ??  Subtitle (12px, opacity 0.7)      ???
???????????????????????????????????????????
?                                         ?
?   Content (24px padding)                ?
?                                         ?
?   ?????????????????????????             ?  Dividers between sections
?                                         ?
?   More content...                       ?
?                                         ?
???????????????????????????????????????????
?              [Cancel] [Save]            ?  Footer (Surface1, 24,16 padding)
???????????????????????????????????????????
```

### User Avatar

| Property | Value |
|----------|-------|
| Size | 40x40px |
| CornerRadius | 20px (circular) |
| Background | `PrimaryDark` |
| Text | 18px, SemiBold, White |
| Content | First letter of username |

---

## Page Layouts

### Main Window

| Region | Width | Background |
|--------|-------|------------|
| Sidebar (expanded) | 280px | `SidebarBg` |
| Sidebar (collapsed) | 56px | `Surface1` |
| Content | Flexible | `AppBg` |
| Video Panel | Flexible | `SectionBg` |
| Messages Panel | Flexible | `SectionBg` |

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
| Body text | 4.5:1 | ? 7:1+ |
| Large text (18px+) | 3:1 | ? 7:1+ |
| UI components | 3:1 | ? 4.5:1+ |

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
<!-- ? Correct -->
<Border Background="{DynamicResource Primary}"/>

<!-- ? Wrong - Never hardcode colors -->
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
