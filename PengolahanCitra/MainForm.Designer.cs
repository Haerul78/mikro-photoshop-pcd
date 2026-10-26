using PengolahanCitra.UI;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                DisposeImages();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            this.topBar = new System.Windows.Forms.Panel();
            this.fileActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOpen = new NeuButton();
            this.btnSave = new NeuButton();
            this.btnExport = new NeuButton();
            this.btnUndo = new NeuButton();
            this.btnRedo = new NeuButton();
            this.btnReset = new NeuButton();
            this.btnCompare = new NeuButton();

            this.toolRail = new System.Windows.Forms.FlowLayoutPanel();
            this.toolAdjust = new NeuButton();
            this.toolEdge = new NeuButton();
            this.toolGeometry = new NeuButton();
            this.toolArithmetic = new NeuButton();

            this.inspector = new System.Windows.Forms.Panel();
            this.histogramPanel = new HistogramPanel();
            this.filterPanel = new FilterPanel();
            this.edgePanel = new EdgePanel();
            this.geometryPanel = new GeometryPanel();
            this.arithmeticPanel = new ArithmeticPanel();

            this.canvasHost = new System.Windows.Forms.Panel();
            this.canvasFrame = new NeuPanel();
            this.imageViewer = new ImageViewer();

            this.statusBar = new System.Windows.Forms.Panel();
            this.labelFile = new System.Windows.Forms.Label();
            this.labelSize = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.zoomBar = new System.Windows.Forms.FlowLayoutPanel();
            this.zoomSlider = new NeuSlider();
            this.labelZoomValue = new System.Windows.Forms.Label();
            this.btnZoomFit = new NeuButton();
            this.btnZoom100 = new NeuButton();

            this.topBar.SuspendLayout();
            this.fileActions.SuspendLayout();
            this.toolRail.SuspendLayout();
            this.inspector.SuspendLayout();
            this.canvasHost.SuspendLayout();
            this.canvasFrame.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.zoomBar.SuspendLayout();
            this.SuspendLayout();
            //
            // topBar: Open, Save, Ekspor, Undo, Redo, Reset ... Before/After
            //
            this.topBar.BackColor = NeuTheme.Surface;
            this.topBar.Controls.Add(this.fileActions);
            this.topBar.Controls.Add(this.btnCompare);
            this.topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.topBar.Name = "topBar";
            this.topBar.Padding = new System.Windows.Forms.Padding(10, 6, 14, 0);
            this.topBar.Size = new System.Drawing.Size(1300, 70);
            this.fileActions.Controls.Add(this.btnOpen);
            this.fileActions.Controls.Add(this.btnSave);
            this.fileActions.Controls.Add(this.btnExport);
            this.fileActions.Controls.Add(this.btnUndo);
            this.fileActions.Controls.Add(this.btnRedo);
            this.fileActions.Controls.Add(this.btnReset);
            this.fileActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileActions.Name = "fileActions";
            this.fileActions.WrapContents = false;
            InitTextButton(this.btnOpen, "btnOpen", "Buka", NeuTheme.Icons.Open, 112, "Buka gambar (Ctrl+O)", this.btnOpen_Click);
            InitTextButton(this.btnSave, "btnSave", "Simpan", NeuTheme.Icons.Save, 120, "Simpan gambar (Ctrl+S)", this.btnSave_Click);
            InitTextButton(this.btnExport, "btnExport", "Ekspor .txt", NeuTheme.Icons.Export, 140, "Simpan nilai piksel sebagai teks", this.btnExport_Click);
            InitIconButton(this.btnUndo, "btnUndo", NeuTheme.Icons.Undo, "Undo (Ctrl+Z)", this.btnUndo_Click);
            InitIconButton(this.btnRedo, "btnRedo", NeuTheme.Icons.Redo, "Redo (Ctrl+Y)", this.btnRedo_Click);
            InitTextButton(this.btnReset, "btnReset", "Reset", NeuTheme.Icons.Reset, 112, "Kembalikan ke gambar asli", this.btnReset_Click);
            this.btnUndo.Margin = new System.Windows.Forms.Padding(18, 0, 0, 0);
            InitTextButton(this.btnCompare, "btnCompare", "Before / After", NeuTheme.Icons.Compare, 168,
                "Bandingkan gambar asli (kiri) dengan hasil (kanan); geser garis di kanvas", this.btnCompare_Click);
            this.btnCompare.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // toolRail: pilihan alat (tombol bulat)
            //
            this.toolRail.BackColor = NeuTheme.Surface;
            this.toolRail.Controls.Add(this.toolAdjust);
            this.toolRail.Controls.Add(this.toolEdge);
            this.toolRail.Controls.Add(this.toolGeometry);
            this.toolRail.Controls.Add(this.toolArithmetic);
            this.toolRail.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolRail.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.toolRail.Name = "toolRail";
            this.toolRail.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            this.toolRail.Size = new System.Drawing.Size(92, 760);
            this.toolRail.WrapContents = false;
            InitToolButton(this.toolAdjust, "toolAdjust", NeuTheme.Icons.Adjust, "Penyesuaian & filter", this.toolAdjust_Click);
            InitToolButton(this.toolEdge, "toolEdge", NeuTheme.Icons.Edge, "Deteksi tepi", this.toolEdge_Click);
            InitToolButton(this.toolGeometry, "toolGeometry", NeuTheme.Icons.Geometry, "Geometri: rotasi, translasi, flip", this.toolGeometry_Click);
            InitToolButton(this.toolArithmetic, "toolArithmetic", NeuTheme.Icons.Arithmetic, "Aritmatika citra", this.toolArithmetic_Click);
            //
            // inspector: histogram + alat yang aktif (bisa di-scroll)
            //
            this.inspector.AutoScroll = true;
            this.inspector.BackColor = NeuTheme.Surface;
            this.inspector.Controls.Add(this.arithmeticPanel);
            this.inspector.Controls.Add(this.geometryPanel);
            this.inspector.Controls.Add(this.edgePanel);
            this.inspector.Controls.Add(this.filterPanel);
            this.inspector.Controls.Add(this.histogramPanel);
            this.inspector.Dock = System.Windows.Forms.DockStyle.Right;
            this.inspector.Name = "inspector";
            this.inspector.Padding = new System.Windows.Forms.Padding(6, 0, 14, 8);
            this.inspector.Size = new System.Drawing.Size(308, 760);
            this.histogramPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.histogramPanel.Name = "histogramPanel";
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Name = "filterPanel";
            this.edgePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.edgePanel.Name = "edgePanel";
            this.edgePanel.Visible = false;
            this.geometryPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.geometryPanel.Name = "geometryPanel";
            this.geometryPanel.Visible = false;
            this.arithmeticPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.arithmeticPanel.Name = "arithmeticPanel";
            this.arithmeticPanel.Visible = false;
            //
            // canvas: bidang cekung berisi viewer
            //
            this.canvasHost.BackColor = NeuTheme.Surface;
            this.canvasHost.Controls.Add(this.canvasFrame);
            this.canvasHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvasHost.Name = "canvasHost";
            this.canvasHost.Padding = new System.Windows.Forms.Padding(4, 8, 8, 4);
            this.canvasFrame.Controls.Add(this.imageViewer);
            this.canvasFrame.Depth = NeuTheme.DepthInset;
            this.canvasFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.canvasFrame.Name = "canvasFrame";
            this.canvasFrame.Padding = new System.Windows.Forms.Padding(14);
            this.canvasFrame.Style = NeuStyle.Inset;
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.PixelClicked += new System.EventHandler<System.Drawing.Point>(this.imageViewer_PixelClicked);
            this.imageViewer.ZoomChanged += new System.EventHandler(this.imageViewer_ZoomChanged);
            this.imageViewer.FileDropped += new System.EventHandler<string>(this.imageViewer_FileDropped);
            //
            // statusBar: nama file, ukuran, status, zoom
            //
            this.statusBar.BackColor = NeuTheme.Surface;
            this.statusBar.Controls.Add(this.labelStatus);
            this.statusBar.Controls.Add(this.labelSize);
            this.statusBar.Controls.Add(this.labelFile);
            this.statusBar.Controls.Add(this.zoomBar);
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Name = "statusBar";
            this.statusBar.Padding = new System.Windows.Forms.Padding(16, 0, 10, 0);
            this.statusBar.Size = new System.Drawing.Size(1300, 54);
            InitStatusLabel(this.labelFile, "labelFile", NeuTheme.ValueFont, NeuTheme.Ink, "Belum ada gambar");
            this.labelFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelFile.AutoSize = true;
            InitStatusLabel(this.labelSize, "labelSize", NeuTheme.LabelFont, NeuTheme.InkMuted, "");
            this.labelSize.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelSize.AutoSize = true;
            this.labelSize.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            InitStatusLabel(this.labelStatus, "labelStatus", NeuTheme.LabelFont, NeuTheme.InkMuted, "Siap");
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatus.Padding = new System.Windows.Forms.Padding(24, 0, 8, 0);
            this.zoomBar.BackColor = NeuTheme.Surface;
            this.zoomBar.Controls.Add(this.zoomSlider);
            this.zoomBar.Controls.Add(this.labelZoomValue);
            this.zoomBar.Controls.Add(this.btnZoomFit);
            this.zoomBar.Controls.Add(this.btnZoom100);
            this.zoomBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.zoomBar.Name = "zoomBar";
            this.zoomBar.Size = new System.Drawing.Size(390, 54);
            this.zoomBar.WrapContents = false;
            this.zoomSlider.AccessibleName = "Zoom";
            this.zoomSlider.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.zoomSlider.Maximum = ImageViewer.MaxZoom;
            this.zoomSlider.Minimum = ImageViewer.MinZoom;
            this.zoomSlider.Name = "zoomSlider";
            this.zoomSlider.Size = new System.Drawing.Size(170, 46);
            this.zoomSlider.SmallChange = 5;
            this.zoomSlider.LargeChange = 25;
            this.zoomSlider.Value = 100;
            this.zoomSlider.ValueChanged += new System.EventHandler(this.zoomSlider_ValueChanged);
            InitStatusLabel(this.labelZoomValue, "labelZoomValue", NeuTheme.ValueFont, NeuTheme.AccentInk, "100%");
            this.labelZoomValue.Size = new System.Drawing.Size(52, 52);
            this.labelZoomValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            InitTextButton(this.btnZoomFit, "btnZoomFit", "Fit", null, 72, "Muat seluruh gambar di kanvas", this.btnZoomFit_Click);
            InitTextButton(this.btnZoom100, "btnZoom100", "100%", null, 80, "Ukuran asli (1 piksel = 1 piksel layar)", this.btnZoom100_Click);
            this.btnZoomFit.Height = 52;
            this.btnZoom100.Height = 52;
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = NeuTheme.Surface;
            this.ClientSize = new System.Drawing.Size(1300, 820);
            this.Controls.Add(this.canvasHost);
            this.Controls.Add(this.inspector);
            this.Controls.Add(this.toolRail);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.topBar);
            this.Font = NeuTheme.LabelFont;
            this.ForeColor = NeuTheme.Ink;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1080, 680);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pengolahan Citra Digital - Mikro Photoshop";
            this.topBar.ResumeLayout(false);
            this.fileActions.ResumeLayout(false);
            this.toolRail.ResumeLayout(false);
            this.inspector.ResumeLayout(false);
            this.canvasHost.ResumeLayout(false);
            this.canvasFrame.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.zoomBar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void InitTextButton(NeuButton button, string name, string text, string icon, int width, string tip, System.EventHandler onClick)
        {
            button.Name = name;
            button.Text = text;
            button.Icon = icon;
            button.AccessibleName = text;
            button.Margin = System.Windows.Forms.Padding.Empty;
            button.Size = new System.Drawing.Size(width, 60);
            button.Click += onClick;
            this.toolTip.SetToolTip(button, tip);
        }

        private void InitIconButton(NeuButton button, string name, string icon, string tip, System.EventHandler onClick)
        {
            button.Name = name;
            button.Icon = icon;
            button.Circular = true;
            button.AccessibleName = tip;
            button.Margin = System.Windows.Forms.Padding.Empty;
            button.Size = new System.Drawing.Size(60, 60);
            button.Click += onClick;
            this.toolTip.SetToolTip(button, tip);
        }

        private void InitToolButton(NeuButton button, string name, string icon, string tip, System.EventHandler onClick)
        {
            InitIconButton(button, name, icon, tip, onClick);
            button.Size = new System.Drawing.Size(72, 72);
            button.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        }

        private static void InitStatusLabel(System.Windows.Forms.Label label, string name, System.Drawing.Font font, System.Drawing.Color color, string text)
        {
            label.BackColor = NeuTheme.Surface;
            label.Font = font;
            label.ForeColor = color;
            label.Name = name;
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.UseMnemonic = false;
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Panel topBar;
        private System.Windows.Forms.FlowLayoutPanel fileActions;
        private NeuButton btnOpen;
        private NeuButton btnSave;
        private NeuButton btnExport;
        private NeuButton btnUndo;
        private NeuButton btnRedo;
        private NeuButton btnReset;
        private NeuButton btnCompare;
        private System.Windows.Forms.FlowLayoutPanel toolRail;
        private NeuButton toolAdjust;
        private NeuButton toolEdge;
        private NeuButton toolGeometry;
        private NeuButton toolArithmetic;
        private System.Windows.Forms.Panel inspector;
        private HistogramPanel histogramPanel;
        private FilterPanel filterPanel;
        private EdgePanel edgePanel;
        private GeometryPanel geometryPanel;
        private ArithmeticPanel arithmeticPanel;
        private System.Windows.Forms.Panel canvasHost;
        private NeuPanel canvasFrame;
        private ImageViewer imageViewer;
        private System.Windows.Forms.Panel statusBar;
        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.FlowLayoutPanel zoomBar;
        private NeuSlider zoomSlider;
        private System.Windows.Forms.Label labelZoomValue;
        private NeuButton btnZoomFit;
        private NeuButton btnZoom100;
    }
}
