using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra
{
    /// <summary>
    /// Panel Filter: thumbnail, preview ukuran penuh, brightness, dan Apply
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

            // Debounce: preview brightness baru dihitung setelah slider berhenti sejenak
            brightnessTimer.Tick += (s, e) => { brightnessTimer.Stop(); Fire(UpdateBrightnessPreviewAsync()); };
        }

        private void PrepareFilterPanel()
        {
            filterPanel.ResetSelection();

            RgbImage thumbnail = thumbnailSource;
            double sizeRatio = (double)thumbnail.Width / document.Current.Width;

            foreach (FilterKind kind in Enum.GetValues(typeof(FilterKind)))
            {
                // Tile memiliki bitmap ini dan men-dispose-nya saat diganti
                filterPanel.SetThumbnail(kind, FilterCatalog.Apply(thumbnail, kind, sizeRatio).ToBitmap());
            }
        }

        internal async Task SelectFilterAsync(FilterKind kind)
        {
            if (isBusy || activePanel != SidePanel.Filter) return;

            int requestId = ++previewRequestId;
            RgbImage source = document.Current;
            int brightness = filterPanel.Brightness;

            var result = await RunBusyAsync($"Menerapkan {kind}...", () =>
            {
                RgbImage filtered = FilterCatalog.Apply(source, kind);
                RgbImage shown = brightness == 0 ? filtered : ColorFilters.Brightness(filtered, brightness);
                return Tuple.Create(filtered, shown.ToBitmap());
            });
            if (result == null) return;

            if (requestId != previewRequestId || activePanel != SidePanel.Filter)
            {
                result.Item2.Dispose();
                return;
            }

            ShowPreview(result.Item2);
            filterPreview = new FilterPreview { Kind = kind, Image = result.Item1 };
            SetStatus($"Preview {kind} — klik Apply Filter untuk menerapkan");
        }

        private async Task UpdateBrightnessPreviewAsync()
        {
            if (activePanel != SidePanel.Filter || isBusy) return;

            int requestId = ++previewRequestId;
            RgbImage source = CurrentFilterResult() ?? document.Current;
            int brightness = filterPanel.Brightness;

            Bitmap bitmap;
            try
            {
                bitmap = await Task.Run(() => (brightness == 0 ? source : ColorFilters.Brightness(source, brightness)).ToBitmap());
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return;
            }

            if (requestId != previewRequestId || activePanel != SidePanel.Filter)
            {
                bitmap.Dispose();
                return;
            }

            ShowPreview(bitmap);
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

            Fire(ApplyOperationAsync($"Menerapkan {label}...", src =>
            {
                RgbImage filtered = cached ?? FilterCatalog.Apply(src, kind);
                return brightness == 0 ? filtered : ColorFilters.Brightness(filtered, brightness);
            }, $"Filter {label} diterapkan"));
        }

        /// <summary>
        /// Hasil filter yang di-cache, hanya bila sesuai filter yang sedang dipilih
        /// </summary>
        private RgbImage CurrentFilterResult()
        {
            return filterPreview != null && filterPreview.Kind == filterPanel.SelectedFilter ? filterPreview.Image : null;
        }
    }
}
