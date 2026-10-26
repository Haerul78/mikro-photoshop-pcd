using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Gambar RGB 8-bit. Piksel disimpan dalam satu array 1D berurutan R, G, B per baris (tanpa padding).
    /// Konvensi: objek ini tidak diubah setelah selesai dibuat, sehingga aman dibagi antar thread.
    /// Setiap operasi menghasilkan RgbImage baru.
    /// </summary>
    public sealed class RgbImage
    {
        public const int Channels = 3;

        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; }

        public RgbImage(int width, int height)
            : this(width, height, new byte[CheckedLength(width, height)])
        {
        }

        public RgbImage(int width, int height, byte[] pixels)
        {
            if (pixels == null) throw new ArgumentNullException(nameof(pixels));
            if (pixels.Length != CheckedLength(width, height))
                throw new ArgumentException("Jumlah data piksel tidak sesuai dengan ukuran gambar.", nameof(pixels));

            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Stride => Width * Channels;
        public long PixelCount => (long)Width * Height;

        public int IndexOf(int x, int y) => (y * Width + x) * Channels;

        public Color GetColor(int x, int y)
        {
            int i = IndexOf(x, y);
            return Color.FromArgb(Pixels[i], Pixels[i + 1], Pixels[i + 2]);
        }

        public bool SameSizeAs(RgbImage other) => other != null && other.Width == Width && other.Height == Height;

        /// <summary>
        /// Buat gambar abu-abu (R = G = B) dari satu plane intensitas
        /// </summary>
        public static RgbImage FromGrayPlane(byte[] gray, int width, int height)
        {
            var result = new RgbImage(width, height);
            byte[] dst = result.Pixels;

            Parallel.For(0, height, y =>
            {
                int g = y * width;
                int d = g * Channels;
                for (int x = 0; x < width; x++, g++, d += Channels)
                {
                    dst[d] = dst[d + 1] = dst[d + 2] = gray[g];
                }
            });

            return result;
        }

        #region Konversi Bitmap

        /// <summary>
        /// Baca Bitmap memakai LockBits. Alpha diabaikan (sama seperti GetPixel().R/G/B).
        /// </summary>
        public static unsafe RgbImage FromBitmap(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            int width = bitmap.Width;
            int height = bitmap.Height;
            var image = new RgbImage(width, height);
            byte[] dst = image.Pixels;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte* scan0 = (byte*)data.Scan0;
                int stride = data.Stride;

                Parallel.For(0, height, y =>
                {
                    byte* src = scan0 + (long)y * stride;
                    int d = y * width * Channels;
                    for (int x = 0; x < width; x++, src += 4, d += Channels)
                    {
                        // Urutan byte di memori: B, G, R, A
                        dst[d] = src[2];
                        dst[d + 1] = src[1];
                        dst[d + 2] = src[0];
                    }
                });
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return image;
        }

        public unsafe Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* scan0 = (byte*)data.Scan0;
                int stride = data.Stride;
                byte[] src = Pixels;
                int width = Width;

                Parallel.For(0, Height, y =>
                {
                    byte* dst = scan0 + (long)y * stride;
                    int s = y * width * Channels;
                    for (int x = 0; x < width; x++, dst += 3, s += Channels)
                    {
                        dst[0] = src[s + 2];
                        dst[1] = src[s + 1];
                        dst[2] = src[s];
                    }
                });
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        #endregion

        private static int CheckedLength(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Ukuran gambar harus lebih dari 0.");

            long length = (long)width * height * Channels;
            if (length > int.MaxValue)
                throw new OutOfMemoryException($"Gambar {width} x {height} terlalu besar untuk diproses.");

            return (int)length;
        }
    }
}
