# 📋 Detaylı Görev Dokümanı

## 🏗 1. Proje Kurulumu ve Temel Yapı

### 1.1 Solution ve Projelerin Oluşturulması
```bash
dotnet new sln -n FinanceManagement

# API Katmanı
dotnet new webapi -n FinanceManagement.API

# Application Katmanı
dotnet new classlib -n FinanceManagement.Application

# Domain Katmanı
dotnet new classlib -n FinanceManagement.Domain

# Infrastructure Katmanı
dotnet new classlib -n FinanceManagement.Infrastructure

# Solution'a Projelerin Eklenmesi
dotnet sln add src/FinanceManagement.API/FinanceManagement.API.csproj
dotnet sln add src/FinanceManagement.Application/FinanceManagement.Application.csproj
dotnet sln add src/FinanceManagement.Domain/FinanceManagement.Domain.csproj
dotnet sln add src/FinanceManagement.Infrastructure/FinanceManagement.Infrastructure.csproj
```

### 1.2 Gerekli NuGet Paketlerinin Eklenmesi
```bash
# API Katmanı
cd src/FinanceManagement.API
dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Application Katmanı
cd ../FinanceManagement.Application
dotnet add package MediatR

# Infrastructure Katmanı
cd ../FinanceManagement.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
```

## 🏛 2. Domain Katmanı Geliştirmeleri

### 2.1 Temel Entity'ler
```csharp
// BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// User.cs
public class User : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ICollection<Account> Accounts { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}

// Account.cs
public class Account : BaseEntity
{
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}
```

### 2.2 Enum ve Değer Nesneleri
```csharp
public enum TransactionType
{
    Income,
    Expense
}

public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    
    public Money(decimal amount, string currency = "TRY")
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }
}
```

## 🗄 3. Veritabanı ve ORM Yapılandırması

### 3.1 AppDbContext
```csharp
public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");
            
        // Diğer konfigürasyonlar...
    }
}
```

### 3.2 Migration İşlemleri
```bash
dotnet ef migrations add InitialCreate --project src/FinanceManagement.Infrastructure

dotnet ef database update --project src/FinanceManagement.Infrastructure
```

## 🔐 4. Kimlik Doğrulama ve Yetkilendirme

### 4.1 JWT Yapılandırması
```csharp
// Program.cs veya Startup.cs
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration["Jwt:Issuer"],
        ValidAudience = Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
    };
});
```

## 🚀 5. API Geliştirme

### 5.1 Controller Örneği
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(CreateTransactionCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
    }
}
```

## 📊 6. Frontend Geliştirme

### 6.1 Proje Kurulumu ve Metronic Entegrasyonu
```bash
# React projesi oluşturma
npx create-react-app client-app --template typescript
cd client-app

# Temel bağımlılıklar
npm install @reduxjs/toolkit react-redux
npm install axios formik yup
npm install chart.js react-chartjs-2
npm install @fullcalendar/react @fullcalendar/daygrid

# Metronic tema dosyalarını kopyala (public/assets altına)
# Not: Lisanslı Metronic dosyalarınızı buraya eklemelisiniz
cp -r /path/to/metronic/theme/* public/assets/

# Özel SCSS yapılandırması
mkdir -p src/assets/scss
```

### 6.2 Tema Yapılandırması
```scss
// src/assets/scss/style.scss
@import "~bootstrap/scss/functions";
@import "~bootstrap/scss/variables";
@import "~bootstrap/scss/mixins";

// Metronic değişkenlerini geçersiz kıl
$primary: #7239EA;
$success: #50CD89;
$info: #7239EA;
$warning: #FFC700;
$danger: #F1416C;

// Metronic temel stillerini içe aktar
@import "~@/assets/sass/plugins";
@import "~@/assets/sass/style";

// Özel bileşen stilleri
@import "components/card";
@import "components/buttons";
@import "components/forms";
```

### 6.3 Ana Layout Yapısı
```jsx
// src/layouts/MainLayout.tsx
import {Outlet} from 'react-router-dom';
import {Aside} from '../components/aside/Aside';
import {Header} from '../components/header/Header';
import {ScrollTop} from '../components/scroll-top/ScrollTop';

export function MainLayout() {
  return (
    <div className="d-flex flex-column flex-root">
      <div className="page d-flex flex-row flex-column-fluid">
        <Aside />
        <div className="wrapper d-flex flex-column flex-row-fluid" id="kt_wrapper">
          <Header />
          <div id="kt_content" className="content d-flex flex-column flex-column-fluid">
            <div className="post d-flex flex-column-fluid">
              <div className="container-xxl">
                <Outlet />
              </div>
            </div>
          </div>
        </div>
      </div>
      <ScrollTop />
    </div>
  );
}
```

### 6.4 Dashboard Bileşeni
```jsx
// src/pages/dashboard/DashboardPage.tsx
import {PageTitle} from '../../_metronic/layout/core';
import {StatsWidget} from '../../_metronic/partials/widgets';

export function DashboardPage() {
  return (
    <>
      <PageTitle>Genel Bakış</PageTitle>
      
      <div className="row g-5 g-xl-8">
        <div className="col-xl-4">
          <StatsWidget
            className="card-xl-stretch mb-xl-8"
            title="Toplam Bakiye"
            amount="24,500.00 ₺"
            trend="up"
            trendValue="2.5%"/>
        </div>
        
        <div className="col-xl-8">
          <div className="card card-xl-stretch">
            <div className="card-header border-0 pt-5">
              <h3 className="card-title align-items-start flex-column">
                <span className="card-label fw-bolder fs-3 mb-1">Aylık Özet</span>
                <span className="text-muted fw-bold fs-7">Son 6 aylık gelir ve giderleriniz</span>
              </h3>
            </div>
            <div className="card-body">
              <Chart options={chartOptions} series={chartSeries} type="bar" height={350} />
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
```

### 6.2 Örnek Bileşen
```tsx
// TransactionList.tsx
import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../app/hooks';
import { fetchTransactions } from '../features/transactions/transactionSlice';

export const TransactionList = () => {
  const dispatch = useAppDispatch();
  const { transactions, status, error } = useAppSelector(state => state.transactions);

  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchTransactions());
    }
  }, [status, dispatch]);

  if (status === 'loading') return <div>Yükleniyor...</div>;
  if (error) return <div>Hata: {error}</div>;

  return (
    <div>
      <h2>İşlemler</h2>
      <ul>
        {transactions.map(transaction => (
          <li key={transaction.id}>
            {transaction.description} - {transaction.amount}
          </li>
        ))}
      </ul>
    </div>
  );
};
```

## 🧪 7. Test Stratejisi

### 7.1 Birim Test Örneği
```csharp
[TestClass]
public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockRepo;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _mockRepo = new Mock<ITransactionRepository>();
        _service = new TransactionService(_mockRepo.Object);
    }

    [TestMethod]
    public async Task CreateTransaction_WithValidData_ReturnsTransaction()
    {
        // Arrange
        var command = new CreateTransactionCommand
        {
            Amount = 100,
            Description = "Test Transaction",
            Type = TransactionType.Income
        };

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
                .ReturnsAsync((Transaction t) => t);

        // Act
        var result = await _service.CreateTransactionAsync(command);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(100, result.Amount);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }
}
```

## 🚀 8. Dağıtım ve CI/CD

### 8.1 Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FinanceManagement.API/FinanceManagement.API.csproj", "FinanceManagement.API/"]
RUN dotnet restore "FinanceManagement.API/FinanceManagement.API.csproj"
COPY . .
WORKDIR "/src/FinanceManagement.API"
RUN dotnet build "FinanceManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FinanceManagement.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "FinanceManagement.API.dll"]
```

### 8.2 docker-compose.yml
```yaml
version: '3.8'

services:
  finance-api:
    build:
      context: .
      dockerfile: FinanceManagement.API/Dockerfile
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/data/finance.db
    volumes:
      - finance-data:/data
    restart: unless-stopped

volumes:
  finance-data:
```

## 📅 9. Geliştirme Süreci ve Çalışma Planı

### Haftalık Plan

**Hafta 1-2: Temel Altyapı**
- [ ] Proje yapılandırması
- [ ] Temel entity'lerin oluşturulması
- [ ] Veritabanı bağlantısı ve migration'lar

**Hafta 3-4: Çekirdek İşlevler**
- [ ] Kullanıcı yönetimi ve kimlik doğrulama
- [ ] Hesap yönetimi API'ları
- [ ] İşlem yönetimi API'ları

**Hafta 5-6: Frontend Geliştirme**
- [ ] Kimlik doğrulama ekranları
- [ ] Dashboard tasarımı
- [ ] İşlem yönetimi arayüzü

**Hafta 7-8: Raporlama ve Test**
- [ ] Chart.js entegrasyonu
- [ ] Birim testlerin yazılması
- [ ] Entegrasyon testleri

## 📞 10. İletişim ve Destek

- **Proje Yöneticisi**: [İsim Soyisim]
- **E-posta**: [e-posta@example.com]
- **Telefon**: [Telefon Numarası]
- **Toplantı Günleri**: Her Çarşamba 14:00-15:00

---
*Son Güncelleme: 16 Aralık 2025*
