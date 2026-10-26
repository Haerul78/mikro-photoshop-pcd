using System;
using System.Drawing;
using System.Windows.Forms;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Alat "Geometri": rotasi, translasi, dan flip
    /// </summary>
    public class GeometryPanel : InspectorSection
    {
        private readonly NeuNumberBox angle;
        private readonly NeuNumberBox offsetX;
        private readonly NeuNumberBox offsetY;

        public GeometryPanel()
        {
            angle = new NeuNumberBox { Minimum = -360, Maximum = 360, Value = 0, Height = 36, Text = "Sudut rotasi" };
            offsetX = new NeuNumberBox { Minimum = -10000, Maximum = 10000, Value = 0, Height = 36, Text = "Geser X" };
            offsetY = new NeuNumberBox { Minimum = -10000, Maximum = 10000, Value = 0, Height = 36, Text = "Geser Y" };

            NeuButton rotate = NeuLayout.Button("Putar");
            rotate.Click += (s, e) => RotateRequested?.Invoke(this, angle.Value);

            NeuButton translate = NeuLayout.Button("Geser", NeuTheme.Icons.Move);
            translate.Click += (s, e) => TranslateRequested?.Invoke(this, new Point(offsetX.Value, offsetY.Value));

            NeuButton flipH = NeuLayout.Button("Horizontal", NeuTheme.Icons.FlipH);
            flipH.Click += (s, e) => FlipRequested?.Invoke(this, true);
            NeuButton flipV = NeuLayout.Button("Vertikal", NeuTheme.Icons.FlipV);
            flipV.Click += (s, e) => FlipRequested?.Invoke(this, false);

            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Rotasi"),
                NeuLayout.Hint("Searah jarum jam. Rotasi berulang dihitung dari gambar sebelum rotasi pertama.", 34),
                NeuLayout.Row(rotate.Height, QuickRotate(-90), QuickRotate(45), QuickRotate(90), QuickRotate(180)),
                NeuLayout.FieldLabel("Sudut lain (derajat)"),
                NeuLayout.Row(rotate.Height, Inset(angle), rotate),

                NeuLayout.Title("Translasi"),
                NeuLayout.Row(18, NeuLayout.FieldLabel("X (piksel)"), NeuLayout.FieldLabel("Y (piksel)")),
                NeuLayout.Row(52, Inset(offsetX), Inset(offsetY)),
                translate,

                NeuLayout.Title("Flip"),
                NeuLayout.Row(flipH.Height, flipH, flipV));
        }

        /// <summary>Argumen: sudut tambahan (derajat, searah jarum jam)</summary>
        public event EventHandler<int> RotateRequested;

        public event EventHandler<Point> TranslateRequested;

        /// <summary>Argumen: true = horizontal, false = vertikal</summary>
        public event EventHandler<bool> FlipRequested;

        private NeuButton QuickRotate(int degrees)
        {
            NeuButton button = NeuLayout.Button($"{degrees}°");
            button.AccessibleName = $"Putar {degrees} derajat";
            button.Click += (s, e) => RotateRequested?.Invoke(this, degrees);
            return button;
        }

        /// <summary>Beri ruang di sekitar input agar sejajar dengan tombol timbul di sebelahnya</summary>
        private static Control Inset(NeuNumberBox box)
        {
            var host = new Panel
            {
                BackColor = NeuTheme.Surface,
                Padding = new Padding(8)
            };
            box.Dock = DockStyle.Fill;
            host.Controls.Add(box);
            return host;
        }
    }
}
