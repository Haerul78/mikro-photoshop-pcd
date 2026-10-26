using PengolahanCitra.Imaging;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Histogram gambar yang sedang tampil (termasuk preview), dengan pilihan RGB atau luminance
    /// </summary>
    public class HistogramPanel : InspectorSection
    {
        private readonly HistogramView view;
        private readonly NeuSegmented modeSwitch;

        public HistogramPanel()
        {
            view = new HistogramView { Height = 132 };

            modeSwitch = new NeuSegmented { Height = 38, AccessibleName = "Mode histogram" };
            modeSwitch.Items = new[] { "RGB", "Luminance" };
            modeSwitch.SelectedIndexChanged += (s, e) =>
                view.Mode = modeSwitch.SelectedIndex == 0 ? HistogramMode.Rgb : HistogramMode.Luminance;

            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Histogram"),
                NeuLayout.Spacer(6),
                view,
                NeuLayout.Spacer(8),
                modeSwitch);
        }

        public void ShowHistogram(Histogram histogram) => view.Histogram = histogram;
    }
}
