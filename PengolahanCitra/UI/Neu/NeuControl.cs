using System;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Dasar semua kontrol neumorphic: double buffered, latar Surface, dan focus ring untuk keyboard
    /// </summary>
    public abstract class NeuControl : Control
    {
        protected NeuControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            BackColor = NeuTheme.Surface;
            ForeColor = NeuTheme.Ink;
            Font = NeuTheme.LabelFont;
        }

        /// <summary>Tampilkan focus ring hanya saat fokus didapat lewat keyboard</summary>
        protected bool ShowFocusRing => Focused && ShowFocusCues;

        /// <summary>Warna teks sesuai state enabled</summary>
        protected Color TextColor(Color enabledColor) => Enabled ? enabledColor : NeuTheme.InkDisabled;

        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(NeuTheme.Surface);

        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }

        protected static Rectangle Deflate(Rectangle r, int amount)
        {
            r.Inflate(-amount, -amount);
            return r;
        }

        /// <summary>Glyph ikon: tanpa elipsis dan tanpa clipping agar tidak terpotong</summary>
        protected static void DrawIcon(Graphics g, string glyph, Font font, Rectangle bounds, Color color)
        {
            TextRenderer.DrawText(g, glyph, font, bounds, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoClipping | TextFormatFlags.NoPrefix);
        }

        protected static void DrawText(Graphics g, string text, Font font, Rectangle bounds, Color color,
            TextFormatFlags align = TextFormatFlags.HorizontalCenter)
        {
            TextRenderer.DrawText(g, text, font, bounds, color,
                align | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }
    }
}
