---
name: winform-developer
description: Guidelines and best practices for developing Windows Forms applications in C#. Use when designing user interfaces, implementing control logic, handling events, or ensuring UI responsiveness and architecture in WinForms.
---

# WinForms Developer Skill

## Overview
This skill provides expert guidance on designing and implementing robust, responsive, and maintainable Windows Forms applications. It covers UI layout, event handling, asynchronous programming, and architectural patterns like MVP.

## Quick Start
- **UI Layout:** Use Anchoring and Docking instead of absolute coordinates. See [ui-best-practices.md](references/ui-best-practices.md).
- **Responsiveness:** Never block the UI thread. Use `async/await`. See [responsiveness.md](references/responsiveness.md).
- **Architecture:** Separate your business logic from the Form. Use MVP where possible. See [architecture.md](references/architecture.md).

## Core Guidelines

### 1. UI Design & Layout
- Prefer `TableLayoutPanel` and `FlowLayoutPanel` for dynamic layouts.
- Always set mnemonics (`&`) for main buttons and labels.
- Ensure logical `TabIndex` for keyboard users.
- Use `DPI` awareness to support high-resolution screens.

### 2. Event Handling
- Keep event handlers lean. Delegate work to service classes or presenters.
- Use `sender` and `EventArgs` appropriately.
- Avoid nesting complex logic inside `Click` events.

### 3. Data Management
- Use `BindingSource` for robust data binding.
- Implement `INotifyPropertyChanged` in your models for automatic UI updates.
- Use `DataGridView` with virtual mode for large datasets.

## Detailed References
- [UI Best Practices](references/ui-best-practices.md): Layout, consistency, and accessibility.
- [UI Responsiveness](references/responsiveness.md): Asynchronous programming and background tasks.
- [Architecture Patterns](references/architecture.md): MVP pattern, data binding, and resource management.
