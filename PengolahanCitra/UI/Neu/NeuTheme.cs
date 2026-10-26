using System.Drawing;
using System.Drawing.Text;
using System.Linq;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Token design system neumorphism: warna, radius, kedalaman shadow, font, dan ikon.
    /// Semua permukaan memakai satu warna (Surface); bentuk dibedakan hanya lewat shadow.
    /// </summary>
    public static class NeuTheme
    {
        #region Warna

        /// <summary>Background dan permukaan semua kontrol</summary>
        public static readonly Color Surface = Color.FromArgb(0xE0, 0xE5, 0xEC);

        /// <summary>Shadow terang (cahaya dari kiri-atas)</summary>
        public static readonly Color Highlight = Color.FromArgb(0xFF, 0xFF, 0xFF);

        /// <summary>Shadow gelap (kanan-bawah)</summary>
        public static readonly Color Shade = Color.FromArgb(0xA3, 0xB1, 0xC6);

        /// <summary>Satu-satunya warna aksen: state aktif, isi slider, fokus</summary>
        public static readonly Color Accent = Color.FromArgb(0x4A, 0x7B, 0xF7);

        /// <summary>Aksen yang lebih gelap untuk teks kecil (kontras ±4,6:1 terhadap Surface)</summary>
        public static readonly Color AccentInk = Color.FromArgb(0x2C, 0x5B, 0xD6);

        /// <summary>Teks utama (kontras ±11:1)</summary>
        public static readonly Color Ink = Color.FromArgb(0x2E, 0x34, 0x40);

        /// <summary>Label sekunder (kontras ±4,6:1, lolos WCAG AA)</summary>
        public static readonly Color InkMuted = Color.FromArgb(0x5B, 0x65, 0x77);

        /// <summary>Teks/ikon kontrol yang dinonaktifkan</summary>
        public static readonly Color InkDisabled = Color.FromArgb(0x9A, 0xA4, 0xB4);

        #endregion

        #region Bentuk

        public const int RadiusLarge = 20;    // panel, kanvas
        public const int RadiusMedium = 14;   // tombol, tile
        public const int RadiusSmall = 10;    // input, segmen

        /// <summary>Jarak shadow dari bentuk (px)</summary>
        public const int DepthRaised = 5;
        public const int DepthButton = 3;
        public const int DepthInset = 4;
        public const int DepthSmall = 2;

        /// <summary>Ruang di sekeliling bentuk agar shadow timbul tidak terpotong</summary>
        public static int ShadowMargin(int depth) => depth * 3 + 2;

        #endregion

        #region Font & Ikon

        private static readonly string[] InstalledFonts =
            new InstalledFontCollection().Families.Select(f => f.Name).ToArray();

        private static string Pick(params string[] names) =>
            names.FirstOrDefault(n => InstalledFonts.Contains(n)) ?? "Microsoft Sans Serif";

        private static readonly string TextFamily = Pick("Segoe UI Variable Text", "Segoe UI");
        private static readonly string SemiboldFamily = Pick("Segoe UI Semibold", "Segoe UI");
        private static readonly string IconFamily = Pick("Segoe Fluent Icons", "Segoe MDL2 Assets");

        /// <summary>Judul panel</summary>
        public static readonly Font TitleFont = new Font(SemiboldFamily, 11f);

        /// <summary>Label dan teks tombol</summary>
        public static readonly Font LabelFont = new Font(TextFamily, 9f);

        /// <summary>Teks kecil (caption thumbnail, status bar)</summary>
        public static readonly Font SmallFont = new Font(TextFamily, 8.25f);

        /// <summary>Nilai angka (slider, ukuran gambar)</summary>
        public static readonly Font ValueFont = new Font(SemiboldFamily, 9f);

        public static readonly Font IconFont = new Font(IconFamily, 12f);
        public static readonly Font IconFontSmall = new Font(IconFamily, 10f);

        #endregion

        /// <summary>
        /// Kode glyph Segoe Fluent Icons / MDL2 Assets yang dipakai aplikasi
        /// </summary>
        public static class Icons
        {
            public const string Open = "";        // OpenFolderHorizontal
            public const string Save = "";        // Save
            public const string Export = "";      // Document
            public const string Undo = "";
            public const string Redo = "";
            public const string Reset = "";       // Refresh
            public const string Compare = "";     // ClosePane (dua panel)
            public const string Adjust = "";      // Color
            public const string Edge = "";        // Edit (garis)
            public const string Geometry = "";    // Rotate
            public const string Arithmetic = "";  // Calculator
            public const string Add = "";
            public const string Subtract = "";
            public const string Multiply = "";    // Cancel (x)
            public const string Divide = "";      // Divider
            public const string FlipH = "";       // Switch
            public const string FlipV = "";       // Sort
            public const string Move = "";        // Move
            public const string Check = "";
            public const string Cancel = "";
            public const string Up = "";
            public const string Down = "";
        }
    }
}
