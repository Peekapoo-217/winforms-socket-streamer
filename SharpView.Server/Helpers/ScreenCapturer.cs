using System.Drawing.Imaging;

namespace SharpView.Server.Helpers;

/// <summary>
/// Captures the primary screen and compresses it to JPEG with configurable quality.
/// All <see cref="Bitmap"/> objects are disposed internally to prevent GDI+ memory leaks.
/// </summary>
public sealed class ScreenCapturer
{
    private int _jpegQuality;
    private readonly ImageCodecInfo _jpegCodec;
    private readonly EncoderParameters _encoderParams;

    /// <summary>
    /// Initializes a new <see cref="ScreenCapturer"/>.
    /// </summary>
    /// <param name="jpegQuality">
    /// JPEG compression quality from 1 (smallest / worst) to 100 (largest / best).
    /// Recommended range for Remote Desktop: 30–70.
    /// </param>
    public ScreenCapturer(int jpegQuality = 50)
    {
        if (jpegQuality is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(jpegQuality), "JPEG quality must be between 1 and 100.");

        _jpegQuality = jpegQuality;

        // Resolve the JPEG codec once and cache it.
        _jpegCodec = GetJpegCodec();

        // Build encoder parameters once and reuse.
        _encoderParams = new EncoderParameters(1);
        _encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)_jpegQuality);
    }

    /// <summary>
    /// Dynamically updates the JPEG compression quality at runtime.
    /// Disposes the previous <see cref="EncoderParameter"/> to prevent GDI+ memory leaks.
    /// </summary>
    /// <param name="newJpegQuality">New quality level (clamped to 10–100).</param>
    public void UpdateQuality(int newJpegQuality)
    {
        _jpegQuality = Math.Clamp(newJpegQuality, 10, 100);
        _encoderParams.Param[0].Dispose();
        _encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)_jpegQuality);
    }

    /// <summary>
    /// Captures the entire primary screen and returns it as a JPEG-compressed byte array.
    /// </summary>
    /// <returns>JPEG image bytes ready to be sent over the network.</returns>
    public byte[] CaptureScreen()
    {
        var bounds = Screen.PrimaryScreen!.Bounds;

        using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);

        return CompressToJpeg(bitmap);
    }

    /// <summary>
    /// Captures a specific region of the screen.
    /// </summary>
    /// <param name="region">The screen rectangle to capture.</param>
    /// <returns>JPEG image bytes of the captured region.</returns>
    public byte[] CaptureRegion(Rectangle region)
    {
        using var bitmap = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.CopyFromScreen(region.Location, Point.Empty, region.Size, CopyPixelOperation.SourceCopy);

        return CompressToJpeg(bitmap);
    }

    /// <summary>
    /// Compresses a <see cref="Bitmap"/> to JPEG bytes using the pre-configured quality level.
    /// </summary>
    private byte[] CompressToJpeg(Bitmap bitmap)
    {
        using var ms = new MemoryStream();
        bitmap.Save(ms, _jpegCodec, _encoderParams);
        return ms.ToArray();
    }

    /// <summary>
    /// Finds the built-in JPEG <see cref="ImageCodecInfo"/> from GDI+.
    /// </summary>
    private static ImageCodecInfo GetJpegCodec()
    {
        var codecs = ImageCodecInfo.GetImageEncoders();

        foreach (var codec in codecs)
        {
            if (codec.MimeType == "image/jpeg")
                return codec;
        }

        throw new InvalidOperationException("JPEG codec not found in GDI+ image encoders.");
    }
}
