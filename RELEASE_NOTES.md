# AutoSnap v1.2.0

AutoSnap v1.2.0 is a major feature update that expands AutoSnap beyond automatic screenshot capture with local video processing and experimental offline AI transcription.

This release includes the local-video features originally planned for v1.1.0 together with the new v1.2.0 transcription system.

---

## What's New

### Local Video Processing

AutoSnap can now process local video files without playing them in real time.

Features include:

- Select local video files for processing
- Read video duration, resolution, frame rate, codec, and audio information
- Extract screenshots at configurable intervals
- Process video frames faster than real-time where supported
- JPEG and PNG snapshot output
- Processing progress and cancellation
- Organized per-video output folders
- Optional transcription of local media

Local video processing uses FFmpeg and FFprobe.

---

## Experimental Transcription — Beta

Transcription is now available as an experimental feature.

Supported transcription sources include:

- Browser tab audio
- Windows system audio
- Local video
- Local audio

All transcription is designed to run locally using Whisper-compatible models.

No paid cloud speech API is required.

### Language Modes

AutoSnap currently provides:

- Auto Detect
- English
- Filipino / Tagalog
- Taglish

Taglish mode is intended to preserve mixed Filipino and English speech rather than translating everything into English.

Example:

> So ngayon, we're going to discuss yung database normalization.

---

## Whisper Model Manager

AutoSnap now includes a local model manager for Whisper transcription.

Supported model options include:

- Tiny
- Base
- Small Multilingual
- Medium
- Large v3
- Large v3 Turbo
- Large v3 Turbo Q5

Whisper models are downloaded separately and are not bundled with the AutoSnap installer.

This keeps the base application significantly smaller.

---

## Hardware-Aware Model Recommendations

The Model Manager can inspect available system resources and provide guidance based on:

- CPU
- Logical processor count
- Installed RAM
- Detected GPU
- Available Whisper backend

AutoSnap distinguishes between models that are better suited for:

- Live transcription
- Offline/local-media transcription

Recommendations are advisory only. Users may still select larger models when desired.

---

## Live Transcription Performance Monitoring

Live transcription now tracks processing performance, including:

- Live audio duration
- Processed duration
- Transcription backlog
- Real-time processing speed

When transcription cannot keep up with incoming audio, AutoSnap can warn that a smaller model may be more appropriate.

Example:

```text
Live       00:05:09
Processed  00:00:15
Backlog    00:04:54
Speed      0.1× realtime
```

Large models can be significantly slower on CPU-only systems.

For live transcription, Base or Small is generally more suitable for lower-end or older CPUs.

Large v3 Turbo Q5, Large v3 Turbo, and Large v3 are primarily better suited to powerful systems or offline processing.

---

## Browser Tab Audio

AutoSnap's existing browser capture system has been extended to support tab audio.

The browser capture workflow continues to use the browser's native sharing permission system.

AutoSnap does not bypass browser permissions.

Typical workflow:

1. Select Browser Tab.
2. Enable transcription.
3. Open the AutoSnap browser sharing helper.
4. Select the desired Chrome tab.
5. Enable **Share tab audio** when Chrome offers the option.
6. Begin capture/transcription.

The selected tab can provide:

```text
MediaStream
├── Video → AutoSnap snapshots
└── Audio → AutoSnap transcription
```

This allows screenshots and transcription to run from the same shared browser source.

---

## Existing Chrome Profile Support

Browser capture continues to work with the user's normal Chrome profile.

AutoSnap does not require:

- A separate Chrome profile
- ChromeDriver
- Selenium
- Remote debugging
- A Chrome extension

Browser tab capture uses Chrome's native media-sharing workflow and explicit user permission.

---

## System Audio Transcription

AutoSnap can capture Windows playback audio for transcription.

This can be useful with applications such as:

- Zoom
- Microsoft Teams
- VLC
- YouTube
- Other desktop applications producing system audio

System audio transcription does not require microphone capture.

---

## Local Media Transcription

Supported local media can be converted and transcribed through the local processing pipeline.

Depending on FFmpeg support, common formats include:

### Video

- MP4
- MKV
- MOV
- AVI
- WebM
- M4V

### Audio

- WAV
- MP3
- M4A
- AAC
- FLAC
- OGG
- WebM

A local video can be processed for both:

- Snapshot extraction
- Transcript generation

in the same processing session.

---

## Transcript Export

Transcripts can be exported as:

- TXT
- SRT
- VTT

Transcript text can also be reviewed and edited before export.

Timestamp information is preserved for subtitle-oriented formats.

---

## Transcript Recovery

AutoSnap includes transcript autosave/recovery support for long sessions.

This reduces the risk of losing an entire live transcript if a session is interrupted unexpectedly.

---

## Screenshot Capture

Existing AutoSnap capture functionality remains available:

- Application window capture
- Display/monitor capture
- Browser tab capture
- Configurable snapshot intervals
- Manual snapshot capture
- Live preview
- JPG and PNG output
- Pause / Resume / Stop controls
- Configurable output directory
- Background/tray operation where enabled

---

## Recommended Transcription Models

### Low-resource systems

**Tiny / Base**

Best when low latency is more important than maximum accuracy.

### Typical systems

**Small Multilingual**

Recommended balance for:

- English
- Filipino
- Taglish
- Live meetings
- Lectures
- Webinars

### Higher-quality offline transcription

**Large v3 Turbo Q5**

Provides a strong quality/size balance but may still be too slow for live transcription on older CPU-only systems.

### Powerful systems

**Large v3 Turbo / Large v3**

Best suited for systems with strong CPUs or compatible accelerated backends, particularly for offline processing.

---

## Important: Transcription Is Experimental

The transcription subsystem is currently considered:

**BETA / EXPERIMENTAL**

Accuracy and performance can vary depending on:

- Selected Whisper model
- CPU performance
- Available acceleration
- Audio quality
- Background noise
- Language switching
- Browser/audio source
- System load

Live transcription may fall behind real-time audio when the selected model is too demanding.

AutoSnap will continue improving transcription performance and reliability in future versions.

---

## Privacy

AutoSnap is designed around local processing.

- Screenshots are stored locally.
- Transcripts are stored locally.
- Whisper inference runs locally.
- Browser capture requires explicit browser permission.
- AutoSnap does not require a cloud transcription API.

Whisper model downloads and dependency downloads naturally require an internet connection.

---

## Requirements

### Operating System

- Windows 10 x64
- Windows 11 x64

### Screenshot Capture

No dedicated GPU is required.

### Transcription

Minimum practical configuration:

- 8 GB RAM
- Modern x64 CPU
- Tiny, Base, or Small model recommended

Recommended:

- 16 GB RAM
- Intel Core i5 / AMD Ryzen 5 class CPU or better
- Small Multilingual for live transcription

Larger models may require substantially more processing time.

---

## FFmpeg

FFmpeg and FFprobe are required for local video processing and local-media audio extraction.

If AutoSnap cannot locate FFmpeg automatically, configure the executable locations under:

**Settings → Video / FFmpeg**

---

## Known Limitations

- Transcription is currently experimental.
- Live transcription performance depends heavily on CPU/model selection.
- Large Whisper models may be much slower than real time on older CPUs.
- Browser tab audio requires the user to explicitly share tab audio.
- Chromium browsers may throttle media capture under some background/minimized conditions.
- Local media processing requires working FFmpeg/FFprobe binaries.
- GPU detection does not automatically mean Whisper is using GPU acceleration.
- AutoSnap is currently unsigned, so Windows SmartScreen may display a warning on first launch.

---

## Upgrade Notes

v1.2.0 includes the features originally planned for v1.1.0.

There is no separate public v1.1.0 release.

Release progression:

```text
v1.0.0
Initial screenshot capture release

↓

v1.2.0
Local Video Processing
+ Experimental Local Transcription
```

---

Thank you for testing AutoSnap.

Transcription remains under active development, so bug reports and performance feedback are especially useful for this release.
