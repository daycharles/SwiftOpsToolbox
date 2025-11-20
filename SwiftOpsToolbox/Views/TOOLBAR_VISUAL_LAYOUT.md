# Notepad View Toolbar - Visual Layout

## Toolbar Overview

The toolbar is designed to match the Notepad++ style with a dark theme and clear button grouping.

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ TOOLBAR (Background: #1A1A1A) ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓│
├─────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                         │
│  LEFT GROUP                                                              RIGHT GROUP                    │
│  ┌────────────────────────────────────────────────────────────────┐    ┌───────────────────────────┐  │
│  │ [New] [Open] [Save] [Save As] │ [Undo] [Redo] │ [Cut] [Copy]   │    │ [Preview] [Popout]        │  │
│  │                                                                 │    │                           │  │
│  │ [Paste] │ [Find] [Replace] │ [B] [I]                          │    │ [Font Family ▼] [Size ▼] │  │
│  └────────────────────────────────────────────────────────────────┘    └───────────────────────────┘  │
│                                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────────────────────┘
```

## Button Groups (Left to Right)

### Group 1: File Operations
```
┌──────┐ ┌──────┐ ┌──────┐ ┌─────────┐
│ New  │ │ Open │ │ Save │ │ Save As │
└──────┘ └──────┘ └──────┘ └─────────┘
Ctrl+N   Ctrl+O   Ctrl+S    
```

### Separator
```
│ (gray vertical line)
```

### Group 2: Edit History
```
┌──────┐ ┌──────┐
│ Undo │ │ Redo │
└──────┘ └──────┘
Ctrl+Z   Ctrl+Y
```

### Separator
```
│ (gray vertical line)
```

### Group 3: Clipboard Operations
```
┌─────┐ ┌──────┐ ┌───────┐
│ Cut │ │ Copy │ │ Paste │
└─────┘ └──────┘ └───────┘
Ctrl+X  Ctrl+C   Ctrl+V
```

### Separator
```
│ (gray vertical line)
```

### Group 4: Search Operations
```
┌──────┐ ┌─────────┐
│ Find │ │ Replace │
└──────┘ └─────────┘
Ctrl+F   Ctrl+H
```

### Separator
```
│ (gray vertical line)
```

### Group 5: Text Formatting
```
┌───┐ ┌───┐
│ B │ │ I │  (Toggle buttons)
└───┘ └───┘
Bold   Italic
```

## Right Side Controls

### Group 6: View Controls
```
┌─────────┐ ┌────────┐
│ Preview │ │ Popout │  (Toggle for Preview)
└─────────┘ └────────┘
```

### Group 7: Font Controls
```
┌─────────────────────┐ ┌──────────┐
│ Font Family Combo   │ │ Size     │
│ (e.g., Consolas)    │ │ (e.g.14) │
└─────────────────────┘ └──────────┘
```

## Color Scheme (Notepad++ Dark Theme)

- **Toolbar Background:** `#1A1A1A` (Dark gray)
- **Button Text:** `White`
- **Button Background:** `Transparent`
- **Button Border:** `#333` (Medium gray)
- **Separator Lines:** `#333` (Medium gray)
- **Hover/Active Background:** `#0B2A44` (Dark blue - inherited from main theme)

## Tooltip Format

Each button displays a tooltip with the action name and keyboard shortcut:
- "New file (Ctrl+N)"
- "Open file (Ctrl+O)"
- "Save file (Ctrl+S)"
- "Undo (Ctrl+Z)"
- "Redo (Ctrl+Y)"
- "Cut (Ctrl+X)"
- "Copy (Ctrl+C)"
- "Paste (Ctrl+V)"
- "Find (Ctrl+F)"
- "Replace (Ctrl+H)"

## Keyboard Shortcuts Summary

| Action      | Shortcut | Location in Toolbar |
|-------------|----------|---------------------|
| New File    | Ctrl+N   | Group 1             |
| Open File   | Ctrl+O   | Group 1             |
| Save File   | Ctrl+S   | Group 1             |
| Undo        | Ctrl+Z   | Group 2             |
| Redo        | Ctrl+Y   | Group 2             |
| Cut         | Ctrl+X   | Group 3             |
| Copy        | Ctrl+C   | Group 3             |
| Paste       | Ctrl+V   | Group 3             |
| Find        | Ctrl+F   | Group 4             |
| Replace     | Ctrl+H   | Group 4             |

## Dialog Windows

### Find Dialog
```
┌─────────────────────────────────────────┐
│ Find                               [X]  │
├─────────────────────────────────────────┤
│                                         │
│ Find what:  [________________]          │
│                                         │
│             ☐ Match case                │
│             ☐ Match whole word only     │
│                                         │
│          [Find Next]  [Close]           │
│                                         │
└─────────────────────────────────────────┘
```

### Find and Replace Dialog
```
┌─────────────────────────────────────────┐
│ Find and Replace                   [X]  │
├─────────────────────────────────────────┤
│                                         │
│ Find what:     [________________]       │
│                                         │
│ Replace with:  [________________]       │
│                                         │
│                ☐ Match case             │
│                ☐ Match whole word only  │
│                                         │
│  [Find Next] [Replace] [Replace All]    │
│  [Close]                                │
│                                         │
└─────────────────────────────────────────┘
```

## Button Dimensions

- **Standard button width:** 60px
- **Narrow button width:** 50-55px (Cut, Copy, Paste, Find)
- **Wide button width:** 70-80px (Save As, Replace, Preview)
- **Small formatting button width:** 36px (Bold, Italic)
- **Button height:** 28px (implicit from content)
- **Margin:** 2px between buttons
- **Separator margin:** 8px horizontal

## Responsive Behavior

The toolbar uses a Grid layout with three columns:
1. **Left column (Auto):** Contains all action buttons
2. **Center column (*):** Flexible spacer (can be used for search box in future)
3. **Right column (Auto):** Contains view controls and font selectors

This ensures the toolbar scales properly with window resizing while keeping buttons visible.

## Accessibility Features

1. **Tooltips:** All buttons have descriptive tooltips
2. **Keyboard shortcuts:** Common operations accessible via keyboard
3. **Focus management:** Operations return focus to the editor
4. **High contrast:** White text on dark background for readability
5. **Logical grouping:** Related functions grouped together with visual separators

## Comparison to Notepad++

**Similarities:**
- Dark theme with similar color scheme
- Grouped buttons with separators
- File operations on the left
- Edit operations in the middle
- Standard clipboard operation order (Cut, Copy, Paste)
- Find/Replace as separate but adjacent buttons
- Font controls on the right

**Differences:**
- Text labels instead of icons (simpler, more accessible)
- Integrated Markdown preview controls
- Tab-based interface (inherited from existing design)
- Fewer total buttons (focused on essential operations)

## Future Enhancement Ideas

1. **Icon mode:** Replace text with icons for more compact toolbar
2. **Customizable toolbar:** Allow users to show/hide buttons
3. **Additional separators:** More visual grouping options
4. **Toolbar themes:** Light mode support
5. **Quick formatting:** More text transformation options
6. **Status indicators:** Show document state (modified, saved, etc.)
