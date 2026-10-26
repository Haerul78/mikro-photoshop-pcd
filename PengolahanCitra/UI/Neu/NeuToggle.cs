using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Saklar on/off neumorphic: label di kiri, track cekung dan knob timbul di kanan
    /// </summary>
    public class NeuToggle : NeuControl
    {
        private const int TrackWidth = 42;
        private const int TrackHeight = 22;
        private const int KnobSize = 16;

        private bool isOn;

        public NeuToggle()
        {
            Size = new Size(200, 36);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.CheckButton;
        }

        public event EventHandler CheckedChanged;

        [DefaultValue(false)]
        public bool Checked
        {
            get => isOn;
            set
            {
                if (isOn == value) return;
                isOn = value;
                AccessibleDescription = value ? "Aktif" : "Nonaktif";
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private Rectangle TrackBounds =>
            new Rectangle(Width - TrackWidth - NeuTheme.ShadowMargin(NeuTheme.DepthSmall) + 4,
                          (Height - TrackHeight) / 2, TrackWidth, TrackHeight);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle track = TrackBounds;

            DrawText(g, Text, Font, new Rectangle(0, 0, track.X - 8, Height), TextColor(NeuTheme.Ink), TextFormatFlags.Left);

            NeuPainter.DrawSurface(g, track, TrackHeight / 2, NeuStyle.Inset, NeuTheme.DepthSmall);

            if (isOn)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                RectangleF fill = track;
                fill.Inflate(-3, -3);
                using (var brush = new SolidBrush(Enabled ? NeuTheme.Accent : NeuTheme.InkDisabled))
                using (GraphicsPath path = NeuPainter.RoundedRect(fill, fill.Height / 2))
                {
                    g.FillPath(brush, path);
                }
            }

            int knobX = isOn ? track.Right - KnobSize - 3 : track.X + 3;
            var knob = new Rectangle(knobX, track.Y + (TrackHeight - KnobSize) / 2, KnobSize, KnobSize);
            NeuPainter.DrawSurface(g, knob, KnobSize / 2, NeuStyle.Raised, NeuTheme.DepthSmall);

            if (ShowFocusRing) NeuPainter.DrawFocusRing(g, track, TrackHeight / 2);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Focus();
            Checked = !Checked;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode != Keys.Space) return;
            Checked = !Checked;
            e.Handled = true;
        }
    }
}
