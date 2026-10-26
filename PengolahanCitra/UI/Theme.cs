using System;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Warna dan pembuat kontrol bergaya gelap yang dipakai semua panel
    /// </summary>
    public static class Theme
    {
        public static readonly Color Background = Color.FromArgb(30, 30, 30);
        public static readonly Color Sidebar = Color.FromArgb(37, 37, 38);
        public static readonly Color Canvas = Color.FromArgb(28, 28, 28);
        public static readonly Color Control = Color.FromArgb(45, 45, 48);
        public static readonly Color ControlLight = Color.FromArgb(63, 63, 70);
        public static readonly Color Accent = Color.FromArgb(138, 43, 226);
        public static readonly Color AccentDark = Color.FromArgb(75, 0, 130);
        public static readonly Color Text = Color.White;
        public static readonly Color TextMuted = Color.LightGray;

        public static readonly Font TitleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font SectionFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        public static readonly Font NormalFont = new Font("Segoe UI", 9F);
        public static readonly Font SmallFont = new Font("Segoe UI", 8F);

        public static Button CreateButton(string text, Color backColor, EventHandler onClick, int height = 34)
        {
            var button = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Text,
                FlatStyle = FlatStyle.Flat,
                Font = NormalFont,
                Height = height,
                Dock = DockStyle.Top,
                Cursor = Cursors.Hand,
                UseMnemonic = false,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 0, 4)
            };
            button.FlatAppearance.BorderSize = 0;
            if (onClick != null) button.Click += onClick;
            return button;
        }

        public static Label CreateTitle(string text) => new Label
        {
            Text = text,
            Font = TitleFont,
            ForeColor = Text,
            UseMnemonic = false,
            Dock = DockStyle.Top,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter
        };

        public static Label CreateSectionLabel(string text) => new Label
        {
            Text = text,
            Font = SectionFont,
            ForeColor = Text,
            UseMnemonic = false,
            Dock = DockStyle.Top,
            Height = 26,
            TextAlign = ContentAlignment.BottomLeft
        };

        public static NumericUpDown CreateNumeric(int min, int max, int value) => new NumericUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            BackColor = Control,
            ForeColor = Text,
            TextAlign = HorizontalAlignment.Center,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 4, 2)
        };

        /// <summary>
        /// Baris berisi beberapa kontrol dengan lebar sama
        /// </summary>
        public static TableLayoutPanel CreateRow(params Control[] controls)
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = controls.Length,
                RowCount = 1,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = Padding.Empty
            };

            for (int i = 0; i < controls.Length; i++)
            {
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / controls.Length));
                controls[i].Dock = DockStyle.Fill;
                controls[i].Margin = new Padding(0, 2, i < controls.Length - 1 ? 4 : 0, 2);
                row.Controls.Add(controls[i]);
            }

            return row;
        }

        /// <summary>
        /// Tambahkan kontrol ke container ber-Dock Top sesuai urutan baca (atas → bawah).
        /// WinForms menata Dock dari indeks terakhir, jadi urutannya dibalik di sini.
        /// Kontrol ber-Dock mengabaikan Margin, jadi Margin.Bottom diganti panel pengisi.
        /// </summary>
        public static void StackTopDown(Control container, params Control[] controls)
        {
            for (int i = controls.Length - 1; i >= 0; i--)
            {
                if (controls[i].Margin.Bottom > 0)
                {
                    container.Controls.Add(new Panel { Dock = DockStyle.Top, Height = controls[i].Margin.Bottom });
                }
                container.Controls.Add(controls[i]);
            }
        }
    }
}
