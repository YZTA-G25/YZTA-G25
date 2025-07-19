# YZTA - G25

## 📚 İçindekiler

<details>
<summary>📋 Genel Bilgiler</summary>

- [Takım Elemanları](#takım-elemanları)
- [Oyun İsmi](#oyun-i̇smi)
- [Oyun Logosu](#oyun-logosu)
- [Oyun Açıklaması](#oyun-açıklaması)
- [Oyun Hikayesi](#oyun-hikayesi)

</details>

<details>
<summary>🎮 Oyun Detayları</summary>

- [Oyun Özellikleri](#oyun-özellikleri)
- [Kontrol Şeması](#kontrol-şeması)
- [Kaos Olayları](#kaos-olayları)
- [Hedef Kitle](#hedef-kitle)
- [Teknik Özellikler](#teknik-özellikler)

</details>

<details>
<summary>🛠️ Development</summary>

- [Development Roadmap](#development-roadmap)
- [Product Backlog](#product-backlog)
- [Sprint 1](#sprint-1)
- [Sprint 2](#sprint-2)

</details>

<details>
<summary>📈 Proje Yönetimi</summary>

- [Pazarlama Planı](#pazarlama-planı)
- [Jüri Notları](#jüri-notları)

</details>

---

## Takım Logosu
![Logo](assets/team-logo.png)

## Takım Elemanları

| Photo | Name | Title |
|-------|------|-------|
| ![Dev1](assets/dev1-photo.jpg) | Erkut Kılınç | Scrum Manager | 
| ![Designer](assets/dev2-photo.jpg) | Zeynep Salman | Project Owner | 
| ![Dev2](assets/dev3-photo.jpg) | Yiğit Aydın | Developer | 
| ![Dev3](assets/designer-photo.jpg) | Ecem Kayra Cesur | Developer |

---

# Oyun İsmi
## **Friendslop**

## Oyun Logosu
![Oyun Logo](assets/game-logo.png)
![Text Logo](assets/text-logo.png)

## Oyun Açıklaması

**Friendslop**, kooperatif oynanış mekanikleriyle öne çıkan, heyecan verici ve komik bir **Arcade Party Game**'dir. İki oyuncu tamamen farklı roller üstlenerek birlikte yemek tarifleri hazırlar: bir oyuncu elleri kontrol ederek malzemeleri toplar ve yemekleri hazırlarken, diğer oyuncu gözleri kontrol ederek çevreyi gözlemler ve partnerine rehberlik eder. Oyuncular, sürekli değişen kaotik olaylarla mücadele ederken, işbirliği ve iletişim becerilerini test eden benzersiz bir deneyim yaşarlar.

## Oyun Hikayesi

Büyülü bir partner bulma sitesine üye olan iki kişi birbirleri ile eşleştiklerinde kendilerini sitenin uyumluluk testi içerisinde bulurlar. Aynı bedeni paylaşan bu çift, gerçekten uyumlu olduklarını görmek için etraflarında dönen kaosa rağmen başarılı bir şekilde sınavları geçmeli ve ilişkilerini kanıtlamalıdır.

## Oyun Özellikleri

- 🎮 **Kooperatif Multiplayer** (Network + Local [İkinci Öncelik])
- 🍳 **Benzersiz Asimetrik Oynanış** (El vs Göz kontrolü)
- ⚡ **Dinamik Kaos Sistemi** (Fırtına, Levitasyon, Halüsinasyon vb.)
- 📖 **İnteraktif Tarif Defteri** Mekaniği
- 🎯 **Arkadaşlık Testi** Oynanışı
- 🎨 **Cilalanmış 3D Görsel Tasarım**
- 🎵 **Yoğun Ses Tasarımı ve Müzik**

## Kontrol Şeması

<details>
<summary>🎮 Player Control Systems</summary>

### El Oyuncusu (Hand Player) : Klavye Mouse
- **WASD**: Karakter hareketi
- **Mouse XY**: El X/Z ekseni kontrolü  
- **Sağ Tık + Mouse Y**: El yükselt/alçalt
- **Sağ Tık + Mouse X**: Eli Z ekseni üzerinde döndür
- **Sol Tık**: Obje tutma/bırakma

### El Oyuncusu (Hand Player) : Gamepad
- **Sol Stick**: Karakter Hareketi
- **Sağ Stick**: El X/Z ekseni kontrolü
- **L1/R1 veya LT/RT**: El yükselt/alçalt
- **L2/R2 veya LB/RB + Sağ Stick**: Eli Z ekseni üzerinde döndür
- **L2/R2 veya LB/RB**: Obje tutma/bırakma 

### Göz Oyuncusu (Eye Player)
- **Mouse**: Karakter kafasının rotasyonu
- **Sol Tık**: Etkileşim (Lever vb.)
- **Gamepad**: Sol stick kafa rotasyonu, Yüz tuşları etkileşim

</details>

## Kaos Olayları

<details>
<summary>⚡ Chaos Event Systems</summary>

- 🌪️ **Fırtına**: Eşyaları savurur, sığınak açılmalı
- ✨ **Eşya Levitasyonu**: Malzemeler havaya uçar, büyü durdurulmalı
- 👁️ **Göz Kamaşması**: Görüş bozulur, gözlük takılmalı
- ⚖️ **Denge Bozulması**: Karakter sallanır, denge sağlanmalı
- 🌀 **Halüsinasyon**: Yanıltıcı görüntüler, ilaç alınmalı

</details>

## Hedef Kitle

- 🎯 **Kooperatif Oyun Severler**
- 👫 **Arkadaş Grupları ve Çiftler** - 🎮 **Party Game Meraklıları**
- 🧩 **Puzzle ve Koordinasyon Oyunu Severler**
- 👥 **16+ Yaş Grubu**

## Teknik Özellikler

- **Platform**: PC (Windows)
- **Engine**: Unity 6 LTS
- **Network**: Unity Netcode for GameObjects
- **Input**: Unity Input System (Gamepad + Mouse/Keyboard)
- **Rendering**: Universal Render Pipeline (URP)
- **Target**: 60 FPS, 1080p

## Development Roadmap

<details>
<summary>📅 6 Haftalık Development Sprint Hedefleri</summary>

| Hafta | Milestone | Temel Özellikler |
|-------|-----------|------------------|
| **1** | Foundation | Network setup, Basic controls |
| **2** | Core Systems | Player interaction, Recipe system |
| **3** | Recipe Flow | Complete cooking mechanics |
| **4** | Chaos Events | All chaos systems implemented |
| **5** | Polish | Balancing, UI/UX improvements |
| **6** | Release | Final optimization, Bug fixes |

</details>

## Product Backlog

[GDD and Technical Documentation](https://docs.google.com/document/d/1ZmhoZyXFm3eA4U6i_Zuzi-xYOyjhhTCf5N6YDwXcX9U/edit?usp=sharing)
[Jira Board](https://yzta-g25.atlassian.net/jira/core/projects/GBG/timeline?rangeMode=weeks)

---

## Sprint 1

<details>
<summary>🚀 Sprint 1 - Foundation</summary>

**Sprint Notları**: Temel network altyapısı ve player kontrol sistemlerinin implementasyonu neredeyse tamamlandı.

**Sprint içinde tamamlanması tahmin edilen puan**: 60 Puan -> Sprint içinde atadığımız görevlerin puanlarının üstüne bir miktar çıkarak bir deneme hedefi olarak seçtik. Bu sayede bundan sonraki sprintlerde yoğunluğumuza göre hesaplama yapabilir ve daha gerçekçi hedefler koyabiliriz.

**Sprint içerisinde ulaşılan puan**: 40

**Puan tamamlama mantığı**: 
- 1 Puan: Çok basit, tek adımlık, bilinen bir görev.
- 2 Puan: Birkaç adımı olan, basit ama biraz kurulum gerektiren bir görev.
- 3 Puan: Standart bir geliştirme görevi. Ne yapılacağı net, belirli bir kodlama eforu gerektiriyor.
- 5 Puan: Birden fazla parçayı etkileyen veya yeni bir mimari düşünmeyi gerektiren, daha karmaşık bir görev.

**Daily Scrum**: Toplantılar Slack üzerinden gerçekleştirildi, gün içerisinde Whatsapp aracılığı ile iletişim kuruldu. [Daily Scrum Chats](https://imgur.com/a/WUMZggb)

**Sprint board update**: ![Sprint Board](assets/sprint1-board.png)

### Ürün Durumu: Ekran Görüntüleri
![Screenshot 1](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Level%20DEsign.mp4)
- Level Design Gösterimi Video
![Screenshot 2](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Level%20Design.png)
- Level içerisinden bir fotoğraf
![Screenshot 3](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Mechanics.mp4)
- GamePlay Video

### Jira Ekran Görüntüleri
![Screenshot 1](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Jira%201.png)
![Screenshot 2](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Jira%202.png)

**Sprint Review**:
- Network multiplayer foundation başarıyla kuruldu ✔️
- Basic Audio Manager kuruldu ✔️
- Hand Controller basic mechanics implement edildi ✔️
- Eye Controller test odası kuruluyor ➖
- Basic interaction system çalışır durumda ✔️
- Recipe System temeli kuruldu ✔️
- Cooking Station kuruldu ✔️
- İlk seviye tasarımı tamamlandı (iterasyonlarla daha rafine hale getirilecek) ✔️


**Sprint Retrospective**:

**Yapılan Doğrular**:
- Herkes görevlerini benimsediği için kimse birbirinin yükünü almak zorunda kalmadı
- Toplantı saatlerine uyuldu, toplantılar kısa ve öz yapıldı
- Tanışma toplantısı sonrası oyun mekanikleri ile ilgili kritik kararlar geç kalınmadan verildi
- GDD oluşturuldu mekanikler ve oyun detayları detaylı bir şekilde dokümantasyona eklendi 

**Hatalar**:
- Görev dağılımında daha efektif iletişim yapılmalı, bir developer'ın görevleri karıştırması nedeni ile ortaya çıkan karışıklık 10 puan değerinde iki görevin uzamasına ve bu sprinte puan olarak girilememesine sebep oldu
- Özellikle AI araçlarının kullanımında karışıklığın devam etmemesi amacı ile çeşitli kurallara karar verildi
- Kontrol edilemeyen yoğunluk nedeni ile (seyahat, kişisel problemler, sınavlar, YKS vb.) görevler için ön görülen süreler uzayabildi. Olası engel değerlendirilmesi daha kuvvetli yapılmalı.

**Planlar**:
- Network sync optimization'a odaklanılacak ❗
- UI design için araştırmalar önceliklendirildi ❗
- Audio Design için araştırmalar yapılacak ❗
- Notebook Sistemine başlandı ❗

</details>

---

## Sprint 2

<details>
<summary>🚀 Sprint 2 - Core Systems</summary>

**Sprint Notları**: İkinci sprint, oyuncu sistemlerini tamamlamaya, temel etkileşim döngüsünü (dolaptan al, tezgaha koy) kurmaya ve Kaos Sistemi'nin temellerini atmaya odaklandı. Göz Oyuncusu için lever tabanlı kafa kontrolü başarıyla implemente edildi.

**Sprint içinde tamamlanması tahmin edilen puan**: 50 Puan

**Sprint içerisinde ulaşılan puan**: 45

**Puan tamamlama mantığı**: 
- 1 Puan: Çok basit, tek adımlık, bilinen bir görev.
- 2 Puan: Birkaç adımı olan, basit ama biraz kurulum gerektiren bir görev.
- 3 Puan: Standart bir geliştirme görevi. Ne yapılacağı net, belirli bir kodlama eforu gerektiriyor.
- 5 Puan: Birden fazla parçayı etkileyen veya yeni bir mimari düşünmeyi gerektiren, daha karmaşık bir görev.

**Daily Scrum**: Toplantılar Slack üzerinden devam etti. Whatsapp, anlık iletişim için aktif olarak kullanıldı.

**Sprint board update**: ![Sprint 2 Board](https://imgur.com/a/iM02kJz)

### Ürün Durumu: Ekran Görüntüleri
![Sprint 2 Gameplay Video](https://imgur.com/a/9IWS6AO)
- *Geliştirilmiş Etkileşim ve Lever Mekanikleri Videosu

### Jira Ekran Görüntüleri
![Sprint 2 Jira 1](https://imgur.com/a/ymWSuGu)
- *Sprint 2 Jira Panosu (Sizin tarafınızdan eklenecek)*
**Sprint Review**:
- Gelişmiş Oyuncu Kontrolleri (El ve Göz) tamamlandı ✔️
- Etkileşim Sistemi (Tutma/Bırakma) geliştirildi ve dolap mekaniği eklendi ✔️
- Göz Oyuncusu'nun Kafa Kontrolü (Lever Sistemi) implemente edildi ✔️
- Defter (Notebook) çevirme mekaniği eklendi ✔️
- Temel Kaos Sistemi altyapısı kuruldu (Fırtına olayı üzerinde çalışılıyor) ➖
- UI elemanları (Crosshair, Butonlar) entegre edildi ✔️

**Sprint Retrospective**:

**Yapılan Doğrular**:
- Takım içi teknik iletişim güçlendi, özellikle kod birleştirme (merge) ve senkronizasyon konularında daha dikkatli davranıldı. ![Yeni Pull Request Formatımız](https://imgur.com/a/SEUaxYr)
- İlk sprint'teki hatalardan ders çıkarılarak görev dağılımı daha net yapıldı ve görev takibi Jira üzerinden daha sıkı yürütüldü.
- Asimetrik oyuncu rollerinin (El vs Göz) oynanışını netleştiren önemli kararlar alındı.

**Hatalar**:
- Fizik tabanlı network senkronizasyonu (Kaos Olayı - Fırtına) beklenenden daha zorlayıcı oldu ve tam olarak çözülemedi. Bu durum, bazı görevlerin tamamlanamamasına neden oldu.
- UI elemanlarının entegrasyonu sprint sonuna bırakıldığı için yetişmedi.
- Ekip üyelerinin toplu olarak şehir dışında olması gereken bir hafta olduğu için bütün amaçlara tam olarak ulaşılamadı. (Özellikle Customer sistemi bitirilmediği ve Pull Requestler tam olarak dahil edilmediği için oyun döngüsü daha gösterilemiyor.)
- Scrum Master ev taşıdığı için (ben) bu sprint'in yönetimi eksikti.

**Planlar**:
- Fırtına Kaos Olayı'ndaki senkronizasyon sorununu çözmek **birinci öncelik** ❗
- UI/UX üzerinde detaylı çalışmalara başlanacak (animasyonlar, geri bildirimler vb.) ❗
- Müşteriler entegre edilecek son değişiklikler birleştirilecek❗
- Geçtiğimiz haftanın eksikleri nedeni ile en sert Sprint başlayacak.

</details>

---
**© 2025 YZTA - G25**
