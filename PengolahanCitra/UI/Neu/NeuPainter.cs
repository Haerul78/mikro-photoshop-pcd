using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PengolahanCitra.UI.Neu
{
    public enum NeuStyle
    {
        /// <summary>Tanpa shadow</summary>
        Flat,
        /// <summary>Timbul: shadow terang kiri-atas, gelap kanan-bawah</summary>
        Raised,
        /// <summary>Cekung: shadow di sisi dalam bentuk</summary>
        Inset
    }

    /// <summary>
    /// Menggambar bentuk neumorphic. Shadow dibuat dari mask bentuk yang di-blur sungguhan
    /// (box blur 3 kali ≈ Gaussian), lalu di-cache per ukuran sehingga OnPaint tetap ringan.
    /// </summary>
    public static class NeuPainter
    {
        private const int MaxCacheEntries = 96;
        private const float DarkStrength = 0.85f;
        private const float LightStrength = 0.95f;
        private const float InsetDarkStrength = 1.0f;
        private const float InsetLightStrength = 0.8f;

        private static readonly Dictionary<ShadowKey, Bitmap> Cache = new Dictionary<ShadowKey, Bitmap>();
        private static readonly SolidBrush SurfaceBrush = new SolidBrush(NeuTheme.Surface);

        private struct ShadowKey : IEquatable<ShadowKey>
        {
            public int Width, Height, Radius, Depth;
            public bool Inset;

            public bool Equals(ShadowKey o) =>
                Width == o.Width && Height == o.Height && Radius == o.Radius && Depth == o.Depth && Inset == o.Inset;

            public override bool Equals(object obj) => obj is ShadowKey k && Equals(k);

            public override int GetHashCode() =>
                (((Width * 397 ^ Height) * 397 ^ Radius) * 397 ^ Depth) * 2 + (Inset ? 1 : 0);
        }

        #region Path

        public static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0.5f)
            {
                path.AddRectangle(r);
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region Permukaan

        /// <summary>
        /// Gambar bentuk dengan gaya tertentu. Area di luar bentuk diasumsikan sudah berwarna Surface.
        /// </summary>
        public static void DrawSurface(Graphics g, Rectangle shape, int radius, NeuStyle style, int depth)
        {
            if (shape.Width <= 0 || shape.Height <= 0) return;

            SmoothingMode oldSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (style == NeuStyle.Raised)
            {
                DrawShadowBitmap(g, shape, radius, depth, inset: false);
                FillFace(g, shape, radius);
            }
            else
            {
                FillFace(g, shape, radius);
                if (style == NeuStyle.Inset) DrawShadowBitmap(g, shape, radius, depth, inset: true);
            }

            g.SmoothingMode = oldSmoothing;
        }

        public static void FillFace(Graphics g, Rectangle shape, int radius)
        {
            using (GraphicsPath path = RoundedRect(shape, radius))
            {
                g.FillPath(SurfaceBrush, path);
            }
        }

        /// <summary>
        /// Garis fokus aksen di sekeliling bentuk (untuk navigasi keyboard)
        /// </summary>
        public static void DrawFocusRing(Graphics g, Rectangle shape, int radius)
        {
            SmoothingMode old = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF ring = shape;
            ring.Inflate(2.5f, 2.5f);
            using (var pen = new Pen(NeuTheme.Accent, 2f))
            using (GraphicsPath path = RoundedRect(ring, radius + 2.5f))
            {
                g.DrawPath(pen, path);
            }

            g.SmoothingMode = old;
        }

        private static void DrawShadowBitmap(Graphics g, Rectangle shape, int radius, int depth, bool inset)
        {
            Bitmap shadow = GetShadow(shape.Size, radius, depth, inset);
            int pad = Padding(depth);
            g.DrawImageUnscaled(shadow, shape.X - pad, shape.Y - pad);
        }

        #endregion

        #region Shadow

        private static int Blur(int depth) => depth * 2;
        private static int Padding(int depth) => depth + Blur(depth) * 2;

        private static Bitmap GetShadow(Size size, int radius, int depth, bool inset)
        {
            var key = new ShadowKey { Width = size.Width, Height = size.Height, Radius = radius, Depth = depth, Inset = inset };
            if (Cache.TryGetValue(key, out Bitmap cached)) return cached;

            if (Cache.Count >= MaxCacheEntries)
            {
                foreach (Bitmap bitmap in Cache.Values) bitmap.Dispose();
                Cache.Clear();
            }

            Bitmap created = inset ? CreateInsetShadow(size, radius, depth) : CreateRaisedShadow(size, radius, depth);
            Cache[key] = created;
            return created;
        }

        /// <summary>
        /// Shadow timbul: bentuk digeser ke kanan-bawah (gelap) dan kiri-atas (terang), lalu di-blur
        /// </summary>
        private static Bitmap CreateRaisedShadow(Size size, int radius, int depth)
        {
            int pad = Padding(depth);
            int w = size.Width + pad * 2, h = size.Height + pad * 2;

            float[] dark = RasterizeShape(w, h, new RectangleF(pad + depth, pad + depth, size.Width, size.Height), radius);
            float[] light = RasterizeShape(w, h, new RectangleF(pad - depth, pad - depth, size.Width, size.Height), radius);
            BoxBlur(dark, w, h, Blur(depth));
            BoxBlur(light, w, h, Blur(depth));

            return Compose(w, h, dark, light, mask: null, DarkStrength, LightStrength);
        }

        /// <summary>
        /// Shadow cekung: bagian luar bentuk yang digeser ke kanan-bawah menutupi tepi dalam kiri-atas (gelap),
        /// sebaliknya untuk sisi terang; hasilnya dipotong oleh bentuk itu sendiri
        /// </summary>
        private static Bitmap CreateInsetShadow(Size size, int radius, int depth)
        {
            int pad = Padding(depth);
            int w = size.Width + pad * 2, h = size.Height + pad * 2;
            var shapeRect = new RectangleF(pad, pad, size.Width, size.Height);

            float[] mask = RasterizeShape(w, h, shapeRect, radius);
            float[] dark = RasterizeShape(w, h, new RectangleF(pad + depth, pad + depth, size.Width, size.Height), radius);
            float[] light = RasterizeShape(w, h, new RectangleF(pad - depth, pad - depth, size.Width, size.Height), radius);

            for (int i = 0; i < dark.Length; i++)
            {
                dark[i] = 1f - dark[i];
                light[i] = 1f - light[i];
            }

            BoxBlur(dark, w, h, Blur(depth));
            BoxBlur(light, w, h, Blur(depth));

            return Compose(w, h, dark, light, mask, InsetDarkStrength, InsetLightStrength);
        }

        /// <summary>
        /// Gabungkan shadow gelap lalu terang (source-over) menjadi satu bitmap ARGB
        /// </summary>
        private static Bitmap Compose(int w, int h, float[] dark, float[] light, float[] mask, float darkStrength, float lightStrength)
        {
            var pixels = new int[w * h];
            Color d = NeuTheme.Shade, l = NeuTheme.Highlight;

            for (int i = 0; i < pixels.Length; i++)
            {
                float m = mask == null ? 1f : mask[i];
                float ad = Math.Min(1f, dark[i] * darkStrength) * m;
                float al = Math.Min(1f, light[i] * lightStrength) * m;

                // Terang di atas gelap
                float a = al + ad * (1 - al);
                if (a <= 0.002f) continue;

                float r = (l.R * al + d.R * ad * (1 - al)) / a;
                float g = (l.G * al + d.G * ad * (1 - al)) / a;
                float b = (l.B * al + d.B * ad * (1 - al)) / a;

                pixels[i] = ((int)(a * 255 + 0.5f) << 24) | ((int)r << 16) | ((int)g << 8) | (int)b;
            }

            var bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        /// <summary>
        /// Coverage (0-1) bentuk rounded rect, anti-aliased
        /// </summary>
        private static float[] RasterizeShape(int w, int h, RectangleF rect, int radius)
        {
            using (var bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                using (GraphicsPath path = RoundedRect(rect, radius))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillPath(Brushes.White, path);
                }

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var raw = new int[w * h];
                Marshal.Copy(data.Scan0, raw, 0, raw.Length);
                bitmap.UnlockBits(data);

                var coverage = new float[raw.Length];
                for (int i = 0; i < raw.Length; i++) coverage[i] = ((raw[i] >> 24) & 0xFF) / 255f;
                return coverage;
            }
        }

        /// <summary>
        /// Box blur terpisah (horizontal lalu vertikal), diulang 3 kali agar mendekati Gaussian.
        /// Tepi memakai nilai piksel terdekat.
        /// </summary>
        private static void BoxBlur(float[] data, int w, int h, int blur)
        {
            int radius = Math.Max(1, blur / 2);
            var temp = new float[data.Length];

            for (int pass = 0; pass < 3; pass++)
            {
                BlurLines(data, temp, w, h, radius, horizontal: true);
                BlurLines(temp, data, w, h, radius, horizontal: false);
            }
        }

        private static void BlurLines(float[] src, float[] dst, int w, int h, int radius, bool horizontal)
        {
            int lines = horizontal ? h : w;
            int length = horizontal ? w : h;
            int step = horizontal ? 1 : w;
            float scale = 1f / (radius * 2 + 1);

            for (int line = 0; line < lines; line++)
            {
                int start = horizontal ? line * w : line;
                float sum = 0;

                for (int k = -radius; k <= radius; k++)
                {
                    sum += src[start + Math.Max(0, Math.Min(length - 1, k)) * step];
                }

                for (int i = 0; i < length; i++)
                {
                    dst[start + i * step] = sum * scale;
                    int add = Math.Min(length - 1, i + radius + 1);
                    int remove = Math.Max(0, i - radius);
                    sum += src[start + add * step] - src[start + remove * step];
                }
            }
        }

        #endregion
    }
}
