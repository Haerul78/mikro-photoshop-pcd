using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra
{
    /// <summary>
    /// Alat Penyesuaian &amp; filter: thumbnail, preview ukuran penuh, brightness, Batal, dan Terapkan
    /// </summary>
    public partial class MainForm
    {
        // Hasil filter terpilih tanpa brightness (cache, supaya slider brightness tidak menghitung ulang filter)
        private FilterPreview filterPreview;

        private readonly Timer brightnessTimer = new Timer { Interval = 80 };

        private sealed class FilterPreview
        {
            public FilterKind Kind;
            public RgbImage Image;
        }

        private void InitializeFilterPanel()
        {
            filterPanel.FilterSelected += (s, kind) => Fire(SelectFilterAsync(kind));
            filterPanel.BrightnessChanged += (s, e) => { brightnessTimer.Stop(); brightnessTimer.Start(); };
            filterPanel.ApplyClicked += (s, e) => ApplyFilter();
            filterPanel.CancelClicked += (s, e) => CancelPreview();

            // Debounce: preview brightness baru dihitung setelah slider berhenti sejenak
            brightnessTimer.Tick += (s, e) => { brightnessTimer.Stop(); Fire(UpdateBrightnessPreviewAsync()); };
        }

        private void PrepareFilterPanel()
        {
            brightnessTimer.Stop();
            filterPanel.ResetSelection();

            RgbImage thumbnail = thumbnailSource;
            if (thumbnail == null) return;

            double sizeRatio = (double)thumbnail.Width / document.Current.Width;

            foreach (FilterKind kind in Enum.GetValues(typeof(FilterKind)))
            {
                // Tile memiliki bitmap ini dan men-dispose-nya saat diganti
                filterPanel.SetThumbnail(kind, FilterCatalog.Apply(thumbnail, kind, sizeRatio).ToBitmap());
            }
        }

        internal async Task SelectFilterAsync(FilterKind kind)
        {
            if (isBusy || activeTool != Tool.Adjust) return;

            int requestId = ++previewRequestId;
            RgbImage source = document.Current;
            int brightness = filterPanel.Brightness;
            RgbImage filtered = null;

            PreviewResult preview = await RunBusyAsync($"Menerapkan {kind}...", () =>
            {
                filtered = FilterCatalog.Apply(source, kind);
                return PreviewResult.Create(WithBrightness(filtered, brightness));
            });
            if (!TryShowPreview(requestId, Tool.Adjust, preview)) return;

            filterPreview = new FilterPreview { Kind = kind, Image = filtered };
            SetStatus($"Preview {kind} — klik Terapkan untuk menerapkan, atau Esc untuk batal");
        }

        private async Task UpdateBrightnessPreviewAsync()
        {
            if (activeTool != Tool.Adjust || isBusy || !document.IsLoaded) return;

            int requestId = ++previewRequestId;
            RgbImage source = CurrentFilterResult() ?? document.Current;
            int brightness = filterPanel.Brightness;

            PreviewResult preview;
            try
            {
                preview = await Task.Run(() => PreviewResult.Create(WithBrightness(source, brightness)));
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }

            TryShowPreview(requestId, Tool.Adjust, preview);
        }

        private void ApplyFilter()
        {
            FilterKind kind = filterPanel.SelectedFilter;
            int brightness = filterPanel.Brightness;

            if (kind == FilterKind.Original && brightness == 0)
            {
                ShowWarning("Pilih filter atau ubah brightness terlebih dahulu!");
                return;
            }

            brightnessTimer.Stop();
            RgbImage cached = CurrentFilterResult();
            string label = brightness == 0 ? kind.ToString() : $"{kind} + brightness {brightness}";

            Fire(ApplyOperationAsync($"Menerapkan {label}...",
                src => WithBrightness(cached ?? FilterCatalog.Apply(src, kind), brightness),
                $"Filter {label} diterapkan"));
        }

        /// <summary>
        /// Hasil filter yang di-cache, hanya bila sesuai filter yang sedang dipilih
        /// </summary>
        private RgbImage CurrentFilterResult()
        {
            return filterPreview != null && filterPreview.Kind == filterPanel.SelectedFilter ? filterPreview.Image : null;
        }

        private static RgbImage WithBrightness(RgbImage image, int brightness) =>
            brightness == 0 ? image : ColorFilters.Brightness(image, brightness);
    }
}
