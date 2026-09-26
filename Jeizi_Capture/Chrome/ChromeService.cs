using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using AutoSnap.Models;
using Microsoft.Win32;

namespace AutoSnap.Chrome;

public class ChromeService : IDisposable
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
    private Process? _chromeProcess;
    private ChromeConnectionInfo? _connectionInfo;
    private ChromeConnectionState _state = ChromeConnectionState.Disconnected;
    private readonly object _stateLock = new();
    private bool _disposed;

    public ChromeConnectionState State
    {
        get { lock (_stateLock) return _state; }
        private set
        {
            lock (_stateLock)
            {
                if (_state == value) return;
                _state = value;
            }
            ConnectionStateChanged?.Invoke(this, value);
        }
    }

    public ChromeConnectionInfo? CurrentConnection => _connectionInfo;
    public bool IsConnected => State == ChromeConnectionState.Connected;

    public event EventHandler<ChromeConnectionState>? ConnectionStateChanged;
    public event EventHandler<string>? ChromeExited;

    public static string? FindChromeExecutable(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
        {
            return customPath;
        }

        var candidatePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        };

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Search Windows Registry App Paths
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe")
                         ?? Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe");

            var val = key?.GetValue("")?.ToString();
            if (!string.IsNullOrWhiteSpace(val) && File.Exists(val))
            {
                return val;
            }
        }
        catch
        {
            // Ignore registry read issues
        }

        return null;
    }

    public static int FindAvailablePort(int preferredPort = 9222)
    {
        if (preferredPort > 0 && IsPortAvailable(preferredPort))
        {
            return preferredPort;
        }

        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static bool IsPortAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ChromeConnectionInfo> LaunchCaptureChromeAsync(
        ChromeSettings settings,
        CancellationToken cancellationToken = default)
    {
        State = ChromeConnectionState.Starting;

        string? chromeExe = FindChromeExecutable(settings.CustomChromeExecutablePath);
        if (string.IsNullOrWhiteSpace(chromeExe) || !File.Exists(chromeExe))
        {
            State = ChromeConnectionState.Error;
            throw new FileNotFoundException("Google Chrome executable could not be found. Please configure the Chrome path in Settings.", chromeExe);
        }

        string profileDir = string.IsNullOrWhiteSpace(settings.ProfileDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSnap", "ChromeProfile")
            : settings.ProfileDirectory;

        if (!Directory.Exists(profileDir))
        {
            Directory.CreateDirectory(profileDir);
        }

        int port = settings.DebugPort > 0 ? settings.DebugPort : FindAvailablePort(9222);

        var startInfo = new ProcessStartInfo
        {
            FileName = chromeExe,
            Arguments = $"--remote-debugging-port={port} --remote-debugging-address=127.0.0.1 --user-data-dir=\"{profileDir}\" --no-first-run --no-default-browser-check",
            UseShellExecute = false
        };

        try
        {
            _chromeProcess = Process.Start(startInfo);
            if (_chromeProcess == null)
            {
                State = ChromeConnectionState.Error;
                throw new InvalidOperationException("Failed to launch Google Chrome process.");
            }

            _chromeProcess.EnableRaisingEvents = true;
            _chromeProcess.Exited += ChromeProcess_Exited;

            _connectionInfo = new ChromeConnectionInfo
            {
                ExecutablePath = chromeExe,
                ProfileDirectory = profileDir,
                DebuggingPort = port,
                ProcessId = _chromeProcess.Id,
                StartedByAutoSnap = true
            };

            State = ChromeConnectionState.Connecting;

            // Wait for DevTools endpoint to become available
            bool isReady = await WaitForDevToolsEndpointAsync(port, TimeSpan.FromSeconds(12), cancellationToken);
            if (!isReady)
            {
                State = ChromeConnectionState.Error;
                throw new TimeoutException($"Chrome DevTools did not respond on 127.0.0.1:{port} within the timeout.");
            }

            State = ChromeConnectionState.Connected;
            return _connectionInfo;
        }
        catch (Exception)
        {
            State = ChromeConnectionState.Error;
            throw;
        }
    }

    public async Task<ChromeConnectionInfo> ConnectToExistingAsync(int port, CancellationToken cancellationToken = default)
    {
        State = ChromeConnectionState.Connecting;

        try
        {
            string url = $"http://127.0.0.1:{port}/json/version";
            var response = await HttpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            _connectionInfo = new ChromeConnectionInfo
            {
                DebuggingPort = port,
                StartedByAutoSnap = false
            };

            State = ChromeConnectionState.Connected;
            return _connectionInfo;
        }
        catch (Exception ex)
        {
            State = ChromeConnectionState.Error;
            throw new InvalidOperationException($"Could not connect to Chrome debugging endpoint on port {port}: {ex.Message}", ex);
        }
    }

    private async Task<bool> WaitForDevToolsEndpointAsync(int port, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        string url = $"http://127.0.0.1:{port}/json/version";

        while (stopwatch.Elapsed < timeout && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await HttpClient.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                // Chrome is still starting up
            }

            await Task.Delay(250, cancellationToken);
        }

        return false;
    }

    public async Task<List<ChromeTabInfo>> GetTabsAsync(CancellationToken cancellationToken = default)
    {
        if (_connectionInfo == null)
        {
            throw new InvalidOperationException("Not connected to any Chrome debugging instance.");
        }

        string url = $"http://127.0.0.1:{_connectionInfo.DebuggingPort}/json/list";
        try
        {
            var response = await HttpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tabs = await response.Content.ReadFromJsonAsync<List<ChromeTabInfo>>(cancellationToken: cancellationToken);
            if (tabs == null) return new List<ChromeTabInfo>();

            // Filter for page targets only
            return tabs.Where(t => string.Equals(t.Type, "page", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        catch (HttpRequestException ex)
        {
            State = ChromeConnectionState.ConnectionLost;
            throw new InvalidOperationException($"Failed to retrieve Chrome tabs: {ex.Message}", ex);
        }
    }

    private void ChromeProcess_Exited(object? sender, EventArgs e)
    {
        State = ChromeConnectionState.ConnectionLost;
        ChromeExited?.Invoke(this, "The dedicated Chrome instance has exited.");
    }

    public void CleanupOnAppExit(bool closeIfStartedByAutoSnap)
    {
        if (closeIfStartedByAutoSnap && _connectionInfo?.StartedByAutoSnap == true && _chromeProcess != null)
        {
            try
            {
                if (!_chromeProcess.HasExited)
                {
                    _chromeProcess.CloseMainWindow();
                    if (!_chromeProcess.WaitForExit(1500))
                    {
                        _chromeProcess.Kill();
                    }
                }
            }
            catch
            {
                // Suppress cleanup error on shutdown
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_chromeProcess != null)
            {
                _chromeProcess.Exited -= ChromeProcess_Exited;
                _chromeProcess.Dispose();
            }
        }
    }
}
