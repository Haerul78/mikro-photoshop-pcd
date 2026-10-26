using System;

namespace PengolahanCitra.Imaging
{
    public enum FilterKind
    {
        Original,
        Red,
        Green,
        Blue,
        Grayscale,
        Threshold,
        Negative,
        Gaussian,
        Sharpen,
        Equalizer,
        Smoothing,
        Contrast
    }

    /// <summary>
    /// Satu-satunya tempat yang memetakan jenis filter ke implementasinya.
    /// Dipakai untuk preview ukuran penuh maupun thumbnail.
    /// </summary>
    public static class FilterCatalog
    {
        public const int ThresholdValue = 128;
        public const int GaussianPasses = 5;
        public const int SmoothingPasses = 3;
        public const bool UseStrongSharpen = true;
        public const int SharpenPasses = 1;

        /// <summary>
        /// Terapkan filter. sizeRatio = ukuran gambar ini dibanding gambar asli (untuk thumbnail),
        /// supaya kekuatan blur pada thumbnail sebanding dengan hasil sebenarnya.
        /// </summary>
        public static RgbImage Apply(RgbImage source, FilterKind kind, double sizeRatio = 1.0)
        {
            switch (kind)
            {
                case FilterKind.Original: return source;
                case FilterKind.Red: return ColorFilters.RedChannel(source);
                case FilterKind.Green: return ColorFilters.GreenChannel(source);
                case FilterKind.Blue: return ColorFilters.BlueChannel(source);
                case FilterKind.Grayscale: return ColorFilters.Grayscale(source);
                case FilterKind.Threshold: return ColorFilters.Threshold(source, ThresholdValue);
                case FilterKind.Negative: return ColorFilters.Negative(source);
                case FilterKind.Gaussian: return Convolution.GaussianBlur(source, ScalePasses(GaussianPasses, sizeRatio));
                case FilterKind.Sharpen: return Convolution.Sharpen(source, UseStrongSharpen, SharpenPasses);
                case FilterKind.Equalizer: return Histogram.EqualizeLuminance(source);
                case FilterKind.Smoothing: return Convolution.MeanBlur(source, ScalePasses(SmoothingPasses, sizeRatio));
                case FilterKind.Contrast: return ColorFilters.ContrastStretching(source);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        /// <summary>
        /// Variansi blur sebanding dengan jumlah pass, sedangkan radius sebanding dengan ukuran gambar,
        /// jadi jumlah pass diskalakan dengan kuadrat rasio ukuran
        /// </summary>
        private static int ScalePasses(int passes, double sizeRatio)
        {
            if (sizeRatio >= 1.0) return passes;
            return Math.Max(1, (int)Math.Round(passes * sizeRatio * sizeRatio));
        }
    }
}
