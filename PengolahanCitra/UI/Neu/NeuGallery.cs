using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI.Neu
{
    /// <summary>
    /// Grid thumbnail (NeuTile); hanya satu tile yang terpilih pada satu waktu
    /// </summary>
    public class NeuGallery<TKey> : FlowLayoutPanel
    {
        public const int TileWidth = 116;
        public const int TileHeight = 108;

        private readonly Dictionary<TKey, NeuTile> tiles = new Dictionary<TKey, NeuTile>();

        public NeuGallery(IEnumerable<TKey> keys, Func<TKey, string> getLabel)
        {
            BackColor = NeuTheme.Surface;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            WrapContents = true;

            foreach (TKey key in keys)
            {
                var tile = new NeuTile
                {
                    Text = getLabel(key),
                    AccessibleName = getLabel(key),
                    Size = new Size(TileWidth, TileHeight),
                    Margin = Padding.Empty
                };
                tile.Click += (s, e) => ItemClicked?.Invoke(this, key);
                tiles.Add(key, tile);
                Controls.Add(tile);
            }
        }

        public event EventHandler<TKey> ItemClicked;

        /// <summary>Tinggi yang dibutuhkan agar semua tile terlihat pada lebar tertentu</summary>
        public int PreferredHeightFor(int width)
        {
            int columns = Math.Max(1, width / TileWidth);
            int rows = (tiles.Count + columns - 1) / columns;
            return rows * TileHeight;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            int height = PreferredHeightFor(Width);
            if (Height != height) Height = height;
        }

        public void Select(TKey key)
        {
            foreach (var pair in tiles)
            {
                pair.Value.Selected = EqualityComparer<TKey>.Default.Equals(pair.Key, key);
            }
        }

        public void ClearSelection()
        {
            foreach (NeuTile tile in tiles.Values) tile.Selected = false;
        }

        public void SetThumbnail(TKey key, Image thumbnail) => tiles[key].SetThumbnail(thumbnail);
    }
}
