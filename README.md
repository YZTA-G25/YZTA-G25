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
-Level Design Gösterimi Video
![Screenshot 2](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Level%20Design.png)
-Level içerisinden bir fotoğraf
![Screenshot 3](https://github.com/YZTA-G25/YZTA-G25/blob/main/Assets/Github%20Assets/Mechanics.mp4)
-GamePlay Video

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
<summary>🔥 Sprint 2 - Core Systems</summary>

**Sprint Notları**: Network optimization ve core gameplay mechanics üzerine odaklanıldı. Player interaction sistemleri geliştirildi ve ilk chaos event implementasyonları tamamlandı.

**Sprint içinde tamamlanması tahmin edilen puan**: 45 Puan -> Sprint 1 deneyimimize dayanarak daha gerçekçi bir hedef belirledik.

**Sprint içerisinde ulaşılan puan**: 52

**Daily Scrum**: Toplantılar Discord üzerinden gerçekleştirildi, günlük progress Slack'te paylaşıldı. [Daily Scrum Chats](https://imgur.com/a/S2UMZgp)

**Sprint board update**: ![Sprint Board](assets/sprint2-board.png)

### Ürün Durumu: Ekran Görüntüleri
[YZTA-G25 Sprint 2 Media Folder](https://drive.google.com/drive/folders/1YZTAg25Sprint2Media/view?usp=sharing)
- Network multiplayer gameplay demonstration
- Score system ve UI implementasyonu

### Jira Ekran Görüntüleri
![Screenshot 1](assets/sprint2-jira1.png)
![Screenshot 2](assets/sprint2-jira2.png)

**Sprint Review**:
- Network synchronization optimize edildi ✔️
- Score system NetworkVariable ile implement edildi ✔️
- Player setup management coroutine'ler ile geliştirildi ✔️
- Eye-Hand player camera coordination düzeltildi ✔️
- Basic UI score bar sistemi eklendi ✔️
- Lever interaction system optimize edildi (10 frame polling) ✔️
- Lobby rate limiting ve backoff mechanism eklendi ✔️

**Sprint Retrospective**:

**Yapılan Doğrular**:
- Sprint 1'den çıkarılan dersler uygulandı
- Network issues proaktif şekilde çözüldü
- Code review süreçleri iyileştirildi
- Task dağılımında daha iyi iletişim kuruldu

**Hatalar**:
- Bazı network timing issues daha uzun sürdü
- UI tasarım kararları geç alındı
- Test coverage yetersiz kaldı

**Planlar**:
- Chaos event sistemlerine odaklanılacak ❗
- Recipe system expansion ❗
- Polish ve optimization ❗

</details>

---

## Sprint 3

<details>
<summary>🎯 Sprint 3 - Recipe Flow & Polish</summary>

**Sprint Notları**: Recipe system'in tamamlanması, chaos event implementation'ları ve oyun deneyiminin polish edilmesi üzerine odaklanıldı. Oyunun core loop'u tamamlandı.

**Sprint içinde tamamlanması tahmin edilen puan**: 80

**Sprint içerisinde ulaşılan puan**: 72

**Daily Scrum**: Toplantılar Discord üzerinden düzenli gerçekleştirildi, kritik bug'lar Slack'te hızlıca paylaşıldı. 

### Ürün Durumu: Ekran Görüntüleri ve Videolar
[YZTA-G25 Sprint 3 Media Folder](https://drive.google.com/drive/folders/1UQlYtGBiET3GFD6cGGasp8oMk7fQhv34?usp=sharing)
- Tam multiplayer gameplay experience
- Recipe defteri ve cooking mechanics
- Storm, levitation ve diğer chaos event'ler
- Oyun içi ekran görüntüleri ve UI screenshots

**Sprint Review**:
- Complete recipe system implemented ✔️
- Storm chaos event sistemi tamamlandı ✔️
- Object levitation chaos event eklendi ✔️
- Networked scoring system fully functional ✔️
- Performance optimization ve frame rate iyileştirmeleri ✔️
- Audio feedback sistemleri eklendi ✔️
- UI/UX polish ve visual improvements ✔️
- Multiplayer stability artırıldı ✔️
- Hand-Eye coordination mechanics refined ✔️
- Sinematik Çekimler Yapıldı ✔️

**Sprint Retrospective**:

**Yapılan Doğrular**:
- Recipe system tasarımı kullanıcı deneyimi odaklı geliştirildi
- Chaos event'ler oyun deneyimini zenginleştirdi
- Network performance önemli ölçüde iyileştirildi
- Team coordination en iyi seviyesinde
- Günlük toplantı alındı
- Code quality standards uygulandı

**Hatalar**:
- Bazı chaos event'lerin balancing'i geç fark edildi
- Audio integration planlanandan daha uzun sürdü
- Visual feedback sistemleri son dakikada eklendi

**Planlar**:
- Final polish ve bug fixing ❗
- Performance optimization son tuşları ❗
- Playtesting feedback integration ❗
- Release preparation ❗

</details>

**© 2025 [Place Holder] - GameJam 2025 Project**
