using System;
using System.Collections.Generic;
using System.Linq;

namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// State gambar yang sedang diedit: gambar asli, gambar saat ini, riwayat Undo/Redo, dan state rotasi.
    /// Tidak bergantung pada UI.
    /// </summary>
    public sealed class ImageDocument
    {
        public const int MaxHistorySteps = 20;
        public const long MaxHistoryBytes = 512L * 1024 * 1024;

        private readonly LinkedList<RgbImage> undoHistory = new LinkedList<RgbImage>();
        private readonly Stack<RgbImage> redoHistory = new Stack<RgbImage>();

        // Rotasi selalu dihitung dari gambar sebelum rotasi pertama, supaya 45° + 45° = 90° persis
        // dan kanvas tidak membesar setiap kali diputar
        private RgbImage rotationBase;

        public RgbImage Original { get; private set; }
        public RgbImage Current { get; private set; }
        public string FilePath { get; private set; }
        public int RotationAngle { get; private set; }

        public bool IsLoaded => Current != null;
        public bool CanUndo => undoHistory.Count > 0;
        public bool CanRedo => redoHistory.Count > 0;

        public event EventHandler Changed;

        public void Load(RgbImage image, string filePath)
        {
            Original = image ?? throw new ArgumentNullException(nameof(image));
            Current = image;
            FilePath = filePath;
            undoHistory.Clear();
            redoHistory.Clear();
            ClearRotation();
            OnChanged();
        }

        /// <summary>
        /// Ganti gambar saat ini dengan hasil operasi (bisa di-Undo)
        /// </summary>
        public void Commit(RgbImage result)
        {
            CommitCore(result);
            ClearRotation();
            OnChanged();
        }

        public void ResetToOriginal() => Commit(Original);

        #region Rotation

        /// <summary>
        /// Sumber dan sudut total untuk rotasi berikutnya
        /// </summary>
        public (RgbImage source, int totalAngle) PrepareRotation(int deltaAngle)
        {
            RgbImage source = rotationBase ?? Current;
            int total = (((RotationAngle + deltaAngle) % 360) + 360) % 360;
            return (source, total);
        }

        public void CommitRotation(RgbImage rotated, RgbImage source, int totalAngle)
        {
            CommitCore(rotated);
            rotationBase = source;
            RotationAngle = totalAngle;
            OnChanged();
        }

        private void ClearRotation()
        {
            rotationBase = null;
            RotationAngle = 0;
        }

        #endregion

        #region Undo / Redo

        public void Undo()
        {
            if (!CanUndo) return;

            redoHistory.Push(Current);
            Current = undoHistory.Last.Value;
            undoHistory.RemoveLast();
            ClearRotation();
            OnChanged();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            undoHistory.AddLast(Current);
            Current = redoHistory.Pop();
            ClearRotation();
            OnChanged();
        }

        private void CommitCore(RgbImage result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (!IsLoaded) throw new InvalidOperationException("Belum ada gambar yang dibuka.");

            undoHistory.AddLast(Current);
            redoHistory.Clear();
            Current = result;
            TrimHistory();
        }

        /// <summary>
        /// Buang langkah tertua bila jumlah langkah atau total memori riwayat melewati batas
        /// </summary>
        private void TrimHistory()
        {
            while (undoHistory.Count > MaxHistorySteps ||
                   (undoHistory.Count > 1 && HistoryBytes() > MaxHistoryBytes))
            {
                undoHistory.RemoveFirst();
            }
        }

        private long HistoryBytes() => undoHistory.Sum(image => (long)image.Pixels.Length);

        #endregion

        private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}
