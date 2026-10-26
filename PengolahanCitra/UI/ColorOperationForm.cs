using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI
{
    public enum ColorOperation
    {
        ColorSlicing,
        PseudoColor
    }

    /// <summary>
    /// Popup setelah user mengklik gambar: pilih Color Slicing (berdasarkan warna yang diklik)
    /// atau Pewarnaan Semu
    /// </summary>
    public class ColorOperationForm : Form
    {
        private readonly TrackBar toleranceTrackBar;
        private readonly CheckBox grayBackgroundCheckBox;

        public ColorOperationForm(Color selectedColor)
        {
            SelectedColor = selectedColor;

            Text = "Pilih Operasi Warna";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 400);
            BackColor = Theme.Control;
            Padding = new Padding(20, 10, 20, 15);

            var swatch = new Panel
            {
                BackColor = selectedColor,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Left,
                Width = 60
            };
            var rgbLabel = new Label
            {
                Text = $"R: {selectedColor.R}  G: {selectedColor.G}  B: {selectedColor.B}",
                Font = new Font("Consolas", 10F),
                ForeColor = Theme.Text,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var colorRow = new Panel { Dock = DockStyle.Top, Height = 60 };
            colorRow.Controls.Add(rgbLabel);
            colorRow.Controls.Add(swatch);

            var toleranceValue = new Label
            {
                Text = "50",
                Dock = DockStyle.Right,
                Width = 40,
                Font = Theme.SectionFont,
                ForeColor = Theme.Accent,
                TextAlign = ContentAlignment.BottomRight
            };
            Label toleranceLabel = Theme.CreateSectionLabel("Toleransi warna");
            toleranceLabel.Controls.Add(toleranceValue);

            toleranceTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 200,
                Value = 50,
                TickFrequency = 20,
                Dock = DockStyle.Top
            };
            toleranceTrackBar.ValueChanged += (s, e) => toleranceValue.Text = toleranceTrackBar.Value.ToString();

            grayBackgroundCheckBox = new CheckBox
            {
                Text = "Background grayscale (bukan hitam)",
                ForeColor = Theme.Text,
                Font = Theme.NormalFont,
                Dock = DockStyle.Top,
                Height = 26
            };

            Button cancel = Theme.CreateButton("✕ Batal", Theme.ControlLight, null, 30);
            cancel.DialogResult = DialogResult.Cancel;
            CancelButton = cancel;

            Theme.StackTopDown(this,
                Theme.CreateTitle("Pilih Operasi Warna"),
                Theme.CreateSectionLabel("Warna yang dipilih"),
                colorRow,
                toleranceLabel,
                toleranceTrackBar,
                grayBackgroundCheckBox,
                Theme.CreateButton("Color Slicing", Theme.Accent, (s, e) => Choose(ColorOperation.ColorSlicing), 40),
                Theme.CreateSectionLabel("Tidak bergantung warna yang dipilih:"),
                Theme.CreateButton("Pewarnaan Semu", Theme.AccentDark, (s, e) => Choose(ColorOperation.PseudoColor), 36),
                new Panel { Dock = DockStyle.Top, Height = 10 },
                cancel);
        }

        public Color SelectedColor { get; }
        public ColorOperation Operation { get; private set; }
        public int Tolerance => toleranceTrackBar.Value;
        public bool UseGrayBackground => grayBackgroundCheckBox.Checked;

        private void Choose(ColorOperation operation)
        {
            Operation = operation;
            DialogResult = DialogResult.OK;
        }
    }
}
