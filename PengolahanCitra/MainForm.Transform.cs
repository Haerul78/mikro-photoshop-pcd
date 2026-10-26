using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PengolahanCitra.Imaging;

namespace PengolahanCitra
{
    /// <summary>
    /// Alat Geometri (rotasi, translasi, flip) dan Aritmatika citra
    /// </summary>
    public partial class MainForm
    {
        private void InitializeTransformPanels()
        {
            arithmeticPanel.ArithmeticRequested += (s, operation) => ChooseSecondImage(operation);
            geometryPanel.RotateRequested += (s, angle) => Fire(RotateAsync(angle));
            geometryPanel.TranslateRequested += (s, offset) => Fire(ApplyOperationAsync("Menggeser gambar...",
                src => Geometry.Translate(src, offset.X, offset.Y),
                $"Translasi (X: {offset.X}, Y: {offset.Y}) diterapkan"));
            geometryPanel.FlipRequested += (s, horizontal) => Fire(ApplyOperationAsync("Flip gambar...",
                horizontal ? (Func<RgbImage, RgbImage>)Geometry.FlipHorizontal : Geometry.FlipVertical,
                horizontal ? "Flip horizontal diterapkan" : "Flip vertikal diterapkan"));
        }

        private void ChooseSecondImage(ArithmeticOperation operation)
        {
            if (!EnsureImageLoaded()) return;

            using (var dialog = new OpenFileDialog { Filter = ImageFile.OpenFilter, Title = "Pilih gambar kedua" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Fire(ApplyArithmeticAsync(operation, dialog.FileName));
            }
        }

        /// <summary>
        /// Gambar kedua di-resize ke ukuran gambar utama bila ukurannya berbeda
        /// </summary>
        internal async Task ApplyArithmeticAsync(ArithmeticOperation operation, string secondImagePath)
        {
            Size secondSize = Size.Empty;
            string label = Arithmetic.GetLabel(operation);
            RgbImage before = document.Current;

            await ApplyOperationAsync($"{label} citra...", src =>
            {
                RgbImage second = ImageFile.Load(secondImagePath);
                secondSize = new Size(second.Width, second.Height);
                return Arithmetic.Apply(src, Geometry.Resize(second, src.Width, src.Height), operation);
            }, $"{label} citra diterapkan");

            RgbImage current = document.Current;
            if (current != before && (secondSize.Width != current.Width || secondSize.Height != current.Height))
            {
                SetStatus($"{label} citra diterapkan (gambar kedua {secondSize.Width} x {secondSize.Height} di-resize ke {current.Width} x {current.Height})");
            }
        }

        /// <summary>
        /// Rotasi bersifat kumulatif, tapi selalu dihitung dari gambar sebelum rotasi pertama
        /// sehingga kualitas tidak turun dan kanvas tidak terus membesar
        /// </summary>
        internal async Task RotateAsync(int deltaAngle)
        {
            if (isBusy || !EnsureImageLoaded()) return;

            var (source, totalAngle) = document.PrepareRotation(deltaAngle);
            RenderedImage rendered = await RunBusyAsync("Memutar gambar...", () => Render(Geometry.Rotate(source, totalAngle)));
            if (rendered == null) return;

            document.CommitRotation(rendered.Image, source, totalAngle);
            ShowRendered(rendered);
            SetStatus($"Rotasi total: {totalAngle}°");
        }
    }
}
