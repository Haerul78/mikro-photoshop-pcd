using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PengolahanCitra.UI
{
    /// <summary>
    /// Kumpulan ThumbnailTile; hanya satu yang terpilih pada satu waktu
    /// </summary>
    public class ThumbnailGallery<TKey> : FlowLayoutPanel
    {
        private readonly Dictionary<TKey, ThumbnailTile> tiles = new Dictionary<TKey, ThumbnailTile>();

        public ThumbnailGallery(IEnumerable<TKey> keys, Func<TKey, string> getLabel)
        {
            Dock = DockStyle.Top;
            AutoScroll = true;
            Padding = new Padding(8, 0, 0, 0);

            foreach (TKey key in keys)
            {
                var tile = new ThumbnailTile(getLabel(key), key);
                tile.Click += (s, e) => ItemClicked?.Invoke(this, key);
                tiles.Add(key, tile);
                Controls.Add(tile);
            }
        }

        public event EventHandler<TKey> ItemClicked;

        public IEnumerable<TKey> Keys => tiles.Keys;

        /// <summary>Tinggi yang dibutuhkan agar semua tile terlihat untuk lebar tertentu</summary>
        public int PreferredHeightFor(int width)
        {
            int columns = Math.Max(1, (width - Padding.Horizontal) / 102);
            int rows = (tiles.Count + columns - 1) / columns;
            return rows * 88 + 4;
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
            foreach (ThumbnailTile tile in tiles.Values) tile.Selected = false;
        }

        public void SetThumbnail(TKey key, Image thumbnail) => tiles[key].SetThumbnail(thumbnail);

        public void ClearThumbnails()
        {
            foreach (ThumbnailTile tile in tiles.Values) tile.SetThumbnail(null);
        }
    }
}
