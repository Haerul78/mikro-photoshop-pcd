using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Helper menyusun kontrol di panel inspector (kolom kanan) secara vertikal
    /// </summary>
    public static class NeuLayout
    {
        /// <summary>
        /// Tambahkan kontrol ber-Dock Top sesuai urutan baca (atas → bawah).
        /// WinForms menata Dock dari indeks terakhir, jadi urutannya dibalik di sini.
        /// </summary>
        public static void StackTopDown(Control container, params Control[] controls)
        {
            for (int i = controls.Length - 1; i >= 0; i--)
            {
                controls[i].Dock = DockStyle.Top;
                container.Controls.Add(controls[i]);
            }
        }

        /// <summary>Judul bagian (mis. "Histogram", "Filter")</summary>
        public static Label Title(string text) => new Label
        {
            Text = text,
            Font = NeuTheme.TitleFont,
            ForeColor = NeuTheme.Ink,
            BackColor = NeuTheme.Surface,
            Height = 34,
            Padding = new Padding(4, 0, 0, 0),
            TextAlign = ContentAlignment.BottomLeft,
            UseMnemonic = false
        };

        /// <summary>Teks penjelas kecil di bawah judul</summary>
        public static Label Hint(string text, int height = 36) => new Label
        {
            Text = text,
            Font = NeuTheme.SmallFont,
            ForeColor = NeuTheme.InkMuted,
            BackColor = NeuTheme.Surface,
            Height = height,
            Padding = new Padding(4, 2, 4, 0),
            UseMnemonic = false
        };

        public static Control Spacer(int height) => new Panel { Height = height, BackColor = NeuTheme.Surface };

        /// <summary>Beberapa kontrol dengan lebar sama dalam satu baris</summary>
        public static TableLayoutPanel Row(int height, params Control[] controls)
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = controls.Length,
                RowCount = 1,
                Height = height,
                BackColor = NeuTheme.Surface,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            for (int i = 0; i < controls.Length; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / controls.Length));
                controls[i].Dock = DockStyle.Fill;
                controls[i].Margin = Padding.Empty;
                row.Controls.Add(controls[i], i, 0);
            }

            return row;
        }

        /// <summary>Tombol teks (+ ikon opsional) dengan tinggi visual ±36 px</summary>
        public static NeuButton Button(string text, string icon = null, bool primary = false) => new NeuButton
        {
            Text = text,
            Icon = icon,
            Primary = primary,
            Height = 36 + NeuTheme.ShadowMargin(NeuTheme.DepthButton) * 2
        };

        /// <summary>Label kecil di atas sebuah input</summary>
        public static Label FieldLabel(string text) => new Label
        {
            Text = text,
            Font = NeuTheme.SmallFont,
            ForeColor = NeuTheme.InkMuted,
            BackColor = NeuTheme.Surface,
            Height = 18,
            Padding = new Padding(6, 0, 0, 0),
            TextAlign = ContentAlignment.BottomLeft,
            UseMnemonic = false
        };
    }
}
