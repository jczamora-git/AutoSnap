namespace AutoSnap.Models;

public class CaptureSettings
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
    public string CustomIntervalText { get; set; } = "5";
    public string CustomIntervalUnit { get; set; } = "Seconds";
    public ImageFormatType Format { get; set; } = ImageFormatType.Jpg;
    public int JpegQuality { get; set; } = 90;
    public ImageScaleType Scale { get; set; } = ImageScaleType.Original;
    public string OutputFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "AutoSnap");

    public CaptureSettings Clone()
    {
        return new CaptureSettings
        {
            Interval = this.Interval,
            CustomIntervalText = this.CustomIntervalText,
            CustomIntervalUnit = this.CustomIntervalUnit,
            Format = this.Format,
            JpegQuality = this.JpegQuality,
            Scale = this.Scale,
            OutputFolder = this.OutputFolder
        };
    }
}
