using System;
using System.Threading.Tasks;
using PengolahanCitra.Imaging;

namespace PengolahanCitra
{
    /// <summary>
    /// Panel Edge: thumbnail metode, preview ukuran penuh, threshold Canny, dan Terapkan
    /// </summary>
    public partial class MainForm
    {
        private EdgePreview edgePreview;

        private sealed class EdgePreview
        {
            public EdgeMethod Method;
            public int Low;
            public int High;
            public RgbImage Image;

            public bool Matches(EdgeMethod method, int low, int high) =>
                Method == method && (method != EdgeMethod.Canny || (Low == low && High == high));
        }

        private void InitializeEdgePanel()
        {
            edgePanel.MethodSelected += (s, method) => Fire(SelectEdgeMethodAsync(method));
            edgePanel.RefreshCannyClicked += (s, e) => RefreshCanny();
            edgePanel.ApplyClicked += (s, e) => ApplyEdge();
        }

        private void PrepareEdgePanel()
        {
            edgePanel.ClearSelection();

            // Thumbnail tetap dibuat walau threshold sedang tidak valid (Low > High)
            int low = Math.Min(edgePanel.LowThreshold, edgePanel.HighThreshold);
            int high = Math.Max(edgePanel.LowThreshold, edgePanel.HighThreshold);

            foreach (EdgeMethod method in Enum.GetValues(typeof(EdgeMethod)))
            {
                UpdateEdgeThumbnail(method, low, high);
            }
        }

        private void UpdateEdgeThumbnail(EdgeMethod method, int low, int high)
        {
            RgbImage edges = EdgeDetection.Apply(edgeThumbnailSource, method, low, high);
            edgePanel.SetThumbnail(method, Geometry.FitWithin(edges, ThumbnailSourceSize).ToBitmap());
        }

        internal async Task SelectEdgeMethodAsync(EdgeMethod method)
        {
            if (isBusy || activePanel != SidePanel.Edge) return;
            if (!TryGetCannyThresholds(method, out int low, out int high)) return;

            int requestId = ++previewRequestId;
            RgbImage source = document.Current;

            var result = await RunBusyAsync($"Deteksi tepi {method}...", () =>
            {
                RgbImage edges = EdgeDetection.Apply(source, method, low, high);
                return Tuple.Create(edges, edges.ToBitmap());
            });
            if (result == null) return;

            if (requestId != previewRequestId || activePanel != SidePanel.Edge)
            {
                result.Item2.Dispose();
                return;
            }

            ShowPreview(result.Item2);
            edgePreview = new EdgePreview { Method = method, Low = low, High = high, Image = result.Item1 };
            SetStatus($"Preview {method} — klik Terapkan untuk menerapkan");
        }

        private void RefreshCanny()
        {
            if (!TryGetCannyThresholds(EdgeMethod.Canny, out int low, out int high)) return;

            UpdateEdgeThumbnail(EdgeMethod.Canny, low, high);

            if (edgePanel.SelectedMethod == EdgeMethod.Canny) Fire(SelectEdgeMethodAsync(EdgeMethod.Canny));
            else SetStatus($"Thumbnail Canny diperbarui (Low {low}, High {high})");
        }

        private void ApplyEdge()
        {
            if (!(edgePanel.SelectedMethod is EdgeMethod method))
            {
                ShowWarning("Pilih metode edge detection terlebih dahulu!");
                return;
            }

            if (!TryGetCannyThresholds(method, out int low, out int high)) return;

            RgbImage cached = edgePreview != null && edgePreview.Matches(method, low, high) ? edgePreview.Image : null;

            Fire(ApplyOperationAsync($"Deteksi tepi {method}...",
                src => cached ?? EdgeDetection.Apply(src, method, low, high),
                $"Edge detection {method} diterapkan"));
        }

        /// <summary>
        /// Threshold Canny dari UI; Low harus lebih kecil atau sama dengan High.
        /// Metode selain Canny selalu valid.
        /// </summary>
        private bool TryGetCannyThresholds(EdgeMethod method, out int low, out int high)
        {
            low = edgePanel.LowThreshold;
            high = edgePanel.HighThreshold;

            if (method == EdgeMethod.Canny && low > high)
            {
                ShowWarning("Low Threshold tidak boleh lebih besar dari High Threshold!");
                return false;
            }

            return true;
        }
    }
}
