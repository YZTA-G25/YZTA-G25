**YZTA - G25**

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
| ![Dev2](assets/dev2-photo.jpg) | Zeynep Salman | Project Owner | 
| ![Dev3](assets/dev3-photo.jpg) | Yiğit Aydın | Developer | 
| ![Designer](assets/designer-photo.jpg) | Ecem Kayra Cesur | Developer |

---

# Oyun İsmi
## **[Place Holder]**

## Oyun Logosu
![Oyun Logo](assets/game-logo.png)
![Text Logo](assets/text-logo.png)

## Oyun Açıklaması

**[Place Holder]**, kooperatif oynanış mekanikleriyle öne çıkan, heyecan verici ve komik bir **Arcade Party Game**'dir. İki oyuncu tamamen farklı roller üstlenerek birlikte yemek tarifleri hazırlar: bir oyuncu elleri kontrol ederek malzemeleri toplar ve yemekleri hazırlarken, diğer oyuncu gözleri kontrol ederek çevreyi gözlemler ve partnerine rehberlik eder. Oyuncular, sürekli değişen kaotik olaylarla mücadele ederken, işbirliği ve iletişim becerilerini test eden benzersiz bir deneyim yaşarlar.

## Oyun Hikayesi

Büyülü bir partner bulma sitesine üye olan iki kişi birbileri ile eşleştiklerinde kendilerini sitenin uyumluluk testi içerisinded bulurlar. Aynı bedeni paylaşan bu çift gerçekten uyumlu olduklarını görmek için etraflarında dönen kaosa rağmen başarılı bir şekilde sınavları geçmeli ve ilikilerini kanıtlamalıdır.

## Oyun Özellikleri

- 🎮 **Kooperatif Multiplayer** (Network + Local [Second Priority])
- 🍳 **Benzersiz Asymmetric Gameplay** (El vs Göz kontrolü)
- ⚡ **Dinamik Kaos Sistemi** (Fırtına, Levitasyon, Halüsinasyon vb.)
- 📖 **Interaktif Tarif Defteri** Mekaniği
- 🎯 **Arkadaşlık Testi** Oynanışı
- 🎨 **Polished 3D Görsel Tasarım**
- 🎵 **Yoğun Ses Tasarımı ve Müzik**

## Kontrol Şeması

<details>
<summary>🎮 Player Control Systems</summary>

### El Oyuncusu (Hand Player) : Klavye Mouse
- **WASD**: Karakter hareketi
- **Mouse XZ**: El X/Z ekseni kontrolü  
- **Q/E**: El yükselt/alçalt
- **Sağ Tık + Q/E**: Eli Z ekseni üzerinde Solda/Sağa döndür
- **Sol Tık**: Obje tutma/bırakma
### El Oyuncusu (Hand Player) : Gamepad
- **Sol Stick**: Karakter Hareketi
- **Sağ Stick**: El X/Z ekseni kontrolü
- **L1(LT)/R1(RT)**: El yükselt/alçalt
- **L2(LB) + L1/R1**: Eli Z ekseni üzerinde Sola/Sağa döndür
- **R2(RB)**: Obje tutma/bırakma 

### Göz Oyuncusu (Eye Player)
- **WASD**: Karakter hareketi
- **Sol Tık**: Etkileşim
- **Gamepad**: Sol stick hareket, R2 Etkileşim

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
- 👫 **Arkadaş Grupları ve Çiftler**  
- 🎮 **Party Game Meraklıları**
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
-1 Puan: Çok basit, tek adımlık, bilinen bir görev.
-2 Puan: Birkaç adımı olan, basit ama biraz kurulum gerektiren bir görev.
-3 Puan: Standart bir geliştirme görevi. Ne yapılacağı net, belirli bir kodlama eforu gerektiriyor.
-5 Puan: Birden fazla parçayı etkileyen veya yeni bir mimari düşünmeyi gerektiren, daha karmaşık bir görev.

**Daily Scrum**: Toplantılar Slack üzerinden gerçekleştirildi, gün içerisinde Whatsapp aracılığı ile iletişim kuruldu. [Daily Scrum Chats](https://imgur.com/a/WUMZggb)

**Sprint board update**: ![Sprint Board](assets/sprint1-board.png)

### Ürün Durumu: Ekran Görüntüleri
![Screenshot 1](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Level%20DEsign.mp4)
![Screenshot 2](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Level%20Design.png)
![Screenshot 3](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Mechanics.mp4)

**Sprint Review**:
- Network multiplayer foundation başarıyla kuruldu ✔️
- Basic Audio Manager kuruldu ✔️
- Hand Controller basic mechanics implement edildi ✔️
- Eye Controller test odası kuruluyor ➖
- Basic interaction system çalışır durumda ✔️
- Recipe System temeli kuruldu ✔️
- Cooking Station kuruldu ✔️
- İlk seviye tasarımı tamamlandı (iterasyonlarla daha rafine hale getirilecek) ✔️

**Yapılan Doğrular**:
- Herkes görevlerini benimsediği için kimse birbirinin yükünü almak zorunda kalmadı
- Toplantı saatlerine uyuldu, toplantılar kısa ve öz yapıldı
- Tanışma toplantısı sonrası oyun mekanikleri ile ilgili kritik kararlar geç kalınmadan verildi
- GDD oluşturuldu mekanikler ve oyun detayları detaylı bir şekilde dokümantasyona eklendi 

**Hatalar**:
- Görev dağılımında daha efektif iletişim yapılmalı, bir developer'ın görevleri karıştırması nedeni ile ortaya çıkan karışıklık 10 puan değerinde iki görevin uzamasına ve bu sprinte puan olarak girilememesine sebep oldu
- Özellikle AI araçlarının kullanımında karışıklığın devam etmemesi amacı ile çeşitli kurallara karar verildi
- Kontrol edilemeyen yoğunluk nedeni ile (seyahat, kişisel problemler, sınavlar, YKS vb.) görevler için ön görülen süreler uzayabildi. Olası engel değerlendirilmesi daha kuvvetli yapılmalı.

**Sprint Retrospective**:
- Network sync optimization'a odaklanılacak ❗
- UI design için araştırmalar önceliklendirildi ❗
- Audio Design için araştırmalar yapılacak ❗
- Notebook Sistemine başlandı ❗

</details>

**© 2025 [Place Holder] - GameJam 2025 Project**
