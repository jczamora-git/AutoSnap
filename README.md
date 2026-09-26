# AutoSnap

AutoSnap is an automatic screenshot capture and local speech-to-text transcription application for Windows.

## Features

### 📸 Screen Capture
- **Application Window Capture**: Capture any active desktop window accurately.
- **Display / Monitor Capture**: Capture entire monitors or specific displays in multi-monitor setups.
- **Chrome / Browser Tab Capture**: Capture individual browser tabs directly from your existing Chrome session using native media sharing (`getDisplayMedia`).
- **Configurable Intervals**: Choose preset intervals (1s, 5s, 10s, 30s, 1m, 5m, etc.) or custom timings.
- **Immediate Manual Snapshot**: Capture instant on-demand snapshots without interrupting your timer.
- **Live Preview with Smart Throttling**: Real-time visual feedback and preview of the selected source.
- **Image Formatting**: Save as customizable JPEG (with quality control) or lossless PNG.
- **System Tray Support**: Run in the background with tray notifications and quick controls.

### 🎞 Local Video Processing
- **Interval-Based Frame Extraction**: Extract frames across full videos (e.g. every 1 minute or 5 minutes) fast and non-real-time.
- **Wide Format Support**: Works with `.mp4`, `.mkv`, `.mov`, `.avi`, `.webm`, `.m4v` via FFmpeg/FFprobe.
- **Combined Processing**: Extract snapshots and/or transcribe full video audio in a single job.
- **Progress Tracking**: Real-time progress bar, snapshot count, and cancellation support.

### 🎙 Local Whisper Transcription [BETA]
- **100% Offline & Private**: Runs Whisper locally on your machine—no cloud APIs, subscriptions, or telemetry.
- **Experimental Feature**: Real-time transcription is in active beta. Accuracy, latency, and throughput vary depending on system hardware and selected model.
- **Hardware-Aware Model Recommendations**: Built-in system hardware detection evaluates your CPU, core count, and memory to recommend optimal models.
- **Multi-Source Audio**:
  - **System Audio**: Capture and transcribe Windows desktop audio output (Zoom, Teams, meetings, YouTube, lectures) via WASAPI loopback without capturing microphone noise.
  - **Browser Tab Audio**: Stream and transcribe tab audio directly alongside browser screenshots.
  - **Local Media**: Transcribe local video and audio files (`.wav`, `.mp3`, `.m4a`, `.mp4`, etc.).
- **Multilingual & Taglish Support**:
  - **Taglish (Recommended)**: Preserves mixed Filipino and English speech without translating.
  - **English**
  - **Filipino / Tagalog**
  - **Auto Detect**
- **Multi-Format Export**: Export live transcripts to **TXT**, **SRT** (SubRip), and **VTT** (WebVTT).
- **Live Transcript Editor**: View and edit transcripts before exporting.
- **Transcript Recovery / Autosave**: Automatically protects long sessions against crashes with autosaves every 30 seconds.
- **Whisper Model Manager**: Download and manage official local Whisper models (`Tiny`, `Base`, `Small`, `Medium`, `Large v3`, `Large v3 Turbo`, `Large v3 Turbo Q5`) on demand.

---

## Transcription — Experimental

AutoSnap includes an integrated local speech-to-text transcription engine powered by Whisper.

> **Status: Experimental / Beta**  
> Local transcription is currently in beta. Please keep the following in mind:
> - **Local Processing**: All transcription is executed 100% offline on your device.
> - **Hardware Sensitivity**: Performance, transcription speed, and live latency depend heavily on your CPU, available RAM, and chosen model.
> - **Live Audio Latency**: On lower-spec systems or when running large models without hardware acceleration, live transcription may run behind real time. AutoSnap provides real-time performance indicators (Real-Time Factor and speed multiplier) to alert you if processing falls behind.
> - **Feedback & Issues**: Because this feature is under active development, we encourage users to report transcription bugs, latency issues, or feedback via [GitHub Issues](https://github.com/jczamora-git/AutoSnap/issues).

---

## Requirements

- **Operating System**: Windows 10 / Windows 11 (64-bit)
- **Browser**: Google Chrome (or Chromium-based browser) for Browser Tab capture and audio streaming.
- **FFmpeg & FFprobe**: Required for local video processing and media audio extraction. AutoSnap features an integrated 1-click FFmpeg manager (installed to `%LocalAppData%\AutoSnap\Tools\FFmpeg\`), and also supports custom paths or system PATH.
- **Whisper Model**: Downloaded on demand via the built-in Model Manager (stored in `%LocalAppData%\AutoSnap\Models\Whisper\`).
- The self-contained `win-x64` release includes all required .NET desktop runtime components.

---

## Download & Installation

Download the latest release for Windows from the [GitHub Releases](https://github.com/jczamora-git/AutoSnap/releases) page.

Available distribution packages:
- **Windows Installer** (`AutoSnap-Setup-v1.2.0.exe`): Standard Windows setup wizard with Start Menu shortcuts, optional Desktop shortcut, and uninstaller.
- **Portable ZIP** (`AutoSnap-v1.2.0-portable.zip`): Standalone archive that can be extracted and run immediately without installation.
- **SHA256 Checksum** (`AutoSnap-v1.2.0-portable.zip.sha256`): SHA256 integrity checksum.

> **Note**: AutoSnap is currently unsigned, so Windows SmartScreen may show a warning on first launch. Click **More info** &rarr; **Run anyway** to proceed.

---

## Quick Start Guide

### 1. Screen Capture
1. Select **Capture** from the top navigation.
2. Choose your capture source: **Application Window**, **Display / Monitor**, or **Chrome Tab**.
3. Set your desired interval (e.g. 10 seconds or 1 minute).
4. Click **Start** to begin capturing screenshots.

### 2. Video Processing
1. Select **Local Video** from the top navigation.
2. Click **Browse Video File...** and select a video.
3. Choose whether to **Extract Snapshots** (and interval) and/or **Generate Transcript**.
4. Click **Process Video**.

### 3. Live Transcription
1. Select **Transcription** from the top navigation.
2. Ensure you have downloaded a Whisper model (click **Manage Models...**).
3. Select your audio source (**System Audio**, **Browser Tab Audio**, or **Local Media File**).
4. Select your language (e.g. **Taglish** or **English**).
5. Click **Start Transcription**.
6. Export as **TXT**, **SRT**, or **VTT** when finished.

---

## Privacy & Security

- All screenshots, video frames, and audio transcriptions are processed and stored strictly on your local machine.
- Browser tab capture requires explicit user consent via Chrome's native sharing prompt.
- AutoSnap runs its capture bridge strictly on local loopback (`127.0.0.1`).
- AutoSnap does not send audio or screenshots to external cloud APIs.

---

## Version

AutoSnap v1.2.0
