# Scraper Service Kullanım Kılavuzu

Bu servis, çeşitli etkinlik sitelerinden (Biletino, Biletinial, Bubilet, Biletix) veri çekmek için tasarlanmıştır.

## 🚀 Sistemi Başlatma

Tüm sistemi (veritabanları, kuyruklar ve servisler) başlatmak için proje ana dizininde şu komutu çalıştırın:

```bash
docker-compose up -d --build
```

Bu komut şunları başlatır:
- **MongoDB**: Verilerin kaydedildiği veritabanı.
- **RabbitMQ**: Mesaj kuyruğu.
- **Selenium Grid**: Tarayıcı otomasyonu için.
- **FlareSolverr**: Cloudflare korumasını aşmak için.
- **ScraperService**: Ana servis.

## ⚙️ Otomatik Çalışma

Servis ayağa kalktığında `ScraperBackgroundService` otomatik olarak devreye girer ve tanımlı tüm siteleri (Bubilet, Biletinial, Biletino, Biletix) sırayla taramaya başlar.

## 🎮 Manuel Tetikleme (API)

Belirli bir siteyi veya tüm siteleri manuel olarak taratmak isterseniz aşağıdaki API endpoint'lerini kullanabilirsiniz.

### 1. Tek Bir Siteyi Tetikleme
Belirli bir siteyi (örn: Biletino) ve şehri (opsiyonel) taratmak için:

**Biletino (İstanbul):**
```bash
curl -X POST "http://localhost:5004/api/Scraper/start/Biletino?city=istanbul" \
     -H "Content-Type: application/json" \
     -d '{}'
```

**Biletinial (Tüm Şehirler):**
```bash
curl -X POST "http://localhost:5004/api/Scraper/start/Biletinial" \
     -H "Content-Type: application/json" \
     -d '{}'
```

**Desteklenen Site İsimleri:**
- `Biletino`
- `Biletinial`
- `Bubilet`
- `Biletix`

### 2. Tüm Siteleri Tetikleme
Tüm tanımlı scraper'ları çalıştırmak için:

```bash
curl -X POST "http://localhost:5004/api/Scraper/start" \
     -H "Content-Type: application/json" \
     -d '{}'
```

## 📊 Logları İzleme

Servisin ne yaptığını görmek için logları takip edebilirsiniz:

```bash
docker logs -f etkinlik-scraperservice
```

**Önemli Loglar:**
- `Matched Biletino`: Scraper bulundu ve çalışmaya başlıyor.
- `Found X potential event bodies`: Sayfada kaç etkinlik bulunduğu.
- `Publishing X events`: Etkinliklerin kuyruğa gönderildiği (ve veritabanına yazıldığı).

## 💾 Verileri Kontrol Etme (MongoDB)

Kaydedilen etkinlik sayılarını görmek için:

```bash
docker exec etkinlik-mongo mongosh -u admin -p 'Password123!' --authenticationDatabase admin EventCatalogDb --eval 'db.Events.aggregate([{$group: {_id: "$Source", count: {$sum: 1}}}]).toArray()'
```
