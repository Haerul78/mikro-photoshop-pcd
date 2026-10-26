using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using PengolahanCitra.Imaging;
using PengolahanCitra.UI;

namespace PengolahanCitra
{
    /// <summary>
    /// Window utama. File ini berisi state, buka/simpan, undo/redo, dan helper umum.
    /// Logika tiap panel ada di MainForm.Filter.cs, MainForm.Edge.cs, dan MainForm.Transform.cs.
    /// </summary>
    public partial class MainForm : Form
    {
        private const int ThumbnailSourceSize = 80;

        // Deteksi tepi (terutama Canny) pada gambar 80 px kehilangan hampir semua tepi,
        // jadi thumbnail Edge dihitung dari versi 240 px lalu diperkecil
        private const int EdgeThumbnailSourceSize = 240;
        private const long LargeTextExportPixels = 2_000_000;

        private enum SidePanel { None, Filter, Edge, Transform }

        private readonly ImageDocument document = new ImageDocument();
        private readonly string baseTitle;

        // Bitmap tampilan untuk document.Current (milik form, bukan milik viewer)
        private Bitmap currentBitmap;

        // Versi kecil document.Current, sumber thumbnail panel Filter dan Edge
        private RgbImage thumbnailSource;
        private RgbImage edgeThumbnailSource;

        // Preview filter/edge yang sedang ditampilkan (null = menampilkan currentBitmap)
        private Bitmap previewBitmap;

        // Naik setiap kali preview diganti/dibatalkan; hasil async yang id-nya lama dibuang
        private int previewRequestId;

        private SidePanel activePanel = SidePanel.None;
        private bool isBusy;
        private readonly InputBlocker inputBlocker;

        public MainForm()
        {
            InitializeComponent();
            baseTitle = Text;
            inputBlocker = new InputBlocker(this);

            document.Changed += (s, e) => UpdateCommandStates();
            InitializeFilterPanel();
            InitializeEdgePanel();
            InitializeTransformPanel();
            UpdateCommandStates();
        }

        /// <summary>
        /// Hasil render sebuah gambar: dibuat di background thread, dipakai di UI thread
        /// </summary>
        private sealed class RenderedImage
        {
            public RgbImage Image;
            public Bitmap Bitmap;
            public Histogram Histogram;
            public RgbImage Thumbnail;
            public RgbImage EdgeThumbnail;
        }

        private static RenderedImage Render(RgbImage image)
        {
            Bitmap bitmap = image.ToBitmap();
            Size edgeSize = Geometry.FitSize(image.Width, image.Height, EdgeThumbnailSourceSize);
            Size thumbSize = Geometry.FitSize(image.Width, image.Height, ThumbnailSourceSize);

            using (Bitmap edgeThumb = Geometry.ResizeBitmap(bitmap, edgeSize.Width, edgeSize.Height))
            using (Bitmap thumb = Geometry.ResizeBitmap(edgeThumb, thumbSize.Width, thumbSize.Height))
            {
                return new RenderedImage
                {
                    Image = image,
                    Bitmap = bitmap,
                    Histogram = Histogram.Compute(image),
                    Thumbnail = RgbImage.FromBitmap(thumb),
                    EdgeThumbnail = RgbImage.FromBitmap(edgeThumb)
                };
            }
        }

        #region Toolbar

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = ImageFile.OpenFilter, Title = "Pilih Gambar" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Fire(OpenImageAsync(dialog.FileName));
            }
        }

        private void btnHome_Click(object sender, EventArgs e) => ShowPanel(SidePanel.None);
        private void btnFilter_Click(object sender, EventArgs e) => TogglePanel(SidePanel.Filter);
        private void btnEdge_Click(object sender, EventArgs e) => TogglePanel(SidePanel.Edge);
        private void btnTransform_Click(object sender, EventArgs e) => TogglePanel(SidePanel.Transform);
        private void btnUndo_Click(object sender, EventArgs e) => Fire(UndoAsync());
        private void btnRedo_Click(object sender, EventArgs e) => Fire(RedoAsync());

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (!EnsureImageLoaded()) return;
            RgbImage original = document.Original;
            Fire(ApplyOperationAsync("Mengembalikan gambar original...", _ => original, "Gambar dikembalikan ke original"));
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!EnsureImageLoaded()) return;

            using (var dialog = new SaveFileDialog
            {
                Filter = ImageFile.SaveFilter,
                Title = "Simpan Gambar",
                FileName = SuggestedFileName("_edit") + ".png",
                InitialDirectory = SuggestedDirectory()
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Fire(SaveImageAsync(dialog.FileName));
            }
        }

        private void btnSaveText_Click(object sender, EventArgs e)
        {
            if (!EnsureImageLoaded()) return;

            RgbImage image = document.Current;
            if (image.PixelCount > LargeTextExportPixels)
            {
                double megabytes = image.PixelCount * 12 / 1024.0 / 1024.0;
                string question = $"Gambar {image.Width} x {image.Height} akan menghasilkan file teks sekitar {megabytes:F0} MB. Lanjutkan?";
                if (MessageBox.Show(this, question, "Simpan .txt", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            }

            using (var dialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Simpan Matrix",
                FileName = SuggestedFileName("_matrix") + ".txt",
                InitialDirectory = SuggestedDirectory()
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Fire(SaveMatrixTextAsync(dialog.FileName));
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!isBusy)
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.O: btnOpen.PerformClick(); return true;
                    case Keys.Control | Keys.S: btnSave.PerformClick(); return true;
                    case Keys.Control | Keys.Z: Fire(UndoAsync()); return true;
                    case Keys.Control | Keys.Y:
                    case Keys.Control | Keys.Shift | Keys.Z: Fire(RedoAsync()); return true;
                    case Keys.Escape when activePanel != SidePanel.None: ShowPanel(SidePanel.None); return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Open, Save, Undo

        internal async Task OpenImageAsync(string path)
        {
            RenderedImage rendered = await RunBusyAsync("Membuka gambar...", () => Render(ImageFile.Load(path)));
            if (rendered == null) return;

            document.Load(rendered.Image, path);
            ShowPanel(SidePanel.None);
            ShowRendered(rendered);
            SetZoom(imageViewer.FitZoomPercent());
            SetStatus($"Dibuka: {Path.GetFileName(path)}");
        }

        internal async Task SaveImageAsync(string path)
        {
            RgbImage image = document.Current;
            bool saved = await RunBusyAsync("Menyimpan gambar...", () => { ImageFile.Save(image, path); return true; });
            if (saved) SetStatus($"Disimpan: {Path.GetFileName(path)}");
        }

        internal async Task SaveMatrixTextAsync(string path)
        {
            RgbImage image = document.Current;
            bool saved = await RunBusyAsync("Menyimpan matrix...", () => { ImageFile.SaveMatrixText(image, path); return true; });
            if (saved) SetStatus($"Matrix disimpan: {Path.GetFileName(path)}");
        }

        internal async Task UndoAsync()
        {
            if (isBusy || !document.CanUndo) return;
            document.Undo();
            await ShowCurrentAsync("Undo");
        }

        internal async Task RedoAsync()
        {
            if (isBusy || !document.CanRedo) return;
            document.Redo();
            await ShowCurrentAsync("Redo");
        }

        private async Task ShowCurrentAsync(string doneText)
        {
            RgbImage image = document.Current;
            RenderedImage rendered = await RunBusyAsync("Memuat gambar...", () => Render(image));
            if (rendered == null) return;

            ShowRendered(rendered);
            SetStatus(doneText);
        }

        #endregion

        #region Operasi gambar

        /// <summary>
        /// Jalankan operasi pada document.Current di background thread, lalu jadikan hasilnya gambar saat ini.
        /// </summary>
        internal async Task ApplyOperationAsync(string busyText, Func<RgbImage, RgbImage> operation, string doneText)
        {
            if (isBusy || !EnsureImageLoaded()) return;

            RgbImage source = document.Current;
            RenderedImage rendered = await RunBusyAsync(busyText, () => Render(operation(source)));
            if (rendered == null) return;

            document.Commit(rendered.Image);
            ShowRendered(rendered);
            SetStatus(doneText);
        }

        /// <summary>
        /// Jalankan pekerjaan berat di background; input ke window dikunci selama proses.
        /// Bila gagal, error ditampilkan (setelah kunci dilepas) dan hasilnya default.
        /// </summary>
        private async Task<T> RunBusyAsync<T>(string busyText, Func<T> work)
        {
            SetBusy(true, busyText);
            Exception error = null;
            T result = default(T);
            try
            {
                result = await Task.Run(work);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                SetBusy(false, null);
            }

            if (error is OutOfMemoryException) ShowError("Memori tidak cukup untuk memproses gambar sebesar ini.");
            else if (error != null) ShowError(error.Message);
            return result;
        }

        /// <summary>
        /// Selama sibuk, klik dan ketikan ke window ini dibuang lewat message filter.
        /// Menonaktifkan (Enabled = false) container yang berisi kontrol fokus jauh lebih mahal,
        /// karena WinForms memindahkan fokus ke kontrol lain.
        /// </summary>
        private void SetBusy(bool busy, string text)
        {
            if (busy == isBusy) return;

            isBusy = busy;
            if (busy) Application.AddMessageFilter(inputBlocker);
            else Application.RemoveMessageFilter(inputBlocker);

            toolbar.Enabled = !busy;
            UseWaitCursor = busy;
            if (busy) SetStatus(text);
            UpdateCommandStates();
        }

        private sealed class InputBlocker : IMessageFilter
        {
            private const int WM_KEYFIRST = 0x0100, WM_KEYLAST = 0x0109;
            private const int WM_LBUTTONDOWN = 0x0201, WM_MOUSELAST = 0x020E;

            private readonly Form owner;

            public InputBlocker(Form owner) => this.owner = owner;

            public bool PreFilterMessage(ref Message m)
            {
                bool isInput = (m.Msg >= WM_KEYFIRST && m.Msg <= WM_KEYLAST) ||
                               (m.Msg >= WM_LBUTTONDOWN && m.Msg <= WM_MOUSELAST);
                return isInput && Control.FromChildHandle(m.HWnd)?.FindForm() == owner;
            }
        }

        /// <summary>
        /// Tampilkan gambar baru sebagai gambar saat ini. Panel preview (Filter/Edge) ditutup
        /// karena isinya dibuat dari gambar lama.
        /// </summary>
        private void ShowRendered(RenderedImage rendered)
        {
            if (activePanel == SidePanel.Filter || activePanel == SidePanel.Edge) ShowPanel(SidePanel.None);
            else DiscardPreview();

            Bitmap old = currentBitmap;
            currentBitmap = rendered.Bitmap;
            imageViewer.Image = currentBitmap;
            old?.Dispose();

            thumbnailSource = rendered.Thumbnail;
            edgeThumbnailSource = rendered.EdgeThumbnail;
            histogramPanel.ShowHistogram(rendered.Histogram);
            histogramPanel.Visible = true;
            UpdateTitle();
        }

        #endregion

        #region Panel & Preview

        private void TogglePanel(SidePanel panel)
        {
            if (!EnsureImageLoaded()) return;
            ShowPanel(activePanel == panel ? SidePanel.None : panel);
        }

        private void ShowPanel(SidePanel panel)
        {
            DiscardPreview();
            activePanel = panel;

            filterPanel.Visible = panel == SidePanel.Filter;
            edgePanel.Visible = panel == SidePanel.Edge;
            transformPanel.Visible = panel == SidePanel.Transform;
            sidebarRight.Visible = panel != SidePanel.None;

            if (panel == SidePanel.Filter) PrepareFilterPanel();
            if (panel == SidePanel.Edge) PrepareEdgePanel();
        }

        private void ShowPreview(Bitmap preview)
        {
            Bitmap old = previewBitmap;
            previewBitmap = preview;
            imageViewer.Image = preview;
            old?.Dispose();
        }

        /// <summary>
        /// Buang preview dan tampilkan kembali gambar saat ini
        /// </summary>
        private void DiscardPreview()
        {
            previewRequestId++;
            filterPreview = null;
            edgePreview = null;

            imageViewer.Image = currentBitmap;
            previewBitmap?.Dispose();
            previewBitmap = null;
        }

        #endregion

        #region Viewer & Zoom

        private void imageViewer_PixelClicked(object sender, Point pixel)
        {
            if (isBusy || !document.IsLoaded) return;

            // Warna diambil dari gambar saat ini (bukan dari preview), yaitu data yang akan diproses
            Color color = document.Current.GetColor(pixel.X, pixel.Y);

            using (var popup = new ColorOperationForm(color))
            {
                if (popup.ShowDialog(this) != DialogResult.OK) return;

                if (popup.Operation == ColorOperation.PseudoColor)
                {
                    Fire(ApplyOperationAsync("Pewarnaan semu...", ColorFilters.PseudoColor, "Pewarnaan semu diterapkan"));
                }
                else
                {
                    int tolerance = popup.Tolerance;
                    bool grayBackground = popup.UseGrayBackground;
                    Fire(ApplyOperationAsync("Color slicing...",
                        src => ColorFilters.ColorSlicing(src, color, tolerance, grayBackground),
                        $"Color slicing (R:{color.R} G:{color.G} B:{color.B}, toleransi {tolerance}) diterapkan"));
                }
            }
        }

        private void trackBarZoom_Scroll(object sender, EventArgs e) => SetZoom(trackBarZoom.Value);
        private void btnZoomFit_Click(object sender, EventArgs e) => SetZoom(imageViewer.FitZoomPercent());
        private void btnZoom100_Click(object sender, EventArgs e) => SetZoom(100);

        private void imageViewer_ZoomChanged(object sender, EventArgs e)
        {
            int zoom = imageViewer.ZoomPercent;
            trackBarZoom.Value = Math.Max(trackBarZoom.Minimum, Math.Min(trackBarZoom.Maximum, zoom));
            labelZoomValue.Text = $"{zoom}%";
        }

        private void SetZoom(int percent)
        {
            imageViewer.ZoomPercent = percent;
            imageViewer_ZoomChanged(imageViewer, EventArgs.Empty);
        }

        #endregion

        #region Helper

        /// <summary>
        /// Jalankan task dari event handler. Error yang terjadi setelah await tidak hilang diam-diam,
        /// tetapi diteruskan ke Application.ThreadException dan ditampilkan.
        /// </summary>
        private static async void Fire(Task task) => await task;

        private bool EnsureImageLoaded()
        {
            if (document.IsLoaded) return true;
            ShowWarning("Silakan buka gambar terlebih dahulu!");
            return false;
        }

        private void UpdateCommandStates()
        {
            btnUndo.Enabled = !isBusy && document.CanUndo;
            btnRedo.Enabled = !isBusy && document.CanRedo;
        }

        private void UpdateTitle()
        {
            RgbImage image = document.Current;
            string name = document.FilePath == null ? "" : Path.GetFileName(document.FilePath);
            Text = image == null ? baseTitle : $"{baseTitle} - {name} ({image.Width} x {image.Height})";
        }

        private string SuggestedFileName(string suffix) =>
            document.FilePath == null ? "image" + suffix : Path.GetFileNameWithoutExtension(document.FilePath) + suffix;

        private string SuggestedDirectory() =>
            document.FilePath == null ? "" : Path.GetDirectoryName(document.FilePath);

        private void SetStatus(string text) => labelStatus.Text = text;

        private void ShowError(string message) =>
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void ShowWarning(string message) =>
            MessageBox.Show(this, message, "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void DisposeImages()
        {
            imageViewer.Image = null;
            currentBitmap?.Dispose();
            previewBitmap?.Dispose();
        }

        #endregion
    }
}
