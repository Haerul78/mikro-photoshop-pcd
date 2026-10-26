using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Panel kanan: pilihan metode deteksi tepi, threshold Canny, dan tombol Terapkan
    /// </summary>
    public class EdgePanel : UserControl
    {
        private readonly ThumbnailGallery<EdgeMethod> gallery;
        private readonly NumericUpDown lowThreshold;
        private readonly NumericUpDown highThreshold;

        public EdgePanel()
        {
            BackColor = Theme.Sidebar;
            Padding = new Padding(8);

            gallery = new ThumbnailGallery<EdgeMethod>(Enum.GetValues(typeof(EdgeMethod)).Cast<EdgeMethod>(), EdgeDetection.GetLabel);
            gallery.Height = gallery.PreferredHeightFor(230);
            gallery.ItemClicked += (s, method) =>
            {
                SelectedMethod = method;
                gallery.Select(method);
                MethodSelected?.Invoke(this, method);
            };

            lowThreshold = Theme.CreateNumeric(0, 255, EdgeDetection.DefaultCannyLow);
            highThreshold = Theme.CreateNumeric(0, 255, EdgeDetection.DefaultCannyHigh);

            var labels = Theme.CreateRow(CreateSmallLabel("Low"), CreateSmallLabel("High"));
            labels.Height = 24;

            Theme.StackTopDown(this,
                Theme.CreateTitle("Deteksi Tepi"),
                gallery,
                Theme.CreateSectionLabel("Threshold Canny"),
                labels,
                Theme.CreateRow(lowThreshold, highThreshold),
                Theme.CreateButton("Refresh Canny", Theme.Accent, (s, e) => RefreshCannyClicked?.Invoke(this, EventArgs.Empty), 28),
                new Panel { Dock = DockStyle.Top, Height = 16 },
                Theme.CreateButton("✓ Terapkan", Theme.Accent, (s, e) => ApplyClicked?.Invoke(this, EventArgs.Empty), 36));
        }

        public event EventHandler<EdgeMethod> MethodSelected;
        public event EventHandler RefreshCannyClicked;
        public event EventHandler ApplyClicked;

        public EdgeMethod? SelectedMethod { get; private set; }

        public int LowThreshold => (int)lowThreshold.Value;
        public int HighThreshold => (int)highThreshold.Value;

        public void ClearSelection()
        {
            SelectedMethod = null;
            gallery.ClearSelection();
        }

        public void SetThumbnail(EdgeMethod method, Image thumbnail) => gallery.SetThumbnail(method, thumbnail);

        public void ClearThumbnails() => gallery.ClearThumbnails();

        private static Label CreateSmallLabel(string text) => new Label
        {
            Text = text,
            Font = Theme.SmallFont,
            ForeColor = Theme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter
        };
    }
}
