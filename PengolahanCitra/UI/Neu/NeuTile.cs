using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Kartu thumbnail yang bisa dipilih: timbul saat normal, cekung dengan cincin aksen saat terpilih.
    /// Tile memiliki bitmap thumbnail-nya dan men-dispose-nya saat diganti.
    /// </summary>
    public class NeuTile : NeuControl
    {
        private const int CaptionHeight = 20;
        private const int ImageRadius = 8;

        private Image thumbnail;
        private bool selected;

        public NeuTile()
        {
            Size = new Size(104, 104);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.RadioButton;
        }

        [DefaultValue(false)]
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value) return;
                selected = value;
                AccessibleDescription = value ? "Terpilih" : null;
                Invalidate();
            }
        }

        public void SetThumbnail(Image image)
        {
            Image old = thumbnail;
            thumbnail = image;
            old?.Dispose();
            Invalidate();
        }

        private Rectangle ShapeBounds => Deflate(ClientRectangle, NeuTheme.ShadowMargin(NeuTheme.DepthSmall));

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle shape = ShapeBounds;

            NeuPainter.DrawSurface(g, shape, NeuTheme.RadiusMedium, selected ? NeuStyle.Inset : NeuStyle.Raised, NeuTheme.DepthSmall);

            var imageArea = new Rectangle(shape.X + 7, shape.Y + 7, shape.Width - 14, shape.Height - 14 - CaptionHeight);
            if (thumbnail != null && imageArea.Width > 0 && imageArea.Height > 0)
            {
                Rectangle fitted = Fit(thumbnail.Size, imageArea);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (GraphicsPath clip = NeuPainter.RoundedRect(fitted, ImageRadius))
                {
                    GraphicsState state = g.Save();
                    g.SetClip(clip);
                    g.DrawImage(thumbnail, fitted);
                    g.Restore(state);
                }
            }

            var caption = new Rectangle(shape.X + 4, shape.Bottom - CaptionHeight - 4, shape.Width - 8, CaptionHeight);
            DrawText(g, Text, selected ? NeuTheme.ValueFont : NeuTheme.SmallFont, caption,
                TextColor(selected ? NeuTheme.AccentInk : NeuTheme.Ink));

            if (selected)
            {
                using (var pen = new Pen(NeuTheme.Accent, 2f))
                using (GraphicsPath ring = NeuPainter.RoundedRect(RectangleF.Inflate(shape, -1, -1), NeuTheme.RadiusMedium - 1))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawPath(pen, ring);
                }
            }

            if (ShowFocusRing) NeuPainter.DrawFocusRing(g, shape, NeuTheme.RadiusMedium);
        }

        private static Rectangle Fit(Size image, Rectangle area)
        {
            double scale = Math.Min((double)area.Width / image.Width, (double)area.Height / image.Height);
            int w = Math.Max(1, (int)(image.Width * scale));
            int h = Math.Max(1, (int)(image.Height * scale));
            return new Rectangle(area.X + (area.Width - w) / 2, area.Y + (area.Height - h) / 2, w, h);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode != Keys.Space && e.KeyCode != Keys.Enter) return;
            OnClick(EventArgs.Empty);
            e.Handled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) SetThumbnail(null);
            base.Dispose(disposing);
        }
    }
}
