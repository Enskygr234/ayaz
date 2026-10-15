<div align="center">

![ayaz](docs/afis.svg)

[![İndir](https://img.shields.io/badge/⬇_İndir-Windows_64_bit-8ecae6?style=for-the-badge)](https://github.com/Enskygr234/ayaz/releases/latest)
[![.NET 8](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Lisans](https://img.shields.io/badge/lisans-MIT-e63946?style=for-the-badge)](LICENSE)

</div>

Terminalde hava durumu ve en önemli sorunun cevabı: **kar ne zaman?**

```
> ayaz tromso -g 5

  Tromso, Norveç · 15:00

     \  /         Parçalı bulutlu
   _ /''.-.       4.2 °C  (hissedilen -3.3 °C)
     \_(   ).     Rüzgâr 32 km/sa
     /(___(__)    Nem %52

  Sal 13 Eki  Kapalı                    0° /   4°
  Çar 14 Eki  Çisenti                   4° /   8°   1.3 mm
  Per 15 Eki  Çisenti                   7° /  10°   2 mm
  Cum 16 Eki  Çisenti                   3° /   7°   9.3 mm
  Cmt 17 Eki  Çisenti                   1° /   2°   12 mm

  * 9 gün sonra (Per 22 Eki) kar bekleniyor: 0.4 cm
```

## Özellikler

- **Dünyadaki her şehir** — adını yazman yeter, koordinat aramana gerek yok
- **Anlık durum** — sıcaklık, hissedilen sıcaklık, rüzgâr, nem ve küçük bir çizim
- **16 güne kadar tahmin** — günlük en düşük / en yüksek sıcaklık ve yağış
- **Kar ne zaman?** — önümüzdeki 16 günü tarar, ilk kar yağışını haber verir
- **API anahtarı gerekmez** — veriler [Open-Meteo](https://open-meteo.com)'dan gelir

## Kurulum

### Hazır exe

1. [Releases](https://github.com/Enskygr234/ayaz/releases/latest) sayfasından `ayaz.exe` dosyasını indir.
2. İstediğin klasöre koy, o klasörde terminal açıp `ayaz` yaz.

Çalışması için [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) kurulu olmalı. Windows "bilinmeyen yayıncı" uyarısı gösterirse **Ek bilgi → Yine de çalıştır** de; exe imzalı olmadığı için çıkar.

### Kaynaktan derle

[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) gerekir.

```
git clone https://github.com/Enskygr234/ayaz.git
cd ayaz
build.bat
```

`EXE\ayaz.exe` oluşur. Derlemeden denemek için: `dotnet run -- erzurum`

## Kullanım

```
ayaz [şehir] [seçenekler]
```

| Komut | Ne yapar |
|---|---|
| `ayaz` | Ankara için hava durumu |
| `ayaz erzurum` | Erzurum için hava durumu |
| `ayaz "new york" -g 7` | 7 günlük tahmin |
| `ayaz kars --renksiz` | Renk kullanmadan yaz |

| Seçenek | Açıklama |
|---|---|
| `-g`, `--gun <sayı>` | Kaç günlük tahmin gösterilsin (1-16, varsayılan 3) |
| `--renksiz` | Renk kullanma (`NO_COLOR` ortam değişkeni de aynı işi görür) |
| `-s`, `--surum` | Sürümü yaz |
| `-h`, `--yardim` | Yardımı göster |

Çıkış kodları: `0` başarılı, `1` hatalı kullanım ya da şehir bulunamadı, `2` sunucuya ulaşılamadı.

## Lisans

[MIT](LICENSE) © Winterdeveloper

<!-- ENS:ALT -->
---

<div align="center">

**❄️ ENS Developer'ın diğer projeleri**

🖥️ Windows: [Buz Arşiv](https://github.com/Enskygr234/buz-arsiv) · [Ayaz Launcher](https://github.com/Enskygr234/Ayaz-launcher) · [Masaüstü Kar](https://github.com/Enskygr234/masaustu-kar) · [Klavye Ayazı](https://github.com/Enskygr234/klavye-ayazi) · [Güvenlik Kilidi](https://github.com/Enskygr234/GuvenlikKilidi)

[🌐 Bütün projeler](https://enskygr234.github.io/ensdewloper.github.io/) · [👤 GitHub profilim](https://github.com/Enskygr234)

<sub>Kış gelir, kod yazılır.</sub>

</div>
<!-- /ENS:ALT -->
