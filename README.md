# ShipMank
Aplikasi pemesanan tiket rekreasi laut yang memudahkan wisatawan dalam memilih dan memesan kapal sesuai lokasi, waktu perjalanan, serta preferensi mereka. ShipMank menyediakan opsi kapal yang sudah terverifikasi kelayakan dan kualitasnya sehingga wisatawan dapat berlibur dengan aman dan nyaman.

Kelompok ShipMank<br>
Ketua Kelompok: Grace Anre Marcheline - 23/522362/TK/57654<br>
Anggota 1: P. Atalie Savista Arunata - 23/522436/TK/57668<br>
Anggota 2: Dhafarel Hariyanto - 23/522772/TK/57743<br>

## Berikut merupakan class diagram dari aplikasi ShipMank
<img width="842" height="871" alt="shipship-Halaman-5 drawio" src="https://github.com/user-attachments/assets/438319a7-0dd1-4014-826c-668fee6b32f5" />

---

## Teknologi yang Digunakan
Aplikasi ini dibangun menggunakan arsitektur **Object-Oriented Programming (OOP)** dengan stack teknologi berikut:

### **Core & Framework**
* **Bahasa Pemrograman:** C# (.NET Framework0
* **Framework UI:** WPF (Windows Presentation Foundation)

### **Database & Penyimpanan**
* **Database:** NeonDB PostgreSQL
* **Driver/Provider:** Npgsql 

### **Integrasi & Libraries**
* **Payment Gateway:** Midtrans Sandbox API
* **PDF Generation:** QuestPDF (untuk mencetak E-Ticket/Receipt)
* **QR Code:** QRCoder (untuk verifikasi tiket)
* **JSON Handling:** Newtonsoft.Json
* **Autentikasi:** Google Authentication (via OAuth 2.0)

---

## Alur & Fitur Utama
Aplikasi ShipMank memiliki alur penggunaan (*User Flow*) yang sistematis:

### 1. Autentikasi Pengguna 
* Pengguna melakukan **Registrasi** dan **Login** menggunakan akun Google.
* Data pengguna disimpan aman di database PostgreSQL NeonDB.

### 2. Eksplorasi Kapal 
* **Pencarian & Filter:** Pengguna dapat mencari kapal berdasarkan nama, lokasi (pelabuhan), kapasitas, harga, dan tipe kapal.
* **Detail Kapal:** Menampilkan informasi rinci seperti kapasitas, harga, fasilitas, availibilitas, dan rating review dari pengguna lain.

### 3. Booking & Payment
* **Booking:** Pengguna memilih tanggal dan kapal yang diinginkan. Sistem melakukan validasi ketersediaan (*Availability Check*) untuk mencegah *double booking*.
* **Integrasi Midtrans:**
    * Sistem membuat request pembayaran ke API Midtrans.
    * Pengguna mendapatkan **Kode Virtual Account (VA)** sesuai bank yang dipilih (BCA, Mandiri, BNI, BRI).
    * Status pembayaran diperbarui secara *real-time* atau melalui sinkronisasi manual (`Unpaid` -> `Upcoming`).

### 4. History & E-Ticket
* **Manajemen Status:** Pengguna dapat melihat status tiket:
    * `Unpaid`: Menunggu pembayaran.
    * `Upcoming`: Lunas, siap berangkat.
    * `Completed`: Perjalanan selesai.
    * `Cancelled`: Dibatalkan (refund otomatis jika didukung).
* **Cetak Tiket:** Pengguna dapat mengunduh **E-Receipt (PDF)** yang berisi detail perjalanan dan **QR Code** unik untuk discan saat keberangkatan.
