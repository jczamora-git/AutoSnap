namespace AutoSnap.BrowserCapture;

public static class BrowserCapturePage
{
    public static string GetHtml()
    {
        return """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>AutoSnap — Browser Tab Capture</title>
    <style>
        :root {
            --primary: #0078d7;
            --primary-hover: #0063b1;
            --bg: #f8f9fa;
            --card-bg: #ffffff;
            --text-main: #212529;
            --text-muted: #6c757d;
            --border: #e9ecef;
            --success: #28a745;
            --danger: #dc3545;
            --warning: #ffc107;
        }

        * {
            box-sizing: border-box;
            margin: 0;
            padding: 0;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
        }

        body {
            background-color: var(--bg);
            color: var(--text-main);
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            padding: 24px;
        }

        .container {
            background: var(--card-bg);
            width: 100%;
            max-width: 640px;
            border-radius: 16px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
            border: 1px solid var(--border);
            overflow: hidden;
        }

        .header {
            padding: 24px 32px;
            background: #ffffff;
            border-bottom: 1px solid var(--border);
            display: flex;
            align-items: center;
            justify-content: space-between;
        }

        .header-title h1 {
            font-size: 20px;
            font-weight: 600;
            color: var(--text-main);
        }

        .header-title p {
            font-size: 13px;
            color: var(--text-muted);
            margin-top: 4px;
        }

        .ws-badge {
            font-size: 12px;
            padding: 4px 10px;
            border-radius: 20px;
            font-weight: 500;
            background: #e9ecef;
            color: var(--text-muted);
        }

        .ws-badge.connected {
            background: #d4edda;
            color: #155724;
        }

        .content {
            padding: 32px;
            text-align: center;
        }

        .status-box {
            background: #f1f3f5;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 24px;
            border: 1px dashed #ced4da;
        }

        .status-badge {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
            font-weight: 600;
            margin-bottom: 6px;
            color: var(--text-muted);
        }

        .status-dot {
            width: 10px;
            height: 10px;
            border-radius: 50%;
            background-color: #adb5bd;
            display: inline-block;
        }

        .status-active .status-dot {
            background-color: var(--success);
            box-shadow: 0 0 0 4px rgba(40, 167, 69, 0.2);
        }

        .status-active .status-badge {
            color: #155724;
        }

        .source-name {
            font-size: 16px;
            font-weight: 600;
            color: var(--text-main);
            margin-top: 4px;
            word-break: break-word;
        }

        .source-hint {
            font-size: 13px;
            color: var(--text-muted);
            margin-top: 6px;
        }

        .btn-group {
            display: flex;
            gap: 12px;
            justify-content: center;
            flex-wrap: wrap;
            margin-bottom: 24px;
        }

        button {
            cursor: pointer;
            padding: 12px 24px;
            font-size: 14px;
            font-weight: 600;
            border-radius: 8px;
            border: none;
            transition: all 0.2s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .btn-primary {
            background-color: var(--primary);
            color: white;
        }

        .btn-primary:hover {
            background-color: var(--primary-hover);
            transform: translateY(-1px);
        }

        .btn-danger {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }

        .btn-danger:hover {
            background-color: #f1b0b7;
        }

        .btn-secondary {
            background-color: #e9ecef;
            color: var(--text-main);
        }

        .btn-secondary:hover {
            background-color: #dee2e6;
        }

        .video-container {
            width: 100%;
            max-height: 240px;
            border-radius: 8px;
            overflow: hidden;
            background: #000;
            display: none;
            margin-top: 16px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }

        video {
            width: 100%;
            height: 100%;
            max-height: 240px;
            object-fit: contain;
            display: block;
        }

        .notice {
            font-size: 13px;
            color: var(--text-muted);
            line-height: 1.5;
            margin-top: 16px;
            background: #eef5fc;
            border-radius: 8px;
            padding: 12px;
            border-left: 4px solid var(--primary);
            text-align: left;
        }

        canvas {
            display: none;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <div class="header-title">
                <h1>AutoSnap Browser Capture</h1>
                <p>Capture a tab from your current Chrome session</p>
            </div>
            <div id="wsBadge" class="ws-badge">Connecting to AutoSnap...</div>
        </div>

        <div class="content">
            <div id="statusBox" class="status-box">
                <div class="status-badge">
                    <span class="status-dot"></span>
                    <span id="statusText">Waiting for source...</span>
                </div>
                <div id="sourceName" class="source-name">No tab selected</div>
                <div id="sourceHint" class="source-hint">Click below to open Chrome's tab sharing picker</div>
            </div>

            <div class="btn-group">
                <button id="btnChoose" class="btn-primary" onclick="startCapture()">
                    📺 Choose Chrome Tab
                </button>
                <button id="btnChange" class="btn-secondary" style="display: none;" onclick="startCapture()">
                    🔄 Change Tab
                </button>
                <button id="btnStop" class="btn-danger" style="display: none;" onclick="stopCapture()">
                    ⏹ Stop Sharing
                </button>
            </div>

            <div id="videoWrapper" class="video-container">
                <video id="captureVideo" autoplay playsinline muted></video>
            </div>

            <div class="notice">
                💡 <strong>Instructions:</strong> When prompted by Chrome, select the <strong>Chrome Tab</strong> section and choose the tab you wish to capture (e.g. Google Meet, YouTube, etc.). AutoSnap will automatically start receiving frames.
            </div>

            <canvas id="captureCanvas"></canvas>
        </div>
    </div>

    <script>
        let ws = null;
        let currentStream = null;
        let audioCtx = null;
        let audioSourceNode = null;
        let audioProcessor = null;

        const video = document.getElementById('captureVideo');
        const canvas = document.getElementById('captureCanvas');
        const ctx = canvas.getContext('2d');

        const wsBadge = document.getElementById('wsBadge');
        const statusBox = document.getElementById('statusBox');
        const statusText = document.getElementById('statusText');
        const sourceName = document.getElementById('sourceName');
        const sourceHint = document.getElementById('sourceHint');
        const btnChoose = document.getElementById('btnChoose');
        const btnChange = document.getElementById('btnChange');
        const btnStop = document.getElementById('btnStop');
        const videoWrapper = document.getElementById('videoWrapper');

        function connectWebSocket() {
            const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
            const wsUrl = `${protocol}//${location.host}/ws`;

            ws = new WebSocket(wsUrl);

            ws.onopen = () => {
                wsBadge.textContent = '● AutoSnap Connected';
                wsBadge.classList.add('connected');
                
                // If stream is already active, notify AutoSnap
                if (currentStream && currentStream.active) {
                    notifyStreamStarted();
                }
            };

            ws.onclose = () => {
                wsBadge.textContent = 'AutoSnap Disconnected';
                wsBadge.classList.remove('connected');
                setTimeout(connectWebSocket, 2000);
            };

            ws.onerror = () => {
                ws.close();
            };

            ws.onmessage = (event) => {
                try {
                    const msg = JSON.parse(event.data);
                    handleServerMessage(msg);
                } catch (e) {
                    console.error('Failed to parse WebSocket message', e);
                }
            };
        }

        function handleServerMessage(msg) {
            if (msg.type === 'requestFrame') {
                sendFrame(msg.requestId, msg.format || 'jpeg', msg.quality || 0.9);
            } else if (msg.type === 'stopStream') {
                stopCapture();
            }
        }

        async function startCapture() {
            if (!navigator.mediaDevices || !navigator.mediaDevices.getDisplayMedia) {
                alert('getDisplayMedia is not supported in this browser environment.');
                return;
            }

            try {
                // Request tab sharing with optional audio
                const stream = await navigator.mediaDevices.getDisplayMedia({
                    video: {
                        displaySurface: 'browser'
                    },
                    audio: true
                });

                if (currentStream) {
                    cleanupStream();
                }

                currentStream = stream;
                video.srcObject = stream;
                await video.play();

                videoWrapper.style.display = 'block';

                const track = stream.getVideoTracks()[0];
                const tabTitle = track ? track.label || 'Chrome Tab' : 'Chrome Tab';

                const audioTracks = stream.getAudioTracks();
                const hasAudio = audioTracks.length > 0;

                statusBox.classList.add('status-active');
                statusText.textContent = 'Sharing Active';
                sourceName.textContent = tabTitle;
                sourceHint.textContent = hasAudio
                    ? 'AutoSnap is capturing video snapshots and streaming tab audio.'
                    : 'AutoSnap is capturing video snapshots (tab audio not shared).';

                btnChoose.style.display = 'none';
                btnChange.style.display = 'inline-flex';
                btnStop.style.display = 'inline-flex';

                notifyStreamStarted();
                setupAudioStreaming(stream);

                if (track) {
                    track.onended = () => {
                        handleStreamEnded();
                    };
                }
            } catch (err) {
                console.warn('Capture cancelled or failed:', err);
            }
        }

        function setupAudioStreaming(stream) {
            const audioTracks = stream.getAudioTracks();
            const hasAudio = audioTracks.length > 0;

            sendWs({
                type: 'audioTrackStatus',
                hasAudio: hasAudio
            });

            if (!hasAudio) return;

            try {
                audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 16000 });
                audioSourceNode = audioCtx.createMediaStreamSource(stream);
                audioProcessor = audioCtx.createScriptProcessor(4096, 1, 1);

                audioProcessor.onaudioprocess = (e) => {
                    if (!ws || ws.readyState !== WebSocket.OPEN) return;
                    const inputData = e.inputBuffer.getChannelData(0);
                    const pcmData = new Int16Array(inputData.length);
                    for (let i = 0; i < inputData.length; i++) {
                        const s = Math.max(-1, Math.min(1, inputData[i]));
                        pcmData[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
                    }
                    ws.send(pcmData.buffer);
                };

                audioSourceNode.connect(audioProcessor);
                audioProcessor.connect(audioCtx.destination);
            } catch (ex) {
                console.warn('Audio streaming setup failed:', ex);
            }
        }

        function cleanupStream() {
            if (audioProcessor) {
                try { audioProcessor.disconnect(); } catch {}
                audioProcessor = null;
            }
            if (audioSourceNode) {
                try { audioSourceNode.disconnect(); } catch {}
                audioSourceNode = null;
            }
            if (audioCtx) {
                try { audioCtx.close(); } catch {}
                audioCtx = null;
            }
            if (currentStream) {
                currentStream.getTracks().forEach(t => t.stop());
                currentStream = null;
            }
        }

        function notifyStreamStarted() {
            if (!currentStream) return;
            const track = currentStream.getVideoTracks()[0];
            const settings = track ? track.getSettings() : {};

            sendWs({
                type: 'streamStarted',
                title: track ? track.label : 'Chrome Tab',
                width: settings.width || video.videoWidth || 1280,
                height: settings.height || video.videoHeight || 720
            });
        }

        function stopCapture() {
            cleanupStream();
            handleStreamEnded();
        }

        function handleStreamEnded() {
            cleanupStream();
            video.srcObject = null;
            videoWrapper.style.display = 'none';

            statusBox.classList.remove('status-active');
            statusText.textContent = 'Waiting for source...';
            sourceName.textContent = 'No tab selected';
            sourceHint.textContent = 'Click below to choose a Chrome tab to share';

            btnChoose.style.display = 'inline-flex';
            btnChange.style.display = 'none';
            btnStop.style.display = 'none';

            sendWs({ type: 'streamEnded' });
        }

        function sendFrame(requestId, format, quality) {
            if (!currentStream || video.readyState < 2 || video.videoWidth === 0) {
                sendWs({
                    type: 'frameError',
                    requestId: requestId,
                    message: 'Video track is not ready or stream ended.'
                });
                return;
            }

            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

            const mime = format === 'png' ? 'image/png' : 'image/jpeg';
            const dataUrl = canvas.toDataURL(mime, quality);
            const base64 = dataUrl.substring(dataUrl.indexOf(',') + 1);

            sendWs({
                type: 'frameData',
                requestId: requestId,
                data: base64,
                width: canvas.width,
                height: canvas.height
            });
        }

        function sendWs(data) {
            if (ws && ws.readyState === WebSocket.OPEN) {
                ws.send(JSON.stringify(data));
            }
        }

        // Initialize WebSocket connection
        connectWebSocket();
    </script>
</body>
</html>
""";
    }
}
