using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using PengolahanCitra.Imaging;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Kanvas gambar dengan zoom, scroll, dan mode Before/After (split yang bisa digeser).
    /// Gambar selalu digambar apa adanya, tanpa efek. Viewer tidak memiliki bitmap yang ditampilkan.
    /// Zoom > 100% digambar langsung (nearest neighbor, hanya area terlihat);
    /// zoom &lt; 100% memakai cache hasil perkecil berkualitas tinggi.
    /// </summary>
    public class ImageViewer : ScrollableControl
    {
        public const int MinZoom = 5;
        public const int MaxZoom = 400;

        private const int DividerGrab = 14;
        private const int KnobSize = 30;

        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff" };

        private readonly ScaledImage main = new ScaledImage();
        private readonly ScaledImage compare = new ScaledImage();
        private readonly Timer cacheTimer = new Timer { Interval = 150 };

        private int zoomPercent = 100;
        private bool compareMode;
        private float splitRatio = 0.5f;
        private bool draggingDivider;
        private bool suppressClick;

        public ImageViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            AutoScroll = true;
            AllowDrop = true;
            TabStop = true;
            BackColor = NeuTheme.Surface;
            Cursor = Cursors.Cross;
            AccessibleRole = AccessibleRole.Graphic;
            AccessibleName = "Kanvas gambar";
            cacheTimer.Tick += (s, e) => BuildScaledCaches();
        }

        /// <summary>User mengklik piksel gambar (koordinat gambar asli)</summary>
        public event EventHandler<Point> PixelClicked;

        /// <summary>Zoom berubah (termasuk lewat Ctrl + scroll)</summary>
        public event EventHandler ZoomChanged;

        /// <summary>File gambar di-drag ke kanvas</summary>
        public event EventHandler<string> FileDropped;

        public string EmptyText { get; set; } = "Buka gambar untuk mulai, atau seret file gambar ke sini";

        /// <summary>Gambar utama (After). Tidak di-dispose oleh viewer.</summary>
        public Image Image
        {
            get => main.Source;
            set
            {
                bool sizeChanged = main.Source == null || value == null || main.Source.Size != value.Size;
                main.SetSource(value);
                UpdateScrollSize(sizeChanged);
                RequestCache();
                Invalidate();
            }
        }

        /// <summary>Gambar pembanding (Before) untuk mode split. Tidak di-dispose oleh viewer.</summary>
        public Image CompareImage
        {
            get => compare.Source;
            set
            {
                compare.SetSource(value);
                RequestCache();
                Invalidate();
            }
        }

        public bool CompareMode
        {
            get => compareMode;
            set
            {
                compareMode = value;
                Invalidate();
            }
        }

        private bool IsComparing => compareMode && main.Source != null && compare.Source != null;

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
                main.ClearCache();
                compare.ClearCache();
                UpdateScrollSize(false);
                CenterOn(center);
                RequestCache();
                Invalidate();
                ZoomChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Zoom terbesar (maksimal 100%) yang membuat seluruh gambar terlihat</summary>
        public int FitZoomPercent()
        {
            Image image = main.Source;
            if (image == null) return 100;
            double zoomX = (ClientSize.Width - 8) * 100.0 / image.Width;
            double zoomY = (ClientSize.Height - 8) * 100.0 / image.Height;
            return ColorMath.Clamp((int)Math.Floor(Math.Min(zoomX, zoomY)), MinZoom, 100);
        }

        #region Layout

        private Size ScaledSize(Image image) => image == null
            ? Size.Empty
            : new Size(Math.Max(1, (int)((long)image.Width * zoomPercent / 100)),
                       Math.Max(1, (int)((long)image.Height * zoomPercent / 100)));

        /// <summary>Posisi gambar utama: di tengah bila lebih kecil dari area, ikut scroll bila lebih besar</summary>
        private Rectangle MainBounds
        {
            get
            {
                Size size = ScaledSize(main.Source);
                int x = size.Width < ClientSize.Width ? (ClientSize.Width - size.Width) / 2 : AutoScrollPosition.X;
                int y = size.Height < ClientSize.Height ? (ClientSize.Height - size.Height) / 2 : AutoScrollPosition.Y;
                return new Rectangle(new Point(x, y), size);
            }
        }

        /// <summary>Gambar pembanding memakai zoom yang sama dan titik tengah yang sama dengan gambar utama</summary>
        private Rectangle CompareBounds
        {
            get
            {
                Rectangle m = MainBounds;
                Size size = ScaledSize(compare.Source);
                return new Rectangle(m.X + (m.Width - size.Width) / 2, m.Y + (m.Height - size.Height) / 2, size.Width, size.Height);
            }
        }

        private int SplitX => (int)(ClientSize.Width * splitRatio);

        private void UpdateScrollSize(bool resetPosition)
        {
            AutoScrollMinSize = ScaledSize(main.Source);
            if (resetPosition) AutoScrollPosition = Point.Empty;
        }

        private PointF ViewCenterInImage()
        {
            if (main.Source == null) return PointF.Empty;
            Rectangle bounds = MainBounds;
            return new PointF((ClientSize.Width / 2f - bounds.X) * 100f / zoomPercent,
                              (ClientSize.Height / 2f - bounds.Y) * 100f / zoomPercent);
        }

        private void CenterOn(PointF imagePoint)
        {
            if (main.Source == null) return;
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

            if (main.Source == null)
            {
                TextRenderer.DrawText(g, EmptyText, NeuTheme.LabelFont, ClientRectangle, NeuTheme.InkMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            if (!IsComparing)
            {
                DrawScaled(g, main, MainBounds, e.ClipRectangle);
                return;
            }

            int split = SplitX;
            DrawScaled(g, compare, CompareBounds, Rectangle.Intersect(e.ClipRectangle, new Rectangle(0, 0, split, ClientSize.Height)));
            DrawScaled(g, main, MainBounds, Rectangle.Intersect(e.ClipRectangle, new Rectangle(split, 0, ClientSize.Width - split, ClientSize.Height)));
            DrawDivider(g, split);
        }

        /// <summary>Gambar satu image di bounds, hanya bagian yang ada di clip</summary>
        private void DrawScaled(Graphics g, ScaledImage item, Rectangle bounds, Rectangle clip)
        {
            Rectangle visible = Rectangle.Intersect(bounds, clip);
            if (visible.IsEmpty) return;

            GraphicsState state = g.Save();
            g.SetClip(visible);

            if (item.Cache != null)
            {
                g.DrawImageUnscaled(item.Cache, bounds.Location);
            }
            else
            {
                float scale = zoomPercent / 100f;
                var source = new RectangleF((visible.X - bounds.X) / scale, (visible.Y - bounds.Y) / scale,
                                            visible.Width / scale, visible.Height / scale);
                g.InterpolationMode = zoomPercent >= 100 ? InterpolationMode.NearestNeighbor : InterpolationMode.Low;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(item.Source, visible, source, GraphicsUnit.Pixel);
            }

            g.Restore(state);
        }

        /// <summary>Garis pembagi, knob timbul, dan label Before/After</summary>
        private void DrawDivider(Graphics g, int split)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(NeuTheme.Surface, 3f))
            {
                g.DrawLine(pen, split, 0, split, ClientSize.Height);
            }

            var knob = new Rectangle(split - KnobSize / 2, ClientSize.Height / 2 - KnobSize / 2, KnobSize, KnobSize);
            NeuPainter.DrawSurface(g, knob, KnobSize / 2, NeuStyle.Raised, NeuTheme.DepthSmall);
            TextRenderer.DrawText(g, NeuTheme.Icons.FlipH, NeuTheme.IconFontSmall, knob, NeuTheme.AccentInk,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            if (Focused && ShowFocusCues) NeuPainter.DrawFocusRing(g, knob, KnobSize / 2);

            DrawPill(g, "Before", new Point(split - 10, 12), alignRight: true);
            DrawPill(g, "After", new Point(split + 10, 12), alignRight: false);
        }

        private static void DrawPill(Graphics g, string text, Point anchor, bool alignRight)
        {
            Size size = TextRenderer.MeasureText(text, NeuTheme.SmallFont);
            var rect = new Rectangle(alignRight ? anchor.X - size.Width - 16 : anchor.X, anchor.Y, size.Width + 16, size.Height + 6);
            NeuPainter.FillFace(g, rect, rect.Height / 2);
            TextRenderer.DrawText(g, text, NeuTheme.SmallFont, rect, NeuTheme.Ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void RequestCache()
        {
            cacheTimer.Stop();
            if (main.Source != null && zoomPercent < 100) cacheTimer.Start();
        }

        private void BuildScaledCaches()
        {
            cacheTimer.Stop();
            if (zoomPercent >= 100) return;

            main.BuildCache(ScaledSize(main.Source));
            compare.BuildCache(ScaledSize(compare.Source));
            Invalidate();
        }

        #endregion

        #region Mouse & Keyboard

        private bool NearDivider(Point p) => IsComparing && Math.Abs(p.X - SplitX) <= DividerGrab;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            suppressClick = false;
            if (e.Button == MouseButtons.Left && NearDivider(e.Location))
            {
                draggingDivider = true;
                suppressClick = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (draggingDivider)
            {
                splitRatio = Math.Max(0.02f, Math.Min(0.98f, e.X / (float)Math.Max(1, ClientSize.Width)));
                Invalidate();
                return;
            }

            Cursor = NearDivider(e.Location) ? Cursors.SizeWE : Cursors.Cross;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            draggingDivider = false;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (suppressClick || main.Source == null || e.Button != MouseButtons.Left) return;

            Rectangle bounds = MainBounds;
            if (!bounds.Contains(e.Location)) return;

            int x = (int)((long)(e.X - bounds.X) * main.Source.Width / bounds.Width);
            int y = (int)((long)(e.Y - bounds.Y) * main.Source.Height / bounds.Height);
            PixelClicked?.Invoke(this, new Point(Math.Min(x, main.Source.Width - 1), Math.Min(y, main.Source.Height - 1)));
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control && main.Source != null)
            {
                int step = zoomPercent >= 100 ? 25 : 10;
                ZoomPercent = zoomPercent + (e.Delta > 0 ? step : -step);
                return;
            }

            base.OnMouseWheel(e);
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            return (IsComparing && (key == Keys.Left || key == Keys.Right)) || base.IsInputKey(keyData);
        }

        /// <summary>Panah kiri/kanan menggeser garis Before/After</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!IsComparing) return;
            if (e.KeyCode == Keys.Left) splitRatio = Math.Max(0.02f, splitRatio - 0.05f);
            else if (e.KeyCode == Keys.Right) splitRatio = Math.Min(0.98f, splitRatio + 0.05f);
            else return;
            e.Handled = true;
            Invalidate();
        }

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effect = DroppedFile(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            string file = DroppedFile(e);
            if (file != null) FileDropped?.Invoke(this, file);
        }

        private static string DroppedFile(DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files) || files.Length != 1) return null;
            string ext = Path.GetExtension(files[0]).ToLowerInvariant();
            return Array.IndexOf(ImageExtensions, ext) >= 0 ? files[0] : null;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheTimer.Dispose();
                main.ClearCache();
                compare.ClearCache();
            }
            base.Dispose(disposing);
        }

        /// <summary>Gambar sumber + cache hasil perkecil</summary>
        private sealed class ScaledImage
        {
            public Image Source { get; private set; }
            public Bitmap Cache { get; private set; }

            public void SetSource(Image image)
            {
                Source = image;
                ClearCache();
            }

            public void BuildCache(Size size)
            {
                ClearCache();
                if (Source != null) Cache = Geometry.ResizeBitmap(Source, size.Width, size.Height);
            }

            public void ClearCache()
            {
                Cache?.Dispose();
                Cache = null;
            }
        }
    }
}
