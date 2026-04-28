# Theme Specification

Each theme is a single JSON file in the `Themes/` directory. The application loads all `*.json` files at startup and makes them available for switching at runtime.

## File Identity

| Field | Type | Description |
|-------|------|-------------|
| `Key` | string | Unique identifier, e.g. `dark-desktop` |
| `DisplayName` | string | Human-readable name shown in the UI |
| `Variant` | `"Dark"` or `"Light"` | Controls dark/light mode logic |
| `FormFactor` | `"Desktop"` or `"Tablet"` | Controls touch-target and density adjustments |

---

## Colors

Global color tokens used throughout the application. Components should reference these tokens rather than hardcoding hex values.

### Colors.Accent

The primary interactive color of the application (Eaton Blue).

| Token | Usage |
|-------|-------|
| `Default` | Buttons, links, active states, app bar |
| `Hover` | Hover / focus states |
| `Pressed` | Pressed / active states |

### Colors.Status

Semantic colors that convey meaning.

| Token | Usage |
|-------|-------|
| `Success` | Confirmations, live indicators |
| `Warning` | Caution states |
| `Error` | Errors, destructive actions |
| `Info` | Informational messages |

### Colors.Background

Layered backgrounds that create visual depth. Listed from lowest to highest elevation.

| Token | Elevation | Usage |
|-------|-----------|-------|
| `App` | Base | Full-window background |
| `Raised` | +1 | Cards, sections, panels |
| `Elevated` | +2 | Dropdowns, popovers |
| `Overlay` | +3 | Modals, dialogs |

### Colors.Foreground

Text colors at different emphasis levels.

| Token | Usage |
|-------|-------|
| `Primary` | Headings, body text, high-emphasis labels |
| `Secondary` | Supporting text, field labels |
| `Hint` | Placeholder text, disabled labels |

### Colors.Border

| Token | Usage |
|-------|-------|
| `Default` | Standard dividers and borders |
| `Secondary` | Low-emphasis separators |

---

## Sizing

Numeric dimensions (in device-independent pixels) that control spacing, typography, and control sizes.

### Sizing.Page

| Token | Description |
|-------|-------------|
| `MarginHorizontal` | Left/right page padding |
| `MarginVertical` | Top/bottom page padding |

### Sizing.Typography

Font sizes for structural text elements.

| Token | Description |
|-------|-------------|
| `PageTitle` | Main page heading |
| `PageSubtitle` | Secondary page heading |
| `SectionHeader` | Section divider labels |

### Sizing.Icon

| Token | Description |
|-------|-------------|
| `Default` | Standard icon size |
| `Small` | Inline or secondary icons |

### Sizing.Button

| Token | Description |
|-------|-------------|
| `MinHeight` | Minimum touch/click target |
| `PaddingHorizontal` | Left/right inner spacing |
| `PaddingVertical` | Top/bottom inner spacing |

### Sizing.Input

| Token | Description |
|-------|-------------|
| `MinHeight` | Minimum field height |
| `PaddingHorizontal` | Left/right inner spacing |
| `PaddingVertical` | Top/bottom inner spacing |

### Sizing.ListItem

| Token | Description |
|-------|-------------|
| `MinHeight` | Minimum row height |

### Sizing.Avatar

| Token | Description |
|-------|-------------|
| `Size` | Width and height |
| `CornerRadius` | Border radius (set to half of `Size` for a circle) |

---

## Components

Component-specific overrides. These extend or override the global `Colors` and `Sizing` tokens for particular UI areas.

### Components.AppBar

| Token | Description |
|-------|-------------|
| `Height` | App bar height |

### Components.Sidebar

| Token | Description |
|-------|-------------|
| `Width` | Sidebar panel width |
| `SectionHeaderFontSize` | Font size for sidebar group headers |
| `Colors.Background` | Sidebar background |
| `Colors.CardBackground` | Background for cards within the sidebar |
| `Colors.Border` | Sidebar edge border |
| `Colors.HeaderForeground` | Sidebar header text color |

### Components.VideoPanel

| Token | Description |
|-------|-------------|
| `Colors.Background` | Video area background |
| `Colors.HeaderBackground` | Default header bar background |
| `Colors.HeaderNeutral` | Header color when idle |
| `Colors.HeaderLive` | Header color during live view |
| `Colors.HeaderPlayback` | Header color during playback |

### Components.MessageLog

Each severity level (`Error`, `Warning`, `Debug`, `Default`) has:

| Token | Description |
|-------|-------------|
| `Foreground` | Text color |
| `Background` | Row highlight color |

### Components.UserProfile

| Token | Description |
|-------|-------------|
| `ConnectedBackground` | Badge color when connected |
| `DisconnectedBackground` | Badge color when disconnected |

### Components.CameraLock

| Token | Description |
|-------|-------------|
| `AlarmLockedForeground` | Lock icon color for alarm-triggered locks |
| `UserLockedForeground` | Lock icon color for user-triggered locks |

### Components.AlarmStatus

| Token | Description |
|-------|-------------|
| `Active` | Alarm is currently firing |
| `Inactive` | Alarm is idle |
| `Acknowledged` | Alarm has been acknowledged by an operator |
| `Tampered` | Sensor tamper detected |
| `Default` | Fallback for unknown states |

---

## Color Format

All colors use the `#AARRGGBB` or `#RRGGBB` hex format. When alpha is omitted, the color is fully opaque.

## Adding a New Theme

1. Copy an existing JSON file in `Themes/`.
2. Change `Key`, `DisplayName`, `Variant`, and `FormFactor`.
3. Adjust colors and sizing values.
4. Restart the application — the new theme appears automatically.
