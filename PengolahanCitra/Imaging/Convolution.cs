using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Kernel konvolusi 3x3 beserta pembagi dan offset
    /// </summary>
    public sealed class Kernel3x3
    {
        public int[] Weights { get; }   // 9 nilai, baris per baris
        public int Divisor { get; }
        public int Offset { get; }

        public Kernel3x3(int[] weights, int divisor = 0, int offset = 0)
        {
            Weights = weights;

            int sum = 0;
            foreach (int w in weights) sum += w;

            // Divisor 0 = pakai jumlah kernel (atau 1 bila jumlahnya 0)
            Divisor = divisor != 0 ? divisor : (sum != 0 ? sum : 1);
            Offset = offset;
        }

        /// <summary>
        ///   [1 2 1]
        ///   [2 4 2] ÷ 16
        ///   [1 2 1]
        /// </summary>
        public static readonly Kernel3x3 Gaussian = new Kernel3x3(new[] { 1, 2, 1, 2, 4, 2, 1, 2, 1 }, 16);

        /// <summary>
        /// Mean / Box Blur, semua 1 ÷ 9
        /// </summary>
        public static readonly Kernel3x3 Mean = new Kernel3x3(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 9);

        public static readonly Kernel3x3 SharpenSoft = new Kernel3x3(new[] { 0, -1, 0, -1, 5, -1, 0, -1, 0 });

        public static readonly Kernel3x3 SharpenStrong = new Kernel3x3(new[] { -1, -1, -1, -1, 9, -1, -1, -1, -1 });
    }

    /// <summary>
    /// Konvolusi 3x3 pada gambar RGB. Tepi gambar memakai piksel terdekat (clamp).
    /// </summary>
    public static class Convolution
    {
        public static RgbImage GaussianBlur(RgbImage source, int passes) =>
            Apply(source, Kernel3x3.Gaussian, ColorMath.Clamp(passes, 1, 20));

        public static RgbImage MeanBlur(RgbImage source, int passes) =>
            Apply(source, Kernel3x3.Mean, ColorMath.Clamp(passes, 1, 20));

        public static RgbImage Sharpen(RgbImage source, bool strong, int passes) =>
            Apply(source, strong ? Kernel3x3.SharpenStrong : Kernel3x3.SharpenSoft, ColorMath.Clamp(passes, 1, 5));

        /// <summary>
        /// Konvolusi berulang. Memakai dua buffer bergantian (ping-pong), bukan array baru tiap pass.
        /// </summary>
        public static RgbImage Apply(RgbImage source, Kernel3x3 kernel, int passes = 1)
        {
            var current = new RgbImage(source.Width, source.Height);
            ConvolveInto(source, current, kernel);
            if (passes <= 1) return current;

            var spare = new RgbImage(source.Width, source.Height);
            for (int i = 1; i < passes; i++)
            {
                ConvolveInto(current, spare, kernel);
                var temp = current;
                current = spare;
                spare = temp;
            }

            return current;
        }

        private static void ConvolveInto(RgbImage source, RgbImage target, Kernel3x3 kernel)
        {
            int width = source.Width;
            int height = source.Height;
            int stride = source.Stride;
            byte[] src = source.Pixels;
            byte[] dst = target.Pixels;
            int[] k = kernel.Weights;
            int div = kernel.Divisor;
            int offset = kernel.Offset;

            Parallel.For(0, height, y =>
            {
                // Offset awal baris atas, tengah, bawah (di-clamp di tepi gambar)
                int rowUp = (y > 0 ? y - 1 : 0) * stride;
                int rowMid = y * stride;
                int rowDown = (y < height - 1 ? y + 1 : y) * stride;

                for (int x = 0; x < width; x++)
                {
                    int left = (x > 0 ? x - 1 : 0) * 3;
                    int mid = x * 3;
                    int right = (x < width - 1 ? x + 1 : x) * 3;

                    for (int c = 0; c < 3; c++)
                    {
                        int sum =
                            src[rowUp + left + c] * k[0] + src[rowUp + mid + c] * k[1] + src[rowUp + right + c] * k[2] +
                            src[rowMid + left + c] * k[3] + src[rowMid + mid + c] * k[4] + src[rowMid + right + c] * k[5] +
                            src[rowDown + left + c] * k[6] + src[rowDown + mid + c] * k[7] + src[rowDown + right + c] * k[8];

                        dst[rowMid + mid + c] = ColorMath.ClampToByte(sum / div + offset);
                    }
                }
            });
        }
    }
}
