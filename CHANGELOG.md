# Changelog

## v1.2.0

### Added

- Local video processing
- FFmpeg/FFprobe video metadata extraction
- Interval-based video screenshot extraction
- Local media transcription
- Live Windows system-audio transcription
- Browser-tab audio transcription
- Local Whisper transcription
- English transcription mode
- Filipino / Tagalog transcription mode
- Taglish transcription mode
- Automatic language detection
- Whisper Model Manager
- Tiny, Base, Small, Medium, Large v3, Large v3 Turbo, and Large v3 Turbo Q5 models
- Hardware-aware model recommendations
- Live transcription performance/backlog monitoring
- TXT transcript export
- SRT subtitle export
- VTT subtitle export
- Transcript autosave/recovery

### Improved

- Browser capture can provide video and tab audio from one shared source
- Existing Chrome-profile browser capture workflow
- Source-specific transcription configuration
- Model selection guidance for live vs. offline processing
- Cancellation and async resource lifecycle handling
- Whisper Model Manager download lifecycle
- Long-running transcription responsiveness

### Fixed

- CancellationTokenSource disposal race conditions
- WhisperModelManagerForm disposed-object crash
- Model Manager list population regression
- Background UI updates occurring after dialog disposal
- Model download cancellation lifecycle handling

### Experimental

The transcription subsystem is currently marked Beta/Experimental.

Live transcription performance depends heavily on the selected model and available hardware. Larger models may fall behind real-time audio on CPU-only systems.

### Notes

The features originally planned for v1.1.0 have been merged into v1.2.0. No standalone v1.1.0 public release is planned.
