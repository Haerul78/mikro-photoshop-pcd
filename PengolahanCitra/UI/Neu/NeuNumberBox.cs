using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Input angka dalam bidang cekung, dengan tombol naik/turun di kanan.
    /// Bisa diketik, digeser dengan panah atas/bawah, atau roda mouse.
    /// </summary>
    public class NeuNumberBox : NeuControl
    {
        private const int ArrowWidth = 24;

        private readonly TextBox box;
        private int minimum;
        private int maximum = 100;
        private int value;

        public NeuNumberBox()
        {
            Size = new Size(100, 34);
            Cursor = Cursors.IBeam;

            box = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = NeuTheme.Surface,
                ForeColor = NeuTheme.Ink,
                Font = NeuTheme.LabelFont,
                TextAlign = HorizontalAlignment.Center,
                Text = "0"
            };
            box.KeyDown += OnBoxKeyDown;
            box.Leave += (s, e) => Commit();
            box.GotFocus += (s, e) => Invalidate();
            box.LostFocus += (s, e) => Invalidate();
            Controls.Add(box);
            LayoutBox();
        }

        public event EventHandler ValueChanged;

        [DefaultValue(0)]
        public int Minimum
        {
            get => minimum;
            set { minimum = value; Value = this.value; }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => maximum;
            set { maximum = value; Value = this.value; }
        }

        [DefaultValue(0)]
        public int Value
        {
            get => value;
            set
            {
                int clamped = Math.Max(minimum, Math.Min(maximum, value));
                box.Text = clamped.ToString();
                if (clamped == this.value) return;
                this.value = clamped;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public override string Text
        {
            get => base.Text;
            set { base.Text = value; box.AccessibleName = value; }
        }

        private Rectangle UpBounds => new Rectangle(Width - ArrowWidth - 4, 3, ArrowWidth, Height / 2 - 3);
        private Rectangle DownBounds => new Rectangle(Width - ArrowWidth - 4, Height / 2, ArrowWidth, Height / 2 - 3);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutBox();
        }

        private void LayoutBox()
        {
            if (box == null) return;
            int height = box.PreferredHeight;
            box.SetBounds(10, (Height - height) / 2, Math.Max(10, Width - ArrowWidth - 18), height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle shape = Deflate(ClientRectangle, 1);
            NeuPainter.DrawSurface(g, shape, NeuTheme.RadiusSmall, NeuStyle.Inset, NeuTheme.DepthSmall);

            Color arrow = TextColor(NeuTheme.InkMuted);
            DrawIcon(g, NeuTheme.Icons.Up, NeuTheme.IconFontSmall, UpBounds, arrow);
            DrawIcon(g, NeuTheme.Icons.Down, NeuTheme.IconFontSmall, DownBounds, arrow);

            if (box.Focused) NeuPainter.DrawFocusRing(g, Deflate(shape, 2), NeuTheme.RadiusSmall - 2);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (UpBounds.Contains(e.Location)) Step(1);
            else if (DownBounds.Contains(e.Location)) Step(-1);
            box.Focus();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Step(e.Delta > 0 ? 1 : -1);
        }

        private void OnBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: Step(1); e.Handled = true; break;
                case Keys.Down: Step(-1); e.Handled = true; break;
                case Keys.Enter: Commit(); e.Handled = e.SuppressKeyPress = true; break;
            }
        }

        private void Step(int delta)
        {
            Commit();
            Value = value + delta;
            box.SelectAll();
        }

        /// <summary>Ambil angka yang diketik; teks tidak valid dikembalikan ke nilai terakhir</summary>
        private void Commit()
        {
            Value = int.TryParse(box.Text.Trim(), out int typed) ? typed : value;
        }
    }
}
