# Notepad View Toolbar Documentation

## Overview

The Notepad view in SwiftOpsToolbox features a Notepad++-inspired toolbar that provides quick access to common text editing operations. The toolbar is designed with a dark theme (#1A1A1A background) and includes support for both mouse clicks and keyboard shortcuts.

## Current Features

### File Operations
- **New** (Ctrl+N) - Create a new document tab
- **Open** (Ctrl+O) - Open an existing text or Word document
- **Save** (Ctrl+S) - Save the current document
- **Save As** - Save the current document with a new name

### Edit Operations
- **Undo** (Ctrl+Z) - Undo the last action
- **Redo** (Ctrl+Y) - Redo the last undone action
- **Cut** (Ctrl+X) - Cut selected text to clipboard
- **Copy** (Ctrl+C) - Copy selected text to clipboard
- **Paste** (Ctrl+V) - Paste text from clipboard

### Search Operations
- **Find** (Ctrl+F) - Open Find dialog to search for text
- **Replace** (Ctrl+H) - Open Find & Replace dialog

### Formatting Operations
- **Bold** - Toggle bold formatting for selected text
- **Italic** - Toggle italic formatting for selected text

### View Operations
- **Preview** - Toggle Markdown preview pane
- **Popout** - Open preview in separate window

### Font Controls
- **Font Family** - Dropdown to select font family
- **Font Size** - Dropdown to select font size

## Architecture

### Files
- `Views/NotepadView.xaml` - UI layout and toolbar structure
- `Views/NotepadView.xaml.cs` - Event handlers and business logic
- `Views/FindDialog.xaml` - Find dialog UI
- `Views/FindDialog.xaml.cs` - Find functionality
- `Views/FindReplaceDialog.xaml` - Find & Replace dialog UI
- `Views/FindReplaceDialog.xaml.cs` - Find & Replace functionality

### Design Principles
1. **Minimal changes** - The toolbar builds on the existing structure
2. **Consistency** - All buttons follow the same styling (white text, transparent background, #333 border)
3. **Accessibility** - Tooltips and keyboard shortcuts for all major actions
4. **Separation of concerns** - Dialogs are separate windows, not inline UI

## How to Add New Toolbar Buttons

### Step 1: Add Button to XAML

In `Views/NotepadView.xaml`, locate the toolbar section (inside the `Border` with `Grid.Row="0"`). Add your button to the appropriate `StackPanel`:

```xaml
<Button x:Name="BtnYourAction" 
        Content="Your Action" 
        Width="70" 
        Margin="2" 
        Foreground="White" 
        Background="Transparent" 
        BorderBrush="#333" 
        ToolTip="Your action description (Ctrl+Key)" />
```

**Important styling guidelines:**
- Use consistent margins (`Margin="2"`)
- Keep foreground white and background transparent
- Use `#333` for border color
- Add tooltips with keyboard shortcuts if applicable
- Group related buttons together and separate groups with:
  ```xaml
  <Rectangle Width="1" Height="24" Fill="#333" Margin="8,0" />
  ```

### Step 2: Add Event Handler in Code-Behind

In `Views/NotepadView.xaml.cs`, add the event handler registration in the constructor:

```csharp
BtnYourAction.Click += BtnYourAction_Click;
```

Then implement the handler method:

```csharp
/// <summary>
/// Description of what your action does.
/// </summary>
private void BtnYourAction_Click(object? sender, RoutedEventArgs e)
{
    var tab = Tabs.SelectedItem as TabItem;
    var rtb = FindRtb(tab);
    if (rtb == null) return;

    // Your action logic here
    
    rtb.Focus();
}
```

### Step 3: Add Keyboard Shortcut (Optional)

To add a keyboard shortcut:

1. In `Views/NotepadView.xaml`, add to the `UserControl.InputBindings` section:
   ```xaml
   <KeyBinding Key="Y" Modifiers="Ctrl" Command="{Binding YourActionCommand}" />
   ```

2. In `Views/NotepadView.xaml.cs`, add a command property:
   ```csharp
   public ICommand YourActionCommand { get; }
   ```

3. Initialize the command in the constructor (before `InitializeComponent()`):
   ```csharp
   YourActionCommand = new RelayCommand(YourAction);
   ```

4. Add a helper method:
   ```csharp
   private void YourAction(object? parameter) => BtnYourAction_Click(null, new RoutedEventArgs());
   ```

## Styling and Theme Support

The toolbar currently uses a dark theme that matches Notepad++:
- Background: `#1A1A1A`
- Border: `#333`
- Text: `White`
- Active button background: `#0B2A44`

To support light mode in the future, consider:
1. Creating theme resources in `App.xaml` or `MainWindow.xaml`
2. Binding colors to `DynamicResource` instead of hardcoded values
3. Adding theme toggle functionality to switch between color schemes

## Best Practices

1. **Always check for null** - Ensure the active tab and RichTextBox exist before performing operations
2. **Focus management** - Call `rtb.Focus()` after operations to return focus to the editor
3. **User feedback** - Use MessageBox for important user notifications
4. **Documentation** - Add XML documentation comments (///) for all public methods
5. **Tooltips** - Always include helpful tooltips with keyboard shortcuts
6. **Consistency** - Follow the existing patterns for button styling and event handling

## Testing Recommendations

When adding new features:
1. Test with multiple tabs open
2. Test with empty documents and documents with content
3. Test keyboard shortcuts in addition to button clicks
4. Verify focus behavior after operations
5. Test edge cases (no selection, entire document selected, etc.)

## Future Enhancements

Potential future additions:
- Toolbar visibility toggle
- Customizable button order
- Icon-based buttons instead of text labels
- Additional formatting options (underline, strikethrough, etc.)
- Quick text transformations (uppercase, lowercase, etc.)
- Line operations (delete line, duplicate line, etc.)
- Multi-file search and replace

## Support

For questions or issues with the toolbar, please refer to the main SwiftOpsToolbox documentation or create an issue in the repository.
