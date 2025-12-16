# 📊 Kişisel Finans ve Harcama Takip Uygulaması

ASP.NET Core, EF Core ve SQLite kullanılarak geliştirilmiş, temiz mimari prensiplerine uygun kişisel finans yönetim uygulaması.

## 🚀 Özellikler

- ✅ Gelir/Gider takibi
- 📊 Kategorilere göre harcama analizi
- 🔐 JWT tabanlı kimlik doğrulama
- 📈 Chart.js ile interaktif raporlar
- 🗂 Temiz Mimari (Clean Architecture)
- 🏦 Çoklu hesap yönetimi
- 📱 Responsive tasarım

## 🛠 Teknoloji Yığını

- **Backend:** ASP.NET Core 7.0
- **Veritabanı:** SQLite
- **ORM:** Entity Framework Core 7.0
- **Kimlik Doğrulama:** JWT Bearer Token
- **Frontend:** React.js (API tüketimi için)
- **Görselleştirme:** Chart.js
- **Test:** xUnit, Moq

## 📂 Proje Yapısı (Clean Architecture)

```
FinanceManagement/
├── src/
│   ├── FinanceManagement.API/          # API Katmanı
│   ├── FinanceManagement.Application/  # Uygulama Katmanı
│   ├── FinanceManagement.Domain/       # Domain Katmanı
│   └── FinanceManagement.Infrastructure/ # Altyapı Katmanı
├── tests/
│   ├── FinanceManagement.UnitTests/    # Birim Testler
│   └── FinanceManagement.IntegrationTests/ # Entegrasyon Testleri
└── README.md
```

## 🔧 Kurulum

1. Gereksinimler:
   - .NET 7.0 SDK
   - Node.js 16+
   - SQLite

2. Bağımlılıkları yükleyin:
   ```bash
   dotnet restore
   cd ClientApp
   npm install
   ```

3. Veritabanını oluşturun:
   ```bash
   dotnet ef database update --project src/FinanceManagement.Infrastructure
   ```

4. Uygulamayı başlatın:
   ```bash
   dotnet run --project src/FinanceManagement.API
   ```

## 📚 API Dokümantasyonu

Uygulama çalıştığında Swagger UI üzerinden API dokümantasyonuna erişebilirsiniz:
- Swagger UI: `https://localhost:5001/swagger`

## 📊 Veritabanı Şeması

```mermaid
erDiagram
    USER ||--o{ ACCOUNT : has
    USER ||--o{ TRANSACTION : creates
    ACCOUNT ||--o{ TRANSACTION : contains
    CATEGORY ||--o{ TRANSACTION : categorizes
    
    USER {
        string Id PK
        string UserName
        string Email
        string PasswordHash
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    ACCOUNT {
        Guid Id PK
        string Name
        decimal Balance
        string UserId FK
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    TRANSACTION {
        Guid Id PK
        decimal Amount
        string Description
        DateTime Date
        TransactionType Type
        Guid CategoryId FK
        Guid AccountId FK
        string UserId FK
        bool IsDeleted
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    CATEGORY {
        Guid Id PK
        string Name
        string Icon
        string Color
        TransactionType Type
        string UserId FK
    }
```




