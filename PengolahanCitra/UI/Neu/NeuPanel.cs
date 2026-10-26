using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Container dengan permukaan timbul atau cekung. Kontrol anak diletakkan di dalam Padding.
    /// </summary>
    public class NeuPanel : Panel
    {
        private NeuStyle style = NeuStyle.Raised;
        private int radius = NeuTheme.RadiusLarge;
        private int depth = NeuTheme.DepthRaised;

        public NeuPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = NeuTheme.Surface;
        }

        [DefaultValue(NeuStyle.Raised)]
        public NeuStyle Style
        {
            get => style;
            set { style = value; Invalidate(); }
        }

        [DefaultValue(NeuTheme.RadiusLarge)]
        public int Radius
        {
            get => radius;
            set { radius = value; Invalidate(); }
        }

        [DefaultValue(NeuTheme.DepthRaised)]
        public int Depth
        {
            get => depth;
            set { depth = value; Invalidate(); }
        }

        /// <summary>Bentuk permukaan; panel timbul memberi ruang untuk shadow di luar bentuk</summary>
        public Rectangle ShapeBounds
        {
            get
            {
                Rectangle r = ClientRectangle;
                int margin = style == NeuStyle.Raised ? NeuTheme.ShadowMargin(depth) : 1;
                r.Inflate(-margin, -margin);
                return r;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(NeuTheme.Surface);
            NeuPainter.DrawSurface(e.Graphics, ShapeBounds, radius, style, depth);
        }
    }
}
