# 🔌 Veritabanı Bağlantı Rehberi

## 📊 Tüm Veritabanları Özet

| Veritabanı | Port | Kullanıcı | Şifre | Database(ler) |
|------------|------|-----------|-------|---------------|
| **PostgreSQL** | 5432 | admin | Password123! | FollowerDb, keycloakdb |
| **MongoDB** | 27017 | admin | Password123! | EventCatalogDb |
| **MS SQL Server** | 1433 | sa | Password123! | master, IdentityDb |
| **Redis** | 6379 | - | (no auth) | - |
| **RabbitMQ** | 5672, 15672 | guest | guest | - |

---

## 1️⃣ PostgreSQL

### 🎯 GUI Bağlantı (Önerim: TablePlus)

**TablePlus:** https://tableplus.com/

#### Keycloak Veritabanı:
```
Name: Keycloak DB
Host: localhost
Port: 5432
User: admin
Password: Password123!
Database: keycloakdb
SSL Mode: Disable
```

#### Follower Veritabanı:
```
Name: Follower DB
Host: localhost
Port: 5432
User: admin
Password: Password123!
Database: FollowerDb
SSL Mode: Disable
```

### 🔗 Connection String (.NET)

```csharp
// Keycloak
"Host=localhost;Port=5432;Database=keycloakdb;Username=admin;Password=Password123!"

// FollowerService
"Host=localhost;Port=5432;Database=FollowerDb;Username=admin;Password=Password123!"
```

### 💻 Komut Satırı

```bash
# Keycloak DB'ye bağlan
docker exec -it etkinlik-postgres psql -U admin -d keycloakdb

# Tablo listesi
\dt

# Clients tablosunu görüntüle
SELECT client_id, enabled FROM client;

# Çıkış
\q
```

---

## 2️⃣ MongoDB

### 🎯 GUI Bağlantı (MongoDB Compass)

**MongoDB Compass:** https://www.mongodb.com/try/download/compass

```
Connection String: mongodb://admin:Password123!@localhost:27017/?authSource=admin
```

### 💻 Komut Satırı

```bash
# MongoDB Shell'e bağlan
docker exec -it etkinlik-mongo mongosh -u admin -p Password123! --authenticationDatabase admin

# EventCatalogDb'yi kullan
use EventCatalogDb

# Collection'ları listele
show collections
```

---

## 3️⃣ MS SQL Server

### 🎯 GUI Bağlantı (Azure Data Studio)

**Azure Data Studio:** https://azure.microsoft.com/products/data-studio

```
Server: localhost,1433
Username: sa
Password: Password123!
Database: master
Trust Server Certificate: True
```

---

## 4️⃣ Redis

### 🎯 GUI Bağlantı (RedisInsight)

**RedisInsight:** https://redis.io/insight/

```
Host: localhost
Port: 6379
```

### 💻 Komut Satırı

```bash
# Redis CLI
docker exec -it etkinlik-redis redis-cli

# Tüm key'leri listele
KEYS *
```

---

## 5️⃣ RabbitMQ

### 🎯 Management UI

```
URL: http://localhost:15672
Username: guest
Password: guest
```

---

## 🚀 Hızlı Başlangıç

1. **TablePlus İndir:** https://tableplus.com/
2. **PostgreSQL Bağlantısı Ekle** (yukarıdaki bilgileri kullan)
3. **keycloakdb'ye bağlan** → 144 tablo göreceksin!

**Tüm detaylar bu dosyada! 📖**
