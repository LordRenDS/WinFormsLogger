# WinForms UI Best Practices

## Layout and Positioning

- **Avoid Absolute Positioning:** Do not rely on hardcoded coordinates (`Location` and `Size`).
- **Use Anchoring and Docking:**
    - `Anchor`: Maintains a constant distance from the edges of its parent container.
    - `Dock`: Pins a control to an edge of its parent container (Top, Bottom, Left, Right, Fill).
- **Auto Scaling:** Set `AutoScaleMode` to `DPI` or `Font` to ensure the UI looks consistent across different screen resolutions and scaling factors.
- **Container Controls:**
    - `FlowLayoutPanel`: Automatically arranges child controls in a row or column.
    - `TableLayoutPanel`: Organizes controls in a grid, allowing for proportional resizing.
    - `SplitContainer`: Provides resizable panels.

## Visual Consistency

- **System Colors:** Use `SystemColors` instead of hardcoded colors to respect the user's OS theme.
- **Fonts:** Use standard system fonts (e.g., `Segoe UI`) and avoid small font sizes.
- **Spacing:** Maintain consistent margins and padding between controls (standard is usually 6-12 pixels).

## Accessibility

- **Tab Order:** Ensure a logical `TabIndex` sequence for keyboard navigation.
- **Labels:** Use `Label` controls and set the `Label.Target` or mnemonic keys (e.g., `&Name`) to associate them with input controls.
- **AccessibleName/Description:** Provide meaningful names for screen readers.

## User Feedback

- **ToolTips:** Provide `ToolTip` info for complex or icon-only buttons.
- **Status Bar:** Use `StatusStrip` to provide non-intrusive feedback about long-running operations.
- **Cursor:** Change the cursor to `Cursors.WaitCursor` during short blocking operations.
