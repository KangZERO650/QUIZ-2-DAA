# Report Quiz 2 DAA : Deadlock Chase (V2)

| Nama              | NRP        |
|-------------------|------------|
| Raziq Danish Safaraz          | 5025241229      |
| Aminnudin Wijaya          | 5025241229      |
| Herdian Tri Wardhana           | 5025241229      |

## 1. Design

### 1.1 Deskripsi Umum

Deadlock Chase dirancang sebagai permainan berbasis *grid* 2 dimensi berukuran **32 × 32 sel**. Setiap sel merepresentasikan satu unit ruang permainan diskret yang dapat ditempati oleh entitas (pemain maupun hantu). Pilihan desain ini memastikan bahwa seluruh logika navigasi dapat dimodelkan secara matematis dengan tepat, menghindari ambiguitas posisi yang lazim terjadi pada ruang kontinu.

### 1.2 Representasi Masalah: Graf Berbobot Seragam

Ruang permainan direpresentasikan sebagai **graf tak-berarah berbobot seragam** $G = (V, E, W)$, dengan:

* **Vertices** $V$: Himpunan simpul yang direpresentasikan oleh matriks dua dimensi `nodes[x, y]`, di mana setiap elemen merupakan objek `Node` yang menyimpan status dan biaya lintasan.
* **Edges** $E$: Koneksi 4-arah (*cardinal directions*) antar simpul bertetangga yang hanya terbentuk apabila atribut `isWalkable` bernilai *true*, secara efektif mengenkode geometri rintangan ke dalam struktur graf.
* **Weight** $W$: Bobot seragam $w = 1$ untuk setiap perpindahan antar simpul, menyederhanakan masalah pencarian jalur terpendek menjadi persoalan minimisasi jumlah langkah.

### 1.3 Penerapan Algoritma Greedy

Algoritma yang diterapkan adalah **Dijkstra**, yang diklasifikasikan dalam paradigma **Greedy**. Pada setiap iterasi, algoritma secara konsisten memilih simpul dengan nilai `gCost` (akumulasi biaya dari titik awal) terkecil dari *open set*. Pilihan lokal optimal ini, dalam konteks graf tanpa bobot negatif, secara matematis terbukti menghasilkan **solusi global optimal**.

### 1.4 Dasar Teori

Kebenaran algoritma Dijkstra sebagai solusi optimal bertumpu pada dua properti fundamental:

* **Optimal Substructure:** Masalah pencarian jalur terpendek memiliki sifat bahwa jalur optimal dari simpul sumber $s$ ke tujuan $t$ selalu mengandung jalur-jalur terpendek menuju setiap simpul perantara $v$ di sepanjang rute tersebut. Properti ini memungkinkan konstruksi solusi akhir secara inkremental dari solusi sub-permasalahan yang lebih kecil, sehingga menjustifikasi pendekatan pemrograman dinamis implisit di balik algoritma Dijkstra.
* **Greedy Choice Property:** Dengan tidak adanya bobot negatif, keputusan greedy untuk selalu mengekspansi simpul berbiaya terendah tidak akan pernah perlu direvisi, memastikan bahwa setiap simpul yang masuk ke *closed set* telah ditetapkan jarak optimalnya secara permanen.

---

## 2. Implementation

### 2.1 Struktur Code

Implementasi pada kode `C#` memanfaatkan dua struktur data primer yang dipilih dengan pertimbangan performa:

* **`List<Node>` (Open Set):** Menyimpan simpul-simpul kandidat yang akan diekspansi. Pemilihan simpul dengan `gCost` minimum dilakukan melalui iterasi linear, yang merupakan area dengan potensi optimasi lebih lanjut (lihat bagian Evaluasi).
* **`HashSet<Node>` (Closed Set):** Menyimpan simpul-simpul yang telah diproses sepenuhnya. Penggunaan `HashSet` secara krusial memberikan kompleksitas pencarian rata-rata $O(1)$ untuk operasi `Contains()`, berbanding terbalik dengan `List.Contains()` yang memerlukan $O(n)$.

### 2.2 Mekanika AI Ghost (Algoritma Dijkstra)

Alur eksekusi algoritma pada fungsi `FindPath()` berjalan sebagai berikut:

1. **Inisialisasi:** Simpul awal (`startNode`) dimasukkan ke dalam `openSet`.
2. **Seleksi Greedy:** Pada setiap iterasi, simpul `currentNode` dengan `gCost` terkecil dipilih dari `openSet` melalui iterasi linear.
3. **Terminasi Dini:** Jika `currentNode` sama dengan `targetNode`, rekonstruksi jalur dilakukan via `RetracePath()` menggunakan rantai pointer `parent`.
4. **Ekspansi Tetangga:** Setiap simpul tetangga yang *walkable* dan belum ada di `closedSet` dievaluasi. Jika biaya baru lebih kecil, `gCost` dan `parent` diperbarui, lalu simpul ditambahkan ke `openSet`.
5. **Terminasi Gagal:** Jika `openSet` habis tanpa menemukan target, fungsi mengembalikan `null`.

### 2.3 Mekanika Fisika dan Integrasi Visual

* **Rotasi Cube Roller:** Menggunakan `Quaternion.AngleAxis` untuk komputasi rotasi matematis presisi 90 derajat, dikombinasikan dengan *Coordinate Snapping* guna menjamin sinkronisasi posisi entitas pemain dengan pusat node grid dan mencegah akumulasi kesalahan floating-point.
* **Random Speed Boost:** Mekanisme ketidakpastian (*unpredictability*) diimplementasikan melalui `Coroutine` yang memodifikasi kecepatan hantu secara dinamis dalam interval acak 5–15 detik, menciptakan tantangan yang tidak dapat diprediksi secara deterministik oleh pemain.
* **Visual Feedback Real-Time:** Perubahan warna material hantu menjadi ungu saat *speed boost* aktif berfungsi sebagai sinyal visual yang jelas, meningkatkan keterbacaan status AI kepada pemain (*game readability*).

### 2.4 Manajemen Data dan User Experience (UX)

* **State Lock (`isCollected`):** Flag idempoten pada entitas `Collectible` mencegah *race condition* pada sistem skor, memastikan satu item hanya dapat dikumpulkan satu kali meskipun dipicu secara berulang dalam satu frame.
* **Hold-to-Quit:** Mekanisme keluar dengan akumulasi `Time.deltaTime` selama 2 detik pada tombol Escape memberikan jeda reflektif bagi pemain, mencegah terminasi sesi yang tidak disengaja.
* **Score Persistence:** Pemanfaatan variabel `static` untuk mempertahankan data skor selama transisi antar *scene* memilih pendekatan ringan dibandingkan solusi serialisasi penuh (seperti `PlayerPrefs`), yang tepat untuk siklus hidup permainan berskala kecil ini.

---

## 3. Evaluation

#### 3.1 Kompleksitas Ruang dan Waktu

* Time Complexity: O(V + E), karena setiap node dan sisi diperiksa maksimal satu kali dalam gridterstruktur.
* Space Complexity: O(V) untuk penyimpanan status node pada memori global selama proses pencarian.
  
### 3.2 Analisis Correctnes

* **Terminasi:** Dijamin karena jumlah simpul $|V|$ dalam grid berukuran tetap bersifat finit. Setiap iterasi memindahkan tepat satu simpul dari `openSet` ke `closedSet`, sehingga algoritma pasti berhenti dalam paling banyak $|V|$ iterasi.
* **Optimalitas:** Dalam graf dengan bobot seragam $w = 1$ dan tanpa bobot negatif, pemilihan greedy berdasarkan `gCost` terkecil dijamin menghasilkan jalur dengan jumlah langkah minimum. Invariant Dijkstra terpenuhi: setiap simpul yang masuk `closedSet` telah memiliki nilai `gCost` yang optimal secara permanen.
* **Rekonstruksi Jalur:** Mekanisme `RetracePath()` yang menelusuri rantai pointer `parent` secara terjamin akan menghasilkan jalur lengkap dari target kembali ke sumber, selama simpul target berhasil ditemukan.

**Catatan:** Implementasi tidak secara eksplisit menangani kasus graf yang tidak terhubung (*disconnected graph*), namun pengembalian nilai `null` berfungsi sebagai sinyal implisit yang dapat ditangkap oleh lapisan logika AI di atasnya.

---

## 4. Conclusion

Secara keseluruhan, *Deadlock Chase V2* telah berhasil mengimplementasikan sistem permainan berbasis *grid* 2D berukuran 32 × 32 yang dimodelkan sebagai graf tak-berarah berbobot seragam. Fondasi penerapan algortimam dalam permainan ini diperoleh AI hantu yang memanfaatkan algoritma Dijkstra (pendekatan *Greedy*) untuk komputasi pencarian jalur terpendek.

Selain kelengkapan algoritmik, dalam permainanan ini kami menyatukan beberapa elemen penting lainnya, yaitu:
* **Mekanika Visual & Fisika:** Pergerakan dan rotasi presisi pada pemain, serta integrasi *random speed boost* berbasis *Coroutine* yang memberikan dinamika tantangan tambahan.
* **Manajemen UX & Data:** Penerapan fitur penunjang pengalaman bermain yang komprehensif, mulai dari *state lock* pada sistem skor, perlindungan terminasi via *hold-to-quit*, hingga *score persistence* antar *scene*.

Walaupun masih terdapat ruang untuk optimasi performa di masa mendatang, seluruh arsitektur—mulai dari logika navigasi, mekanika permainan, hingga interaksi antarmuka telah kami lihat terintegrasi dengan cukup baik pada saat ini.

## Demostrassi Game

<img width="1920" height="1080" alt="Screenshot (199)" src="https://github.com/user-attachments/assets/a00238f5-ba77-4f48-91ba-1fe3cecea6d8" />

