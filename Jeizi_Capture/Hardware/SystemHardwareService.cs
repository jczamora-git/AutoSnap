using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using AutoSnap.Transcription;

namespace AutoSnap.Hardware;

public class SystemHardwareService
{
    private static SystemHardwareInfo? _cachedHardwareInfo;
    private static readonly object _lock = new();

    public static SystemHardwareInfo GetSystemHardwareInfo()
    {
        lock (_lock)
        {
            if (_cachedHardwareInfo != null)
                return _cachedHardwareInfo;

            _cachedHardwareInfo = DetectHardware();
            return _cachedHardwareInfo;
        }
    }

    private static SystemHardwareInfo DetectHardware()
    {
        string cpuName = "Unknown CPU";
        int logicalCores = Environment.ProcessorCount;
        int physicalCores = Math.Max(1, logicalCores / 2);
        long totalRam = 8L * 1024 * 1024 * 1024;
        long availRam = 4L * 1024 * 1024 * 1024;
        string gpuName = "Unknown / Integrated";
        long vram = 0;
        string osVersion = RuntimeInformation.OSDescription;
        string arch = RuntimeInformation.ProcessArchitecture.ToString();

        try
        {
            // 1. Processor info
            using var searcherProc = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor");
            foreach (var item in searcherProc.Get())
            {
                if (item["Name"] != null)
                {
                    cpuName = item["Name"].ToString()?.Trim() ?? cpuName;
                }
                if (item["NumberOfLogicalProcessors"] != null && int.TryParse(item["NumberOfLogicalProcessors"].ToString(), out int logProc))
                {
                    logicalCores = logProc;
                }
                if (item["NumberOfCores"] != null && int.TryParse(item["NumberOfCores"].ToString(), out int phyProc))
                {
                    physicalCores = phyProc;
                }
                break;
            }
        }
        catch { }

        try
        {
            // 2. Memory info
            using var searcherComp = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (var item in searcherComp.Get())
            {
                if (item["TotalPhysicalMemory"] != null && long.TryParse(item["TotalPhysicalMemory"].ToString(), out long totalMem))
                {
                    totalRam = totalMem;
                }
                break;
            }

            using var searcherOs = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (var item in searcherOs.Get())
            {
                if (item["FreePhysicalMemory"] != null && long.TryParse(item["FreePhysicalMemory"].ToString(), out long freeKb))
                {
                    availRam = freeKb * 1024;
                }
                break;
            }
        }
        catch
        {
            try
            {
                var gcInfo = GC.GetGCMemoryInfo();
                if (gcInfo.TotalAvailableMemoryBytes > 0)
                {
                    totalRam = gcInfo.TotalAvailableMemoryBytes;
                }
            }
            catch { }
        }

        try
        {
            // 3. GPU info
            using var searcherGpu = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
            foreach (var item in searcherGpu.Get())
            {
                string? name = item["Name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    gpuName = name.Trim();
                    if (item["AdapterRAM"] != null && long.TryParse(item["AdapterRAM"].ToString(), out long vramBytes) && vramBytes > 0)
                    {
                        vram = vramBytes;
                    }
                    break;
                }
            }
        }
        catch { }

        return new SystemHardwareInfo
        {
            CpuName = cpuName,
            LogicalCoreCount = logicalCores,
            PhysicalCoreCount = physicalCores,
            TotalPhysicalMemoryBytes = totalRam,
            AvailablePhysicalMemoryBytes = availRam,
            GpuName = gpuName,
            DedicatedVramBytes = vram,
            OsVersion = osVersion,
            Architecture = arch,
            WhisperBackend = "CPU (whisper.net runtime)"
        };
    }

    public static ModelRecommendation GetRecommendation(WhisperModelInfo model, SystemHardwareInfo? hardwareInfo = null)
    {
        var hw = hardwareInfo ?? GetSystemHardwareInfo();
        double ramGb = hw.TotalPhysicalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
        int cores = hw.LogicalCoreCount;

        string id = model.Name.ToLowerInvariant();

        switch (id)
        {
            case "tiny":
                return new ModelRecommendation
                {
                    Model = model,
                    LiveRating = ModelRating.Excellent,
                    LiveRatingText = "Excellent (Live)",
                    LiveRatingColor = Color.FromArgb(40, 167, 69),
                    LiveGuidance = "Minimal memory and CPU requirements. Fast real-time live transcription.",
                    OfflineRating = ModelRating.Excellent,
                    OfflineRatingText = "Supported (Offline)",
                    OfflineRatingColor = Color.FromArgb(40, 167, 69),
                    OfflineGuidance = "Ultra-fast extraction for local audio/video.",
                    EstimatedUseSummary = "Live: Ultra fast (RTF < 0.2) | Offline: Ultra fast"
                };

            case "base":
                return new ModelRecommendation
                {
                    Model = model,
                    LiveRating = ModelRating.Excellent,
                    LiveRatingText = "Excellent (Live)",
                    LiveRatingColor = Color.FromArgb(40, 167, 69),
                    LiveGuidance = "Lightweight and fast. Runs comfortably in real-time on 2–4 core CPUs.",
                    OfflineRating = ModelRating.Excellent,
                    OfflineRatingText = "Supported (Offline)",
                    OfflineRatingColor = Color.FromArgb(40, 167, 69),
                    OfflineGuidance = "Very fast local media processing.",
                    EstimatedUseSummary = "Live: Very fast (RTF ~ 0.3) | Offline: Very fast"
                };

            case "small":
                if (ramGb >= 8 && cores >= 4)
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.Recommended,
                        LiveRatingText = "Recommended (Live)",
                        LiveRatingColor = Color.FromArgb(40, 167, 69),
                        LiveGuidance = "Recommended balanced model for real-time live transcription on this system.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended (Offline)",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "Optimal balance of speed and multilingual accuracy for local files.",
                        EstimatedUseSummary = "Live: Real-time (RTF ~ 0.6–0.9) | Offline: Fast"
                    };
                }
                else
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.Usable,
                        LiveRatingText = "Usable",
                        LiveRatingColor = Color.FromArgb(0, 120, 215),
                        LiveGuidance = "Runs reliably. Base model is faster if CPU is heavily loaded.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "Reliable for offline media.",
                        EstimatedUseSummary = "Live: Moderate | Offline: Good"
                    };
                }

            case "medium":
                if (cores >= 6 && ramGb >= 16)
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.Usable,
                        LiveRatingText = "Usable",
                        LiveRatingColor = Color.FromArgb(0, 120, 215),
                        LiveGuidance = "Capable of live transcription on multi-core CPUs.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "High accuracy for offline media.",
                        EstimatedUseSummary = "Live: Moderate | Offline: Good"
                    };
                }
                else
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.MayBeSlow,
                        LiveRatingText = "May Fall Behind (Live)",
                        LiveRatingColor = Color.FromArgb(255, 140, 0),
                        LiveGuidance = "May not maintain real-time speed on 2–4 core CPUs. Small or Base recommended for live audio.",
                        OfflineRating = ModelRating.Usable,
                        OfflineRatingText = "Usable (Offline)",
                        OfflineRatingColor = Color.FromArgb(0, 120, 215),
                        OfflineGuidance = "Usable for offline files where real-time speed is not required.",
                        EstimatedUseSummary = "Live: Slower than realtime (RTF > 1.0) | Offline: Usable"
                    };
                }

            case "turbo q5":
            case "large v3 turbo q5":
                if (cores >= 6 && ramGb >= 16)
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.Recommended,
                        LiveRatingText = "Recommended (Live & Offline)",
                        LiveRatingColor = Color.FromArgb(40, 167, 69),
                        LiveGuidance = "High-quality quantized Turbo model with strong performance on modern multi-core systems.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended (Offline)",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "Excellent quality-to-size balance for local media files.",
                        EstimatedUseSummary = "Live: Good | Offline: Excellent"
                    };
                }
                else
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.NotRecommended,
                        LiveRatingText = "Not Recommended for Live",
                        LiveRatingColor = Color.FromArgb(220, 53, 69),
                        LiveGuidance = "Too heavy for real-time live CPU transcription on this machine (leads to growing delay). Use Small or Base for live audio.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended for Offline",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "High accuracy with lower storage and memory requirements. Excellent for local video/audio.",
                        EstimatedUseSummary = "Live: Severely behind (RTF >> 1.0) | Offline: Recommended for Quality"
                    };
                }

            case "large v3 turbo":
                if (cores >= 8 && ramGb >= 16)
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.Recommended,
                        LiveRatingText = "Recommended",
                        LiveRatingColor = Color.FromArgb(40, 167, 69),
                        LiveGuidance = "High accuracy with fast Turbo inference on high-core systems.",
                        OfflineRating = ModelRating.Recommended,
                        OfflineRatingText = "Recommended",
                        OfflineRatingColor = Color.FromArgb(40, 167, 69),
                        OfflineGuidance = "High-speed and high-accuracy processing for local files.",
                        EstimatedUseSummary = "Live: Good | Offline: Excellent"
                    };
                }
                else
                {
                    return new ModelRecommendation
                    {
                        Model = model,
                        LiveRating = ModelRating.NotRecommended,
                        LiveRatingText = "Not Recommended for Live",
                        LiveRatingColor = Color.FromArgb(220, 53, 69),
                        LiveGuidance = "Full Large v3 Turbo is too demanding for real-time live CPU transcription on this system.",
                        OfflineRating = ModelRating.Usable,
                        OfflineRatingText = "Usable (Offline)",
                        OfflineRatingColor = Color.FromArgb(0, 120, 215),
                        OfflineGuidance = "High accuracy for offline media processing.",
                        EstimatedUseSummary = "Live: Severely behind (RTF >> 1.0) | Offline: Usable"
                    };
                }

            case "large v3":
                return new ModelRecommendation
                {
                    Model = model,
                    LiveRating = ModelRating.NotRecommended,
                    LiveRatingText = "Not Recommended for Live",
                    LiveRatingColor = Color.FromArgb(220, 53, 69),
                    LiveGuidance = "Maximum accuracy but highly intensive. Not suitable for live CPU transcription.",
                    OfflineRating = ramGb >= 16 ? ModelRating.Usable : ModelRating.NotRecommended,
                    OfflineRatingText = ramGb >= 16 ? "Usable (Offline)" : "Not Recommended",
                    OfflineRatingColor = ramGb >= 16 ? Color.FromArgb(0, 120, 215) : Color.FromArgb(220, 53, 69),
                    OfflineGuidance = "Highest accuracy for offline files where execution time is not constrained.",
                    EstimatedUseSummary = "Live: Severely behind (RTF >> 1.0) | Offline: High Accuracy"
                };

            default:
                return new ModelRecommendation
                {
                    Model = model,
                    LiveRating = ModelRating.Usable,
                    LiveRatingText = "Usable",
                    LiveRatingColor = Color.FromArgb(0, 120, 215),
                    LiveGuidance = "Standard model support.",
                    OfflineRating = ModelRating.Usable,
                    OfflineRatingText = "Usable",
                    OfflineRatingColor = Color.FromArgb(0, 120, 215),
                    OfflineGuidance = "Standard model support.",
                    EstimatedUseSummary = "Live: Normal | Offline: Normal"
                };
        }
    }

    public static string GetPrimaryLiveRecommendedModelName(SystemHardwareInfo? hardwareInfo = null)
    {
        var hw = hardwareInfo ?? GetSystemHardwareInfo();
        double ramGb = hw.TotalPhysicalMemoryBytes / (1024.0 * 1024.0 * 1024.0);
        int cores = hw.LogicalCoreCount;

        if (cores >= 6 && ramGb >= 16)
        {
            return "Large v3 Turbo Q5";
        }
        else if (cores >= 4 && ramGb >= 8)
        {
            return "Small";
        }
        else
        {
            return "Base";
        }
    }

    public static string GetPrimaryOfflineRecommendedModelName(SystemHardwareInfo? hardwareInfo = null)
    {
        var hw = hardwareInfo ?? GetSystemHardwareInfo();
        double ramGb = hw.TotalPhysicalMemoryBytes / (1024.0 * 1024.0 * 1024.0);

        if (ramGb >= 16)
        {
            return "Large v3 Turbo Q5";
        }
        else if (ramGb >= 8)
        {
            return "Small";
        }
        else
        {
            return "Base";
        }
    }

    public static string GetPrimaryRecommendedModelName(SystemHardwareInfo? hardwareInfo = null)
    {
        return GetPrimaryLiveRecommendedModelName(hardwareInfo);
    }

    public static bool IsModelSuitableForLive(string modelName, SystemHardwareInfo? hardwareInfo = null)
    {
        var hw = hardwareInfo ?? GetSystemHardwareInfo();
        string id = modelName.ToLowerInvariant();

        if (id.Contains("tiny") || id.Contains("base") || id.Contains("small"))
            return true;

        if (hw.LogicalCoreCount >= 6 && (hw.TotalPhysicalMemoryBytes / (1024.0 * 1024.0 * 1024.0)) >= 16)
            return true;

        return false;
    }
}
