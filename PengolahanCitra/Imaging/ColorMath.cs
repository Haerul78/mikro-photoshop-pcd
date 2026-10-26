namespace PengolahanCitra.Imaging
{
    /// <summary>
    /// Fungsi matematika kecil yang dipakai di semua operasi
    /// </summary>
    public static class ColorMath
    {
        public static byte ClampToByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        /// <summary>
        /// Double dipotong ke int (bukan dibulatkan) lalu di-clamp ke 0-255
        /// </summary>
        public static byte ClampToByte(double value) => ClampToByte((int)value);

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Grayscale dengan rumus luminance (ITU-R BT.601).
        /// Dipakai oleh semua modul: filter Grayscale, Threshold, histogram Gray, Edge, dan Equalizer
        /// </summary>
        public static byte Luminance(byte r, byte g, byte b) => (byte)(0.299 * r + 0.587 * g + 0.114 * b);

        /// <summary>
        /// Lookup table 256 entri dari sebuah fungsi intensitas
        /// </summary>
        public static byte[] CreateLut(System.Func<int, int> map)
        {
            var lut = new byte[256];
            for (int i = 0; i < 256; i++) lut[i] = ClampToByte(map(i));
            return lut;
        }
    }
}
