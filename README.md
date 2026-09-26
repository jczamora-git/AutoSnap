# AutoSnap

AutoSnap is a Windows desktop application for automatically capturing screenshots from selected displays, application windows, and supported browser tabs at configurable intervals.

## Features

- **Application Window Capture**: Capture any active desktop window accurately.
- **Display / Monitor Capture**: Capture entire monitors or specific displays in multi-monitor setups.
- **Chrome / Browser Tab Capture**: Capture individual browser tabs directly from your existing Chrome session using native media sharing.
- **Configurable Intervals**: Choose preset intervals (1s, 5s, 10s, 30s, 1m, 5m, etc.) or set custom seconds, minutes, and hours.
- **Manual Snapshot**: Capture instant on-demand screenshots without resetting your timer.
- **Live Preview**: Real-time visual feedback and preview of the selected source.
- **Capture Control**: Start, pause, resume, and stop capture sessions anytime.
- **Image Formatting**: Save as customizable JPEG (with quality control) or lossless PNG.
- **Configurable Output Directory**: Easily organize and open destination folders directly from the app.
- **Session Statistics**: Live countdown timer, total snapshot counter, and elapsed session duration.
- **System Tray Support**: Minimize to system tray with background capture and quick actions.
- **Local Video**: Planned for a future release (Phase 2).

## Browser Tab Capture

AutoSnap uses the browser's native `getDisplayMedia()` screen-sharing permission workflow. When you click **Choose Chrome Tab**, AutoSnap opens a lightweight local helper page in your existing Chrome session. When you click **Choose Chrome Tab** in the helper page, Chrome displays its standard tab sharing picker, allowing you to select any existing open tab (such as Google Meet, YouTube, Gmail, Docs, etc.).

AutoSnap does not bypass browser capture permissions and does not require:
- Isolated browser profiles
- Remote debugging flags (`--remote-debugging-port`)
- Chrome extensions
- ChromeDriver or Selenium

AutoSnap does not access passwords, cookies, authentication tokens, or browsing history.

## Requirements

- **Operating System**: Windows 10 / Windows 11 (64-bit)
- **Browser**: Google Chrome (or compatible Chromium browser) for Browser Tab Capture
- The self-contained `win-x64` release includes all required .NET components and does not require a separate .NET runtime installation.

## Download & Installation

Download the latest release for Windows from the [GitHub Releases](https://github.com/jczamora-git/AutoSnap/releases) page.

Available distribution packages:
- **Windows Installer** (`AutoSnap-Setup-vX.Y.Z.exe`): Standard Windows setup wizard with Start Menu shortcuts, optional Desktop shortcut, and uninstaller.
- **Portable ZIP** (`AutoSnap-vX.Y.Z-portable.zip`): Standalone archive that can be extracted and run immediately without installation.
- **SHA256 Checksum** (`AutoSnap-vX.Y.Z-portable.zip.sha256`): SHA256 integrity checksum.

> **Note**: AutoSnap is currently unsigned, so Windows SmartScreen may show a warning on first launch. Click **More info** &rarr; **Run anyway** to proceed.

## Usage

1. **Choose Capture Source**: Select **Application Window**, **Display / Monitor**, or **Chrome Tab**.
2. **Select Target**:
   - For Window/Display: Click **Change...** to pick the specific window or monitor.
   - For Chrome Tab: Click **Choose Chrome Tab** to open the local helper page, then pick the tab from Chrome's sharing dialog.
3. **Choose Interval**: Select your desired capture frequency (e.g. 5 seconds, 10 seconds, 1 minute, or custom).
4. **Select Output Directory**: View or customize the save location in Settings.
5. **Start Capture**: Click **Start** to begin automatic capture, or click **Take Snapshot Now** for an immediate capture.
6. Use **Pause**, **Resume**, and **Stop** as needed.

## Privacy & Security

- All screenshots are captured and stored strictly on your local machine.
- Browser tab capture requires explicit user consent via Chrome's native sharing prompt.
- AutoSnap runs its capture bridge strictly on local loopback (`127.0.0.1`).
- AutoSnap does not collect telemetry or upload screenshots to external servers.

## Version

AutoSnap v1.0.0
