using System.Drawing;
using System.IO;

namespace SharpView.Client.Helpers;

/// <summary>
/// Converts received JPEG byte arrays back into <see cref="Image"/> objects
/// for display in a WinForms <see cref="System.Windows.Forms.PictureBox"/>.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Decodes a JPEG byte array into an <see cref="Image"/>.
    ///
    /// <para><b>IMPORTANT:</b> The caller is responsible for disposing the returned
    /// <see cref="Image"/> when it is no longer needed (e.g. before assigning a new
    /// frame to a PictureBox).</para>
    /// </summary>
    /// <param name="jpegBytes">The JPEG-compressed image bytes.</param>
    /// <returns>A decoded <see cref="Image"/> ready for display.</returns>
    /// <exception cref="ArgumentException">Thrown when the byte array is null or empty.</exception>
    public static Image BytesToImage(byte[] jpegBytes)
    {
        if (jpegBytes is null || jpegBytes.Length == 0)
            throw new ArgumentException("Image byte array cannot be null or empty.", nameof(jpegBytes));

        // We must keep the MemoryStream alive for the lifetime of the Image
        // (GDI+ requirement). Copy into a new Bitmap to decouple from the stream.
        using var ms = new MemoryStream(jpegBytes);
        using var original = Image.FromStream(ms);

        // Return an independent copy so the MemoryStream can be safely disposed.
        return new Bitmap(original);
    }

    /// <summary>
    /// Safely replaces the image in a <see cref="System.Windows.Forms.PictureBox"/>,
    /// disposing the previous frame to prevent GDI+ handle leaks.
    /// </summary>
    /// <param name="pictureBox">The target PictureBox control.</param>
    /// <param name="jpegBytes">The new JPEG frame bytes.</param>
    public static void UpdatePictureBox(System.Windows.Forms.PictureBox pictureBox, byte[] jpegBytes)
    {
        var newImage = BytesToImage(jpegBytes);
        var oldImage = pictureBox.Image;

        pictureBox.Image = newImage;

        // Dispose the previous frame AFTER assignment to avoid flicker.
        oldImage?.Dispose();
    }
}
