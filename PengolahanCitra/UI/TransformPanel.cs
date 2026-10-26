using System;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Panel kanan: aritmatika citra, rotasi, translasi, dan flip
    /// </summary>
    public class TransformPanel : UserControl
    {
        private readonly NumericUpDown customAngle;
        private readonly NumericUpDown offsetX;
        private readonly NumericUpDown offsetY;

        public TransformPanel()
        {
            BackColor = Theme.Sidebar;
            Padding = new Padding(8);
            AutoScroll = true;

            customAngle = Theme.CreateNumeric(-360, 360, 0);
            offsetX = Theme.CreateNumeric(-10000, 10000, 0);
            offsetY = Theme.CreateNumeric(-10000, 10000, 0);

            Theme.StackTopDown(this,
                Theme.CreateTitle("Aritmatika & Geometri"),

                Theme.CreateSectionLabel("Aritmatika Citra"),
                ArithmeticButton("Tambah Citra", ArithmeticOperation.Add),
                ArithmeticButton("Kurangi Citra", ArithmeticOperation.Subtract),
                ArithmeticButton("Kalikan Citra", ArithmeticOperation.Multiply),
                ArithmeticButton("Bagi Citra", ArithmeticOperation.Divide),

                Theme.CreateSectionLabel("Rotasi (searah jarum jam)"),
                Theme.CreateRow(RotateButton(-90), RotateButton(45), RotateButton(90), RotateButton(180)),
                Theme.CreateRow(customAngle, Theme.CreateButton("Putar", Theme.Accent,
                    (s, e) => RotateRequested?.Invoke(this, (int)customAngle.Value), 26)),

                Theme.CreateSectionLabel("Translasi (X, Y)"),
                Theme.CreateRow(offsetX, offsetY),
                Theme.CreateButton("Geser", Theme.Accent,
                    (s, e) => TranslateRequested?.Invoke(this, new System.Drawing.Point((int)offsetX.Value, (int)offsetY.Value))),

                Theme.CreateSectionLabel("Flip"),
                Theme.CreateRow(
                    Theme.CreateButton("↔ Horizontal", Theme.Accent, (s, e) => FlipRequested?.Invoke(this, true)),
                    Theme.CreateButton("↕ Vertikal", Theme.Accent, (s, e) => FlipRequested?.Invoke(this, false))));
        }

        public event EventHandler<ArithmeticOperation> ArithmeticRequested;

        /// <summary>Argumen: sudut tambahan (derajat, searah jarum jam)</summary>
        public event EventHandler<int> RotateRequested;

        public event EventHandler<System.Drawing.Point> TranslateRequested;

        /// <summary>Argumen: true = horizontal, false = vertikal</summary>
        public event EventHandler<bool> FlipRequested;

        private Button ArithmeticButton(string text, ArithmeticOperation operation) =>
            Theme.CreateButton(text, Theme.AccentDark, (s, e) => ArithmeticRequested?.Invoke(this, operation));

        private Button RotateButton(int angle)
        {
            Button button = Theme.CreateButton($"{angle}°", Theme.ControlLight, (s, e) => RotateRequested?.Invoke(this, angle), 26);
            button.Margin = new Padding(0, 2, 3, 2);
            return button;
        }
    }
}
