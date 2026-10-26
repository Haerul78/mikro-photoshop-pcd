using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    public enum EdgeMethod
    {
        Roberts,
        Prewitt,
        Sobel,
        Canny
    }

    /// <summary>
    /// Deteksi tepi pada luminance gambar. Hasilnya gambar abu-abu (magnitude gradient).
    /// </summary>
    public static class EdgeDetection
    {
        public const int DefaultCannyLow = 50;
        public const int DefaultCannyHigh = 150;

        private static readonly int[] PrewittX = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
        private static readonly int[] PrewittY = { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
        private static readonly int[] SobelX = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
        private static readonly int[] SobelY = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };

        public static string GetLabel(EdgeMethod method) => method.ToString();

        public static RgbImage Apply(RgbImage source, EdgeMethod method,
            int cannyLow = DefaultCannyLow, int cannyHigh = DefaultCannyHigh)
        {
            switch (method)
            {
                case EdgeMethod.Roberts: return Roberts(source);
                case EdgeMethod.Prewitt: return Gradient3x3(source, PrewittX, PrewittY);
                case EdgeMethod.Sobel: return Gradient3x3(source, SobelX, SobelY);
                case EdgeMethod.Canny: return Canny(source, cannyLow, cannyHigh);
                default: throw new ArgumentOutOfRangeException(nameof(method));
            }
        }

        #region Roberts, Prewitt, Sobel

        /// <summary>
        /// Roberts Cross: Gx = P(x,y) - P(x+1,y+1), Gy = P(x+1,y) - P(x,y+1)
        /// </summary>
        private static RgbImage Roberts(RgbImage source)
        {
            int width = source.Width, height = source.Height;
            byte[] gray = ColorFilters.ToGrayPlane(source);
            var result = new byte[gray.Length];

            Parallel.For(0, height, y =>
            {
                int row = y * width;
                int rowDown = (y < height - 1 ? y + 1 : y) * width;

                for (int x = 0; x < width; x++)
                {
                    int right = x < width - 1 ? x + 1 : x;
                    int gx = gray[row + x] - gray[rowDown + right];
                    int gy = gray[row + right] - gray[rowDown + x];
                    result[row + x] = Magnitude(gx, gy);
                }
            });

            return RgbImage.FromGrayPlane(result, width, height);
        }

        /// <summary>
        /// Gradient dengan sepasang kernel 3x3 (Prewitt / Sobel)
        /// </summary>
        private static RgbImage Gradient3x3(RgbImage source, int[] kernelX, int[] kernelY)
        {
            int width = source.Width, height = source.Height;
            byte[] gray = ColorFilters.ToGrayPlane(source);
            var result = new byte[gray.Length];

            Parallel.For(0, height, y =>
            {
                int up = (y > 0 ? y - 1 : 0) * width;
                int mid = y * width;
                int down = (y < height - 1 ? y + 1 : y) * width;

                for (int x = 0; x < width; x++)
                {
                    int left = x > 0 ? x - 1 : 0;
                    int right = x < width - 1 ? x + 1 : x;

                    int p0 = gray[up + left], p1 = gray[up + x], p2 = gray[up + right];
                    int p3 = gray[mid + left], p4 = gray[mid + x], p5 = gray[mid + right];
                    int p6 = gray[down + left], p7 = gray[down + x], p8 = gray[down + right];

                    int gx = p0 * kernelX[0] + p1 * kernelX[1] + p2 * kernelX[2] + p3 * kernelX[3] + p4 * kernelX[4] +
                             p5 * kernelX[5] + p6 * kernelX[6] + p7 * kernelX[7] + p8 * kernelX[8];
                    int gy = p0 * kernelY[0] + p1 * kernelY[1] + p2 * kernelY[2] + p3 * kernelY[3] + p4 * kernelY[4] +
                             p5 * kernelY[5] + p6 * kernelY[6] + p7 * kernelY[7] + p8 * kernelY[8];

                    result[mid + x] = Magnitude(gx, gy);
                }
            });

            return RgbImage.FromGrayPlane(result, width, height);
        }

        private static byte Magnitude(int gx, int gy) => ColorMath.ClampToByte((int)Math.Sqrt(gx * gx + gy * gy));

        #endregion

        #region Canny

        private const byte WeakEdge = 128;
        private const byte StrongEdge = 255;

        /// <summary>
        /// Canny: Gaussian Blur → Sobel → Non-Maximum Suppression → Double Threshold → Hysteresis
        /// </summary>
        private static RgbImage Canny(RgbImage source, int lowThreshold, int highThreshold)
        {
            int width = source.Width, height = source.Height;

            byte[] gray = ColorFilters.ToGrayPlane(Convolution.GaussianBlur(source, 2));

            // Gradient Sobel. Arah disimpan sebagai sektor 0-3 (0°, 45°, 90°, 135°) agar hemat memori
            var magnitude = new float[gray.Length];
            var sector = new byte[gray.Length];

            Parallel.For(1, Math.Max(1, height - 1), y =>
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int i = y * width + x;
                    int p0 = gray[i - width - 1], p1 = gray[i - width], p2 = gray[i - width + 1];
                    int p3 = gray[i - 1], p5 = gray[i + 1];
                    int p6 = gray[i + width - 1], p7 = gray[i + width], p8 = gray[i + width + 1];

                    int gx = -p0 + p2 - 2 * p3 + 2 * p5 - p6 + p8;
                    int gy = -p0 - 2 * p1 - p2 + p6 + 2 * p7 + p8;

                    magnitude[i] = (float)Math.Sqrt(gx * gx + gy * gy);
                    sector[i] = AngleToSector(Math.Atan2(gy, gx) * 180.0 / Math.PI);
                }
            });

            byte[] edges = NonMaxSuppressionAndThreshold(magnitude, sector, width, height, lowThreshold, highThreshold);
            Hysteresis(edges, width, height);

            return RgbImage.FromGrayPlane(edges, width, height);
        }

        private static byte AngleToSector(double angle)
        {
            if (angle < 0) angle += 180;

            if (angle < 22.5 || angle >= 157.5) return 0;
            if (angle < 67.5) return 1;
            if (angle < 112.5) return 2;
            return 3;
        }

        /// <summary>
        /// Tipiskan tepi (hanya puncak lokal searah gradient yang dipertahankan),
        /// lalu klasifikasi: ≥ high = strong (255), ≥ low = weak (128), sisanya 0.
        /// Sumbu y gambar mengarah ke bawah, jadi gradient 45° menunjuk ke kanan-bawah.
        /// </summary>
        private static byte[] NonMaxSuppressionAndThreshold(float[] magnitude, byte[] sector, int width, int height, int low, int high)
        {
            var result = new byte[magnitude.Length];

            Parallel.For(1, Math.Max(1, height - 1), y =>
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int i = y * width + x;
                    float q, r;

                    switch (sector[i])
                    {
                        case 0: q = magnitude[i + 1]; r = magnitude[i - 1]; break;                     // 0°
                        case 1: q = magnitude[i + width + 1]; r = magnitude[i - width - 1]; break;     // 45°
                        case 2: q = magnitude[i + width]; r = magnitude[i - width]; break;             // 90°
                        default: q = magnitude[i + width - 1]; r = magnitude[i - width + 1]; break;    // 135°
                    }

                    float m = magnitude[i];
                    if (m < q || m < r) continue;

                    int value = Math.Min((int)m, 255);
                    if (value >= high) result[i] = StrongEdge;
                    else if (value >= low) result[i] = WeakEdge;
                }
            });

            return result;
        }

        /// <summary>
        /// Flood fill dari setiap strong edge: weak edge yang tersambung (langsung atau berantai) jadi strong,
        /// weak edge yang tidak tersambung dibuang
        /// </summary>
        private static void Hysteresis(byte[] edges, int width, int height)
        {
            var stack = new Stack<int>();
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i] == StrongEdge) stack.Push(i);
            }

            while (stack.Count > 0)
            {
                int index = stack.Pop();
                int cx = index % width;
                int cy = index / width;

                for (int ny = Math.Max(cy - 1, 0); ny <= Math.Min(cy + 1, height - 1); ny++)
                {
                    for (int nx = Math.Max(cx - 1, 0); nx <= Math.Min(cx + 1, width - 1); nx++)
                    {
                        int n = ny * width + nx;
                        if (edges[n] == WeakEdge)
                        {
                            edges[n] = StrongEdge;
                            stack.Push(n);
                        }
                    }
                }
            }

            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i] == WeakEdge) edges[i] = 0;
            }
        }

        #endregion
    }
}
