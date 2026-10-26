using System;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Kotak thumbnail + label yang bisa dipilih. Tile memiliki bitmap thumbnail-nya.
    /// </summary>
    public class ThumbnailTile : Panel
    {
        private readonly PictureBox pictureBox;
        private readonly Label label;
        private bool selected;

        public ThumbnailTile(string text, object tag)
        {
            Tag = tag;
            Size = new Size(96, 82);
            Margin = new Padding(3);
            Padding = new Padding(3);
            Cursor = Cursors.Hand;

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Theme.Canvas
            };

            label = new Label
            {
                Text = text,
                Dock = DockStyle.Bottom,
                Height = 18,
                Font = Theme.SmallFont,
                ForeColor = Theme.Text,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(pictureBox);
            Controls.Add(label);

            // Klik di gambar atau label dianggap klik pada tile
            pictureBox.Click += (s, e) => OnClick(e);
            label.Click += (s, e) => OnClick(e);
        }

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                BackColor = value ? Theme.Accent : Color.Transparent;
            }
        }

        public void SetThumbnail(Image thumbnail)
        {
            Image old = pictureBox.Image;
            pictureBox.Image = thumbnail;
            if (old != null && !ReferenceEquals(old, thumbnail)) old.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) SetThumbnail(null);
            base.Dispose(disposing);
        }
    }
}
