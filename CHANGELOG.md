# Changelog

All notable changes to AutoSnap will be documented in this file.

## [v1.2.0] - Combined Release

AutoSnap v1.2.0 is a major release combining the local video frame extraction capabilities (originally planned for v1.1) and the offline local Whisper speech-to-text transcription engine into a unified desktop suite.

### Video Processing (Former v1.1.0 features)
- **Local Video Selection**: Added support for `.mp4`, `.mkv`, `.mov`, `.avi`, `.webm`, `.m4v`.
- **FFmpeg/FFprobe Integration**: Non-real-time interval-based frame extraction (1s, 5s, 10s, 30s, 1m, 5m, etc.).
- **Video Metadata Inspection**: Extracts duration, resolution, frame rate, video codec, and audio stream presence.
- **Combined Processing**: Extract snapshots and/or generate transcripts in a single processing session.
- **Progress & Cancellation**: Responsive progress bar, snapshot counter, timestamp tracking, and cancellation support.

### Transcription (v1.2.0 features)
- **Local Whisper Engine**: Integrated offline local Whisper speech recognition (`Whisper.net` and `whisper.cpp` native runtime).
- **System Audio Capture**: Live Windows output audio transcription via WASAPI loopback without capturing ambient microphone noise.
- **Browser Tab Audio**: Stream and transcribe Chrome tab audio directly through `getDisplayMedia` and Web Audio API.
- **Local Media Transcription**: Transcribe local video and audio files (`.wav`, `.mp3`, `.m4a`, `.mp4`, etc.).
- **Language Modes**: Supported **Taglish** (preserves mixed Tagalog + English speech without translating), **English**, **Filipino / Tagalog**, and **Auto Detect**.
- **Multi-Format Export**: Export transcripts to **TXT**, **SRT** (SubRip), and **VTT** (WebVTT).
- **Live Transcript Editor**: Real-time transcript viewing and editing before export.
- **Transcript Autosave & Recovery**: Periodic session autosave every 30 seconds to protect against interruptions.
- **Whisper Model Manager**: On-demand download and management for `Tiny`, `Base`, `Small Multilingual`, and `Medium` models.

---

## [v1.0.0] - Initial Release

- **Window & Monitor Capture**: Screen capture for application windows and multi-monitor setups.
- **Native Browser Tab Capture**: Seamless tab capture from existing Chrome sessions via `getDisplayMedia`.
- **Configurable Intervals & Formats**: Preset and custom intervals, JPG and PNG output.
- **System Tray & Live Preview**: Background execution, notifications, and real-time preview.
- **Release Automation**: GitHub Actions CI/CD with Windows installer (`Inno Setup`) and portable packages.
