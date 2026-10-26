using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI.Neu
{
    public enum HistogramMode
    {
        Rgb,
        Luminance
    }

    /// <summary>
    /// Grafik histogram (skala log) di dalam bidang cekung.
    /// Mode RGB menumpuk tiga kurva transparan; mode Luminance menampilkan satu kurva abu-abu.
    /// </summary>
    public class HistogramView : NeuControl
    {
        private static readonly Color RedCurve = Color.FromArgb(0xE5, 0x48, 0x4D);
        private static readonly Color GreenCurve = Color.FromArgb(0x30, 0xA4, 0x6C);
        private static readonly Color BlueCurve = Color.FromArgb(0x3E, 0x63, 0xDD);

        private Histogram histogram;
        private HistogramMode mode = HistogramMode.Rgb;

        public HistogramView()
        {
            Size = new Size(220, 120);
            AccessibleRole = AccessibleRole.Chart;
            AccessibleName = "Histogram";
        }

        public Histogram Histogram
        {
            get => histogram;
            set { histogram = value; Invalidate(); }
        }

        public HistogramMode Mode
        {
            get => mode;
            set { mode = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle frame = Deflate(ClientRectangle, 1);
            NeuPainter.DrawSurface(g, frame, NeuTheme.RadiusMedium, NeuStyle.Inset, NeuTheme.DepthInset);

            Rectangle plot = Deflate(frame, 10);
            if (histogram == null)
            {
                DrawText(g, "Belum ada gambar", NeuTheme.SmallFont, plot, NeuTheme.InkMuted);
                return;
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (mode == HistogramMode.Rgb)
            {
                double max = Math.Max(LogMax(histogram.Red), Math.Max(LogMax(histogram.Green), LogMax(histogram.Blue)));
                DrawCurve(g, plot, histogram.Red, max, RedCurve);
                DrawCurve(g, plot, histogram.Green, max, GreenCurve);
                DrawCurve(g, plot, histogram.Blue, max, BlueCurve);
            }
            else
            {
                DrawCurve(g, plot, histogram.Gray, LogMax(histogram.Gray), NeuTheme.InkMuted);
            }
        }

        private static double LogMax(int[] bins)
        {
            double max = 0;
            foreach (int v in bins) max = Math.Max(max, Math.Log10(1 + v));
            return max;
        }

        /// <summary>Area transparan + garis tepi untuk satu channel</summary>
        private static void DrawCurve(Graphics g, Rectangle plot, int[] bins, double max, Color color)
        {
            if (max <= 0) return;

            var points = new PointF[258];
            points[0] = new PointF(plot.Left, plot.Bottom);
            for (int i = 0; i < 256; i++)
            {
                float x = plot.Left + i * (plot.Width - 1) / 255f;
                float y = plot.Bottom - (float)(Math.Log10(1 + bins[i]) / max) * plot.Height;
                points[i + 1] = new PointF(x, y);
            }
            points[257] = new PointF(plot.Right - 1, plot.Bottom);

            using (var fill = new SolidBrush(Color.FromArgb(70, color)))
            using (var line = new Pen(Color.FromArgb(200, color), 1.2f))
            {
                g.FillPolygon(fill, points);
                var outline = new PointF[256];
                Array.Copy(points, 1, outline, 0, 256);
                g.DrawLines(line, outline);
            }
        }
    }
}
