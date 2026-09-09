# Dungeon Roguelite

Unity 6 (6000.3.23f1) ile geliştirilen, 3D izometrik / üstten bakış (top-down) aksiyon roguelite ve zindan temizleme (dungeon crawler) oyunu.

---

## 1. Proje Genel Bakış

- **Oyun Türü**: Top-down 3D Action Roguelite / Dungeon Crawler
- **Oyun Motoru**: Unity 6000.3.23f1 (Unity 6 LTS)
- **Grafik Yaklaşımı**: Prototip geliştirme aşamasında temiz, net formlar ve yüksek kontrastlı stilize karanlık fantezi atmosferi
- **Oynanabilir Karakterler**:
  - **Savaşçı (Warrior)**: Yakın dövüş ustası; kılıç savurma menzili (2.5m), yüksek can havuzu ve alan temizleme gücü.
  - **Okçu (Archer)**: Uzak menzilli yay ve ok; fiziksel rota izleyen ok mermileri ve yüksek hareket kabiliyeti.
  - **Nişancı (Gunner)**: Seri atışlı tüfek; anında hedefe ulaşan yüksek hızlı hitscan mermiler ve yüksek sürekli DPS.

---

## 2. Oynanış Döngüsü ve Sefer Mekaniği

Oyun, karakter seçiminden başlayarak zindan seferlerine ve kalıcı güçlendirmelere uzanan modüler bir döngüye sahiptir:

```text
Karakter Seçimi (Character Selection)
        ↓
Dünya Haritası (World Map)  ←── Yetenek Ağacı (Kalıcı Güçlendirmeler)
        ↓
Zindan Seçimi (Dungeon 1 / 2 / 3)
        ↓
Düşman Dalgaları (Zombie, Runner, Tank, Ranged)
        ↓
XP Toplama & Seviye Atlama (Level-Up)
        ↓
Geçici Koşu Güçlendirmeleri (Hasar, Saldırı Hızı, Hareket Hızı)
        ↓
Son Dalga Temizliği / Zafer
        ↓
Zindan Tamamlama Ekranı (İlk Temizlemede Kalıcı Puan Ödülü)
        ↓
Sonraki Zindanın Açılması & Seferin Devamı
```

### Düşman Arketipleri
- **Zombi (Zombie)**: Temel yakın dövüş düşmanı; oyuncuyu takip eder ve yakın temasla saldırır (50-60 HP).
- **Koşucu (Runner)**: Hızlı ve çevik sürü düşmanı; yüksek hareket hızıyla oyuncuyu sıkıştırır (25 HP).
- **Tank (Tank)**: Yüksek cana ve ağır vuruş gücüne sahip elit tehdit; telegrafik vuruşlar yapar (150 HP).
- **Menzilli (Ranged)**: Güvenli mesafeden durup nişan alan ve çizgisel menzilli mermiler fırlatan düşman (35 HP).

### Roguelite Sefer ve Kontrol Noktası Semantiği
- **Zafer (Victory)**: Aktif sefer bir sonraki zindana kesintisiz devam eder. Karakter seviyesi, biriken XP ve toplanan geçici yükseltmeler korunur.
- **Yenilgi -> Yeniden Dene (Retry)**: Mevcut zindana giriş kontrol noktasına (entry checkpoint) geri dönülür. O denemede toplanan XP ve yükseltmeler geri alınır (ölümle XP farmı yapılması engellenir).
- **Yenilgi -> Haritaya Dön (Return to Map)**: Aktif sefer tamamen sonlandırılır. Seviye 1'e, XP 0'a, geçici yükseltmeler nötr duruma sıfırlanır.
- **Kalıcı İlerleme Dayanıklılığı**: Zindan kilit açma durumları (`DungeonProgression`) ve karakter kalıcı yetenekleri (`PermanentProgression`), yenilgi veya çıkışlardan etkilenmez.

---

## 3. Kalıcı İlerleme (Permanent Progression) & Ekonomi

Kalıcı ilerleme sistemi, geçici koşu tecrübesinden (`PlayerExperience`) tamamen bağımsızdır ve her karakter için ayrı ayrı takip edilir.

- **Karakter Bazlı İlerleme**: Savaşçı'nın kazandığı puanlar ve açtığı yetenekler Okçu veya Nişancı'yı etkilemez. Her kahraman kendi ilerleme geçmişine sahiptir.
- **İlk Temizleme (First-Clear) Ekonomisi**:
  - **Zindan 1 İlk Temizleme**: 2 Kalıcı Yetenek Puanı
  - **Zindan 2 İlk Temizleme**: 2 Kalıcı Yetenek Puanı
  - **Zindan 3 İlk Temizleme**: 3 Kalıcı Yetenek Puanı
  - **Mevcut Seferde Kazanılabilir Azami Puan**: Karakter başına **7 Puan**.
- **Katı İdempotency**: Bir zindanın aynı karakterle tekrar oynanması 0 ek kalıcı puan verir. Başka bir karakterle ilk kez temizlendiğinde ise o karaktere ödülü eksiksiz verilir.
- **Kalıcılık**: Kalıcı ilerleme verileri `PlayerPrefs` JSON formatında saklanır; zindan tekrarları, yenilgiler ve oyunun kapatılıp açılması durumunda aynen korunur.

---

## 4. Karakter Yetenek Ağaçları (Skill Trees)

Her kahraman için `ScriptableObject` tabanlı, 3 daldan ve her dalda 3 doğrusal aşamadan (Tier 1 -> Tier 2 -> Tier 3) oluşan **9 düğümlü** bir yetenek ağacı bulunmaktadır. Her düğüm 1 kalıcı puan bedelindedir:

### Savaşçı (Warrior)
- **Dayanıklılık (Durability)**:
  - Aşama 1: +10% Azami Can
  - Aşama 2: +10% Azami Can
  - Aşama 3: +15% Azami Can *(Dal Toplamı: +35% Azami Can / 1.35x)*
- **Güç (Power)**:
  - Aşama 1: +10% Hasar
  - Aşama 2: +10% Hasar
  - Aşama 3: +15% Hasar *(Dal Toplamı: +35% Hasar / 1.35x)*
- **Çeviklik (Tempo)**:
  - Aşama 1: +5% Saldırı Hızı
  - Aşama 2: +5% Hareket Hızı
  - Aşama 3: +10% Saldırı Hızı *(Dal Toplamı: +15% Saldırı Hızı, +5% Hareket Hızı)*

### Okçu (Archer)
- **Hassasiyet (Precision)**:
  - Aşama 1: +10% Hasar
  - Aşama 2: +10% Hasar
  - Aşama 3: +15% Hasar *(Dal Toplamı: +35% Hasar / 1.35x)*
- **Yay Çekişi (Tempo)**:
  - Aşama 1: +5% Saldırı Hızı
  - Aşama 2: +5% Saldırı Hızı
  - Aşama 3: +10% Saldırı Hızı *(Dal Toplamı: +20% Saldırı Hızı / 1.20x)*
- **Hayatta Kalma (Survival)**:
  - Aşama 1: +5% Hareket Hızı
  - Aşama 2: +10% Azami Can
  - Aşama 3: +10% Hareket Hızı *(Dal Toplamı: +15% Hareket Hızı, +10% Azami Can)*

### Nişancı (Gunner)
- **Ateş Gücü (Firepower)**:
  - Aşama 1: +10% Hasar
  - Aşama 2: +10% Hasar
  - Aşama 3: +15% Hasar *(Dal Toplamı: +35% Hasar / 1.35x)*
- **Seri Tetik (Cadence)**:
  - Aşama 1: +5% Saldırı Hızı
  - Aşama 2: +5% Saldırı Hızı
  - Aşama 3: +10% Saldırı Hızı *(Dal Toplamı: +20% Saldırı Hızı / 1.20x)*
- **Manevra (Handling)**:
  - Aşama 1: +5% Hareket Hızı
  - Aşama 2: +10% Azami Can
  - Aşama 3: +5% Hareket Hızı *(Dal Toplamı: +10% Hareket Hızı, +10% Azami Can)*

---

## 5. Özellik ve Hasar Katmanlama Modeli (Stat Model)

Tüm oynanış hesaplamaları hiyerarşik çarpan katmanlama prensibini uygular:

$$\text{Etkili Değer} = \text{Temel Değer} \times \text{Kalıcı Çarpan} \times \text{Geçici Çarpan}$$

- **Desteklenen Nitelikler**: Azami Can (`MaxHealth`), Hasar (`Damage`), Saldırı Hızı (`AttackSpeed`), Hareket Hızı (`MovementSpeed`).
- **Taze Can Kuralı (Fresh Health Rule)**: Zindana girişte oyuncunun canı kalıcı çarpanıyla tam olarak ölçeklenir (örn. Savaşçı 100 taban can ile başlar, +35% kalıcı can aldığında 135/135 tam canla doğar).
- **Geçici Yükseltme Katmanı**: Zindan içinde seviye atlandığında seçilen geçici güçlendirmeler kalıcı çarpanların üzerine çarpımsal olarak biner (örn. 100 taban hasar $\times$ 1.35 kalıcı $\times$ 1.20 geçici = 162 etkili hasar).

---

## 6. Dünya Haritası Yetenek Arayüzü (Skill Tree UI)

Kalıcı yetenek harcamaları çatışma dışı bir eylemdir ve Dünya Haritası (`WorldMap.unity`) üzerinden yönetilir:

- **"YETENEKLER" Butonu**: Haritanın sağ alt köşesinde yer alır; yeni bir sahne yüklemeden harita üzerine yetenek panelini açar.
- **Panel İçeriği**: Seçili kahramanın adı, harcanabilir yetenek puanı, 3 ana dal başlığı, 9 adet düğüm kartı ve "KAPAT" butonu.
- **Düğüm Görsel Durumları**:
  - `SATIN ALINDI` (Purchased): Yeşil tonlu arka plan; buton devre dışı, satın alma tamamlandı.
  - `SATIN AL` (Available): Parlak mavi tonlu arka plan; gereksinimler karşılandı ve puan yeterli, tıklanabilir.
  - `YETERSİZ PUAN` (Insufficient Points): Turuncu/kehribar arka plan; önkoşul açık ancak puan yetersiz, buton devre dışı.
  - `KİLİTLİ` (Locked): Koyu gri arka plan; önceki aşamadaki önkoşul düğüm henüz satın alınmamış, buton devre dışı.
- **Bağımsızlık**: Yetenek panelinin açılıp kapanması aktif zindan seçimini, kilitleri veya sefer durumunu değiştirmez.

---

## 7. Mimari ve Kod Tasarımı

- **Bileşen Tabanlı Tasarım (Composition over Inheritance)**: Karakterler ve düşmanlar devasa monolitik sınıflar yerine odaklı bileşenlerden oluşur (`PlayerMovement`, `PlayerAim`, `PlayerHealth`, `PlayerAttack`, `EnemyMovement`, `EnemyHealth`, vb.).
- **Minimalist Arayüzler**: Düşman saldırıları `IEnemyAttack` arayüzü ile soyutlanmıştır. Yakın dövüş, koşucu darbesi, tank vuruşu ve menzilli atış ortak hasar alma sözleşmesini (`IDamageable`) kullanır.
- **Veri Odaklılık (ScriptableObjects)**: Zindanlar (`DungeonDefinition`), dalgalar (`WaveDefinition`), karakterler (`CharacterDefinition`) ve yetenek ağaçları (`SkillTreeDefinition`) kod değiştirmeden düzenlenebilir yapıdadır.
- **Oturum ve Durum Ayrımı**:
  - `CharacterSelectionSession`: Aktif kahraman seçimini taşır.
  - `DungeonRunSession`: Aktif zindan seçimini taşır.
  - `RunProgressionSession`: Zindanlar arası geçici seviye/XP/yükseltme durumunu taşır.
  - `DungeonProgression`: Seferdeki zindanların kilit ve tamamlanma durumunu yönetir.
  - `PermanentProgression`: Karakterlerin kalıcı puan ve yetenek açılışlarını yönetir.

---

## 8. Kontroller

| Eylem | Kontrol Tuşu / Girdi |
| :--- | :--- |
| **Karakter Hareketi** | `W, A, S, D` veya Yön Tuşları |
| **Nişan Alma** | Fare İmleci (Karakter düzleminde 3D izdüşüm) |
| **Birincil Saldırı** | Farenin Sol Tuşu (LMB) veya `Boşluk` (Space) |
| **Arayüz Etkileşimi** | Farenin Sol Tuşu (LMB) |

---

## 9. Kurulum ve Çalıştırma

### Gereksinimler
- **Unity Sürümü**: `6000.3.23f1` (Unity 6)
- **Platform**: PC Standalone (Windows / macOS / Linux)

### Projeyi Açma ve Çalıştırma
1. Unity Hub üzerinden `6000.3.23f1` sürümü ile projeyi açın.
2. `Assets/Scenes/CharacterSelection.unity` sahnesini açın ve Play moduna geçin.
3. Savaşçı, Okçu veya Nişancı karakterinizi seçerek Dünya Haritasına geçin.
4. Zindana girin, dalgaları temizleyin ve ilk zindan zaferinizden sonra Dünya Haritasında **YETENEKLER** menüsünden kalıcı güçlendirmelerinizi açın.

### Build Settings Sahne Sıralaması
- `0`: `Assets/Scenes/CharacterSelection/CharacterSelection.unity`
- `1`: `Assets/Scenes/WorldMap/WorldMap.unity`
- `2`: `Assets/Scenes/Dungeons/Dungeon_Prototype.unity` (Zindan 1)
- `3`: `Assets/Scenes/Dungeons/Dungeon_02.unity` (Zindan 2)
- `4`: `Assets/Scenes/Dungeons/Dungeon_03.unity` (Zindan 3)

---

## 10. Proje Yol Haritası (Roadmap)

- **Phase 0–7**: Temel Dikey Kesit (Vertical Slice) — **TAMAMLANDI**
- **Phase 8**: Çoklu Karakter Sistemi (Savaşçı, Okçu, Nişancı) — **TAMAMLANDI**
- **Phase 9**: Sefer İlerlemesi & Dünya Haritası — **TAMAMLANDI**
- **Phase 10**: Çoklu Zindan Sefer Akışı & Ölçekleme — **TAMAMLANDI**
- **Phase 11**: Savaş Geri Bildirimi & Düşman Çeşitliliği (Zombi, Koşucu, Tank, Menzilli) — **TAMAMLANDI**
- **Phase 12**: Kalıcı İlerleme & Yetenek Ağaçları (Permanent Progression) — **TAMAMLANDI**
- **Phase 13**: Zindan 4 + Karşılaşma Tasarımı (Dungeon 4 + Encounter Design) — **TAMAMLANDI**
- **Phase 14**: Zindan 5 + İlk Boss (Dungeon 5 + First Boss)
- **Phase 15**: Prodüksiyon, Ayarlar ve Yayın Hazırlığı
