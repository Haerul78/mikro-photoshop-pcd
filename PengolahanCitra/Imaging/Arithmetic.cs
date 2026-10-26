using System;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    public enum ArithmeticOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>
    /// Operasi aritmatika per piksel antara dua gambar berukuran sama
    /// </summary>
    public static class Arithmetic
    {
        // Tabel hasil untuk setiap pasangan nilai (a, b): 256 x 256 = 64 KB per operasi
        private static readonly byte[][] Tables =
        {
            CreateTable((a, b) => a + b),
            CreateTable((a, b) => a - b),
            CreateTable((a, b) => a * b / 255),
            CreateTable(Divide)
        };

        public static string GetLabel(ArithmeticOperation operation)
        {
            switch (operation)
            {
                case ArithmeticOperation.Add: return "Penjumlahan";
                case ArithmeticOperation.Subtract: return "Pengurangan";
                case ArithmeticOperation.Multiply: return "Perkalian";
                default: return "Pembagian";
            }
        }

        public static RgbImage Apply(RgbImage first, RgbImage second, ArithmeticOperation operation)
        {
            if (!first.SameSizeAs(second))
                throw new ArgumentException("Kedua gambar harus berukuran sama.");

            byte[] table = Tables[(int)operation];
            var result = new RgbImage(first.Width, first.Height);
            byte[] a = first.Pixels, b = second.Pixels, dst = result.Pixels;
            int stride = first.Stride;

            Parallel.For(0, first.Height, y =>
            {
                int end = (y + 1) * stride;
                for (int i = y * stride; i < end; i++)
                {
                    dst[i] = table[(a[i] << 8) | b[i]];
                }
            });

            return result;
        }

        /// <summary>
        /// result = a * 255 / b. Pembagi 0 dianggap hasil tak hingga → putih (0 / 0 tetap hitam)
        /// </summary>
        private static int Divide(int a, int b)
        {
            if (b == 0) return a == 0 ? 0 : 255;
            return a * 255 / b;
        }

        private static byte[] CreateTable(Func<int, int, int> operation)
        {
            var table = new byte[256 * 256];
            for (int a = 0; a < 256; a++)
            {
                for (int b = 0; b < 256; b++)
                {
                    table[(a << 8) | b] = ColorMath.ClampToByte(operation(a, b));
                }
            }
            return table;
        }
    }
}
