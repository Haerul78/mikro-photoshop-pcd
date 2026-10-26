using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Pilihan beberapa opsi dalam satu track cekung; opsi terpilih tampil timbul
    /// </summary>
    public class NeuSegmented : NeuControl
    {
        private string[] items = new string[0];
        private int selectedIndex;

        public NeuSegmented()
        {
            Size = new Size(200, 36);
            TabStop = true;
            Cursor = Cursors.Hand;
            AccessibleRole = AccessibleRole.PageTabList;
        }

        public event EventHandler SelectedIndexChanged;

        [Browsable(false)]
        public string[] Items
        {
            get => items;
            set { items = value ?? new string[0]; Invalidate(); }
        }

        [DefaultValue(0)]
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                int index = Math.Max(0, Math.Min(items.Length - 1, value));
                if (index == selectedIndex) return;
                selectedIndex = index;
                AccessibleDescription = items.Length > 0 ? items[index] : null;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private Rectangle TrackBounds => Deflate(ClientRectangle, 2);

        private Rectangle SegmentBounds(int index)
        {
            Rectangle track = Deflate(TrackBounds, 4);
            int width = track.Width / Math.Max(1, items.Length);
            return new Rectangle(track.X + index * width, track.Y, width, track.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            NeuPainter.DrawSurface(g, TrackBounds, NeuTheme.RadiusSmall, NeuStyle.Inset, NeuTheme.DepthSmall);

            for (int i = 0; i < items.Length; i++)
            {
                Rectangle segment = SegmentBounds(i);
                bool selected = i == selectedIndex;

                if (selected)
                {
                    Rectangle face = Deflate(segment, 2);
                    NeuPainter.DrawSurface(g, face, NeuTheme.RadiusSmall - 3, NeuStyle.Raised, NeuTheme.DepthSmall);
                    if (ShowFocusRing) NeuPainter.DrawFocusRing(g, face, NeuTheme.RadiusSmall - 3);
                }

                DrawText(g, items[i], selected ? NeuTheme.ValueFont : Font, segment,
                    TextColor(selected ? NeuTheme.AccentInk : NeuTheme.InkMuted));
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            for (int i = 0; i < items.Length; i++)
            {
                if (SegmentBounds(i).Contains(e.Location)) SelectedIndex = i;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            return key == Keys.Left || key == Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Left) { SelectedIndex--; e.Handled = true; }
            if (e.KeyCode == Keys.Right) { SelectedIndex++; e.Handled = true; }
        }
    }
}
