# SwiftOpsToolbox

SwiftOps Toolbox is a fast, offline-friendly productivity suite for Windows. Includes calendar, to-do list, Markdown notes, lightning-fast file search, and secure SFTP client. Built for professionals who value speed, control, and simplicity—no cloud, no bloat.

## Features

### Core Features (Free Tier)
- **Calendar & Events**: Manage your schedule with recurring events and reminders
- **To-Do List**: Keep track of tasks with timestamps
- **Notepad/Markdown**: Read and write markdown documents with basic formatting
- **File Search**: Lightning-fast local file search
- **Clipboard History**: Track and reuse clipboard items

### Pro Features
- Advanced calendar views and features
- Enhanced markdown editing
- Advanced file search with operators
- Secure SFTP client for file transfers

### Business & Enterprise Features
- Team collaboration and sharing
- Centralized management
- Audit logs and compliance features
- Custom branding and integrations

## Tiered Pricing Model

SwiftOps Toolbox now supports a flexible tiered pricing system. See [TIERED_PRICING.md](TIERED_PRICING.md) for details on:
- Feature availability by tier
- Architecture and implementation
- Developer guide for extending the system
- Testing different tier configurations

## Getting Started

1. Launch SwiftOps Toolbox
2. The app defaults to the Free tier with core features enabled
3. Navigate to **Settings** to view your current tier and available features
4. Use the tier selector (demo mode) to experience different feature sets

## Development

This is a WPF application built with .NET 8 targeting Windows. The codebase uses:
- MVVM pattern with CommunityToolkit.Mvvm
- MahApps.Metro for modern UI
- Modular service architecture for extensibility
- Feature flags for tier-based access control

For more information about the tiered pricing implementation, see [TIERED_PRICING.md](TIERED_PRICING.md).

