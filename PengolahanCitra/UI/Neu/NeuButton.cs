using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Tombol neumorphic: timbul saat normal, cekung saat ditekan atau aktif (Checked).
    /// Bisa berisi ikon (Segoe Fluent Icons), teks, atau keduanya; bisa bulat.
    /// </summary>
    public class NeuButton : NeuControl, IButtonControl
    {
        private string icon;
        private bool circular;
        private bool isChecked;
        private bool primary;
        private bool pressed;

        public NeuButton()
        {
            Size = new Size(120, 56);
            Cursor = Cursors.Hand;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
        }

        /// <summary>Glyph ikon dari NeuTheme.Icons (kosong = tanpa ikon)</summary>
        [DefaultValue(null)]
        public string Icon
        {
            get => icon;
            set { icon = value; Invalidate(); }
        }

        /// <summary>Bentuk lingkaran (untuk tombol ikon)</summary>
        [DefaultValue(false)]
        public bool Circular
        {
            get => circular;
            set { circular = value; Invalidate(); }
        }

        /// <summary>State aktif: tampil cekung dengan ikon/teks berwarna aksen</summary>
        [DefaultValue(false)]
        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                AccessibleDescription = value ? "Aktif" : null;
                Invalidate();
            }
        }

        /// <summary>Aksi utama: ikon dan teks berwarna aksen</summary>
        [DefaultValue(false)]
        public bool Primary
        {
            get => primary;
            set { primary = value; Invalidate(); }
        }

        [DefaultValue(NeuTheme.DepthButton)]
        public int Depth { get; set; } = NeuTheme.DepthButton;

        [DefaultValue(DialogResult.None)]
        public DialogResult DialogResult { get; set; }

        public Rectangle ShapeBounds
        {
            get
            {
                Rectangle r = Deflate(ClientRectangle, NeuTheme.ShadowMargin(Depth));
                if (!circular) return r;

                int d = Math.Min(r.Width, r.Height);
                return new Rectangle(r.X + (r.Width - d) / 2, r.Y + (r.Height - d) / 2, d, d);
            }
        }

        public void NotifyDefault(bool value) { }

        public void PerformClick()
        {
            if (Enabled && Visible) OnClick(EventArgs.Empty);
        }

        protected override void OnClick(EventArgs e)
        {
            Form form = FindForm();
            if (form != null && DialogResult != DialogResult.None) form.DialogResult = DialogResult;
            base.OnClick(e);
        }

        #region Paint

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle shape = ShapeBounds;
            int radius = circular ? shape.Width / 2 : NeuTheme.RadiusMedium;

            NeuStyle style = !Enabled ? NeuStyle.Flat : (pressed || isChecked) ? NeuStyle.Inset : NeuStyle.Raised;
            NeuPainter.DrawSurface(g, shape, radius, style, Depth);

            Color color = TextColor(isChecked || primary ? NeuTheme.AccentInk : NeuTheme.Ink);
            Rectangle content = shape;
            if (pressed) content.Offset(0, 1);

            bool hasIcon = !string.IsNullOrEmpty(icon);
            bool hasText = !string.IsNullOrEmpty(Text);

            if (hasIcon && hasText)
            {
                const int iconWidth = 18;
                const int gap = 8;
                Size textSize = TextRenderer.MeasureText(g, Text, Font, Size.Empty, TextFormatFlags.NoPadding);
                int total = iconWidth + gap + textSize.Width;
                int x = content.X + Math.Max(10, (content.Width - total) / 2);

                DrawIcon(g, icon, NeuTheme.IconFont, new Rectangle(x, content.Y, iconWidth, content.Height), color);
                DrawText(g, Text, Font, new Rectangle(x + iconWidth + gap, content.Y, content.Right - x - iconWidth - gap - 6, content.Height),
                    color, TextFormatFlags.Left);
            }
            else if (hasIcon)
            {
                DrawIcon(g, icon, NeuTheme.IconFont, content, color);
            }
            else
            {
                DrawText(g, Text, Font, content, color);
            }

            if (ShowFocusRing) NeuPainter.DrawFocusRing(g, shape, radius);
        }

        #endregion

        #region Input

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            pressed = false;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!pressed) return;
            pressed = false;
            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space)
            {
                pressed = true;
                Invalidate();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                PerformClick();
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode != Keys.Space || !pressed) return;
            pressed = false;
            Invalidate();
            PerformClick();
            e.Handled = true;
        }

        #endregion
    }
}
