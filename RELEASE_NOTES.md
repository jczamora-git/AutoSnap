# AutoSnap v1.2.0 Release Notes

AutoSnap v1.2.0 is a major upgrade that expands AutoSnap from automatic screenshot capture into high-performance local video processing and offline AI speech transcription.

## Key Highlights

### 🎞 Local Video Processing
- Fast, non-real-time interval frame extraction for local video files (`.mp4`, `.mkv`, `.mov`, `.avi`, `.webm`, `.m4v`).
- Automated metadata extraction (duration, resolution, framerate, video/audio codecs) via FFprobe.
- Batch frame extraction with clean timestamp naming (`00-00-00.jpg`, `00-05-00.jpg`).

### 🎙 Local Whisper Transcription
- Fully offline AI speech-to-text powered by Whisper (`Whisper.net` with native runtime).
- **System Audio Transcription**: Transcribe live desktop audio (meetings, Zoom, Teams, YouTube) via WASAPI loopback without capturing background room microphone noise.
- **Browser Tab Audio**: Stream and transcribe browser tab audio directly alongside screenshot capture.
- **Local Media Transcription**: Transcribe audio and video files directly to subtitle/transcript documents.
- **Taglish & Multilingual Support**: First-class support for Taglish, English, and Filipino without unwanted translation into pure English.
- **Multi-Format Export**: One-click export to **TXT**, **SRT**, and **VTT**.
- **Autosave & Recovery**: Protects long-running live transcription sessions with 30-second autosaves.
- **Built-in Model Manager**: Download official Whisper models (`Tiny`, `Base`, `Small Multilingual`, `Medium`) on demand.

## Distribution Assets
- `AutoSnap-Setup-v1.2.0.exe` (Windows Installer)
- `AutoSnap-v1.2.0-portable.zip` (Self-Contained Portable Archive)
- `AutoSnap-v1.2.0-portable.zip.sha256` (SHA256 Checksum)

## Notes & Requirements
- **OS**: Windows 10 / Windows 11 (64-bit)
- **Deployment**: Standalone self-contained Windows x64 build (no .NET runtime installation required).
- **FFmpeg**: Required for video frame extraction and local file audio extraction (detected from PATH, Chocolatey, Scoop, or Settings).
- **Whisper Models**: Downloaded on demand through the Model Manager and saved in `%LocalAppData%\AutoSnap\Models\Whisper\`.
- **Code Signing**: Application binary is currently unsigned; SmartScreen prompt may appear upon first launch.
