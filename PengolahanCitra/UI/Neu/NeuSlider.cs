using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Slider neumorphic: label dan nilai di atas, track cekung, isi aksen dari titik asal, thumb timbul.
    /// Bisa digeser dengan mouse, roda mouse, atau keyboard (panah, PageUp/PageDown, Home/End).
    /// </summary>
    public class NeuSlider : NeuControl
    {
        private const int HeaderHeight = 20;
        private const int TrackHeight = 8;
        private const int ThumbSize = 18;

        private int minimum;
        private int maximum = 100;
        private int value;
        private bool dragging;

        public NeuSlider()
        {
            Size = new Size(200, 54);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.Slider;
        }

        public event EventHandler ValueChanged;

        /// <summary>Terjadi saat user selesai menggeser (mouse dilepas atau tombol keyboard)</summary>
        public event EventHandler ValueCommitted;

        [DefaultValue(0)]
        public int Minimum
        {
            get => minimum;
            set { minimum = value; if (maximum < value) maximum = value; Value = this.value; Invalidate(); }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => maximum;
            set { maximum = value; if (minimum > value) minimum = value; Value = this.value; Invalidate(); }
        }

        [DefaultValue(0)]
        public int Value
        {
            get => value;
            set
            {
                int clamped = Math.Max(minimum, Math.Min(maximum, value));
                if (clamped == this.value) return;
                this.value = clamped;
                AccessibleDescription = FormatValue(clamped);
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [DefaultValue(1)]
        public int SmallChange { get; set; } = 1;

        [DefaultValue(10)]
        public int LargeChange { get; set; } = 10;

        /// <summary>Titik awal isi aksen (mis. 0 untuk brightness -100..100). Default = Minimum.</summary>
        public int? Origin { get; set; }

        /// <summary>Format teks nilai di kanan atas</summary>
        [Browsable(false)]
        public Func<int, string> ValueFormatter { get; set; }

        private string FormatValue(int v) => ValueFormatter != null ? ValueFormatter(v) : v.ToString();

        #region Layout

        private bool HasHeader => !string.IsNullOrEmpty(Text);

        private Rectangle TrackBounds
        {
            get
            {
                int margin = NeuTheme.ShadowMargin(NeuTheme.DepthSmall) + ThumbSize / 2 - 4;
                int top = HasHeader ? HeaderHeight : 0;
                int centerY = top + (Height - top) / 2;
                return new Rectangle(margin, centerY - TrackHeight / 2, Math.Max(1, Width - margin * 2), TrackHeight);
            }
        }

        private int ValueToX(int v)
        {
            Rectangle t = TrackBounds;
            if (maximum == minimum) return t.X;
            return t.X + (int)Math.Round((double)(v - minimum) * t.Width / (maximum - minimum));
        }

        private int XToValue(int x)
        {
            Rectangle t = TrackBounds;
            double ratio = Math.Max(0, Math.Min(1, (double)(x - t.X) / t.Width));
            return minimum + (int)Math.Round(ratio * (maximum - minimum));
        }

        private Rectangle ThumbBounds
        {
            get
            {
                Rectangle t = TrackBounds;
                int cx = ValueToX(value);
                int cy = t.Y + t.Height / 2;
                return new Rectangle(cx - ThumbSize / 2, cy - ThumbSize / 2, ThumbSize, ThumbSize);
            }
        }

        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (HasHeader)
            {
                var header = new Rectangle(TrackBounds.X - 4, 0, Width - (TrackBounds.X - 4) * 2, HeaderHeight);
                DrawText(g, Text, Font, header, TextColor(NeuTheme.Ink), TextFormatFlags.Left);
                DrawText(g, FormatValue(value), NeuTheme.ValueFont, header, TextColor(NeuTheme.AccentInk), TextFormatFlags.Right);
            }

            Rectangle track = TrackBounds;
            NeuPainter.DrawSurface(g, track, TrackHeight / 2, NeuStyle.Inset, NeuTheme.DepthSmall);

            // Isi aksen dari titik asal ke nilai sekarang
            int originX = ValueToX(Math.Max(minimum, Math.Min(maximum, Origin ?? minimum)));
            int valueX = ValueToX(value);
            if (valueX != originX)
            {
                var fill = Rectangle.FromLTRB(Math.Min(originX, valueX), track.Y + 1, Math.Max(originX, valueX), track.Bottom - 1);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Enabled ? NeuTheme.Accent : NeuTheme.InkDisabled))
                using (GraphicsPath path = NeuPainter.RoundedRect(fill, (TrackHeight - 2) / 2f))
                {
                    g.FillPath(brush, path);
                }
            }

            Rectangle thumb = ThumbBounds;
            NeuPainter.DrawSurface(g, thumb, ThumbSize / 2, Enabled ? NeuStyle.Raised : NeuStyle.Flat, NeuTheme.DepthSmall);

            // Titik aksen kecil di tengah thumb sebagai penanda posisi
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var dot = new SolidBrush(Enabled ? NeuTheme.Accent : NeuTheme.InkDisabled))
            {
                g.FillEllipse(dot, thumb.X + ThumbSize / 2 - 3, thumb.Y + ThumbSize / 2 - 3, 6, 6);
            }

            if (ShowFocusRing) NeuPainter.DrawFocusRing(g, thumb, ThumbSize / 2);
        }

        #endregion

        #region Input

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            dragging = true;
            Value = XToValue(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragging) Value = XToValue(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!dragging) return;
            dragging = false;
            ValueCommitted?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!Focused) return;
            Value += e.Delta > 0 ? SmallChange : -SmallChange;
            ValueCommitted?.Invoke(this, EventArgs.Empty);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int old = value;

            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Down: Value -= SmallChange; break;
                case Keys.Right:
                case Keys.Up: Value += SmallChange; break;
                case Keys.PageDown: Value -= LargeChange; break;
                case Keys.PageUp: Value += LargeChange; break;
                case Keys.Home: Value = minimum; break;
                case Keys.End: Value = maximum; break;
                default: return;
            }

            e.Handled = true;
            if (value != old) ValueCommitted?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
