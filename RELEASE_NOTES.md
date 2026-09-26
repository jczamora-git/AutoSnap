# AutoSnap v1.0.0

Initial public release of AutoSnap.

## Highlights

- **Desktop Window & Monitor Capture**: Select any running desktop application or active monitor for high-fidelity periodic captures.
- **Native Browser Tab Capture**: Seamlessly capture any tab in Google Chrome or compatible Chromium browsers without separate profiles or debugging flags.
- **Interval Scheduling**: Automatic screenshot timer with custom durations, countdown timer, and snapshot counter.
- **Immediate Manual Snapshot**: On-demand capture without interrupting or resetting the running schedule.
- **Live Preview with Backpressure**: Responsive preview panel with smart throttling to prevent redundant captures.
- **Storage Management**: Configurable save location, auto-cleanup thresholds, and customizable JPEG/PNG format and quality controls.
- **Tray & Background Execution**: Runs unobtrusively in the Windows system tray with balloon notifications and tray menu commands.

## Browser Capture

The browser capture subsystem uses a secure, local loopback bridge on `127.0.0.1` and Chrome's native `navigator.mediaDevices.getDisplayMedia()` API:
- Works with your existing Chrome profile and logged-in sessions (e.g. Google Meet, YouTube, Gmail).
- Captures the tab content itself rather than the entire browser window or visible screen.
- Respects browser security and user consent.

## Notes

- **OS**: Windows 10 / Windows 11 (x64)
- **Deployment**: Standalone self-contained Windows x64 build (no .NET runtime installation required)
- **Code Signing**: Application binary is currently unsigned; SmartScreen prompt may appear upon first launch.

## Known Limitations

- **Local Video Mode**: Frame extraction from local video files is slated for a future release (Phase 2) and is marked as disabled in v1.0.0.
- **Browser MediaStream Background Throttling**: Chromium browsers may throttle MediaStream frame delivery if the browser window is fully minimized by the operating system.
