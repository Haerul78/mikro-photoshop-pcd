using System;
using System.Drawing;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Histogram R, G, B, dan Gray (luminance) dari satu gambar
    /// </summary>
    public sealed class Histogram
    {
        public int[] Red { get; } = new int[256];
        public int[] Green { get; } = new int[256];
        public int[] Blue { get; } = new int[256];
        public int[] Gray { get; } = new int[256];

        /// <summary>
        /// Keempat histogram dihitung dalam satu kali sapuan gambar
        /// </summary>
        public static Histogram Compute(RgbImage image)
        {
            var histogram = new Histogram();
            int[] red = histogram.Red, green = histogram.Green, blue = histogram.Blue, gray = histogram.Gray;
            byte[] src = image.Pixels;

            for (int i = 0; i < src.Length; i += 3)
            {
                byte r = src[i], g = src[i + 1], b = src[i + 2];
                red[r]++;
                green[g]++;
                blue[b]++;
                gray[ColorMath.Luminance(r, g, b)]++;
            }

            return histogram;
        }

        /// <summary>
        /// Gambar histogram (skala log) untuk ditampilkan
        /// </summary>
        public static Bitmap Render(int[] histogram, Color color, int width = 220, int height = 60)
        {
            var bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (var pen = new Pen(color, 1))
            using (var gridPen = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                g.Clear(Color.FromArgb(28, 28, 28));

                // Skala log supaya bin kecil tetap terlihat
                var logHistogram = new double[256];
                double max = 0;
                for (int i = 0; i < 256; i++)
                {
                    logHistogram[i] = Math.Log10(1 + histogram[i]);
                    max = Math.Max(max, logHistogram[i]);
                }

                g.DrawLine(gridPen, 0, height / 2, width, height / 2);
                if (max == 0) return bitmap;

                for (int i = 0; i < 256; i++)
                {
                    float x = i * width / 256f;
                    float barHeight = (float)(logHistogram[i] / max) * (height - 4);
                    if (barHeight > 0) g.DrawLine(pen, x, height, x, height - barHeight);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Histogram Equalization pada channel Y (luminance) ruang warna YUV.
        /// Warna (U, V) dipertahankan. YUV dihitung ulang dari RGB, tidak disimpan dalam array.
        /// </summary>
        public static RgbImage EqualizeLuminance(RgbImage source)
        {
            byte[] src = source.Pixels;

            // Step 1: histogram Y
            var histogramY = new int[256];
            for (int i = 0; i < src.Length; i += 3)
            {
                histogramY[ColorMath.Clamp((int)LumaY(src[i], src[i + 1], src[i + 2]), 0, 255)]++;
            }

            // Step 2: CDF dan LUT
            var cdf = new long[256];
            cdf[0] = histogramY[0];
            for (int i = 1; i < 256; i++) cdf[i] = cdf[i - 1] + histogramY[i];

            long cdfMin = 0;
            for (int i = 0; i < 256; i++)
            {
                if (cdf[i] > 0) { cdfMin = cdf[i]; break; }
            }

            long denominator = Math.Max(source.PixelCount - cdfMin, 1);
            var lutY = new double[256];
            for (int i = 0; i < 256; i++)
            {
                lutY[i] = cdf[i] == 0 ? 0 : (double)(cdf[i] - cdfMin) / denominator * 255.0;
            }

            // Step 3: ganti Y, konversi balik ke RGB
            var result = new RgbImage(source.Width, source.Height);
            byte[] dst = result.Pixels;
            int stride = source.Stride;

            Parallel.For(0, source.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i += 3)
                {
                    double r = src[i], g = src[i + 1], b = src[i + 2];
                    double u = -0.147 * r - 0.289 * g + 0.436 * b;
                    double v = 0.615 * r - 0.515 * g - 0.100 * b;
                    double newY = lutY[ColorMath.Clamp((int)LumaY(src[i], src[i + 1], src[i + 2]), 0, 255)];

                    dst[i] = ColorMath.ClampToByte(newY + 1.140 * v);
                    dst[i + 1] = ColorMath.ClampToByte(newY - 0.395 * u - 0.581 * v);
                    dst[i + 2] = ColorMath.ClampToByte(newY + 2.032 * u);
                }
            });

            return result;
        }

        private static double LumaY(double r, double g, double b) => 0.299 * r + 0.587 * g + 0.114 * b;
    }
}
