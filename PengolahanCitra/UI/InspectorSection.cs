using System.Windows.Forms;
using PengolahanCitra.UI.Neu;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Dasar bagian di kolom kanan (inspector). Tingginya selalu mengikuti isi,
    /// sehingga kolom kanan bisa di-scroll sebagai satu daftar panjang.
    /// </summary>
    public class InspectorSection : UserControl
    {
        private bool fitting;

        public InspectorSection()
        {
            BackColor = NeuTheme.Surface;
            Padding = new Padding(0, 0, 0, 8);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            FitHeight();
        }

        private void FitHeight()
        {
            if (fitting) return;
            fitting = true;
            try
            {
                int height = Padding.Vertical;
                foreach (Control child in Controls)
                {
                    // Sebelum handle dibuat, Visible anak selalu false walau anak akan tampil,
                    // jadi pada saat itu semua anak dihitung
                    if (child.Visible || !IsHandleCreated) height += child.Height;
                }
                if (Height != height) Height = height;
            }
            finally
            {
                fitting = false;
            }
        }
    }
}
