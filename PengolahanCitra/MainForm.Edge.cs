using System;
using System.Threading.Tasks;
using PengolahanCitra.Imaging;

namespace PengolahanCitra
{
    /// <summary>
    /// Alat Deteksi tepi: thumbnail metode, preview ukuran penuh, threshold Canny, Batal, dan Terapkan.
    /// EdgePanel menjaga Low ≤ High, jadi threshold dari panel selalu valid.
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
            edgePanel.CannyThresholdsChanged += (s, e) => RefreshCanny();
            edgePanel.CancelClicked += (s, e) => CancelPreview();
            edgePanel.ApplyClicked += (s, e) => ApplyEdge();
        }

        private void PrepareEdgePanel()
        {
            edgePanel.ClearSelection();
            if (edgeThumbnailSource == null) return;

            foreach (EdgeMethod method in Enum.GetValues(typeof(EdgeMethod)))
            {
                UpdateEdgeThumbnail(method);
            }
        }

        private void UpdateEdgeThumbnail(EdgeMethod method)
        {
            RgbImage edges = EdgeDetection.Apply(edgeThumbnailSource, method, edgePanel.LowThreshold, edgePanel.HighThreshold);
            edgePanel.SetThumbnail(method, Geometry.FitWithin(edges, ThumbnailSourceSize).ToBitmap());
        }

        internal async Task SelectEdgeMethodAsync(EdgeMethod method)
        {
            if (isBusy || activeTool != Tool.Edge) return;

            int requestId = ++previewRequestId;
            RgbImage source = document.Current;
            int low = edgePanel.LowThreshold;
            int high = edgePanel.HighThreshold;

            PreviewResult preview = await RunBusyAsync($"Deteksi tepi {method}...",
                () => PreviewResult.Create(EdgeDetection.Apply(source, method, low, high)));
            if (!TryShowPreview(requestId, Tool.Edge, preview)) return;

            edgePreview = new EdgePreview { Method = method, Low = low, High = high, Image = preview.Image };
            SetStatus($"Preview {method} — klik Terapkan untuk menerapkan, atau Esc untuk batal");
        }

        /// <summary>Threshold Canny berubah: perbarui thumbnail Canny dan preview bila Canny sedang dipilih</summary>
        private void RefreshCanny()
        {
            if (edgeThumbnailSource == null) return;
            UpdateEdgeThumbnail(EdgeMethod.Canny);

            if (edgePanel.SelectedMethod == EdgeMethod.Canny) Fire(SelectEdgeMethodAsync(EdgeMethod.Canny));
        }

        private void ApplyEdge()
        {
            if (!(edgePanel.SelectedMethod is EdgeMethod method))
            {
                ShowWarning("Pilih metode deteksi tepi terlebih dahulu!");
                return;
            }

            int low = edgePanel.LowThreshold;
            int high = edgePanel.HighThreshold;
            RgbImage cached = edgePreview != null && edgePreview.Matches(method, low, high) ? edgePreview.Image : null;

            Fire(ApplyOperationAsync($"Deteksi tepi {method}...",
                src => cached ?? EdgeDetection.Apply(src, method, low, high),
                $"Deteksi tepi {method} diterapkan"));
        }
    }
}
