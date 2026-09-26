using System.Drawing;
using AutoSnap.Transcription;

namespace AutoSnap.Hardware;

public enum ModelRating
{
    Excellent,
    Recommended,
    Usable,
    MayBeSlow,
    NotRecommended
}

public class ModelRecommendation
{
    public WhisperModelInfo Model { get; init; } = null!;
    
    // Live Transcription Suitability
    public ModelRating LiveRating { get; init; }
    public string LiveRatingText { get; init; } = "Recommended";
    public Color LiveRatingColor { get; init; } = Color.FromArgb(40, 167, 69);
    public string LiveGuidance { get; init; } = string.Empty;

    // Offline / Local Media Processing Suitability
    public ModelRating OfflineRating { get; init; }
    public string OfflineRatingText { get; init; } = "Recommended";
    public Color OfflineRatingColor { get; init; } = Color.FromArgb(40, 167, 69);
    public string OfflineGuidance { get; init; } = string.Empty;

    // General / Default (defaults to Live)
    public ModelRating Rating => LiveRating;
    public string RatingText => LiveRatingText;
    public Color RatingColor => LiveRatingColor;
    public string GuidanceText => LiveGuidance;
    public string EstimatedUseSummary { get; init; } = string.Empty;
}
