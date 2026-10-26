using System;
using System.Drawing;
using System.Linq;
using PengolahanCitra.Imaging;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Alat "Deteksi tepi": grid metode, slider threshold Canny, Batal dan Terapkan
    /// </summary>
    public class EdgePanel : InspectorSection
    {
        private readonly NeuGallery<EdgeMethod> gallery;
        private readonly NeuSlider low;
        private readonly NeuSlider high;

        public EdgePanel()
        {
            gallery = new NeuGallery<EdgeMethod>(Enum.GetValues(typeof(EdgeMethod)).Cast<EdgeMethod>(), EdgeDetection.GetLabel);
            gallery.ItemClicked += (s, method) =>
            {
                SelectedMethod = method;
                gallery.Select(method);
                MethodSelected?.Invoke(this, method);
            };

            low = CreateThresholdSlider("Low", EdgeDetection.DefaultCannyLow);
            high = CreateThresholdSlider("High", EdgeDetection.DefaultCannyHigh);

            // Low tidak boleh melewati High: slider lain ikut bergeser
            low.ValueChanged += (s, e) => { if (low.Value > high.Value) high.Value = low.Value; };
            high.ValueChanged += (s, e) => { if (high.Value < low.Value) low.Value = high.Value; };

            NeuButton cancel = NeuLayout.Button("Batal", NeuTheme.Icons.Cancel);
            cancel.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);
            NeuButton apply = NeuLayout.Button("Terapkan", NeuTheme.Icons.Check, primary: true);
            apply.Click += (s, e) => ApplyClicked?.Invoke(this, EventArgs.Empty);

            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Deteksi tepi"),
                NeuLayout.Hint("Klik metode untuk melihat preview di kanvas.", 22),
                gallery,
                NeuLayout.Title("Threshold Canny"),
                NeuLayout.Spacer(4),
                low,
                high,
                NeuLayout.Spacer(4),
                NeuLayout.Row(cancel.Height, cancel, apply));
        }

        public event EventHandler<EdgeMethod> MethodSelected;

        /// <summary>Threshold Canny selesai diubah (mouse dilepas / tombol keyboard)</summary>
        public event EventHandler CannyThresholdsChanged;

        public event EventHandler ApplyClicked;
        public event EventHandler CancelClicked;

        public EdgeMethod? SelectedMethod { get; private set; }

        public int LowThreshold => low.Value;
        public int HighThreshold => high.Value;

        public void ClearSelection()
        {
            SelectedMethod = null;
            gallery.ClearSelection();
        }

        public void SetThumbnail(EdgeMethod method, Image thumbnail) => gallery.SetThumbnail(method, thumbnail);

        private NeuSlider CreateThresholdSlider(string label, int value)
        {
            var slider = new NeuSlider
            {
                Text = label,
                Minimum = 0,
                Maximum = 255,
                Value = value,
                SmallChange = 1,
                LargeChange = 10,
                Height = 56,
                AccessibleName = "Threshold " + label
            };
            slider.ValueCommitted += (s, e) => CannyThresholdsChanged?.Invoke(this, EventArgs.Empty);
            return slider;
        }
    }
}
