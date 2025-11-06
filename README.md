# 🎯 Last Warrior: Yapay Zeka Destekli TPS(Third Person Shooter) Oyunu

**Last Warrior**, Unity oyun motoru kullanılarak geliştirilmiş, yapay zekâ destekli bir **TPS** oyunudur.  
Oyuncu, özel kuvvet askeri **Teğmen Oğuz Aydın** rolünü üstlenerek terk edilmiş bir askeri depoda düşmanlarla savaşır.  
Yapay zekâ düşman karakterleri, **Finite State Machine (FSM)** tabanlı davranış modelleriyle yönetilmektedir.

## 🪖 Oyun Hakkında

**Tür:** Third Person Shooter (TPS)  
**Motor:** Unity  
**Dil:** C#  
**Tema:** Askerî, Aksiyon, Operasyon 

## 🎬 Senaryo
Eski bir askeri mühimmat deposu, yıllar önce terk edilmiş gibi görünüyor.
Ancak uydu verileri, içeride yasadışı silah üretimi ve veri transferi yapıldığını gösteriyor. 
Sen, **Teğmen Oğuz Aydın**, özel kuvvetlerden geriye kalan tek askersin.  
Ekibinle bir anda iletişim kesildi.  
Şimdi yalnızsın…

## 🤖 Yapay Zekâ Sistemi

Oyundaki düşman NPC’ler, **Finite State Machine (FSM)** yaklaşımı ile kontrol edilir.  
Her düşman, duruma göre farklı davranışlar sergiler:

| Durum | Açıklama |
|--------|-----------|
| **Idle** | NPC devriye sırasını bekler veya çevresini gözlemler. |
| **Patrol** | Belirlenen `Waypoints` arasında devriye gezer. |
| **Chase** | Oyuncu tespit edildiğinde peşine düşer. |
| **Attack** | Oyuncu menzile girdiğinde saldırıya geçer. |
| **Die** | Sağlığı tükendiğinde devre dışı kalır. |

FSM sistemi, **esnek** ve **ölçeklenebilir** bir AI altyapısı sağlar.

## 🧱 Oyun Mekanikleri

- **TPS Kamera Kontrolü:** Omuz üzerinden nişan alma sistemi.  
- **Silah Mekaniği:** Nişan alma, ateş etme, mermi ve geri tepme (recoil) simülasyonu.  
- **FSM Tabanlı NPC AI:** Düşmanlar çevreye ve oyuncuya dinamik olarak tepki verir.  
- **Health Sistemi:** Oyuncu ve NPC’ler için hasar ve ölüm mekanikleri.  
- **NavMesh Navigasyonu:** NPC’ler Unity NavMesh sistemi üzerinde hareket eder.  
- **User Interface (UI):**  
  - Sağlık göstergesi  
  - Mermi sayacı  
  - Görev ve ipucu alanı  

## 🧩 Kullanılan Teknolojiler ve Bileşenler

| Teknoloji / Sistem | Açıklama |
|--------------------|-----------|
| **Unity Engine** | Oyun motoru |
| **C#** | Programlama dili |
| **NavMesh Agent** | Düşmanların haritada gezinmesi |
| **FSM (Finite State Machine)** | Düşman yapay zekâ yönetimi |
| **Animator Controller** | Karakter ve NPC animasyonları |
| **Cinemachine** | Kamera kontrolü |
| **Input System** | Oyuncu hareket ve nişan kontrolü |

## 🧠 Proje Yapısı

Assets/
├── Scripts/
│ ├── Player/
│ │ └── PlayerController.cs
│ ├── Enemy/
│ │ ├── EnemyAI.cs
│ │ ├── PatrolState.cs
│ │ └── IdleState.cs
│ └── Health.cs
├── Prefabs/
│ ├── Player.prefab
│ ├── Enemy.prefab
│ └── Waypoint.prefab
├── Scenes/
│ └── MainScene.unity
└── UI/
├── HealthBar
└── AmmoCounter

## 🎮 Yapılması Hedeflenenler

- Oyuncuya mantıklı şekilde tepki verebilen, saldırı ve savunma davranışları sergileyen NPC’ler tasarlamak  
- Gerçekçi hareket ve nişan alma mekaniklerine sahip bir karakter kontrol sistemi geliştirmek  
- Kullanıcı dostu bir arayüz ve sahne geçiş sistemini entegre etmek  
- Oyun içi optimizasyon ve kaynak yönetimini sağlamak

## 🕹️ Uygulama İçeriği

### 🎛️ 1. Menü Ekranı
- Unity’nin **Canvas UI** sistemiyle oluşturuldu.  
- “**Oyuna Başla**”, “**Ayarlar**” ve “**Çıkış**” butonları bulunur.  
- Sahne geçişleri `OnClick()` event’leriyle yönetilir.

### 🧍‍♂️ 2. Karakter ve Silah Sistemi
- Üçüncü şahıs kamera (`Cinemachine`) ile oyuncu karakteri kontrol edilir.  
- Silah, bir **Prefab** olarak karaktere atanmıştır.  
- Ateş etme işlemi **Raycast** yöntemiyle yapılır.  
- **Health.cs** script’i ile oyuncu ve NPC hasar sistemleri yönetilir.  
- Geri tepme (`recoil`) ve animasyon geçişleri **Animator Controller** aracılığıyla yapılır.

### 🤖 3. NPC Yapay Zekâsı
- NPC’ler FSM yapısına göre oyuncuya tepki verir.  
- **Pathfinding:** `NavMeshAgent` kullanılarak yapılır.  
- **Görüş Algısı:** `Physics.Raycast` ile oyuncu tespiti sağlanır.  
- **Saldırı:** Oyuncu belirli bir menzile girdiğinde `Attack()` fonksiyonu devreye girer.

### 🌍 4. Harita ve Oyun Ortamı
- Low-poly model setleriyle oluşturulmuş açık alan harita.  
- NPC’ler NavMesh üzerinde konumlandırıldı.  
- NavMesh **Bake** işlemiyle gezilebilir alanlar tanımlandı.

## 🩸 Karşılaşılan Zorluklar ve Çözümler

| Zorluk | Çözüm |
|--------|--------|
| NPC’lerin engeller arkasında oyuncuyu algılaması | `Raycasting` ile görüş hattı kontrolü eklendi. |
| Pathfinding hataları | NavMesh yeniden oluşturuldu (`Bake`). |
| Menü geçişlerinde donma | Asenkron sahne yükleme (`AsyncOperation`) kullanıldı. |
| Performans düşüşü | Object Pooling ve Coroutine optimizasyonları uygulandı. |
| Mor ekran (Render Pipeline) hatası | Render Pipeline Converter ile çözüldü. |

## 🗺️ Literatür Özeti

- **Millington (2019)** — *Artificial Intelligence for Games*  
  FSM yapısının küçük oyunlarda yüksek performans sunduğunu vurgular.  

- **Smith (2020)** — *AI Behavior in Shooter Games*  
  NavMesh sisteminin Unity’deki en verimli pathfinding çözümü olduğunu belirtir.  

- **Şahin, İhsan (2020)** — *Unity 3D oyun ortamında akıllı ajanlar ile kaçma-kovalama tasarımı*  
  Benzer FSM tabanlı yapay zekâ modellemesi kullanılmıştır.  

- **Zhu, Xianwen (2019)** — *Behavior Tree design of NPCs based on Unity3D*  
  FSM’in gelişmiş alternatifi olan Behavior Tree modelini tanıtır.

## 🧩 Kurulum ve Çalıştırma

1. `https://unity.com/download` üzerinden **Unity 6.2** sürümünü indir.
2. `MainScene.unity` dosyasını çalıştır.  
3. `Oyuna Başla` tuşuna basarak oyunu başlat.  0
4. Karakteri **W, A, S, D** tuşlarıyla hareket ettir,  
   **Mouse** ile nişan al, **Sol tık** ile ateş et.

## 🧭 Geliştirici Notları

- Oyundaki tüm AI davranışları **FSM yapısı** üzerine kuruludur.  
- NavMesh sistemi dinamik olarak **bake** edilmiştir.  
- FPS performansı optimize edilmiştir (LOD, basit gölgelendirme).  
- Tüm modeller ve materyaller proje içi veya açık lisanslı kaynaklardan alınmıştır.

## 📌 Gelecek Planları

- [ ] Yeni harita tasarımı (şehir ortamı)  
- [ ] Farklı düşman tipleri (drone, keskin nişancı, zırhlı birlik)  
- [ ] Multiplayer (kooperatif görev modu)  
- [ ] Sinematik görev geçişleri (Timeline)  
- [ ] Türkçe / İngilizce dil desteği  

## 👨‍💻 Geliştiriciler

- **Barkın Emre Sayar** - `BbSayar`
- **Doğukan Kıralı** - `Dgknkrl`
- **Ceyda Özmen** - `ceydaozmen` 

📚 *Bilişim Sistemleri Mühendisliği — Kocaeli Üniversitesi*  

## 📜 Lisans

Bu proje, kişisel ve eğitim amaçlı kullanım için geliştirilmiştir.  
Ticari kullanım öncesi geliştirici izni gerektirir.
