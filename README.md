# PengolahanCitra — Mikro Photoshop

Aplikasi desktop (C# WinForms, .NET Framework 4.7.2) untuk latihan pengolahan citra digital.

## Fitur

- **Buka & simpan** gambar (JPG, PNG, BMP, GIF, TIFF). File asli tidak dikunci selama aplikasi berjalan.
- **Filter** dengan preview thumbnail: Red/Green/Blue channel, Grayscale, Threshold, Negative, Gaussian blur,
  Sharpen, Histogram Equalization (luminance), Smoothing (mean), Contrast Stretching, plus Brightness.
- **Deteksi tepi**: Roberts, Prewitt, Sobel, Canny (threshold Low/High bisa diatur).
- **Aritmatika citra**: tambah, kurang, kali, bagi (gambar kedua otomatis di-resize).
- **Geometri**: rotasi (45°, 90°, 180°, -90°, sudut bebas), translasi, flip horizontal/vertikal.
- **Operasi warna**: klik gambar untuk Color Slicing berdasarkan warna yang diklik, atau Pewarnaan Semu.
- **Histogram** R, G, B, dan Gray yang selalu mengikuti gambar saat ini.
- **Undo / Redo** (maksimal 20 langkah, dibatasi juga oleh memori).
- **Zoom** 5–400% dengan scroll, tombol Fit dan 100%, atau Ctrl + scroll mouse.
- **Simpan .txt**: ekspor nilai piksel (R,G,B) ke file teks.

Pemrosesan berjalan di background thread, jadi window tidak membeku saat memproses gambar besar.

## Shortcut

| Tombol | Fungsi |
|---|---|
| Ctrl+O | Buka gambar |
| Ctrl+S | Simpan gambar |
| Ctrl+Z | Undo |
| Ctrl+Y / Ctrl+Shift+Z | Redo |
| Esc | Tutup panel samping |

## Build & Run

Buka `PengolahanCitra.sln` di Visual Studio lalu tekan F5, atau dari terminal:

```
MSBuild.exe PengolahanCitra.sln /p:Configuration=Release
PengolahanCitra\bin\Release\PengolahanCitra.exe
```

Aplikasi berjalan sebagai proses 64-bit (`Prefer32Bit = false`) agar gambar besar tidak kehabisan memori.

## Struktur Kode

```
PengolahanCitra/
├── Imaging/                 Logika pengolahan citra (tanpa UI)
│   ├── RgbImage.cs          Gambar RGB dalam array 1D + konversi Bitmap (LockBits)
│   ├── ColorMath.cs         Clamp, luminance, lookup table
│   ├── ColorFilters.cs      Channel, grayscale, threshold, negative, brightness, contrast, color slicing, pseudo color
│   ├── Convolution.cs       Kernel 3x3: Gaussian, mean, sharpen
│   ├── EdgeDetection.cs     Roberts, Prewitt, Sobel, Canny
│   ├── Histogram.cs         Histogram, render histogram, equalization
│   ├── Arithmetic.cs        Operasi dua citra
│   ├── Geometry.cs          Rotasi, flip, translasi, resize
│   ├── FilterCatalog.cs     Daftar filter → implementasi (satu tempat)
│   ├── ImageDocument.cs     Gambar asli/saat ini, Undo/Redo, state rotasi
│   └── ImageFile.cs         Load/save gambar dan ekspor teks
├── UI/                      Kontrol tampilan
│   ├── ImageViewer.cs       Viewer dengan zoom & scroll
│   ├── FilterPanel.cs, EdgePanel.cs, TransformPanel.cs, HistogramPanel.cs
│   ├── ColorOperationForm.cs  Popup Color Slicing / Pewarnaan Semu
│   ├── ThumbnailGallery.cs, ThumbnailTile.cs, Theme.cs
├── MainForm.cs              State utama, buka/simpan, undo, menjalankan operasi di background
├── MainForm.Filter.cs       Logika panel Filter
├── MainForm.Edge.cs         Logika panel Edge
├── MainForm.Transform.cs    Logika panel Aritmatika & Geometri
└── MainForm.Designer.cs     Kerangka layout window
```

Semua objek `RgbImage` tidak diubah setelah dibuat; setiap operasi menghasilkan gambar baru.
Karena itu gambar aman dipakai bersama oleh UI thread, background thread, dan riwayat Undo.
