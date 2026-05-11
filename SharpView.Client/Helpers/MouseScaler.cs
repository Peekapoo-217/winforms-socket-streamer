using System.Drawing;

namespace SharpView.Client.Helpers;

/// <summary>
/// Translates mouse coordinates from a <see cref="System.Windows.Forms.PictureBox"/>
/// (with <c>SizeMode = Zoom</c>) back to the original server screen coordinates.
///
/// <para><b>Why is this needed?</b>
/// When <c>SizeMode = Zoom</c>, the PictureBox scales the image to fit while
/// preserving aspect ratio. This creates black letterbox (top/bottom) or
/// pillarbox (left/right) bars. A raw click position includes these bars,
/// so we must subtract the offset and reverse the scaling.</para>
/// </summary>
public static class MouseScaler
{
    /// <summary>
    /// Converts a click position within a <c>Zoom</c>-mode PictureBox
    /// into the corresponding pixel coordinate on the original image
    /// (i.e., the server's screen).
    /// </summary>
    /// <param name="pb">The PictureBox displaying the streamed image.</param>
    /// <param name="clickLocation">
    /// The raw mouse position relative to the PictureBox's top-left corner
    /// (e.g., from <c>MouseEventArgs.Location</c>).
    /// </param>
    /// <param name="originalScreenSize">
    /// The server's actual screen resolution (e.g., 1920×1080).
    /// </param>
    /// <returns>
    /// The mapped coordinate on the original screen, clamped to valid bounds.
    /// Returns <c>(-1, -1)</c> if the click lands in the letterbox/pillarbox area.
    /// </returns>
    public static Point GetImageCoordinate(
        System.Windows.Forms.PictureBox pb,
        Point clickLocation,
        Size originalScreenSize)
    {
        if (pb.Image is null)
            return new Point(-1, -1);

        // ───────────────────────────────────────────────────────────
        // Step 1: Calculate the scale factor used by Zoom mode.
        //
        //   Zoom mode picks the SMALLER ratio so the entire image
        //   fits inside the PictureBox without cropping:
        //
        //       scaleX = pbWidth  / imgWidth
        //       scaleY = pbHeight / imgHeight
        //       scale  = min(scaleX, scaleY)
        // ───────────────────────────────────────────────────────────

        float scaleX = (float)pb.Width / originalScreenSize.Width;
        float scaleY = (float)pb.Height / originalScreenSize.Height;
        float scale = Math.Min(scaleX, scaleY);

        // ───────────────────────────────────────────────────────────
        // Step 2: Calculate the displayed (scaled) image dimensions.
        //
        //       displayedWidth  = imgWidth  × scale
        //       displayedHeight = imgHeight × scale
        // ───────────────────────────────────────────────────────────

        float displayedWidth = originalScreenSize.Width * scale;
        float displayedHeight = originalScreenSize.Height * scale;

        // ───────────────────────────────────────────────────────────
        // Step 3: Calculate the offset (letterbox/pillarbox padding).
        //
        //   The image is centered inside the PictureBox, so:
        //       offsetX = (pbWidth  - displayedWidth)  / 2
        //       offsetY = (pbHeight - displayedHeight) / 2
        //
        //   If scaleX < scaleY → pillarbox (bars on top/bottom) → offsetY > 0
        //   If scaleY < scaleX → letterbox (bars on left/right)  → offsetX > 0
        // ───────────────────────────────────────────────────────────

        float offsetX = (pb.Width - displayedWidth) / 2f;
        float offsetY = (pb.Height - displayedHeight) / 2f;

        // ───────────────────────────────────────────────────────────
        // Step 4: Translate click position to image-relative coords.
        //
        //       relativeX = clickX - offsetX
        //       relativeY = clickY - offsetY
        //
        //   If relative coords are negative or exceed displayedWidth/Height,
        //   the click was in the letterbox/pillarbox → invalid.
        // ───────────────────────────────────────────────────────────

        float relativeX = clickLocation.X - offsetX;
        float relativeY = clickLocation.Y - offsetY;

        // Check: click is outside the actual image area (in the black bars).
        if (relativeX < 0 || relativeY < 0 ||
            relativeX > displayedWidth || relativeY > displayedHeight)
        {
            return new Point(-1, -1);
        }

        // ───────────────────────────────────────────────────────────
        // Step 5: Reverse the scale to get original image coordinates.
        //
        //       originalX = relativeX / scale
        //       originalY = relativeY / scale
        //
        //   Clamp to [0, width-1] and [0, height-1] for safety.
        // ───────────────────────────────────────────────────────────

        int originalX = (int)Math.Clamp(relativeX / scale, 0, originalScreenSize.Width - 1);
        int originalY = (int)Math.Clamp(relativeY / scale, 0, originalScreenSize.Height - 1);

        return new Point(originalX, originalY);
    }
}
