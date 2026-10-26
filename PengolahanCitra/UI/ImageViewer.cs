using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Menampilkan gambar dengan zoom dan scroll tanpa membuat bitmap baru seukuran hasil zoom.
    /// Zoom > 100% digambar langsung (nearest neighbor, hanya area yang terlihat).
    /// Zoom &lt; 100% memakai cache hasil perkecil berkualitas tinggi yang dibuat setelah zoom berhenti berubah.
    /// </summary>
    public class ImageViewer : ScrollableControl
    {
        public const int MinZoom = 5;
        public const int MaxZoom = 400;

        private Image image;
        private int zoomPercent = 100;
        private Bitmap scaledCache;
        private readonly Timer cacheTimer = new Timer { Interval = 150 };

        public ImageViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            AutoScroll = true;
            BackColor = Theme.Canvas;
            Cursor = Cursors.Cross;
            cacheTimer.Tick += (s, e) => BuildScaledCache();
        }

        /// <summary>Terjadi saat user mengklik piksel gambar (koordinat gambar asli)</summary>
        public event EventHandler<Point> PixelClicked;

        /// <summary>Terjadi saat zoom berubah (termasuk lewat Ctrl + scroll)</summary>
        public event EventHandler ZoomChanged;

        public string EmptyText { get; set; } = "Buka gambar untuk mulai";

        /// <summary>
        /// Gambar yang ditampilkan. Viewer tidak memiliki gambar ini dan tidak men-dispose-nya.
        /// </summary>
        public Image Image
        {
            get => image;
            set
            {
                bool sizeChanged = image == null || value == null || image.Size != value.Size;
                image = value;
                DisposeCache();
                UpdateScrollSize(sizeChanged);
                RequestCache();
                Invalidate();
            }
        }

        public int ZoomPercent
        {
            get => zoomPercent;
            set
            {
                int zoom = ColorMath.Clamp(value, MinZoom, MaxZoom);
                if (zoom == zoomPercent) return;

                // Pertahankan titik tengah layar saat zoom berubah
                PointF center = ViewCenterInImage();
                zoomPercent = zoom;
                DisposeCache();
                UpdateScrollSize(false);
                CenterOn(center);
                RequestCache();
                Invalidate();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Zoom terbesar (maksimal 100%) yang membuat seluruh gambar terlihat
        /// </summary>
        public int FitZoomPercent()
        {
            if (image == null) return 100;
            double zoomX = (ClientSize.Width - 8) * 100.0 / image.Width;
            double zoomY = (ClientSize.Height - 8) * 100.0 / image.Height;
            return ColorMath.Clamp((int)Math.Floor(Math.Min(zoomX, zoomY)), MinZoom, 100);
        }

        #region Layout

        private Size ScaledSize => image == null
            ? Size.Empty
            : new Size(Math.Max(1, (int)((long)image.Width * zoomPercent / 100)),
                       Math.Max(1, (int)((long)image.Height * zoomPercent / 100)));

        /// <summary>
        /// Posisi gambar di layar: di tengah bila lebih kecil dari area, ikut scroll bila lebih besar
        /// </summary>
        private Rectangle ImageBounds
        {
            get
            {
                Size size = ScaledSize;
                int x = size.Width < ClientSize.Width ? (ClientSize.Width - size.Width) / 2 : AutoScrollPosition.X;
                int y = size.Height < ClientSize.Height ? (ClientSize.Height - size.Height) / 2 : AutoScrollPosition.Y;
                return new Rectangle(new Point(x, y), size);
            }
        }

        private void UpdateScrollSize(bool resetPosition)
        {
            AutoScrollMinSize = ScaledSize;
            if (resetPosition) AutoScrollPosition = Point.Empty;
        }

        private PointF ViewCenterInImage()
        {
            if (image == null) return PointF.Empty;
            Rectangle bounds = ImageBounds;
            return new PointF((ClientSize.Width / 2f - bounds.X) * 100f / zoomPercent,
                              (ClientSize.Height / 2f - bounds.Y) * 100f / zoomPercent);
        }

        private void CenterOn(PointF imagePoint)
        {
            if (image == null) return;
            int x = (int)(imagePoint.X * zoomPercent / 100f - ClientSize.Width / 2f);
            int y = (int)(imagePoint.Y * zoomPercent / 100f - ClientSize.Height / 2f);
            AutoScrollPosition = new Point(Math.Max(0, x), Math.Max(0, y));
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if (image == null)
            {
                TextRenderer.DrawText(g, EmptyText, Theme.NormalFont, ClientRectangle, Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            Rectangle bounds = ImageBounds;

            if (scaledCache != null)
            {
                g.DrawImageUnscaled(scaledCache, bounds.Location);
                return;
            }

            // Gambar hanya bagian yang terlihat
            Rectangle visible = Rectangle.Intersect(bounds, e.ClipRectangle);
            if (visible.IsEmpty) return;

            float scale = zoomPercent / 100f;
            var source = new RectangleF((visible.X - bounds.X) / scale, (visible.Y - bounds.Y) / scale,
                                        visible.Width / scale, visible.Height / scale);

            g.InterpolationMode = zoomPercent >= 100 ? InterpolationMode.NearestNeighbor : InterpolationMode.Low;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(image, visible, source, GraphicsUnit.Pixel);
        }

        private void RequestCache()
        {
            cacheTimer.Stop();
            if (image != null && zoomPercent < 100) cacheTimer.Start();
        }

        private void BuildScaledCache()
        {
            cacheTimer.Stop();
            if (image == null || zoomPercent >= 100) return;

            Size size = ScaledSize;
            DisposeCache();
            scaledCache = Geometry.ResizeBitmap(image, size.Width, size.Height);
            Invalidate();
        }

        private void DisposeCache()
        {
            scaledCache?.Dispose();
            scaledCache = null;
        }

        #endregion

        #region Mouse

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (image == null || e.Button != MouseButtons.Left) return;

            Rectangle bounds = ImageBounds;
            if (!bounds.Contains(e.Location)) return;

            int x = (int)((long)(e.X - bounds.X) * image.Width / bounds.Width);
            int y = (int)((long)(e.Y - bounds.Y) * image.Height / bounds.Height);
            PixelClicked?.Invoke(this, new Point(Math.Min(x, image.Width - 1), Math.Min(y, image.Height - 1)));
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && image != null)
            {
                int step = zoomPercent >= 100 ? 25 : 10;
                ZoomPercent = zoomPercent + (e.Delta > 0 ? step : -step);
                return;
            }

            base.OnMouseWheel(e);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheTimer.Dispose();
                DisposeCache();
            }
            base.Dispose(disposing);
        }
    }
}
