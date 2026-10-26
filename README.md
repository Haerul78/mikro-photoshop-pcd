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

- **Before / After**: kanvas dibelah garis yang bisa digeser; kiri gambar asli, kanan hasil atau preview.

Pemrosesan berjalan di background thread, jadi window tidak membeku saat memproses gambar besar.

## Tampilan

Gaya neumorphism: satu warna permukaan `#E0E5EC`, bentuk dibedakan oleh shadow terang (kiri-atas)
dan gelap (kanan-bawah). Tombol dan panel timbul; tombol aktif, slider, input, dan kanvas cekung.
Satu warna aksen `#4A7BF7` untuk state aktif, isi slider, dan focus ring.

```
┌ Top bar: Buka · Simpan · Ekspor .txt · Undo · Redo · Reset ............ Before / After ┐
│ Rail alat │            Kanvas (bidang cekung)              │ Histogram            │
│  ◉ Adjust │                                                 │ Alat aktif:          │
│  ◯ Edge   │                                                 │  Penyesuaian & filter│
│  ◯ Geo    │                                                 │  / Deteksi tepi /    │
│  ◯ Aritm. │                                                 │  Geometri / Aritmatika│
└ Status bar: nama file · ukuran · status ........................... zoom · Fit · 100% ┘
```

Klik gambar di kanvas untuk membuka popup Color Slicing / Pewarnaan Semu.
Gambar juga bisa dibuka dengan menyeret file ke kanvas.

## Shortcut

| Tombol | Fungsi |
|---|---|
| Ctrl+O | Buka gambar |
| Ctrl+S | Simpan gambar |
| Ctrl+Z | Undo |
| Ctrl+Y / Ctrl+Shift+Z | Redo |
| Esc | Batalkan preview |
| Ctrl + scroll mouse | Zoom |
| ← / → (kanvas fokus, Before/After aktif) | Geser garis pembanding |

## Build & Run

Project memakai format SDK-style dengan target .NET Framework 4.7.2, jadi bisa dijalankan dari
VS Code atau terminal (butuh .NET SDK), maupun dari Visual Studio (F5).

```
dotnet build
dotnet run --project PengolahanCitra
```

Build Release:

```
dotnet build -c Release
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
│   ├── Histogram.cs         Histogram, equalization
│   ├── Arithmetic.cs        Operasi dua citra
│   ├── Geometry.cs          Rotasi, flip, translasi, resize
│   ├── FilterCatalog.cs     Daftar filter → implementasi (satu tempat)
│   ├── ImageDocument.cs     Gambar asli/saat ini, Undo/Redo, state rotasi
│   └── ImageFile.cs         Load/save gambar dan ekspor teks
├── UI/
│   ├── Neu/                 Design system neumorphism (tanpa logika aplikasi)
│   │   ├── NeuTheme.cs      Token warna, radius, kedalaman shadow, font, ikon
│   │   ├── NeuPainter.cs    Rounded rect + soft shadow (blur sungguhan, di-cache)
│   │   ├── NeuControl.cs    Dasar kontrol: double buffered, focus ring
│   │   ├── NeuPanel, NeuButton, NeuSlider, NeuToggle, NeuSegmented, NeuNumberBox
│   │   ├── NeuTile, NeuGallery  Thumbnail dan grid thumbnail
│   │   ├── HistogramView.cs Grafik histogram RGB / luminance
│   │   └── NeuLayout.cs     Helper menyusun kontrol di inspector
│   ├── ImageViewer.cs       Kanvas: zoom, scroll, Before/After, drag & drop
│   ├── InspectorSection.cs  Dasar bagian kolom kanan
│   ├── HistogramPanel, FilterPanel, EdgePanel, GeometryPanel, ArithmeticPanel
│   └── ColorOperationForm.cs  Popup Color Slicing / Pewarnaan Semu
├── MainForm.cs              State utama, top bar, rail alat, buka/simpan, undo, operasi di background
├── MainForm.Filter.cs       Logika alat Penyesuaian & filter
├── MainForm.Edge.cs         Logika alat Deteksi tepi
├── MainForm.Transform.cs    Logika alat Geometri & Aritmatika
└── MainForm.Designer.cs     Kerangka layout window
```

Semua objek `RgbImage` tidak diubah setelah dibuat; setiap operasi menghasilkan gambar baru.
Karena itu gambar aman dipakai bersama oleh UI thread, background thread, dan riwayat Undo.
