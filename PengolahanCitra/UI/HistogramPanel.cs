using System.Drawing;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Histogram R, G, B, dan Gray dari gambar saat ini
    /// </summary>
    public class HistogramPanel : UserControl
    {
        private readonly PictureBox red = CreateBox();
        private readonly PictureBox green = CreateBox();
        private readonly PictureBox blue = CreateBox();
        private readonly PictureBox gray = CreateBox();

        public HistogramPanel()
        {
            BackColor = Theme.Sidebar;
            Padding = new Padding(15, 5, 15, 5);

            Theme.StackTopDown(this,
                Theme.CreateTitle("Histogram"),
                Caption("Red"), red,
                Caption("Green"), green,
                Caption("Blue"), blue,
                Caption("Gray (luminance)"), gray);
        }

        public void ShowHistogram(Histogram histogram)
        {
            SetImage(red, histogram == null ? null : Histogram.Render(histogram.Red, Color.Red));
            SetImage(green, histogram == null ? null : Histogram.Render(histogram.Green, Color.Lime));
            SetImage(blue, histogram == null ? null : Histogram.Render(histogram.Blue, Color.DodgerBlue));
            SetImage(gray, histogram == null ? null : Histogram.Render(histogram.Gray, Color.White));
        }

        private static void SetImage(PictureBox box, Image image)
        {
            Image old = box.Image;
            box.Image = image;
            old?.Dispose();
        }

        private static PictureBox CreateBox() => new PictureBox
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Theme.Canvas,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.StretchImage
        };

        private static Label Caption(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 18,
            Font = Theme.SmallFont,
            ForeColor = Theme.TextMuted,
            TextAlign = ContentAlignment.BottomLeft
        };

        protected override void Dispose(bool disposing)
        {
            if (disposing) ShowHistogram(null);
            base.Dispose(disposing);
        }
    }
}
