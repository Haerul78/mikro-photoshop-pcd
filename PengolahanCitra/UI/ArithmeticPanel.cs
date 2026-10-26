using System;
using PengolahanCitra.Imaging;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Alat "Aritmatika citra": operasi gambar ini dengan gambar kedua
    /// </summary>
    public class ArithmeticPanel : InspectorSection
    {
        public ArithmeticPanel()
        {
            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Aritmatika citra"),
                NeuLayout.Hint("Pilih gambar kedua. Ukurannya otomatis disesuaikan dengan gambar ini.", 34),
                OperationButton("Tambah citra", NeuTheme.Icons.Add, ArithmeticOperation.Add),
                OperationButton("Kurangi citra", NeuTheme.Icons.Subtract, ArithmeticOperation.Subtract),
                OperationButton("Kalikan citra", NeuTheme.Icons.Multiply, ArithmeticOperation.Multiply),
                OperationButton("Bagi citra", NeuTheme.Icons.Divide, ArithmeticOperation.Divide));
        }

        public event EventHandler<ArithmeticOperation> ArithmeticRequested;

        private NeuButton OperationButton(string text, string icon, ArithmeticOperation operation)
        {
            NeuButton button = NeuLayout.Button(text, icon);
            button.Click += (s, e) => ArithmeticRequested?.Invoke(this, operation);
            return button;
        }
    }
}
