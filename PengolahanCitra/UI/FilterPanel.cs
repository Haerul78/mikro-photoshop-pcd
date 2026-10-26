using System;
using System.Drawing;
using System.Linq;
using PengolahanCitra.Imaging;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Alat "Penyesuaian &amp; filter": slider brightness, grid thumbnail filter, Batal dan Terapkan
    /// </summary>
    public class FilterPanel : InspectorSection
    {
        private readonly NeuGallery<FilterKind> gallery;
        private readonly NeuSlider brightness;
        private bool suppressEvents;

        public FilterPanel()
        {
            brightness = new NeuSlider
            {
                Text = "Brightness",
                Minimum = -100,
                Maximum = 100,
                Origin = 0,
                SmallChange = 1,
                LargeChange = 10,
                Height = 56,
                AccessibleName = "Brightness",
                ValueFormatter = v => v > 0 ? "+" + v : v.ToString()
            };
            brightness.ValueChanged += (s, e) =>
            {
                if (!suppressEvents) BrightnessChanged?.Invoke(this, EventArgs.Empty);
            };

            gallery = new NeuGallery<FilterKind>(Enum.GetValues(typeof(FilterKind)).Cast<FilterKind>(), k => k.ToString());
            gallery.ItemClicked += (s, kind) => SelectFilter(kind, raiseEvent: true);

            NeuButton cancel = NeuLayout.Button("Batal", NeuTheme.Icons.Cancel);
            cancel.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);
            NeuButton apply = NeuLayout.Button("Terapkan", NeuTheme.Icons.Check, primary: true);
            apply.Click += (s, e) => ApplyClicked?.Invoke(this, EventArgs.Empty);

            NeuLayout.StackTopDown(this,
                NeuLayout.Title("Penyesuaian"),
                NeuLayout.Spacer(4),
                brightness,
                NeuLayout.Title("Filter"),
                NeuLayout.Hint("Klik thumbnail untuk melihat preview di kanvas.", 22),
                gallery,
                NeuLayout.Spacer(4),
                NeuLayout.Row(cancel.Height, cancel, apply));

            SelectFilter(FilterKind.Original, raiseEvent: false);
        }

        public event EventHandler<FilterKind> FilterSelected;
        public event EventHandler BrightnessChanged;
        public event EventHandler ApplyClicked;
        public event EventHandler CancelClicked;

        public FilterKind SelectedFilter { get; private set; }

        public int Brightness
        {
            get => brightness.Value;
            set => brightness.Value = value;
        }

        /// <summary>Kembali ke Original dan brightness 0 tanpa memicu event</summary>
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

        private void SelectFilter(FilterKind kind, bool raiseEvent)
        {
            SelectedFilter = kind;
            gallery.Select(kind);
            if (raiseEvent) FilterSelected?.Invoke(this, kind);
        }
    }
}
