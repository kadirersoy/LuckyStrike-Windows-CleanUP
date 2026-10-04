# LuckyStrike-Windows-CleanUP v3.0 Pro

Windows 10 ve Windows 11 için geliştirilmiş gelişmiş sistem bakım, önbellek temizleme, gizlilik ve optimizasyon aracı.

---

## 📁 Proje Klasör Yapısı

```text
LuckyStrike-Windows-CleanUP/
│
├── 📂 Uygulama/                     -> Doğrudan çift tıklayıp kullanabileceğiniz modern masaüstü uygulaması (.EXE)
│   └── LuckyStrike-CleanUP.exe
│
├── 📂 Klasik-BAT/                   -> Konsol tabanlı toplu iş dosyası (.BAT)
│   └── LuckyStrike-Windows-CleanUP.bat
│
├── 📂 Kaynak-Kodlari/               -> C# WPF projesinin tüm kaynak kodları ve varlıkları
│   ├── App.xaml / App.xaml.cs
│   ├── MainWindow.xaml / MainWindow.xaml.cs
│   ├── CleanUpEngine.cs
│   ├── Converters.cs
│   ├── LuckyStrikeCleanUp.csproj
│   ├── app.manifest
│   ├── app.ico / app.png
│   ├── Properties/
│   └── Derle.bat                    -> Kaynak kodları tek tıkla derleyip "Uygulama" klasörüne aktaran betik
│
├── .gitignore                       -> Git derleme artıklarını süzen yapılandırma
└── README.md                        -> Proje dokümantasyonu
```

---

> [!IMPORTANT]
> **📢 Klasik .BAT Betiği İçin Destek Sonu (End of Support):**  
> v3.0 sürümü itibarıyla klasik `.BAT` betiği dondurulmuş olup, bundan sonraki sürümlerde dağıtımdan kaldırılacaktır. Projenin tüm yeni özellikleri, performans ve stabilite geliştirmeleri yalnızca **`LuckyStrike-CleanUP.exe`** masaüstü uygulaması üzerinden sunulacaktır.

---

## 🚀 Kullanım

### Modern Masaüstü Uygulaması (.EXE)
- **`Uygulama`** klasöründeki **`LuckyStrike-CleanUP.exe`** dosyasına çift tıklayın (UAC izin ekranı açılacaktır).
- 18 temizlik adımından dilediklerinizi seçin.
- **🚀 Temizliği Başlat** butonuna basın.
- Canlı konsoldan adımları takip edebilir, temizlik bitiminde kazanılan toplam disk alanı ve silinen dosya miktarını görüntüleyebilirsiniz.

*(Not: Eski `Klasik-BAT` klasöründeki `.bat` betiği geriye dönük arşiv amaçlı korunmakta olup yeni özellikler almayacaktır.)*

---

## 🛠️ Temizlik ve Bakım Modülleri (18 Adım)

1. **Windows ve Kullanıcı Temp Klasörleri**: `C:\Windows\Temp` ve `%TEMP%`
2. **Windows Update Ön Bellek Dosyaları**: `SoftwareDistribution\Download` (`wuauserv`, `bits`, `dosvc` güvenli kapatılıp açılır)
3. **Dosya Gezgini Geçmişi & RunMRU**: Son kullanılan öğeler ve Çalıştır penceresi kayıtları
4. **Tüm Disklerdeki Geri Dönüşüm Kutuları**: Çöp kutusu tam boşaltma
5. **Windows Olay Günlükleri (Hızlı Motor)**: Hızlı .NET API ile sistem, uygulama ve güvenlik logları
6. **DNS Ön Belleği**: `ipconfig /flushdns` ile eski alan adı çözümleme kayıtları
7. **[Gizlilik] Son Açılan Dosyaları Kapat**: `ShowRecent` ve `ShowFrequent` gezgin geçmiş takibi (İsteğe bağlı)
8. **[Gizlilik] Başlat Menüsü Uygulama Takibi**: `Start_TrackProgs` en çok kullanılan uygulama takibi (İsteğe bağlı)
9. **Hata Raporları & Dökümleri**: `MEMORY.DMP`, `Minidump`, `WER` çökme raporları
10. **Teslim İyileştirme (Delivery Optimization)**: P2P Windows Update indirme ön belleği
11. **DirectX ve GPU Shader Ön Belleği**: NVIDIA GLCache, AMD DXCache, Intel ShaderCache ve D3DSCache
12. **Küçük Resim ve Simge Ön Belleği**: `IconCache.db` ve `thumbcache_*.db` (Kilitleri aşmak için Explorer anlık yenilenir)
13. **Windows Prefetch**: `C:\Windows\Prefetch` eski uygulama ön yükleme verileri
14. **ARP Tablosu ve Ağ Ön Belleği**: `arp -d *`, `nbtstat -R`, `nbtstat -RR`
15. **Windows Defender Tarama Geçmişi**: Zararlı geçmişi ve logları (Varsayılan: Kapalı)
16. **DISM Bileşen Temizliği**: `dism /online /cleanup-image /startcomponentcleanup` (WinSxS temizliği)
17. **Web Tarayıcı Önbellekleri**: Google Chrome, Microsoft Edge, Brave, Opera geçici web önbellekleri (Şifreler korunur)
18. **Eski Windows Sürüm Kalıntıları**: `C:\Windows.old`, `C:\$Windows.~BT` ve `C:\$Windows.~WS`

---

## ⚡ Ek Araçlar & Güvenlik

- **🛡️ Sistem Geri Yükleme Noktası**: Kritik işlemler öncesinde tek tıkla Windows Geri Yükleme Noktası oluşturma.
- **💾 Kazanılan Disk Alanı**: Temizlik öncesi ve sonrası analiz edilerek kazanılan net disk alanı (MB / GB) gösterilir.
- **📦 Winget ile Güncelle**: Tek tıkla `winget upgrade --all` ile tüm kurulu uygulamaları güncelleme.
- **🔄 Güç Menüsü**: Yanlışlıkla basmaları önlemek için onay pencereli Yeniden Başlat & Kapat butonları.
- **📋 Canlı Log**: Otomatik oluşturulan detaylı log dosyası: `%APPDATA%\LuckyCleanTemp\LuckyStrike-Windows-CleanUP_Log.txt`.
