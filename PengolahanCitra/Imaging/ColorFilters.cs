using System;
using System.Drawing;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Filter per piksel: channel RGB, grayscale, threshold, negative, brightness,
    /// contrast stretching, color slicing, dan pewarnaan semu.
    /// Sebagian besar memakai lookup table (LUT) 256 entri, jadi tiap piksel cukup satu akses array.
    /// </summary>
    public static class ColorFilters
    {
        private static readonly byte[] IdentityLut = ColorMath.CreateLut(v => v);
        private static readonly byte[] ZeroLut = new byte[256];

        public static RgbImage RedChannel(RgbImage source) => ApplyLut(source, IdentityLut, ZeroLut, ZeroLut);
        public static RgbImage GreenChannel(RgbImage source) => ApplyLut(source, ZeroLut, IdentityLut, ZeroLut);
        public static RgbImage BlueChannel(RgbImage source) => ApplyLut(source, ZeroLut, ZeroLut, IdentityLut);

        public static RgbImage Grayscale(RgbImage source) => MapLuminance(source, IdentityLut);

        /// <summary>
        /// Binary: luminance > threshold jadi putih, sisanya hitam
        /// </summary>
        public static RgbImage Threshold(RgbImage source, int threshold = 128)
        {
            return MapLuminance(source, ColorMath.CreateLut(v => v > threshold ? 255 : 0));
        }

        /// <summary>
        /// Negative: balik setiap channel warna (255 - nilai)
        /// </summary>
        public static RgbImage Negative(RgbImage source)
        {
            byte[] lut = ColorMath.CreateLut(v => 255 - v);
            return ApplyLut(source, lut, lut, lut);
        }

        public static RgbImage Brightness(RgbImage source, int amount)
        {
            byte[] lut = ColorMath.CreateLut(v => v + amount);
            return ApplyLut(source, lut, lut, lut);
        }

        /// <summary>
        /// Contrast Stretching per channel: output = (input - min) * 255 / (max - min)
        /// </summary>
        public static RgbImage ContrastStretching(RgbImage source)
        {
            int[] min = { 255, 255, 255 };
            int[] max = { 0, 0, 0 };
            byte[] src = source.Pixels;

            for (int i = 0; i < src.Length; i += 3)
            {
                for (int c = 0; c < 3; c++)
                {
                    int v = src[i + c];
                    if (v < min[c]) min[c] = v;
                    if (v > max[c]) max[c] = v;
                }
            }

            var luts = new byte[3][];
            for (int c = 0; c < 3; c++)
            {
                int low = min[c];
                int range = Math.Max(max[c] - low, 1);
                luts[c] = ColorMath.CreateLut(v => (v - low) * 255 / range);
            }

            return ApplyLut(source, luts[0], luts[1], luts[2]);
        }

        /// <summary>
        /// Color Slicing: piksel yang jarak Euclidean-nya ke warna target ≤ tolerance dipertahankan,
        /// sisanya jadi hitam (atau abu-abu bila grayBackground = true).
        /// Jarak dibandingkan dalam bentuk kuadrat (d² ≤ tolerance²) agar tidak perlu Math.Sqrt.
        /// </summary>
        public static RgbImage ColorSlicing(RgbImage source, Color target, int tolerance, bool grayBackground)
        {
            var result = new RgbImage(source.Width, source.Height);
            byte[] src = source.Pixels;
            byte[] dst = result.Pixels;
            int toleranceSquared = Math.Max(tolerance, 0) * Math.Max(tolerance, 0);
            int stride = source.Stride;

            Parallel.For(0, source.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i += 3)
                {
                    byte r = src[i], g = src[i + 1], b = src[i + 2];
                    int dr = r - target.R, dg = g - target.G, db = b - target.B;

                    if (dr * dr + dg * dg + db * db <= toleranceSquared)
                    {
                        dst[i] = r;
                        dst[i + 1] = g;
                        dst[i + 2] = b;
                    }
                    else if (grayBackground)
                    {
                        dst[i] = dst[i + 1] = dst[i + 2] = ColorMath.Luminance(r, g, b);
                    }
                    // else: tetap hitam (array baru sudah berisi 0)
                }
            });

            return result;
        }

        /// <summary>
        /// Pewarnaan semu (pseudo color): intensitas dipetakan ke palet "jet"
        /// biru (gelap) → cyan → kuning → merah (terang)
        /// </summary>
        public static RgbImage PseudoColor(RgbImage source)
        {
            var palette = new byte[256 * 3];
            for (int i = 0; i < 256; i++)
            {
                double t = i / 255.0 * 4.0;
                palette[i * 3] = JetComponent(t - 3);
                palette[i * 3 + 1] = JetComponent(t - 2);
                palette[i * 3 + 2] = JetComponent(t - 1);
            }

            var result = new RgbImage(source.Width, source.Height);
            byte[] src = source.Pixels;
            byte[] dst = result.Pixels;
            int stride = source.Stride;

            Parallel.For(0, source.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i += 3)
                {
                    int p = ColorMath.Luminance(src[i], src[i + 1], src[i + 2]) * 3;
                    dst[i] = palette[p];
                    dst[i + 1] = palette[p + 1];
                    dst[i + 2] = palette[p + 2];
                }
            });

            return result;
        }

        private static byte JetComponent(double distance)
        {
            double value = 1.5 - Math.Abs(distance);
            return ColorMath.ClampToByte((int)Math.Round(Math.Max(0, Math.Min(1, value)) * 255));
        }

        #region Helper

        /// <summary>
        /// Terapkan LUT terpisah untuk masing-masing channel R, G, B
        /// </summary>
        public static RgbImage ApplyLut(RgbImage source, byte[] lutR, byte[] lutG, byte[] lutB)
        {
            var result = new RgbImage(source.Width, source.Height);
            byte[] src = source.Pixels;
            byte[] dst = result.Pixels;
            int stride = source.Stride;

            Parallel.For(0, source.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i += 3)
                {
                    dst[i] = lutR[src[i]];
                    dst[i + 1] = lutG[src[i + 1]];
                    dst[i + 2] = lutB[src[i + 2]];
                }
            });

            return result;
        }

        /// <summary>
        /// Hitung luminance tiap piksel, lewatkan ke LUT, lalu tulis ke ketiga channel (hasil abu-abu)
        /// </summary>
        private static RgbImage MapLuminance(RgbImage source, byte[] lut)
        {
            var result = new RgbImage(source.Width, source.Height);
            byte[] src = source.Pixels;
            byte[] dst = result.Pixels;
            int stride = source.Stride;

            Parallel.For(0, source.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i += 3)
                {
                    dst[i] = dst[i + 1] = dst[i + 2] = lut[ColorMath.Luminance(src[i], src[i + 1], src[i + 2])];
                }
            });

            return result;
        }

        /// <summary>
        /// Plane luminance 1 byte per piksel (dipakai Edge Detection)
        /// </summary>
        public static byte[] ToGrayPlane(RgbImage source)
        {
            var gray = new byte[source.PixelCount];
            byte[] src = source.Pixels;
            int width = source.Width;

            Parallel.For(0, source.Height, y =>
            {
                int g = y * width;
                int i = g * 3;
                for (int x = 0; x < width; x++, g++, i += 3)
                {
                    gray[g] = ColorMath.Luminance(src[i], src[i + 1], src[i + 2]);
                }
            });

            return gray;
        }

        #endregion
    }
}
