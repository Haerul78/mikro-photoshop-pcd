using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Baca/tulis file gambar dan ekspor matrix ke teks
    /// </summary>
    public static class ImageFile
    {
        public const string OpenFilter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff";
        public const string SaveFilter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp";

        /// <summary>
        /// Muat gambar. File dibaca ke memori dulu sehingga tidak terkunci selama aplikasi berjalan.
        /// </summary>
        public static RgbImage Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("File gambar tidak ditemukan.", path);

            using (var stream = new MemoryStream(File.ReadAllBytes(path)))
            using (var bitmap = new Bitmap(stream))
            {
                return RgbImage.FromBitmap(bitmap);
            }
        }

        public static void Save(RgbImage image, string path)
        {
            using (Bitmap bitmap = image.ToBitmap())
            {
                bitmap.Save(path, GetFormat(path));
            }
        }

        /// <summary>
        /// Simpan nilai piksel sebagai teks: satu baris gambar per baris file, tiap piksel "R,G,B"
        /// </summary>
        public static void SaveMatrixText(RgbImage image, string path)
        {
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(false), 1 << 16))
            {
                writer.WriteLine($"Width: {image.Width}");
                writer.WriteLine($"Height: {image.Height}");
                writer.WriteLine("Format: R,G,B per pixel, dipisah spasi");
                writer.WriteLine();

                byte[] pixels = image.Pixels;
                var line = new StringBuilder(image.Width * 12);

                for (int y = 0; y < image.Height; y++)
                {
                    line.Clear();
                    int end = (y + 1) * image.Stride;

                    for (int i = y * image.Stride; i < end; i += 3)
                    {
                        if (line.Length > 0) line.Append(' ');
                        line.Append(pixels[i]).Append(',').Append(pixels[i + 1]).Append(',').Append(pixels[i + 2]);
                    }

                    writer.WriteLine(line);
                }
            }
        }

        private static ImageFormat GetFormat(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return ImageFormat.Jpeg;
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".tif":
                case ".tiff": return ImageFormat.Tiff;
                default: return ImageFormat.Png;
            }
        }
    }
}
