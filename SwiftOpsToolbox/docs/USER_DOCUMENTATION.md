# SwiftOps Toolbox — User Documentation

> Placeholder screenshots are included as suggested filenames under `docs/screenshots/`. Replace placeholders with actual images.

## Table of contents

- [Introduction](#introduction): What this app does
- [Quick Start](#quick-start): Requirements and installation
- [Launching the App](#launching-the-app): How to run locally
- [UI Tour](#ui-tour): Screenshots and descriptions (placeholders)
- [Common Tasks](#common-tasks): How to use main features
- [Troubleshooting](#troubleshooting): Logs, common fixes
- [FAQ](#faq)
- [Contributing & Support](#contributing--support)

---

## Introduction

SwiftOps Toolbox is a developer/operations utility containing desktop and web components used for quick productivity tasks (calendar events, clipboard history, file indexing, SFTP profiles, TODOs, etc.). This document helps new users install, launch, and operate the application.

> NOTE: This repository contains a WPF desktop project under `SwiftOpsToolbox/` and UI components under `Components/` used by the web UI. Adjust instructions below according to the project you intend to run.

---

## Quick Start

### Prerequisites

- Windows 10/11 (for the WPF desktop app)
- .NET SDK 6.0 or later (check `global.json` or project `TargetFramework` if present)
- Optional: Visual Studio 2022/2023 for editing and debugging

### Installation

1. Clone the repository:

```
git clone https://github.com/daycharles/SwiftOpsToolbox.git
cd SwiftOpsToolbox
```

2. Open the solution `SwiftOpsToolbox.slnx` in Visual Studio or build from command line:

```
dotnet build SwiftOpsToolbox.slnx
```

3. If you only need the desktop app, open the `SwiftOpsToolbox` project in Visual Studio and run.

---

## Launching the App

### From Visual Studio

- Open `SwiftOpsToolbox.slnx` and set the startup project to `SwiftOpsToolbox` (WPF) or the relevant web project.
- Press F5 to build and run.

### From command line

- To run the desktop app (after build), use the generated executable in `SwiftOpsToolbox/bin/Debug/net6.0-windows/` (path may vary with target framework):

```
cd SwiftOpsToolbox\bin\Debug\<TFM>\
SwiftOpsToolbox.exe
```

Replace `<TFM>` with the folder name matching the target framework.

---

## UI Tour

Below are recommended screenshots to include in `docs/screenshots/`. Replace the placeholder image paths with actual screenshots.

- **Main Window**

![Main window screenshot](docs/screenshots/mainwindow.png)

_Filename suggestion:_ `docs/screenshots/mainwindow.png`

- **Tab Header / Navigation**

![Tab header screenshot](docs/screenshots/tabheader.png)

_Filename suggestion:_ `docs/screenshots/tabheader.png`

- **Calendar View**

![Calendar screenshot](docs/screenshots/calendar.png)

_Filename suggestion:_ `docs/screenshots/calendar.png`

- **Clipboard / History Panel**

![Clipboard screenshot](docs/screenshots/clipboard.png)

_Filename suggestion:_ `docs/screenshots/clipboard.png`

- **File Index / Search Results**

![File index screenshot](docs/screenshots/fileindex.png)

_Filename suggestion:_ `docs/screenshots/fileindex.png`

- **Settings / User Preferences**

![Settings screenshot](docs/screenshots/settings.png)

_Filename suggestion:_ `docs/screenshots/settings.png`

Screenshot tips:

- Recommended image size: 1280×720 or similar widescreen.
- Use PNG for clarity and lossless text.
- Name files exactly as suggested to keep links working.
- Commit screenshots to the repo under `docs/screenshots/` or add them to a release asset if you prefer not to store large images in Git.

---

## Common Tasks

- Creating a calendar event

  - Open the Calendar tab under `Components/Pages/Calendar.razor` (or the desktop Calendar view)
  - Click + New Event, fill title/time, click Save.

- Copying from Clipboard history

  - Open Clipboard view, click an item to copy back into the clipboard.

- Searching files

  - Open File Index or Search view, type filename or content, press Enter.

- Managing SFTP profiles
  - Edit profile entries in the UI and save; profiles are modeled by `Converters/Models/SftpProfile.cs`.

(Expand these step-by-step guides with screenshots and exact button names where helpful.)

---

## Troubleshooting

- Where to find logs

  - Check `appsettings.json` for configured log paths. If using the desktop app, logs may be in `%LOCALAPPDATA%\SwiftOpsToolbox\logs` or the build output folder — search for `.log` files if unsure.

- Config files

  - Global settings: `appsettings.json` and `appsettings.Development.json` at repo root.
  - Project-level settings may be under `SwiftOpsToolbox/` project files.

- Common fixes
  - Build errors: ensure correct .NET SDK installed and restore NuGet packages with `dotnet restore`.
  - Missing screenshots or resources: add the referenced files under `docs/screenshots/` and commit.

If you encounter a reproducible crash, capture the stack trace and open an issue with steps to reproduce; include the relevant log file and the screenshot of the error if possible.

---

## FAQ

Q: Where do I change app behavior (feature flags, tiers)?

A: Feature flags and user-tier definitions are typically found in model files under `Converters/Models/` (for example, `FeatureFlags.cs`, `UserTier.cs`).

Q: How do I add a new screenshot?

A: Put the image in `docs/screenshots/` and name it to match the placeholder used above. Commit and push to your branch.

---

## Contributing & Support

- To report bugs or request features, open a GitHub issue on the repository.
- For code contributions, fork the repo, create a branch, and submit a pull request with tests or screenshots where relevant.

---

## Files Mentioned

- `appsettings.json` — application configuration
- `appsettings.Development.json` — development overrides
- `SwiftOpsToolbox/` — desktop WPF app project
- `Components/Pages/` — web UI pages (Blazor components)
- `Converters/Models/` — data models used across the app

---

## Next steps (suggested)

- Capture and add the screenshots to `docs/screenshots/` using the filenames above.
- Expand the "Common Tasks" section with step-by-step screenshots.
- Optionally add automated image optimization or a small script to generate thumbnails.

---

_Generated: November 21, 2025_
