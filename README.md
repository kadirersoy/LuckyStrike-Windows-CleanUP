# LuckyStrike Windows CleanUP

<p align="center">
  <img src="docs/images/app_icon.png" width="96" height="96" alt="LuckyStrike Icon" />
</p>

<p align="center">
  <strong>Windows 10 ve Windows 11 için yüksek performanslı, akıllı ve modern sistem bakım & temizlik platformu.</strong>
</p>

<p align="center">
  <a href="https://github.com/kadirersoy/LuckyStrike-Windows-CleanUP/releases"><img src="https://img.shields.io/badge/Release-v3.1.0-00D2FF?style=for-the-badge&logo=windows" alt="Release v3.1.0" /></a>
  <a href="https://github.com/kadirersoy/LuckyStrike-Windows-CleanUP"><img src="https://img.shields.io/badge/Language-C%23_WPF-239120?style=for-the-badge&logo=c-sharp" alt="C#" /></a>
  <a href="https://github.com/kadirersoy/LuckyStrike-Windows-CleanUP"><img src="https://img.shields.io/badge/Platform-Windows_10_%2F_11-0078D6?style=for-the-badge&logo=windows11" alt="Platform" /></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-F58220?style=for-the-badge" alt="License" /></a>
</p>

---

## 📦 Dağıtım & İndirme Seçenekleri

| Paket Türü | Dosya Adı | Açıklama |
| :--- | :--- | :--- |
| **🚀 Windows Installer (Önerilen)** | `LuckyStrike_CleanUP_Setup.msi` | Başlat menüsü ve masaüstü kısayolları içeren tek tıkla kurumsal kurulum paketi. |
| **⚡ Taşınabilir Sürüm (Portable)** | `LuckyStrike-CleanUP.exe` | Kurulum gerektirmeyen, doğrudan çalıştırılabilir bağımsız masaüstü sürümü. |

> 📥 En güncel derlemeleri doğrudan [Releases](https://github.com/kadirersoy/LuckyStrike-Windows-CleanUP/releases) sayfasından edinebilirsiniz.

---

> [!WARNING]
> ### 📢 Klasik .BAT Betiği İçin Destek Sonu (End of Support)
> v3.0 sürümü itibarıyla klasik `.BAT` betiğinin geliştirilmesi ve dağıtımı sonlandırılmıştır. Gelecek tüm geliştirmeler, yeni modüller ve stabilite optimizasyonları yalnızca **WPF masaüstü motoru** üzerinden sunulacaktır.

---

## 🚀 v3.1.0 ile Neler Değişti? (Sürüm Farkları)

* **💾 Canlı Sürücü Durum Rozeti:** Ana pencere başlığına anlık `💾 C: [Boş Alan] / [Toplam Alan]` rozeti entegre edildi. Temizlik öncesi ve sonrasında disk durumu otomatik olarak yenilenir.
* **⚡ Yerel .NET EventLogSession Motoru:** Olay günlükleri harici PowerShell çağırmadan doğrudan C# yerel `.NET` API'si üzerinden anlık (<0.1 saniye) olarak temizlenir.
* **🗑️ Çoklu Sürücü Geri Dönüşüm Kutusu:** Yalnızca C: değil; sistemde takılı tüm hazır disklerin (`C:`, `D:`, `E:` vb.) çöp kutusu verileri tek seferde boşaltılır.
* **🌐 Genişletilmiş Tarayıcı & WebView2 Desteği:** Google Chrome, Microsoft Edge ve Brave (tüm profiller), Mozilla Firefox (`cache2`, `startupCache`), Opera, Vivaldi ve modern Windows uygulamalarının gömülü motoru olan **Edge WebView2** önbellekleri temizlik kapsamına alındı.
* **🎮 Yeni Nesil GPU Shader Önbellekleri:** DirectX D3DSCache'in yanı sıra NVIDIA (`DXCache`, `GLCache`), AMD (`DXCache`, `GLCache`) ve Intel (`ShaderCache`) önbellekleri eklendi.
* **🔄 $GetCurrent Yükseltme Kalıntıları:** Windows Güncelleme Asistanı ve yükseltme süreçlerinin diske bıraktığı `C:\$GetCurrent` artık otomatik taranıp temizlenir.
* **📐 Pencere Boyutlandırma & DPI Uyumu:** Serbest boyutlandırma tutamacı (`Resize Grip`), başlık çubuğuna çift tıklama ile tam ekran/normale dönme ve Per-Monitor V2 yüksek DPI desteği eklendi.
* **🛡️ Süreç Güvenliği & Non-Blocking UI:** Konsol çıktısı ve loglamalar `Dispatcher.BeginInvoke` asenkron mimarisine geçirilerek arayüz takılmaları ve pipe kilitlenme riskleri tamamen giderildi.
* **🏷️ Sürüm Bağımsız Dinamik Arayüz:** Arayüz başlıkları ve log motoru sürüm bağımsız dinamik kurumsal yapıya geçirildi.

---

## 🛠️ Temizlik ve Bakım Modülleri (18 Adım)

1. **Windows ve Kullanıcı Temp Klasörleri**: `C:\Windows\Temp` ve `%TEMP%`
2. **Windows Update Ön Bellek Dosyaları**: `SoftwareDistribution\Download` (`wuauserv`, `bits`, `dosvc` güvenli kapatılıp açılır)
3. **Dosya Gezgini Geçmişi & RunMRU**: Son kullanılan öğeler ve Çalıştır penceresi kayıtları
4. **Tüm Disklerdeki Geri Dönüşüm Kutuları**: Tüm sürücülerdeki silinmiş çöp kutusu verileri
5. **Windows Olay Günlükleri (Hızlı Motor)**: Hızlı .NET API ile sistem, uygulama ve güvenlik günlükleri
6. **DNS Ön Belleği**: `ipconfig /flushdns` ile eski alan adı çözümleme kayıtları
7. **[Gizlilik] Son Açılan Dosyaları Kapat**: `ShowRecent` ve `ShowFrequent` gezgin geçmiş takibi (İsteğe bağlı)
8. **[Gizlilik] Başlat Menüsü Uygulama Takibi**: `Start_TrackProgs` en çok kullanılan uygulama takibi (İsteğe bağlı)
9. **Hata Raporları & Dökümleri**: `MEMORY.DMP`, `Minidump`, `WER` çökme raporları
10. **Teslim İyileştirme (Delivery Optimization)**: P2P Windows Update indirme ön belleği
11. **DirectX ve GPU Shader Ön Belleği**: NVIDIA GLCache/DXCache, AMD DXCache/GLCache, Intel ve D3DSCache
12. **Küçük Resim ve Simge Ön Belleği**: `IconCache.db` ve `thumbcache_*.db` (Kilitleri aşmak için Explorer güvenli yenilenir)
13. **Windows Prefetch**: `C:\Windows\Prefetch` eski uygulama ön yükleme verileri
14. **ARP Tablosu ve Ağ Ön Belleği**: `arp -d *`, `nbtstat -R`, `nbtstat -RR`
15. **Windows Defender Tarama Geçmişi**: Zararlı geçmişi ve logları (Varsayılan: Kapalı)
16. **DISM Bileşen Temizliği**: `dism /online /cleanup-image /startcomponentcleanup` (WinSxS temizliği)
17. **Web Tarayıcı Önbellekleri**: Chrome, Edge, Brave, Firefox, Opera, Vivaldi, WebView2 (Şifreler korunur)
18. **Eski Windows Sürüm Kalıntıları**: `C:\Windows.old`, `C:\$Windows.~BT`, `C:\$Windows.~WS` ve `C:\$GetCurrent`

---

## 🚀 Kullanım

1. **`LuckyStrike_CleanUP_Setup.msi`** dosyasını kurun veya **`LuckyStrike-CleanUP.exe`** taşınabilir sürümünü çalıştırın.
2. UAC onayını vererek yönetici modunda açın.
3. Dilediğiniz temizlik modüllerini seçin veya **Varsayılan** butonuna tıklayın.
4. (İsteğe bağlı) Temizlik öncesi sisteminizi yedeklemek için **Geri Yükleme Noktası** butonuna basın.
5. **🚀 Temizliği Başlat** butonuna tıklayın.
6. İşlem tamamlandığında kazanılan net disk alanı ve silinen dosya adedi raporlanacaktır.

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) kapsamında açık kaynak olarak sunulmaktadır.
