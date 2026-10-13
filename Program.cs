using System.Globalization;
using System.Text;
using System.Text.Json;

// ayaz — terminalde hava durumu ve "kar ne zaman?" sorusunun cevabı.
// Veriler Open-Meteo'dan gelir; API anahtarı gerekmez.

Console.OutputEncoding = Encoding.UTF8;

string sehir = "Ankara";
int gun = 3;
bool renkli = !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") is null;
var adParcalari = new List<string>();

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-h" or "--yardim" or "--help":
            Yardim();
            return 0;
        case "-s" or "--surum":
            Console.WriteLine("ayaz 1.0.0");
            return 0;
        case "--renksiz":
            renkli = false;
            break;
        case "-g" or "--gun":
            if (i + 1 >= args.Length || !int.TryParse(args[++i], out gun) || gun < 1 || gun > 16)
            {
                Console.Error.WriteLine("--gun için 1 ile 16 arasında bir sayı ver.");
                return 1;
            }
            break;
        default:
            if (args[i].StartsWith('-'))
            {
                Console.Error.WriteLine($"Bilinmeyen seçenek: {args[i]}  (yardım için: ayaz --yardim)");
                return 1;
            }
            adParcalari.Add(args[i]);
            break;
    }
}
if (adParcalari.Count > 0) sehir = string.Join(' ', adParcalari);

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
var inv = CultureInfo.InvariantCulture;

try
{
    // 1) Şehir adını koordinata çevir
    var yerUrl = "https://geocoding-api.open-meteo.com/v1/search?count=1&language=tr&format=json&name="
                 + Uri.EscapeDataString(sehir);
    using var yerDoc = JsonDocument.Parse(await http.GetStringAsync(yerUrl));
    if (!yerDoc.RootElement.TryGetProperty("results", out var sonuclar) || sonuclar.GetArrayLength() == 0)
    {
        Console.Error.WriteLine($"\"{sehir}\" diye bir yer bulamadım.");
        return 1;
    }
    var yer = sonuclar[0];
    double enlem = yer.GetProperty("latitude").GetDouble();
    double boylam = yer.GetProperty("longitude").GetDouble();
    string yerAdi = yer.GetProperty("name").GetString() ?? sehir;
    string? ulke = yer.TryGetProperty("country", out var u) ? u.GetString() : null;

    // 2) Anlık durum ve 16 günlük tahmin
    var havaUrl = "https://api.open-meteo.com/v1/forecast"
        + $"?latitude={enlem.ToString(inv)}&longitude={boylam.ToString(inv)}"
        + "&current=temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,weather_code"
        + "&daily=weather_code,temperature_2m_max,temperature_2m_min,snowfall_sum,precipitation_sum"
        + "&timezone=auto&forecast_days=16";
    using var havaDoc = JsonDocument.Parse(await http.GetStringAsync(havaUrl));
    var simdi = havaDoc.RootElement.GetProperty("current");
    var gunluk = havaDoc.RootElement.GetProperty("daily");

    var (aciklama, tur) = Hava.Coz(Sayi(simdi.GetProperty("weather_code")) is double anlikKod ? (int)anlikKod : -1);
    string saat = simdi.GetProperty("time").GetString() is { Length: >= 16 } z ? z[11..16] : "";

    Console.WriteLine();
    Yaz($"  {yerAdi}", ConsoleColor.White);
    Yaz((ulke is null ? "" : $", {ulke}") + (saat == "" ? "" : $" · {saat}") + "\n\n", ConsoleColor.DarkGray);

    string[] resim = Hava.Resim(tur);
    string[] bilgi =
    {
        aciklama,
        $"{Ondalik(simdi.GetProperty("temperature_2m"))} °C  (hissedilen {Ondalik(simdi.GetProperty("apparent_temperature"))} °C)",
        $"Rüzgâr {Ondalik(simdi.GetProperty("wind_speed_10m"))} km/sa",
        $"Nem %{Tam(simdi.GetProperty("relative_humidity_2m"))}",
    };
    for (int i = 0; i < resim.Length; i++)
    {
        Yaz("  " + resim[i].PadRight(16), Hava.Renk(tur));
        Yaz((i < bilgi.Length ? bilgi[i] : "") + "\n", i == 0 ? ConsoleColor.White : ConsoleColor.Gray);
    }
    Console.WriteLine();

    // 3) Günlük satırlar
    var tarihler = gunluk.GetProperty("time");
    var kodlar = gunluk.GetProperty("weather_code");
    var enYuksek = gunluk.GetProperty("temperature_2m_max");
    var enDusuk = gunluk.GetProperty("temperature_2m_min");
    var yagis = gunluk.GetProperty("precipitation_sum");
    var kar = gunluk.GetProperty("snowfall_sum");
    int toplamGun = tarihler.GetArrayLength();

    for (int i = 0; i < Math.Min(gun, toplamGun); i++)
    {
        var tarih = DateOnly.ParseExact(tarihler[i].GetString()!, "yyyy-MM-dd", inv);
        var (gunAciklama, gunTur) = Hava.Coz(Sayi(kodlar[i]) is double k ? (int)k : -1);
        Yaz($"  {Hava.GunAdi(tarih),-12}", ConsoleColor.Gray);
        Yaz($"{gunAciklama,-23}", Hava.Renk(gunTur));
        Yaz($"{Tam(enDusuk[i]),4}° / {Tam(enYuksek[i]),3}°", ConsoleColor.White);
        double karCm = Sayi(kar[i]) ?? 0, yagisMm = Sayi(yagis[i]) ?? 0;
        if (karCm > 0) Yaz($"   {karCm.ToString("0.#", inv)} cm kar", ConsoleColor.Cyan);
        else if (yagisMm > 0) Yaz($"   {yagisMm.ToString("0.#", inv)} mm", ConsoleColor.DarkCyan);
        Console.WriteLine();
    }
    Console.WriteLine();

    // 4) Kar ne zaman?
    int karGunu = -1;
    for (int i = 0; i < toplamGun; i++)
        if ((Sayi(kar[i]) ?? 0) > 0) { karGunu = i; break; }

    if (karGunu < 0)
        Yaz($"  Önümüzdeki {toplamGun} günde kar görünmüyor.\n", ConsoleColor.DarkGray);
    else
    {
        var tarih = DateOnly.ParseExact(tarihler[karGunu].GetString()!, "yyyy-MM-dd", inv);
        string neZaman = karGunu switch { 0 => "Bugün", 1 => "Yarın", _ => $"{karGunu} gün sonra ({Hava.GunAdi(tarih)})" };
        Yaz($"  * {neZaman} kar bekleniyor: {(Sayi(kar[karGunu]) ?? 0).ToString("0.#", inv)} cm\n", ConsoleColor.Cyan);
    }
    Console.WriteLine();
    return 0;
}
catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
{
    Console.Error.WriteLine("Hava durumu sunucusuna ulaşamadım. İnternet bağlantını kontrol et.");
    return 2;
}

void Yaz(string metin, ConsoleColor renk)
{
    if (renkli) Console.ForegroundColor = renk;
    Console.Write(metin);
    if (renkli) Console.ResetColor();
}

// API bazı değerler için null döndürebilir
static double? Sayi(JsonElement e) => e.ValueKind == JsonValueKind.Number ? e.GetDouble() : null;
static string Ondalik(JsonElement e) => Sayi(e)?.ToString("0.#", CultureInfo.InvariantCulture) ?? "?";
static string Tam(JsonElement e) => Sayi(e) is double d ? Math.Round(d).ToString("0", CultureInfo.InvariantCulture) : "?";

static void Yardim() => Console.WriteLine("""
    ayaz — terminalde hava durumu ve "kar ne zaman?"

    Kullanım:
      ayaz [şehir] [seçenekler]

    Örnekler:
      ayaz                  Ankara için hava durumu
      ayaz erzurum          Erzurum için hava durumu
      ayaz "new york" -g 7  7 günlük tahmin

    Seçenekler:
      -g, --gun <sayı>   Kaç günlük tahmin gösterilsin (1-16, varsayılan 3)
          --renksiz      Renk kullanma
      -s, --surum        Sürümü yaz
      -h, --yardim       Bu yardımı göster
    """);

enum Tur { Acik, AzBulutlu, Bulutlu, Sis, Yagmur, Kar, Firtina, Bilinmiyor }

static class Hava
{
    static readonly string[] Gunler = { "Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt" };
    static readonly string[] Aylar = { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

    public static string GunAdi(DateOnly t) => $"{Gunler[(int)t.DayOfWeek]} {t.Day,2} {Aylar[t.Month - 1]}";

    // WMO hava kodu -> Türkçe açıklama ve çizim türü
    public static (string aciklama, Tur tur) Coz(int kod) => kod switch
    {
        0 => ("Açık", Tur.Acik),
        1 => ("Az bulutlu", Tur.AzBulutlu),
        2 => ("Parçalı bulutlu", Tur.AzBulutlu),
        3 => ("Kapalı", Tur.Bulutlu),
        45 or 48 => ("Sisli", Tur.Sis),
        51 or 53 or 55 => ("Çisenti", Tur.Yagmur),
        56 or 57 => ("Donan çisenti", Tur.Yagmur),
        61 => ("Hafif yağmur", Tur.Yagmur),
        63 => ("Yağmur", Tur.Yagmur),
        65 => ("Kuvvetli yağmur", Tur.Yagmur),
        66 or 67 => ("Donan yağmur", Tur.Yagmur),
        71 => ("Hafif kar", Tur.Kar),
        73 => ("Kar", Tur.Kar),
        75 => ("Yoğun kar", Tur.Kar),
        77 => ("Kar taneleri", Tur.Kar),
        80 or 81 => ("Sağanak", Tur.Yagmur),
        82 => ("Kuvvetli sağanak", Tur.Yagmur),
        85 or 86 => ("Kar sağanağı", Tur.Kar),
        95 => ("Gök gürültülü fırtına", Tur.Firtina),
        96 or 99 => ("Dolulu fırtına", Tur.Firtina),
        _ => ("Bilinmiyor", Tur.Bilinmiyor),
    };

    public static ConsoleColor Renk(Tur tur) => tur switch
    {
        Tur.Acik or Tur.AzBulutlu => ConsoleColor.Yellow,
        Tur.Yagmur => ConsoleColor.Blue,
        Tur.Kar => ConsoleColor.Cyan,
        Tur.Firtina => ConsoleColor.Magenta,
        _ => ConsoleColor.Gray,
    };

    public static string[] Resim(Tur tur) => tur switch
    {
        Tur.Acik => new[]
        {
            @"    \   /",
            @"     .-.",
            @"  - (   ) -",
            @"     `-'",
            @"    /   \",
        },
        Tur.AzBulutlu => new[]
        {
            @"   \  /",
            @" _ /''.-.",
            @"   \_(   ).",
            @"   /(___(__)",
            @"",
        },
        Tur.Bulutlu => new[]
        {
            @"",
            @"     .--.",
            @"  .-(    ).",
            @" (___.__)__)",
            @"",
        },
        Tur.Sis => new[]
        {
            @"",
            @" _ - _ - _ -",
            @"  _ - _ - _",
            @" _ - _ - _ -",
            @"",
        },
        Tur.Yagmur => new[]
        {
            @"     .-.",
            @"    (   ).",
            @"   (___(__)",
            @"    ' ' ' '",
            @"   ' ' ' '",
        },
        Tur.Kar => new[]
        {
            @"     .-.",
            @"    (   ).",
            @"   (___(__)",
            @"    *  *  *",
            @"   *  *  *",
        },
        Tur.Firtina => new[]
        {
            @"     .-.",
            @"    (   ).",
            @"   (___(__)",
            @"    /_ ' /_",
            @"     / '  /",
        },
        _ => new[]
        {
            @"",
            @"     ???",
            @"",
            @"",
            @"",
        },
    };
}
