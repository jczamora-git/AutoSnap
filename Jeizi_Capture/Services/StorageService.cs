using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using AutoSnap.Capture;
using AutoSnap.Models;

namespace AutoSnap.Services;

public class StorageService
{
    private static readonly Regex InvalidCharsRegex = new(
        $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
        RegexOptions.Compiled);

    public string SanitizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Snapshot";

        string clean = InvalidCharsRegex.Replace(name, "_").Trim();
        if (clean.Length > 60)
            clean = clean.Substring(0, 60);

        return string.IsNullOrWhiteSpace(clean) ? "Snapshot" : clean;
    }

    public string GenerateFilePath(string baseFolder, string sourceName, ImageFormatType format, DateTime timestamp)
    {
        string dateFolder = timestamp.ToString("yyyy-MM-dd");
        string cleanSourceName = SanitizeName(sourceName);
        string extension = format == ImageFormatType.Png ? ".png" : ".jpg";
        string fileName = $"{cleanSourceName}_{timestamp:yyyy-MM-dd_HH-mm-ss-fff}{extension}";

        string targetDirectory = Path.Combine(baseFolder, dateFolder, cleanSourceName);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        return Path.Combine(targetDirectory, fileName);
    }

    public async Task<SnapshotRecord> SaveSnapshotAsync(
        CapturedFrame frame,
        CaptureSettings settings,
        CancellationToken cancellationToken = default)
    {
        string filePath = GenerateFilePath(
            settings.OutputFolder,
            frame.SourceName,
            settings.Format,
            frame.Timestamp);

        Size finalResolution = frame.Resolution;

        await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using Image imageToSave = PrepareImageForSaving(frame.Image, settings.Scale, out finalResolution);
            cancellationToken.ThrowIfCancellationRequested();

            SaveImageToFile(imageToSave, filePath, settings.Format, settings.JpegQuality);
        }, cancellationToken);

        var fileInfo = new FileInfo(filePath);
        return new SnapshotRecord
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            Timestamp = frame.Timestamp,
            SourceName = frame.SourceName,
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
            Resolution = finalResolution
        };
    }

    private Image PrepareImageForSaving(Image original, ImageScaleType scale, out Size finalSize)
    {
        if (scale == ImageScaleType.Original || (int)scale >= 100)
        {
            finalSize = original.Size;
            return (Image)original.Clone();
        }

        double factor = (int)scale / 100.0;
        int targetWidth = Math.Max(1, (int)(original.Width * factor));
        int targetHeight = Math.Max(1, (int)(original.Height * factor));
        finalSize = new Size(targetWidth, targetHeight);

        var resized = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(original, new Rectangle(0, 0, targetWidth, targetHeight));
        }

        return resized;
    }

    private void SaveImageToFile(Image image, string filePath, ImageFormatType format, int jpegQuality)
    {
        if (format == ImageFormatType.Png)
        {
            image.Save(filePath, ImageFormat.Png);
        }
        else
        {
            ImageCodecInfo? jpegCodec = GetEncoder(ImageFormat.Jpeg);
            if (jpegCodec == null)
            {
                image.Save(filePath, ImageFormat.Jpeg);
                return;
            }

            using var encoderParams = new EncoderParameters(1);
            long clampedQuality = Math.Clamp(jpegQuality, 1, 100);
            using var qualityParam = new EncoderParameter(Encoder.Quality, clampedQuality);
            encoderParams.Param[0] = qualityParam;

            image.Save(filePath, jpegCodec, encoderParams);
        }
    }

    private static ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
    }
}
