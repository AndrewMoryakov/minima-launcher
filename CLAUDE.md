# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MinimalistDesktop is a minimalist application launcher and desktop replacement for Windows 11. It's a WPF application built with .NET 8.0 using the MVVM pattern, inspired by Olauncher (Android) and macOS Spotlight.

**Key characteristic**: This is a desktop replacement utility that can run as the Windows shell, not just a launcher overlay.

### Technology Stack
- .NET 8.0 (target: net8.0-windows)
- WPF for UI
- Windows Forms for system tray
- YamlDotNet 16.3.0 for config.yaml parsing
- System.Text.Json for user settings
- Win32 API (P/Invoke) for system integration

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run in development
dotnet run

# Publish single-file executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Kill running instance before building (Windows)
taskkill //F //IM MinimalistDesktop.exe
```

The application runs with a global hotkey (Win + Space by default). After starting, press Win + Space to show the launcher window.

## Architecture

### MVVM Pattern
- **Views**: XAML UI + code-behind for animations, event handling, and window management
- **ViewModels**: Business logic, data binding, INotifyPropertyChanged implementation
- **Models**: Data classes (AppShortcut, AppSettings, AppConfig)
- **Services**: Core functionality layer
- **Native**: Win32 API P/Invoke wrappers for system integration

### Key Services
- **LaunchService**: Handles launching apps (Standard .exe, UWP, URLs, Commands)
- **SettingsService**: Persists user's pinned apps to %APPDATA%\MinimalistDesktop\apps.json
- **ConfigService**: Reads config.yaml using YamlDotNet
- **AppDiscoveryService**: Discovers installed applications from configured paths
- **HotkeyService**: Registers global hotkeys via Win32 RegisterHotKey API
- **ShellModeManager**: Manages Windows shell replacement functionality

### Win32 Integration Points
The Native folder contains Win32 API wrappers for:
- Global hotkey registration (user32.dll)
- Window positioning (SetWindowPos, Z-order manipulation)
- Explorer process management (for shell replacement mode)
- Taskbar and desktop icon visibility control

## Configuration Files

### config.yaml (application-level)
Defines app discovery paths, system apps, and default pinned apps. Uses YamlDotNet for parsing.

Structure:
- `searchPaths`: Where to look for installed apps (SpecialFolder or Custom paths)
- `systemApps`: Built-in system commands (notepad, calc, etc.)
- `defaultPinnedApps`: Initial pinned apps on first launch
- `serviceFilePrefixes`: Filters out service executables (uninstall, update, etc.)

### apps.json (user-level)
Located at `%APPDATA%\MinimalistDesktop\apps.json`. Stores user's pinned applications. Managed by SettingsService.

## Important Implementation Details

### App List Expansion Behavior
When clicking the search box, the app list expands upward to fill space up to the toolbar. This is handled in LauncherWindow.xaml.cs:

- `Window_PreviewMouseDown`: Detects clicks to expand/collapse list
- `IsInsideInteractiveElements`: Checks if click is on SearchBox or AppListBox
- `EnsureAllAppsShown`: Sets `ShowAllApps = true` to expand list
- `AnimateAppListHeight`: Animates MaxHeight with DoubleAnimation

The list height calculation accounts for toolbar position via `UpdateExpandedListTarget()`.

### ShowAllApps Property
Controls whether to show only pinned apps or all discovered apps:
- `false`: Shows only user's pinned apps (from apps.json)
- `true`: Shows pinned apps + all discovered apps from searchPaths

The FilteredApps collection is rebuilt based on this flag and search query.

### App Discovery Flow
1. On startup, pinned apps load synchronously from apps.json
2. All installed apps load asynchronously via Task.Run in LoadAllApps()
3. Discovery scans paths from config.yaml, filters service files
4. When ShowAllApps is true, both collections merge with pinned apps first

### Desktop Level Window Positioning
The launcher window uses `SetWindowToDesktopLevel()` to position itself at HWND_BOTTOM Z-order, making it behave like a desktop background rather than a foreground window.

## Common Development Tasks

### Changing the Global Hotkey
Edit `App.xaml.cs` in the OnStartup method:
```csharp
_hotkeyService.RegisterHotkey(_launcherWindow, ModifierKeys.Win, Keys.Space);
```

### Adding a New Launch Type
1. Add enum value to `LaunchType` in Models/AppShortcut.cs
2. Implement handler in Services/LaunchService.cs switch statement
3. Update config.yaml parsing in ViewModels/LauncherViewModel.cs

### Modifying UI Colors/Styles
- Background: LauncherWindow.xaml Grid.Background
- List items: AppItemStyle in Window.Resources
- Search box: Border around TextBox in Grid.Row="1"

### Working with Animations
All animations use WPF's DoubleAnimation with EasingFunctions. Key animated properties:
- `AppListBox.MaxHeight`: Expand/collapse animation
- `Window.Opacity`: Fade in/out on show/hide

## Shell Replacement Mode

The application can replace Windows Explorer as the shell via registry:
```
HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\Winlogon\Shell
```

ShellModeManager handles this functionality. When enabled, the app must:
- Hide desktop icons (via Win32ShellHelper)
- Manage Explorer process lifecycle
- Provide a "Restore Desktop" button in the toolbar

## Event Communication Pattern

ViewModels communicate with Views via events, not direct references:
```csharp
public event EventHandler CloseRequested;  // ViewModel -> View
public event EventHandler AppLaunched;      // ViewModel -> View
```

Views subscribe in constructor and handle appropriately (e.g., HideWindow, ShowWindow).

## Commit Style Guidelines

When creating commits for this repository:

- Write commit messages in English
- Use a human, conversational style but keep it concise
- Write in present tense, describing what the change does
- Do not use narrative style ("There was a problem", "It turned out that")
- Do not add emojis or decorative elements
- Do not add "Generated with Claude Code" or "Co-Authored-By: Claude" footers
- Use regular dashes (-) not arrows (→) in bullet points
- Focus on the current state after changes, not the journey to get there

### Commit Authorship
All commits should be authored by the repository owner:
- Author: Andrew_Moryakov <andrew@moryakov.dev>
- Before amending any commit, always verify authorship with `git log -1 --format='%an %ae'`
- Never amend commits from other developers

Example of a good commit:
```
Fix app list expansion on search box click

Window_PreviewMouseDown now handles clicks properly:
- Click on search or list - expand the list
- Click anywhere else - collapse the list
```
