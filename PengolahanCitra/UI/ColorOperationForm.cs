using System.Drawing;
using System.Windows.Forms;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    public enum ColorOperation
    {
        ColorSlicing,
        PseudoColor,
        JetColorMap
    }

    /// <summary>
    /// Popup setelah user mengklik gambar: Color Slicing dan Pewarnaan Semu berdasarkan warna yang diklik,
    /// atau Peta Warna (jet) untuk seluruh gambar
    /// </summary>
    public class ColorOperationForm : Form
    {
        private readonly NeuSlider tolerance;
        private readonly NeuToggle grayBackground;

        public ColorOperationForm(Color selectedColor)
        {
            SelectedColor = selectedColor;

            Text = "Operasi warna";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = NeuTheme.Surface;
            ForeColor = NeuTheme.Ink;
            Font = NeuTheme.LabelFont;
            ClientSize = new Size(340, 692);
            Padding = new Padding(18, 8, 18, 12);

            // Contoh warna dalam bidang cekung, nilai RGB di sebelahnya
            var swatchFrame = new NeuPanel { Style = NeuStyle.Inset, Radius = NeuTheme.RadiusMedium, Depth = NeuTheme.DepthInset, Dock = DockStyle.Left, Width = 76, Padding = new Padding(9) };
            swatchFrame.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = selectedColor });
            var rgb = new Label
            {
                Text = $"R {selectedColor.R}   G {selectedColor.G}   B {selectedColor.B}",
                Font = NeuTheme.ValueFont,
                ForeColor = NeuTheme.Ink,
                Dock = DockStyle.Fill,
                Padding = new Padding(14, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var colorRow = new Panel { Height = 76, BackColor = NeuTheme.Surface };
            colorRow.Controls.Add(rgb);
            colorRow.Controls.Add(swatchFrame);

            tolerance = new NeuSlider { Text = "Toleransi warna", Minimum = 1, Maximum = 200, Value = 50, Height = 56, AccessibleName = "Toleransi warna" };
            grayBackground = new NeuToggle { Text = "Background abu-abu (bukan hitam)", Height = 40 };

            NeuButton slicing = NeuLayout.Button("Terapkan color slicing", NeuTheme.Icons.Check, primary: true);
            slicing.Click += (s, e) => Choose(ColorOperation.ColorSlicing);

            NeuButton pseudo = NeuLayout.Button("Terapkan pewarnaan semu", NeuTheme.Icons.Adjust);
            pseudo.Click += (s, e) => Choose(ColorOperation.PseudoColor);

            NeuButton colorMap = NeuLayout.Button("Terapkan peta warna");
            colorMap.Click += (s, e) => Choose(ColorOperation.JetColorMap);

            NeuButton cancel = NeuLayout.Button("Batal");
            cancel.DialogResult = DialogResult.Cancel;
            CancelButton = cancel;

            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Warna yang diklik"),
                NeuLayout.Spacer(6),
                colorRow,
                tolerance,
                NeuLayout.Hint("Dipakai oleh color slicing dan pewarnaan semu.", 22),
                NeuLayout.Title("Color slicing"),
                NeuLayout.Hint("Pertahankan warna yang mirip dengan warna di atas; warna lain dihilangkan.", 34),
                grayBackground,
                slicing,
                NeuLayout.Title("Pewarnaan semu"),
                NeuLayout.Hint("Warna yang mirip diwarnai dengan warna di atas sesuai tingkat terangnya; sisanya abu-abu.", 34),
                pseudo,
                NeuLayout.Title("Peta warna"),
                NeuLayout.Hint("Seluruh gambar diubah ke gradasi biru, hijau, kuning, merah.", 34),
                colorMap,
                cancel);

            AcceptButton = slicing;
        }

        public Color SelectedColor { get; }
        public ColorOperation Operation { get; private set; }
        public int Tolerance => tolerance.Value;
        public bool UseGrayBackground => grayBackground.Checked;

        private void Choose(ColorOperation operation)
        {
            Operation = operation;
            DialogResult = DialogResult.OK;
        }
    }
}
