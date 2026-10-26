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
            this.toolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHome = new System.Windows.Forms.Button();
            this.btnUndo = new System.Windows.Forms.Button();
            this.btnRedo = new System.Windows.Forms.Button();
            this.btnFilter = new System.Windows.Forms.Button();
            this.btnTransform = new System.Windows.Forms.Button();
            this.btnEdge = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveText = new System.Windows.Forms.Button();
            this.sidebarLeft = new System.Windows.Forms.Panel();
            this.histogramPanel = new PengolahanCitra.UI.HistogramPanel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.sidebarRight = new System.Windows.Forms.Panel();
            this.filterPanel = new PengolahanCitra.UI.FilterPanel();
            this.edgePanel = new PengolahanCitra.UI.EdgePanel();
            this.transformPanel = new PengolahanCitra.UI.TransformPanel();
            this.statusBar = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.zoomBar = new System.Windows.Forms.FlowLayoutPanel();
            this.labelZoom = new System.Windows.Forms.Label();
            this.trackBarZoom = new System.Windows.Forms.TrackBar();
            this.labelZoomValue = new System.Windows.Forms.Label();
            this.btnZoomFit = new System.Windows.Forms.Button();
            this.btnZoom100 = new System.Windows.Forms.Button();
            this.imageViewer = new PengolahanCitra.UI.ImageViewer();
            this.toolbar.SuspendLayout();
            this.sidebarLeft.SuspendLayout();
            this.sidebarRight.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.zoomBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).BeginInit();
            this.SuspendLayout();
            //
            // toolbar
            //
            this.toolbar.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.toolbar.Controls.Add(this.btnHome);
            this.toolbar.Controls.Add(this.btnUndo);
            this.toolbar.Controls.Add(this.btnRedo);
            this.toolbar.Controls.Add(this.btnFilter);
            this.toolbar.Controls.Add(this.btnTransform);
            this.toolbar.Controls.Add(this.btnEdge);
            this.toolbar.Controls.Add(this.btnReset);
            this.toolbar.Controls.Add(this.btnSave);
            this.toolbar.Controls.Add(this.btnSaveText);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(10, 9, 10, 0);
            this.toolbar.Size = new System.Drawing.Size(1284, 56);
            this.toolbar.WrapContents = false;
            //
            // toolbar buttons
            //
            InitToolbarButton(this.btnHome, "btnHome", "Home", this.btnHome_Click);
            InitToolbarButton(this.btnUndo, "btnUndo", "↶ Undo", this.btnUndo_Click);
            InitToolbarButton(this.btnRedo, "btnRedo", "↷ Redo", this.btnRedo_Click);
            InitToolbarButton(this.btnFilter, "btnFilter", "Filter", this.btnFilter_Click);
            InitToolbarButton(this.btnTransform, "btnTransform", "Aritmatika & Geometri", this.btnTransform_Click);
            InitToolbarButton(this.btnEdge, "btnEdge", "Edge", this.btnEdge_Click);
            InitToolbarButton(this.btnReset, "btnReset", "Reset", this.btnReset_Click);
            InitToolbarButton(this.btnSave, "btnSave", "Save", this.btnSave_Click);
            InitToolbarButton(this.btnSaveText, "btnSaveText", "Simpan .txt", this.btnSaveText_Click);
            //
            // sidebarLeft
            //
            this.sidebarLeft.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.sidebarLeft.Controls.Add(this.histogramPanel);
            this.sidebarLeft.Controls.Add(this.btnOpen);
            this.sidebarLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebarLeft.Name = "sidebarLeft";
            this.sidebarLeft.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.sidebarLeft.Size = new System.Drawing.Size(250, 705);
            //
            // histogramPanel
            //
            this.histogramPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.histogramPanel.Name = "histogramPanel";
            this.histogramPanel.Visible = false;
            //
            // btnOpen
            //
            this.btnOpen.BackColor = System.Drawing.Color.BlueViolet;
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOpen.FlatAppearance.BorderSize = 0;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnOpen.ForeColor = System.Drawing.Color.White;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(250, 45);
            this.btnOpen.Text = "Buka Gambar";
            this.btnOpen.UseVisualStyleBackColor = false;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            //
            // sidebarRight
            //
            this.sidebarRight.BackColor = System.Drawing.Color.FromArgb(37, 37, 38);
            this.sidebarRight.Controls.Add(this.filterPanel);
            this.sidebarRight.Controls.Add(this.edgePanel);
            this.sidebarRight.Controls.Add(this.transformPanel);
            this.sidebarRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.sidebarRight.Name = "sidebarRight";
            this.sidebarRight.Size = new System.Drawing.Size(250, 705);
            this.sidebarRight.Visible = false;
            //
            // side panels
            //
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Visible = false;
            this.edgePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.edgePanel.Name = "edgePanel";
            this.edgePanel.Visible = false;
            this.transformPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.transformPanel.Name = "transformPanel";
            this.transformPanel.Visible = false;
            //
            // statusBar
            //
            this.statusBar.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.statusBar.Controls.Add(this.labelStatus);
            this.statusBar.Controls.Add(this.zoomBar);
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(784, 36);
            //
            // labelStatus
            //
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelStatus.ForeColor = System.Drawing.Color.LightGray;
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.labelStatus.Text = "Siap";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // zoomBar
            //
            this.zoomBar.Controls.Add(this.labelZoom);
            this.zoomBar.Controls.Add(this.trackBarZoom);
            this.zoomBar.Controls.Add(this.labelZoomValue);
            this.zoomBar.Controls.Add(this.btnZoomFit);
            this.zoomBar.Controls.Add(this.btnZoom100);
            this.zoomBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.zoomBar.Name = "zoomBar";
            this.zoomBar.Padding = new System.Windows.Forms.Padding(0, 3, 6, 0);
            this.zoomBar.Size = new System.Drawing.Size(390, 36);
            this.zoomBar.WrapContents = false;
            //
            // labelZoom
            //
            this.labelZoom.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelZoom.ForeColor = System.Drawing.Color.LightGray;
            this.labelZoom.Name = "labelZoom";
            this.labelZoom.Size = new System.Drawing.Size(42, 28);
            this.labelZoom.Text = "Zoom";
            this.labelZoom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // trackBarZoom
            //
            this.trackBarZoom.AutoSize = false;
            this.trackBarZoom.LargeChange = 25;
            this.trackBarZoom.Maximum = 400;
            this.trackBarZoom.Minimum = 5;
            this.trackBarZoom.Name = "trackBarZoom";
            this.trackBarZoom.Size = new System.Drawing.Size(170, 28);
            this.trackBarZoom.SmallChange = 5;
            this.trackBarZoom.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarZoom.Value = 100;
            this.trackBarZoom.Scroll += new System.EventHandler(this.trackBarZoom_Scroll);
            //
            // labelZoomValue
            //
            this.labelZoomValue.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.labelZoomValue.ForeColor = System.Drawing.Color.White;
            this.labelZoomValue.Name = "labelZoomValue";
            this.labelZoomValue.Size = new System.Drawing.Size(48, 28);
            this.labelZoomValue.Text = "100%";
            this.labelZoomValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // zoom buttons
            //
            InitToolbarButton(this.btnZoomFit, "btnZoomFit", "Fit", this.btnZoomFit_Click);
            InitToolbarButton(this.btnZoom100, "btnZoom100", "100%", this.btnZoom100_Click);
            this.btnZoomFit.Size = new System.Drawing.Size(48, 26);
            this.btnZoom100.Size = new System.Drawing.Size(48, 26);
            //
            // imageViewer
            //
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.PixelClicked += new System.EventHandler<System.Drawing.Point>(this.imageViewer_PixelClicked);
            this.imageViewer.ZoomChanged += new System.EventHandler(this.imageViewer_ZoomChanged);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.ClientSize = new System.Drawing.Size(1284, 761);
            this.Controls.Add(this.imageViewer);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.sidebarRight);
            this.Controls.Add(this.sidebarLeft);
            this.Controls.Add(this.toolbar);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pengolahan Citra Digital - Mikro Photoshop";
            this.toolbar.ResumeLayout(false);
            this.sidebarLeft.ResumeLayout(false);
            this.sidebarRight.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.zoomBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarZoom)).EndInit();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Gaya tombol toolbar yang seragam
        /// </summary>
        private static void InitToolbarButton(System.Windows.Forms.Button button, string name, string text, System.EventHandler onClick)
        {
            button.AutoSize = true;
            button.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            button.Cursor = System.Windows.Forms.Cursors.Hand;
            button.FlatAppearance.BorderSize = 0;
            button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            button.ForeColor = System.Drawing.Color.White;
            button.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            button.MinimumSize = new System.Drawing.Size(54, 38);
            button.Name = name;
            button.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            button.Text = text;
            button.UseMnemonic = false;
            button.UseVisualStyleBackColor = false;
            button.Click += onClick;
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button btnHome;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnRedo;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.Button btnTransform;
        private System.Windows.Forms.Button btnEdge;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveText;
        private System.Windows.Forms.Panel sidebarLeft;
        private PengolahanCitra.UI.HistogramPanel histogramPanel;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Panel sidebarRight;
        private PengolahanCitra.UI.FilterPanel filterPanel;
        private PengolahanCitra.UI.EdgePanel edgePanel;
        private PengolahanCitra.UI.TransformPanel transformPanel;
        private System.Windows.Forms.Panel statusBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.FlowLayoutPanel zoomBar;
        private System.Windows.Forms.Label labelZoom;
        private System.Windows.Forms.TrackBar trackBarZoom;
        private System.Windows.Forms.Label labelZoomValue;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.Button btnZoom100;
        private PengolahanCitra.UI.ImageViewer imageViewer;
    }
}
