# 🎮 Deadlock Chase — Analysis Report

| Nama              | NRP        |
|-------------------|------------|
| Raziq Danish Safaraz          | 5025241258      |
| Aminnudin Wijaya          | 5025241242      |
| Herdian Tri Wardhana           | 5025241229      |

---

## Daftar Isi

- [1. Design](#1-design)
  - [1.1 Deskripsi Game](#11-deskripsi-game)
  - [1.2 Rancangan Ruang Permainan](#12-rancangan-ruang-permainan)
  - [1.3 Elemen Entitas](#13-elemen-entitas)
  - [1.4 Paradigma Algoritma](#14-paradigma-algoritma)
- [2. Implementation](#2-implementation)
  - [2.1 Arsitektur & Struktur Proyek](#21-arsitektur--struktur-proyek)
  - [2.2 Struktur Data Utama](#22-struktur-data-utama)
  - [2.3 Mekanika AI — GhostAI](#23-mekanika-ai--ghostai)
  - [2.4 Mekanika Player — PlayerController](#24-mekanika-player--playercontroller)
  - [2.5 Sistem Pergerakan — CubeRoller](#25-sistem-pergerakan--cuberoller)
  - [2.6 Sistem Power-Up & Collectible](#26-sistem-power-up--collectible)
  - [2.7 Sistem Portal & Teleportasi](#27-sistem-portal--teleportasi)
  - [2.8 Manajemen Scene & UI](#28-manajemen-scene--ui)
- [3. Evaluation](#3-evaluation)
  - [3.1 Analisis Kompleksitas Waktu](#31-analisis-kompleksitas-waktu)
  - [3.2 Analisis Kompleksitas Ruang](#32-analisis-kompleksitas-ruang)
  - [3.3 Efisiensi Struktur Data](#33-efisiensi-struktur-data)
  - [3.4 Jaminan Kebenaran (Correctness)](#34-jaminan-kebenaran-correctness)
  - [3.5 Identifikasi Bottleneck & Potensi Optimasi](#35-identifikasi-bottleneck--potensi-optimasi)
- [4. Conclusion](#4-conclusion)

---

## 1. Design

### 1.1 Deskripsi Game

**Deadlock Chase** adalah game *survival-chase* 3D berbasis **grid diskret** yang dibangun di atas Unity Engine. Pemain mengendalikan sebuah kubus yang bergerak step-by-step pada arena grid, menghindari kejaran **Ghost** (musuh AI) yang menggunakan algoritma pathfinding cerdas, sambil mengumpulkan collectible dan power-up untuk bertahan selama mungkin dan mengumpulkan skor tertinggi.

### 1.2 Rancangan Ruang Permainan

Ruang permainan direpresentasikan sebagai sebuah **Grid 2D berukuran W × H** (default 32 × 32 = 1.024 sel) yang di-overlay di atas arena 3D. Setiap sel grid dipetakan satu-ke-satu ke posisi dunia (`worldPosition`) dan memiliki atribut biner `isWalkable` yang ditentukan melalui **Physics Sphere-Check** terhadap `obstacleMask` pada saat inisialisasi.

**Representasi Masalah — Weighted Graph (Uniform Cost):**

Grid tersebut secara implisit membentuk sebuah **graph berbobot seragam** G = (V, E) di mana:

- **V** = himpunan seluruh node yang `isWalkable == true`.
- **E** = himpunan edge 4-connected adjacency (atas, bawah, kiri, kanan) dengan bobot konstan **w = 1**.

### 1.3 Elemen Entitas

| Entitas | Deskripsi |
|---|---|
| **Player (Cube)** | Entitas yang dikendalikan pemain, bergerak step-by-step pada grid dengan mekanisme rolling 90°. |
| **Ghost (Enemy AI)** | Entitas musuh otonom yang mengejar player menggunakan Dijkstra's pathfinding. |
| **Collectibles** | Objek yang di-spawn secara stokastik pada node walkable acak; memberikan skor. |
| **Power-Ups** | Variasi collectible yang memberikan efek khusus: *Invincible*, *Freeze Ghost*, *Unlimited Stamina*. |
| **Portal (Teleporter)** | Mekanisme teleportasi instan antar dua titik pada grid. |

### 1.4 Paradigma Algoritma

#### A. Dijkstra's Shortest Path Algorithm

Algoritma inti yang digunakan untuk AI pathfinding Ghost adalah **Dijkstra's Algorithm**. Pemilihan ini didasarkan pada properti berikut:

-  Jika P* adalah jalur terpendek dari node `s` ke node `t` yang melewati node `v`, maka sub-jalur `s → v` dan `v → t` juga merupakan jalur terpendek masing-masing. Properti ini dipenuhi karena bobot edge non-negatif (w = 1 ≥ 0).
- Pada setiap iterasi, Dijkstra memilih node dengan `gCost` minimum dari *open set*. Keputusan greedy ini bersifat **safe** karena semua bobot ≥ 0, menjamin bahwa node yang di-extract tidak akan pernah diperbarui lagi.
- Dijkstra menjamin **shortest path** pada graph dengan bobot non-negatif. Karena semua edge memiliki bobot 1 (uniform), algoritma ini ekuivalen secara output dengan BFS, namun diimplementasikan dengan struktur Dijkstra.

#### B. Greedy Heuristic — Speed Boost AI

Ghost AI menerapkan mekanisme **Random Speed Boost** — sebuah strategi *stochastic greedy*:

Setiap interval acak `t ∈ [minWait, maxWait]`, kecepatan Ghost digandakan selama `boostDuration` detik.

#### C. Stochastic Spawning (Randomized Greedy)

Sistem spawning collectible menggunakan **Randomized Selection** dengan probabilitas:

- `P(PowerUp) = powerUpChance / 100`, sisanya normal collectible (default 30% power-up).
- Posisi spawn dipilih secara **uniform random** dari node walkable, dengan eksklusi posisi player (greedy constraint untuk fairness).

---

## 2. Implementation

### 2.1 Arsitektur & Struktur Proyek
```
Assets/Scripts/
├── Algorithms/          # Algoritma pathfinding
│   ├── DijkstraPathfinding.cs
│   └── Node.cs
├── Core/                # Manager & sistem inti
│   ├── GridManager.cs
│   ├── CollectibleManager.cs
│   ├── PowerUpManager.cs
│   └── SceneTransitionManager.cs
├── Entities/            # Logika entitas game
│   ├── GhostAI.cs
│   └── PlayerController.cs
├── Environment/         # Objek lingkungan
│   ├── Collectible.cs
│   ├── CollectibleRotator.cs
│   └── UniversalPortal.cs
├── Movement/            # Sistem pergerakan
│   └── CubeRoller.cs
└── UI/                  # Antarmuka pengguna
    ├── CountdownManager.cs
    ├── GameOverUI.cs
    ├── HoldToQuit.cs
    └── SceneTrigger.cs
```

**Pola Desain :**

| Pattern | Lokasi | Tujuan |
|---|---|---|
| **Singleton** | `GridManager`, `CollectibleManager`, `PowerUpManager`, `CountdownManager`, `SceneTransitionManager` | Menjamin satu instance global dan akses lintas-komponen. |
| **Component-Based** | Seluruh `MonoBehaviour` | Pemisahan behavior ke komponen independen sesuai paradigma Unity. |
| **Observer (via Unity Events)** | `OnTriggerEnter`, Coroutines | Interaksi reaktif antar entitas melalui collision callback. |
| **Separation of Concerns** | Folder `Algorithms/`, `Core/`, `Entities/`, dll. | Pemisahan tanggung jawab secara modular. |

### 2.2 Struktur Data Utama

| Struktur Data | Lokasi | Tujuan |
|---|---|---|
| `Node[,]` (2D Array) | `GridManager.cs` | Menyimpan seluruh node grid; akses O(1) via indeks `[x, y]`. |
| `Node` (Class) | `Node.cs` | Menyimpan `gridPosition`, `worldPosition`, `isWalkable`, `gCost`, `parent`. |
| `List<Node>` (Open Set) | `DijkstraPathfinding.cs` | Daftar node yang belum dieksplorasi (linear scan untuk extract-min). |
| `HashSet<Node>` (Closed Set) | `DijkstraPathfinding.cs` | Himpunan node yang telah dieksplorasi; lookup O(1) amortized. |
| `List<Node>` (Path) | `GhostAI.cs` | Menyimpan jalur hasil pathfinding dari Ghost ke Player. |

**Definisi `Node` — unit terkecil dalam graph grid (`Node.cs`):**
```csharp
public class Node
{
    public Vector2Int gridPosition; // Koordinat grid (x, y)
    public Vector3 worldPosition;   // Posisi 3D di dunia
    public bool isWalkable;         // Bisa dilewati atau obstacle
    public int gCost;               // Jarak dari startNode (Dijkstra)
    public Node parent;             // Pointer balik untuk retrace jalur

    public void ResetNode()
    {
        gCost = int.MaxValue; // Inisialisasi sebagai ∞ (belum dijelajahi)
        parent = null;
    }
}
```

**Pembuatan grid via Physics Scan (`GridManager.GenerateGrid`):**
```csharp
public void GenerateGrid()
{
    nodes = new Node[gridSize.x, gridSize.y];
    Vector3 worldBottomLeft = transform.position
        - Vector3.right * gridSize.x / 2
        - Vector3.forward * gridSize.y / 2;

    for (int x = 0; x < gridSize.x; x++)
        for (int y = 0; y < gridSize.y; y++)
        {
            Vector3 worldPoint = worldBottomLeft
                + Vector3.right * (x + 0.5f)
                + Vector3.forward * (y + 0.5f);
            // Deteksi obstacle via sphere-cast ke layer obstacle
            bool isObstacle = Physics.CheckSphere(
                worldPoint + Vector3.up * 0.5f, 0.3f, obstacleMask);
            nodes[x, y] = new Node(new Vector2Int(x, y), worldPoint, !isObstacle);
        }
}
```

### 2.3 Mekanika AI — GhostAI

Berikut alur keputusan Ghost AI setiap langkah:

```
┌───────────────────────────────────────────────────┐
│                  GhostAI Loop                     │
│                                                   │
│  1. Cek apakah Ghost frozen → delay & retry       │
│  2. Ambil posisi saat ini → GetNodeFromWorldPoint  │
│  3. Jalankan Dijkstra(ghost_pos, player_pos)       │
│  4. Ambil node pertama dari path → nextNode        │
│  5. Hitung arah gerak (diffX / diffY)              │
│  6. Panggil CubeRoller.Roll(direction, speed)      │
│  7. onComplete → WaitAndMove → kembali ke step 1  │
└───────────────────────────────────────────────────┘
```

**Karakteristik utama:**

- **Re-Pathfinding per Step:** Ghost menghitung ulang jalur Dijkstra **setiap satu langkah** gerak, menjamin jalur selalu akurat terhadap posisi player terbaru (*reactive pursuit*).
- **Decision Delay:** Jeda `0.01 detik` antar keputusan mencegah infinite loop dan memberi frame untuk rendering.
- **Speed Boost Coroutine:** Kecepatan Ghost berfluktuasi secara stokastik antara `normalSpeed` (1×) dan `boostedSpeed` (2×) melalui `RandomSpeedBoostRoutine()`, dengan visual feedback berupa perubahan warna material (merah → ungu).

**Implementasi Dijkstra's pathfinding (`DijkstraPathfinding.cs`):**
```csharp
public static List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
{
    Node startNode = GridManager.Instance.GetNodeFromWorldPoint(startPos);
    Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(targetPos);

    List<Node> openSet = new List<Node>();
    HashSet<Node> closedSet = new HashSet<Node>();
    GridManager.Instance.ResetAllNodes();

    startNode.gCost = 0;
    openSet.Add(startNode);

    while (openSet.Count > 0)
    {
        // Extract-Min: linear scan O(V) — bottleneck utama
        Node current = openSet[0];
        for (int i = 1; i < openSet.Count; i++)
            if (openSet[i].gCost < current.gCost) current = openSet[i];

        openSet.Remove(current);
        closedSet.Add(current);

        if (current == targetNode) return RetracePath(startNode, targetNode);

        foreach (Node neighbor in GridManager.Instance.GetNeighbors(current))
        {
            if (!neighbor.isWalkable || closedSet.Contains(neighbor)) continue;
            int newCost = current.gCost + 1; // bobot edge seragam w=1
            if (newCost < neighbor.gCost)
            {
                neighbor.gCost = newCost;
                neighbor.parent = current;
                if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
            }
        }
    }
    return new List<Node>(); // path tidak ditemukan
}
```

**Eksekusi langkah Ghost setiap giliran (`GhostAI.MakeNextMove`):**
```csharp
void MakeNextMove()
{
    if (PowerUpManager.Instance.IsGhostFrozen)
    { Invoke(nameof(MakeNextMove), 0.5f); return; } // Freeze power-up aktif

    Node currentNode = GridManager.Instance.GetNodeFromWorldPoint(transform.position);
    currentPath = DijkstraPathfinding.FindPath(transform.position, playerTransform.position);

    if (currentPath != null && currentPath.Count > 0)
    {
        Node nextNode = currentPath[0];
        int diffX = nextNode.gridPosition.x - currentNode.gridPosition.x;
        int diffY = nextNode.gridPosition.y - currentNode.gridPosition.y;

        Vector3 moveDir = diffX != 0
            ? new Vector3(Mathf.Clamp(diffX, -1, 1), 0, 0)
            : new Vector3(0, 0, Mathf.Clamp(diffY, -1, 1));

        roller.Roll(moveDir, ghostSpeedMultiplier, () => StartCoroutine(WaitAndMove()));
    }
}
```

**Speed boost stokastik (`GhostAI.RandomSpeedBoostRoutine`):**
```csharp
private IEnumerator RandomSpeedBoostRoutine()
{
    while (true)
    {
        yield return new WaitForSeconds(Random.Range(minWaitBoost, maxWaitBoost));
        ghostSpeedMultiplier = boostedSpeed;
        ghostRenderer.material.color = boostColor; // visual: merah → ungu
        yield return new WaitForSeconds(boostDuration);
        ghostSpeedMultiplier = normalSpeed;
        ghostRenderer.material.color = normalColor; // kembali merah
    }
}

### 2.4 Mekanika Player — PlayerController

- **Input Handling:** Mendukung **WASD** dan **Arrow Keys** (4-directional discrete movement).
- **Stamina System:** Implementasi sprint dengan mekanika bertingkat:
  - *Drain* saat sprint aktif: `currentStamina -= drainRate × Δt`
  - *Refill* saat idle (dengan delay 0.5s): `currentStamina += refillRate × Δt`
  - *Cooldown* setelah sprint berhenti: `sprintCooldownDuration = 1.5s`
  - *Override* saat power-up Unlimited Stamina aktif → stamina selalu penuh.
- **Grid Validation:** Sebelum bergerak, target node dicek walkability via `GridManager.GetNodeFromWorldPoint()`, memastikan pemain tidak bisa menembus obstacle.

**Logika stamina dan sprint cooldown (`PlayerController.HandleTimers`):**
```csharp
void HandleTimers()
{
    bool isUnlimited = PowerUpManager.Instance != null
        && PowerUpManager.Instance.IsUnlimitedStamina;

    // Hitung mundur sprint cooldown
    if (!canSprint)
    {
        sprintCooldownTimer -= Time.deltaTime;
        if (sprintCooldownTimer <= 0) canSprint = true;
    }

    if (isSprinting && roller.IsRolling)
    {
        if (!isUnlimited)
        {
            currentStamina -= drainRate * Time.deltaTime;
            if (currentStamina <= 0) { currentStamina = 0; isSprinting = false; }
        }
        else currentStamina = maxStamina; // power-up unlimited stamina
    }
    else if (currentStamina < maxStamina)
    {
        // Refill setelah delay 0.5s
        if (refillTimer > 0) refillTimer -= Time.deltaTime;
        else currentStamina += refillRate * Time.deltaTime;
    }
    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
}

### 2.5 Sistem Pergerakan — CubeRoller

Animasi pergerakan kubus menggunakan teknik **RotateAround** yang memberikan efek rolling realistis:

- Rotasi 90° di sekitar *pivot point* (edge bawah kubus) ke arah gerakan.
- Kecepatan rotasi: `baseRollSpeed × speedMultiplier` derajat/detik.
- Setelah rotasi selesai, posisi di-**snap** ke grid melalui rounding:
  ```csharp
  transform.position = new Vector3(
      Mathf.Round(transform.position.x * 2) / 2f,
      transform.position.y,
      Mathf.Round(transform.position.z * 2) / 2f
  );
  ```
  Ini menghindari akumulasi floating-point error pada pergerakan berulang.

### 2.6 Sistem Power-Up & Collectible

**Tipe Collectible:**

| Tipe | Efek | Durasi |
|---|---|---|
| `Normal` | +10 skor | — |
| `Invincible` | Player kebal; menyentuh Ghost = Ghost dihancurkan | 5 detik |
| `Freeze` | Ghost membeku (berhenti bergerak) | 5 detik |
| `UnlimitedStamina` | Stamina tidak habis saat sprint | 5 detik |

**Mekanisme Spawning:**
- Hanya **satu** collectible aktif pada satu waktu di arena.
- Setelah dikumpulkan, collectible berikutnya langsung di-spawn pada node walkable acak.
- Flag `isCollected` pada `Collectible` mencegah *double-collection* dari trigger event yang tumpang tindih.

**Deteksi pengambilan collectible (`Collectible.cs`):**
```csharp
public enum CollectibleType { Normal, Invincible, Freeze, UnlimitedStamina }

private void OnTriggerEnter(Collider other)
{
    if (isCollected) return; // guard: cegah double-trigger
    if (other.CompareTag("Player"))
    {
        isCollected = true;
        CollectibleManager.Instance.OnCollected(this); // update skor & spawn berikutnya
        Destroy(gameObject);
    }
}
```

**Update skor dan aktivasi power-up (`CollectibleManager.OnCollected`):**
```csharp
public void OnCollected(Collectible item)
{
    currentScore += item.scoreValue;
    scoreText.text = currentScore.ToString();
    FinalScore = currentScore; // disimpan sebagai static untuk GameOver screen

    if (item.type != CollectibleType.Normal)
        PowerUpManager.Instance.ActivatePowerUp(item.type);

    currentSpawnedItem = null;
    SpawnNext(); // langsung spawn collectible berikutnya
}
```

**Aktivasi power-up via Coroutine (`PowerUpManager.cs`):**
```csharp
public void ActivatePowerUp(CollectibleType type)
{
    StopCoroutine(type.ToString()); // reset jika masih aktif
    StartCoroutine(type.ToString());
}

IEnumerator Freeze()
{
    IsGhostFrozen = true;  // GhostAI akan cek flag ini setiap langkah
    yield return new WaitForSeconds(powerUpDuration);
    IsGhostFrozen = false;
}
// Invincible dan UnlimitedStamina mengikuti pola yang sama
```

**Visual Polish:**
- Collectible memiliki animasi **rotasi kontinu** dan opsional **bobbing sinusoidal**:
  `y(t) = y₀ + A · sin(2π · f · t)`

### 2.7 Sistem Portal & Teleportasi

- Portal menggunakan **Trigger-Based Collision** (`OnTriggerEnter`) untuk mendeteksi player.
- Flag statis `isTeleporting` mencegah *re-entry* saat teleportasi sedang berlangsung.
- Saat teleportasi: `CubeRoller` dinonaktifkan sementara, velocity direset, posisi dipindahkan ke `destination`, lalu `CubeRoller` diaktifkan kembali setelah cooldown (1.5s).

**Urutan teleportasi (`UniversalPortal.TeleportSequence`):**
```csharp
private IEnumerator TeleportSequence(Transform playerTransform)
{
    isTeleporting = true;

    // 1. Nonaktifkan movement selama teleportasi
    CubeRoller roller = playerTransform.GetComponent<CubeRoller>();
    if (roller != null) roller.enabled = false;

    // 2. Reset momentum
    Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
    if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

    // 3. Snap posisi ke node tujuan (tetap pada grid)
    Node targetNode = GridManager.Instance.GetNodeFromWorldPoint(destination.position);
    if (targetNode != null)
        playerTransform.position = targetNode.worldPosition + Vector3.up * 0.5f
            + destination.forward;

    // 4. Aktifkan kembali setelah teleportasi
    if (roller != null) roller.enabled = true;
    yield return new WaitForSeconds(cooldown); // cooldown 1.5s cegah re-entry
    isTeleporting = false;
}

### 2.8 Manajemen Scene & UI

**Scene Flow:**

```
MainMenu  ──▶  GameScene  ──▶  GameOver
   ▲                               │
   └───────────────────────────────┘
```

- **3 Scene:** `MainMenu`, `GameScene`, `GameOver`.
- Transisi scene menggunakan `SceneTransitionManager` dengan efek **fade in/out** melalui `CanvasGroup.alpha` lerp.
- `DontDestroyOnLoad` pada `SceneTransitionManager` menjamin persistensi lintas scene.

**Fitur UI:**

| Fitur | Implementasi |
|---|---|
| **Score Display** | `TextMeshProUGUI` diperbarui real-time via `CollectibleManager`. |
| **Stamina Bar** | `Slider` dengan perubahan warna dinamis: cyan (normal), abu-abu (cooldown), kuning (unlimited). |
| **Countdown** | Animasi scale-down 1.5× → 1× pada teks countdown; memblokir input hingga selesai. |
| **Final Score** | Skor disimpan sebagai `static` property `FinalScore`, persisten antar scene. |
| **Hold-to-Quit** | Menahan tombol Escape selama 2 detik dengan visual feedback `fillAmount`. |

---

## 3. Evaluation

### 3.1 Analisis Kompleksitas Waktu

#### Dijkstra's Pathfinding — `FindPath()`

Misalkan **V** = jumlah node walkable dan **E** = jumlah edge.

**Implementasi saat ini (List — linear scan extract-min):**

```
T(V, E) = O(V² + E)
```

- Setiap iterasi: extract-min O(V) dilakukan O(V) kali → O(V²).
- Relaxation edge: total O(E) dengan `Contains` check tambahan O(V) per neighbor.
- **Worst case** untuk grid 32 × 32: V = 1.024, E ≤ 4V = 4.096 → **T ≈ O(1.048.576)** operasi per panggilan.

**Jika menggunakan Binary Min-Heap (optimasi):**

```
T(V, E) = O((V + E) log V)
```

- Untuk grid yang sama: T ≈ O(5.120 × 10) = **O(51.200)** — **~20× lebih cepat**.

#### Operasi Lainnya

| Operasi | Kompleksitas | Keterangan |
|---|---|---|
| `GenerateGrid()` | O(W × H) | Dipanggil sekali saat `Awake()`. Untuk 32 × 32: O(1.024). |
| `ResetAllNodes()` | O(W × H) | Dipanggil setiap kali `FindPath()` berjalan. |
| `GetNodeFromWorldPoint()` | O(1) | Konversi koordinat world ke indeks via rumus aritmatika. |
| `GetRandomWalkableNode()` | O(1) avg, O(100) worst | Sampling acak dengan maks 100 percobaan. |
| `PlayerController.Update()` | O(1) | Input check, timer decrement, UI update — semua konstan. |
| **Ghost AI per step** | O(V² + E) | Dijkstra + O(1) move execution. Berjalan ~1× per step (bukan per frame). |

### 3.2 Analisis Kompleksitas Ruang

| Komponen | Ruang | Keterangan |
|---|---|---|
| Grid `Node[,]` | O(W × H) | 32 × 32 = 1.024 node |
| Dijkstra Open Set | O(V) worst case | Maksimum seluruh node walkable |
| Dijkstra Closed Set | O(V) worst case | Maksimum seluruh node walkable |
| Path Result | O(V) worst case | Jalur terpanjang mungkin melewati seluruh node |
| Node fields (`gCost`, `parent`) | O(V) | Tersimpan langsung di node (in-place) |
| **Total** | **O(W × H)** | Didominasi oleh grid |

Untuk grid default: **~1.024 node × ~40 byte/node ≈ 40 KB** — sangat ringan untuk aplikasi real-time.

### 3.3 Efisiensi Struktur Data

| Struktur | Operasi | Kompleksitas | Catatan |
|---|---|---|---|
| `Node[,]` | Akses per indeks | O(1) | Akses langsung via array 2D |
| `Node[,]` | `GetNodeFromWorldPoint` | O(1) | Konversi koordinat → indeks via aritmatika |
| `List<Node>` (Open Set) | Extract-Min | O(n) | **Linear scan — bottleneck utama** |
| `List<Node>` (Open Set) | `Contains` | O(n) | Linear search |
| `List<Node>` (Open Set) | `Remove` | O(n) | Shift elemen |
| `HashSet<Node>` (Closed Set) | `Contains` | O(1) amortized | Hash-based lookup |
| `HashSet<Node>` (Closed Set) | `Add` | O(1) amortized | Hash-based insertion |
| `GetNeighbors` | Per-node | O(1) | Maksimum 4 tetangga (konstan) |

> **⚠️ Bottleneck:** Penggunaan `List<Node>` sebagai open set menyebabkan extract-min berjalan dalam O(V), bukan O(log V) seperti pada implementasi dengan **Min-Heap / Priority Queue**.

### 3.4 Jaminan Kebenaran (Correctness)

#### Dijkstra Correctness

- **Invariant:** Setiap node yang masuk ke `closedSet` memiliki `gCost` yang merupakan jarak terpendek dari `startNode`.
- **Proof Sketch:** Karena semua bobot edge w = 1 ≥ 0, greedy extraction dari open set menjamin bahwa tidak ada jalur lebih pendek ke node yang sudah di-close. Ini memenuhi **Dijkstra's Invariant**.
- **Termination:** Setiap iterasi memindahkan satu node dari `openSet` ke `closedSet`. Karena |V| terbatas, loop terminates dalam ≤ |V| iterasi.
- **Completeness:** Jika path ada, Dijkstra akan menemukannya. Jika `openSet` kosong sebelum `targetNode` dicapai, fungsi mengembalikan list kosong (handled correctly).

#### Path Retrace Correctness

- `RetracePath()` menelusuri `parent` pointer dari `endNode` ke `startNode`, kemudian `Reverse()`. Ini benar selama `parent` di-set secara akurat oleh relaxation step.

#### Boundary Safety

- `GetNodeFromWorldPoint()` menggunakan `Mathf.Clamp` → indeks selalu dalam batas `[0, gridSize-1]`, mencegah `IndexOutOfRangeException`.
- `GetNeighbors()` melakukan bounds check eksplisit sebelum mengakses `nodes[checkX, checkY]`.

#### Collision & State Safety

- Flag `isCollected` pada `Collectible` mencegah *double-collection*.
- Flag statis `isTeleporting` pada `UniversalPortal` mencegah *portal re-entry* saat teleportasi sedang berlangsung.

### 3.5 Identifikasi Bottleneck & Potensi Optimasi

| Masalah | Dampak | Solusi yang Direkomendasikan |
|---|---|---|
| Open Set menggunakan `List` | Extract-min O(V) per iterasi → O(V²) total | Ganti dengan `PriorityQueue` / Binary Min-Heap → O((V+E) log V) |
| Pathfinding state in-place pada Node | Tidak scalable untuk multi-agent | Gunakan **per-query state** (dictionary lokal) alih-alih mutasi node global |
| `ResetAllNodes()` setiap pathfinding | O(V) overhead per panggilan | Gunakan **versioned reset** (timestamp/counter) untuk lazy reset |
| `GetNeighbors()` alokasi List baru | Alokasi GC pressure setiap panggilan | Gunakan pre-allocated buffer atau `stackalloc` |

---

## 4. Conclusion

### Kualitas Arsitektur Teknis

Arsitektur *Deadlock Chase* menunjukkan pemisahan tanggung jawab (*separation of concerns*) yang **baik** melalui struktur folder modular (`Algorithms`, `Core`, `Entities`, `Environment`, `Movement`, `UI`) dan penggunaan Singleton pattern untuk manager classes. Desain component-based mengikuti konvensi Unity dengan tepat, memungkinkan extensibility yang wajar. Sistem power-up, stamina, dan scoring diimplementasikan secara bersih dengan state management yang jelas.

### Optimalitas Algoritma

Algoritma Dijkstra menjamin **shortest path yang benar** untuk menyelesaikan masalah pursuit pada graph berbobot seragam. Dalam konteks grid 32 × 32 dengan batasan performa real-time:

- **Secara fungsional:** Algoritma bekerja **100% benar** — path terpendek selalu ditemukan, boundary selalu aman, dan edge case tertangani dengan baik.
- **Secara performa:** Implementasi saat ini berjalan dalam O(V²) per panggilan karena penggunaan `List` sebagai open set. Untuk grid 1.024 node, ini menghasilkan ~10⁶ operasi per langkah Ghost — **masih dalam batas toleransi real-time** (~60 FPS) untuk single agent.
- **Potensi optimasi:** Substitusi `List` dengan `PriorityQueue` / Min-Heap akan menurunkan kompleksitas ke O((V+E) log V), memberikan headroom **~20×** untuk skenario multi-agent atau grid lebih besar.

### Skalabilitas

Penyimpanan data pathfinding secara *in-place* pada node grid (`gCost`, `parent`) membatasi penggunaan concurrent multi-agent pathfinding. Solusinya adalah menggunakan **per-query state** (dictionary lokal) alih-alih mutasi node global, memungkinkan multiple Ghost berjalan secara independen tanpa race condition.

### Verdict

> **Deadlock Chase** memiliki arsitektur yang **solid dan fungsional** untuk skala permainan saat ini, dengan algoritma pathfinding yang **benar dan optimal secara output**. Optimasi pada struktur data open set dan decoupling state pathfinding dari node grid akan menjadikan sistem ini **production-grade** untuk skenario yang lebih kompleks.

---

### 📁 Ringkasan File Proyek

| File | LOC | Peran |
|---|---|---|
| `DijkstraPathfinding.cs` | 75 | Algoritma Dijkstra untuk AI pathfinding |
| `Node.cs` | 25 | Data structure node grid |
| `GridManager.cs` | 127 | Manajemen grid, neighbor lookup, random spawn |
| `CollectibleManager.cs` | 79 | Spawning & scoring collectible |
| `PowerUpManager.cs` | 46 | Aktivasi & durasi power-up |
| `SceneTransitionManager.cs` | 59 | Transisi scene dengan fade effect |
| `GhostAI.cs` | 150 | AI musuh dengan Dijkstra + speed boost |
| `PlayerController.cs` | 167 | Input, sprint, stamina management |
| `CubeRoller.cs` | 45 | Animasi rolling kubus |
| `Collectible.cs` | 28 | Collision handler collectible |
| `CollectibleRotator.cs` | 31 | Animasi visual collectible |
| `UniversalPortal.cs` | 49 | Sistem teleportasi portal |
| `CountdownManager.cs` | 54 | Countdown sebelum game dimulai |
| `GameOverUI.cs` | 15 | Tampilan skor akhir |
| `HoldToQuit.cs` | 39 | Mekanisme hold-to-quit |
| `SceneTrigger.cs` | 32 | Tombol navigasi scene |
| **Total** | **~1.021** | |
