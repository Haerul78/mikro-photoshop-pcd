using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Panel kanan: pilihan filter (thumbnail), brightness, dan tombol Apply
    /// </summary>
    public class FilterPanel : UserControl
    {
        private readonly ThumbnailGallery<FilterKind> gallery;
        private readonly TrackBar brightnessTrackBar;
        private readonly Label brightnessValueLabel;
        private bool suppressEvents;

        public FilterPanel()
        {
            BackColor = Theme.Sidebar;
            Padding = new Padding(8);

            gallery = new ThumbnailGallery<FilterKind>(Enum.GetValues(typeof(FilterKind)).Cast<FilterKind>(), k => k.ToString())
            {
                Dock = DockStyle.Fill
            };
            gallery.ItemClicked += (s, kind) => SelectFilter(kind, raiseEvent: true);

            brightnessValueLabel = new Label
            {
                Text = "0",
                Dock = DockStyle.Right,
                Width = 40,
                Font = Theme.SmallFont,
                ForeColor = Theme.TextMuted,
                TextAlign = ContentAlignment.BottomRight
            };
            Label brightnessLabel = Theme.CreateSectionLabel("Brightness");
            brightnessLabel.Controls.Add(brightnessValueLabel);

            brightnessTrackBar = new TrackBar
            {
                Minimum = -100,
                Maximum = 100,
                TickFrequency = 10,
                SmallChange = 1,
                LargeChange = 10,
                Dock = DockStyle.Top
            };
            brightnessTrackBar.ValueChanged += OnBrightnessValueChanged;

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 160 };
            Theme.StackTopDown(bottom,
                brightnessLabel,
                brightnessTrackBar,
                Theme.CreateButton("Reset Brightness", Theme.ControlLight, (s, e) => Brightness = 0, 26),
                new Panel { Dock = DockStyle.Top, Height = 8 },
                Theme.CreateButton("✓ Apply Filter", Theme.Accent, (s, e) => ApplyClicked?.Invoke(this, EventArgs.Empty), 36));

            Controls.Add(gallery);
            Controls.Add(bottom);
            Controls.Add(Theme.CreateTitle("Filter Preview"));

            SelectFilter(FilterKind.Original, raiseEvent: false);
        }

        public event EventHandler<FilterKind> FilterSelected;
        public event EventHandler BrightnessChanged;
        public event EventHandler ApplyClicked;

        public FilterKind SelectedFilter { get; private set; }

        public int Brightness
        {
            get => brightnessTrackBar.Value;
            set => brightnessTrackBar.Value = value;
        }

        /// <summary>
        /// Kembali ke Original dan brightness 0 tanpa memicu event
        /// </summary>
        public void ResetSelection()
        {
            suppressEvents = true;
            try
            {
                SelectFilter(FilterKind.Original, raiseEvent: false);
                Brightness = 0;
            }
            finally
            {
                suppressEvents = false;
            }
        }

        public void SetThumbnail(FilterKind kind, Image thumbnail) => gallery.SetThumbnail(kind, thumbnail);

        public void ClearThumbnails() => gallery.ClearThumbnails();

        private void SelectFilter(FilterKind kind, bool raiseEvent)
        {
            SelectedFilter = kind;
            gallery.Select(kind);
            if (raiseEvent) FilterSelected?.Invoke(this, kind);
        }

        private void OnBrightnessValueChanged(object sender, EventArgs e)
        {
            brightnessValueLabel.Text = brightnessTrackBar.Value.ToString();
            if (!suppressEvents) BrightnessChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
