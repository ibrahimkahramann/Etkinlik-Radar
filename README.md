# 🎯 Etkinlik-Radar: Bilet Takip Platformu

Konser, tiyatro ve seminer gibi etkinliklerin biletlerini farklı platformlardan (Biletix, Passo, Bubilet) toplayarak tek bir çatı altında sunan mikroservis tabanlı bir platformdur.

## 🏗️ Teknoloji Stack

### Backend
- **.NET 8** - Mikroservisler
- **Keycloak** - Kimlik doğrulama (OAuth2/OpenID Connect)
- **RabbitMQ** - Asenkron mesajlaşma
- **Nginx** - API Gateway

### Veritabanları (Polyglot Persistence)
- **PostgreSQL** - İlişkisel veri (Follower, Keycloak)
- **MongoDB** - NoSQL (Etkinlik verileri)
- **MS SQL Server** - Legacy destek
- **Redis** - Cache

### Frontend
- **Blazor Web App** - Public site + Admin panel

## 📁 Proje Yapısı

```
Etkinlik-Radar/
├── src/
│   ├── Services/
│   │   ├── EventCatalogService/    # Etkinlik yönetimi (MongoDB + Redis)
│   │   ├── FollowerService/        # Takip sistemi (PostgreSQL + CQRS)
│   │   └── ScraperService/         # Web kazıma (RabbitMQ + WebAPI)
│   ├── Clients/
│   │   └── BlazorApp/              # Frontend
│   └── Gateways/
│       └── Nginx/                  # API Gateway
├── keycloak/
│   └── realm-export.json           # Keycloak otomatik kurulum
└── docker-compose.yml              # Tüm servisler
```

## 🚀 Hızlı Başlangıç

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Git

### 1. Projeyi Klonla
```bash
git clone https://github.com/YOUR_USERNAME/Etkinlik-Radar.git
cd Etkinlik-Radar
```

### 2. Docker Servislerini Başlat

⚠️ **ÖNEMLİ:** Docker Desktop'ın çalıştığından emin ol!

```bash
# Tüm servisleri başlat
docker-compose up -d

# Sadece veritabanları + Keycloak
docker-compose up postgres mongo redis rabbitmq keycloak -d

# Logları izle
docker-compose logs -f
```

**İlk başlatmada 30-60 saniye bekle!** (Keycloak realm'i import ediyor)

### 3. Mikroservisleri Çalıştır

```bash
# Tüm solution'ı build et
dotnet build

# Her servisi ayrı terminalde çalıştır
dotnet run --project src/Services/EventCatalogService/EventCatalogService.csproj
dotnet run --project src/Services/FollowerService/FollowerService.csproj
dotnet run --project src/Services/ScraperService/ScraperService.csproj
dotnet run --project src/Clients/BlazorApp/BlazorApp.csproj
```

## 🔗 Erişim URL'leri

| Servis | URL | Açıklama |
|--------|-----|----------|
| **Keycloak Admin** | http://localhost:8080/admin | admin / admin |
| **EventCatalog API** | http://localhost:5002/swagger | Etkinlik servisi |
| **Follower API** | http://localhost:5003/swagger | Takip servisi |
| **Scraper API** | http://localhost:5004/swagger | Kazıma servisi |
| **Blazor App** | http://localhost:5000 | Frontend |
| **RabbitMQ Management** | http://localhost:15672 | guest / guest |
| **Nginx Gateway** | http://localhost | API Gateway |

## 🔑 Keycloak Giriş Bilgileri

### Admin Console
- **URL:** http://localhost:8080/admin
- **Kullanıcı:** `admin`
- **Şifre:** `admin`

### Test Kullanıcıları
| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| testuser | test123 | admin, user |
| adminuser | admin123 | admin |

### API Client Secret'ları
- **eventcatalog-api:** `eventcatalog-secret-2024`
- **follower-api:** `follower-secret-2024`
- **scraper-api:** `scraper-secret-2024`

## 🧪 API Test Örneği

### 1. Keycloak'tan Token Al
```bash
curl -X POST http://localhost:8080/realms/etkinlik-radar/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=eventcatalog-api" \
  -d "client_secret=eventcatalog-secret-2024" \
  -d "grant_type=password" \
  -d "username=testuser" \
  -d "password=test123"
```

### 2. API'yi Token ile Çağır
```bash
TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCI..."

curl -X GET http://localhost:5002/weatherforecast \
  -H "Authorization: Bearer $TOKEN"
```

## 📊 Mikroservis Özellikleri

### EventCatalogService
- **Port:** 5002
- **DB:** MongoDB (EventCatalogDb)
- **Cache:** Redis
- **Özellikler:** Etkinlik CRUD, Redis cache, JWT auth

### FollowerService
- **Port:** 5003
- **DB:** PostgreSQL (FollowerDb)
- **Pattern:** CQRS (MediatR)
- **Özellikler:** Takip sistemi, PostgreSQL, JWT auth

### ScraperService
- **Port:** 5004
- **Queue:** RabbitMQ
- **Özellikler:** Manuel/otomatik web kazıma, asenkron işleme

## 🛠️ Geliştirme

### Solution'ı Aç
```bash
# Visual Studio
open EtkinlikRadar.sln

# VS Code
code .
```

### Yeni Migration Ekle (FollowerService için)
```bash
cd src/Services/FollowerService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Docker Container'ları Yeniden Oluştur
```bash
docker-compose down
docker-compose up --build -d
```

### PostgreSQL'e Bağlan
```bash
docker exec -it etkinlik-postgres psql -U admin -d FollowerDb
```

## 🐛 Sorun Giderme

### Docker başlamıyor?
```bash
# Docker Desktop'ın çalıştığını kontrol et
docker ps

# Varsa çakışan container'ları durdur
docker-compose down
```

### Keycloak realm import olmadı?
```bash
# Keycloak loglarını kontrol et
docker logs etkinlik-keycloak

# Manuel import
docker exec -it etkinlik-keycloak /opt/keycloak/bin/kc.sh import \
  --file /opt/keycloak/data/import/realm-export.json
```

### Port çakışması?
```bash
# Portları kontrol et
lsof -i :8080  # Keycloak
lsof -i :5002  # EventCatalog
lsof -i :5432  # PostgreSQL
```

## 📝 Proje İsterleri

| İster | Durum | Açıklama |
|-------|-------|----------|
| .NET 8 | ✅ | Tüm servisler .NET 8 |
| Public Site | ✅ | Blazor Web App |
| Admin Yönetimi | ✅ | Admin panel planlandı |
| MS SQL Server | ✅ | MS SQL Server container |
| NoSQL | ✅ | MongoDB |
| PostgreSQL | ✅ | FollowerService |
| IdentityServer | ✅ | Keycloak (OAuth2/OIDC) |
| Redis | ✅ | EventCatalogService cache |
| CQRS | ✅ | FollowerService (MediatR) |
| RabbitMQ | ✅ | ScraperService |
| Clean Architecture | 🔄 | Devam ediyor |
| API Gateway | ✅ | Nginx |

## 👥 Katkıda Bulunma

1. Fork'la
2. Feature branch oluştur (`git checkout -b feature/YeniOzellik`)
3. Commit'le (`git commit -m 'Yeni özellik eklendi'`)
4. Push'la (`git push origin feature/YeniOzellik`)
5. Pull Request aç

## 📄 Lisans

Bu proje MIT lisansı altındadır.

## 👨‍💻 Geliştiriciler

- İbrahim Kahraman - 2212729009
- Eren Ali Koca - 2212721021

**Ders:** BLG-423 Mikroservisler  
**Öğretim Üyesi:** Dr. Öğr. Üyesi Serdar PAÇACI  
**Tarih:** 2024-2025 Güz Dönemi