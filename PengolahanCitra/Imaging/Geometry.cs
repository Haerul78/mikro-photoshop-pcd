using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Transformasi geometri: rotasi, flip, translasi, dan resize
    /// </summary>
    public static class Geometry
    {
        #region Rotation

        /// <summary>
        /// Putar searah jarum jam. Kelipatan 90° dikerjakan langsung pada array (lossless);
        /// sudut lain memakai interpolasi bicubic dengan kanvas diperbesar dan sudut kosong berwarna hitam.
        /// </summary>
        public static RgbImage Rotate(RgbImage source, int angleDegrees)
        {
            int angle = ((angleDegrees % 360) + 360) % 360;

            switch (angle)
            {
                case 0: return source;
                case 90: return RotateQuarter(source, 1);
                case 180: return RotateQuarter(source, 2);
                case 270: return RotateQuarter(source, 3);
                default: return RotateFree(source, angle);
            }
        }

        /// <summary>
        /// Rotasi kelipatan 90° searah jarum jam (quarterTurns = 1, 2, atau 3)
        /// </summary>
        private static RgbImage RotateQuarter(RgbImage source, int quarterTurns)
        {
            int w = source.Width, h = source.Height;
            bool swap = quarterTurns % 2 == 1;
            var result = swap ? new RgbImage(h, w) : new RgbImage(w, h);
            byte[] src = source.Pixels, dst = result.Pixels;
            int dstWidth = result.Width;

            Parallel.For(0, h, y =>
            {
                for (int x = 0; x < w; x++)
                {
                    int nx, ny;
                    switch (quarterTurns)
                    {
                        case 1: nx = h - 1 - y; ny = x; break;            // 90°
                        case 2: nx = w - 1 - x; ny = h - 1 - y; break;    // 180°
                        default: nx = y; ny = w - 1 - x; break;           // 270°
                    }

                    int s = (y * w + x) * 3;
                    int d = (ny * dstWidth + nx) * 3;
                    dst[d] = src[s];
                    dst[d + 1] = src[s + 1];
                    dst[d + 2] = src[s + 2];
                }
            });

            return result;
        }

        private static RgbImage RotateFree(RgbImage source, float angle)
        {
            double radians = angle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(radians));
            double sin = Math.Abs(Math.Sin(radians));

            int newWidth = Math.Max(1, (int)Math.Round(source.Width * cos + source.Height * sin));
            int newHeight = Math.Max(1, (int)Math.Round(source.Width * sin + source.Height * cos));

            using (Bitmap input = source.ToBitmap())
            using (var output = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb))
            {
                using (Graphics g = Graphics.FromImage(output))
                {
                    g.Clear(Color.Black);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Putar terhadap titik tengah kanvas baru
                    g.TranslateTransform(newWidth / 2f, newHeight / 2f);
                    g.RotateTransform(angle);
                    g.TranslateTransform(-source.Width / 2f, -source.Height / 2f);
                    g.DrawImage(input, 0, 0, source.Width, source.Height);
                }

                return RgbImage.FromBitmap(output);
            }
        }

        #endregion

        #region Flip & Translate

        public static RgbImage FlipHorizontal(RgbImage source)
        {
            int w = source.Width;
            var result = new RgbImage(w, source.Height);
            byte[] src = source.Pixels, dst = result.Pixels;

            Parallel.For(0, source.Height, y =>
            {
                int row = y * w * 3;
                for (int x = 0; x < w; x++)
                {
                    int s = row + (w - 1 - x) * 3;
                    int d = row + x * 3;
                    dst[d] = src[s];
                    dst[d + 1] = src[s + 1];
                    dst[d + 2] = src[s + 2];
                }
            });

            return result;
        }

        public static RgbImage FlipVertical(RgbImage source)
        {
            int h = source.Height, stride = source.Stride;
            var result = new RgbImage(source.Width, h);

            Parallel.For(0, h, y =>
            {
                Buffer.BlockCopy(source.Pixels, (h - 1 - y) * stride, result.Pixels, y * stride, stride);
            });

            return result;
        }

        /// <summary>
        /// Geser gambar; area yang kosong diisi hitam
        /// </summary>
        public static RgbImage Translate(RgbImage source, int offsetX, int offsetY)
        {
            int w = source.Width, h = source.Height;
            var result = new RgbImage(w, h);

            // Rentang kolom tujuan yang masih mendapat piksel dari sumber
            int dstStartX = Math.Max(0, offsetX);
            int dstEndX = Math.Min(w, w + offsetX);
            if (dstEndX <= dstStartX) return result;

            int byteCount = (dstEndX - dstStartX) * 3;

            Parallel.For(Math.Max(0, offsetY), Math.Min(h, h + offsetY), y =>
            {
                int srcY = y - offsetY;
                Buffer.BlockCopy(source.Pixels, (srcY * w + dstStartX - offsetX) * 3,
                                 result.Pixels, (y * w + dstStartX) * 3, byteCount);
            });

            return result;
        }

        #endregion

        #region Resize

        /// <summary>
        /// Resize dengan interpolasi bicubic. Tepi gambar tidak menjadi gelap karena WrapMode TileFlipXY.
        /// </summary>
        public static RgbImage Resize(RgbImage source, int width, int height)
        {
            if (source.Width == width && source.Height == height) return source;

            using (Bitmap input = source.ToBitmap())
            using (Bitmap output = ResizeBitmap(input, width, height))
            {
                return RgbImage.FromBitmap(output);
            }
        }

        /// <summary>
        /// Perkecil gambar supaya sisi terpanjangnya maksimal maxSide (aspect ratio dipertahankan)
        /// </summary>
        public static RgbImage FitWithin(RgbImage source, int maxSide)
        {
            Size size = FitSize(source.Width, source.Height, maxSide);
            return Resize(source, size.Width, size.Height);
        }

        /// <summary>
        /// Ukuran hasil perkecil dengan sisi terpanjang maksimal maxSide (tidak pernah diperbesar)
        /// </summary>
        public static Size FitSize(int width, int height, int maxSide)
        {
            double ratio = Math.Min(1.0, (double)maxSide / Math.Max(width, height));
            return new Size(Math.Max(1, (int)Math.Round(width * ratio)),
                            Math.Max(1, (int)Math.Round(height * ratio)));
        }

        public static Bitmap ResizeBitmap(Image input, int width, int height)
        {
            var output = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(output))
            using (var attributes = new ImageAttributes())
            {
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(input, new Rectangle(0, 0, width, height),
                    0, 0, input.Width, input.Height, GraphicsUnit.Pixel, attributes);
            }

            return output;
        }

        #endregion
    }
}
