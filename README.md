# MiniMarketCRM

![tests-and-coverage](https://github.com/kaskazeynep/MiniMarketCRM/actions/workflows/main.yml/badge.svg?branch=master)
[![codecov](https://codecov.io/github/kaskazeynep/MiniMarketCRM/graph/badge.svg?flag=application&branch=master)](https://codecov.io/github/kaskazeynep/MiniMarketCRM)


## 📌 Proje Tanımı

**MiniMarketCRM**, küçük ölçekli bir market sisteminin temel operasyonlarını yönetmek amacıyla geliştirilmiş, **katmanlı mimari**ye sahip bir **ASP.NET Core** uygulamasıdır.  
Proje; **clean architecture**, **test edilebilirlik** ve **bakım kolaylığı** prensipleri gözetilerek tasarlanmıştır.

Sistem aşağıdaki temel fonksiyonları kapsamaktadır:
- Müşteri yönetimi
- Ürün ve kategori yönetimi
- Sepet işlemleri
- Sipariş oluşturma ve sipariş yaşam döngüsü yönetimi
- Raporlama ve dashboard özetleri


---

## 🧱 Mimari Yapı

Proje, **Layered Architecture (Katmanlı Mimari)** yaklaşımı ile geliştirilmiştir:

- **MiniMarketCRM.Api**
  - RESTful API controller’ları
  - HTTP request/response yönetimi
- **MiniMarketCRM.Application**
  - İş kuralları (business logic)
  - Service katmanı ve DTO yapıları
- **MiniMarketCRM.DataAccess**
  - Entity Framework Core
  - Database context ve migration işlemleri
- **MiniMarketCRM.Domain**
  - Domain entity’leri
  - Temel iş kuralları ve enum tanımları

Bu mimari yapı sayesinde:
- Katmanlar arası bağımlılık azaltılmıştır
- Test edilebilirlik artırılmıştır
- Sorumluluklar net şekilde ayrıştırılmıştır

---

## 🧪 Test Stratejisi

Projede hem **Unit Test** hem de **System / End-to-End (E2E) Test** yaklaşımları birlikte kullanılmıştır.

### ✔ Test Türleri

- **Unit Test**
  - Application katmanındaki iş kurallarının doğrulanması
- **System / E2E Test**
  - In-memory SQLite veritabanı kullanılarak
  - Gerçek API pipeline’ı üzerinden uçtan uca senaryo testleri

### ✔ Örnek E2E Senaryoları

- Müşteri oluşturma → Ürün oluşturma → Sepete ekleme → Checkout → Sipariş doğrulama
- Ürün CRUD akışı (Create → List → Detail → Update → Delete)
- Sepet iptali sonrası stok geri yükleme
- PATCH ile sipariş durumu güncelleme
- Belirli tarih aralığında sipariş raporu alma

Tüm E2E testler:
- `WebApplicationFactory`
- `SQLite InMemory` veritabanı
- Gerçek HTTP request/response döngüsü

kullanılarak çalıştırılmaktadır.

---

## 🔁 Continuous Integration (CI)

Projede **GitHub Actions** kullanılarak **Continuous Integration (CI)** süreci kurulmuştur.

Aşağıdaki durumlarda otomatik olarak çalışır:
- `push`
- `pull request`

CI pipeline adımları:
1. Bağımlılıkların yüklenmesi (restore)
2. Tüm testlerin çalıştırılması
3. Code coverage verisinin toplanması
4. Coverage raporlarının oluşturulması
5. Raporların artifact olarak saklanması
6. Coverage sonuçlarının Codecov’a gönderilmesi

---

## 📊 Code Coverage

Code coverage ölçümü aşağıdaki araçlar kullanılarak yapılmaktadır:
- **XPlat Code Coverage**
- **Cobertura** formatı
- **ReportGenerator** ile HTML rapor üretimi

Coverage sonuçları **Codecov** platformu üzerinden yayınlanmaktadır.

🔗 **Canlı Coverage Dashboard:**  
https://codecov.io/github/kaskazeynep/MiniMarketCRM

### 🎯 Coverage Kapsamı

Coverage ölçümü özellikle **Application katmanına** odaklanmaktadır; çünkü iş kurallarının doğrulanması bu katmanda gerçekleşmektedir.

Aşağıdaki yapılar bilinçli olarak kapsam dışı bırakılmıştır:
- EF Core migration dosyaları
- Sadece veri taşıma amaçlı DTO sınıfları
- Altyapı ve bootstrap kodları (ör. `Program.cs`)

Bu yaklaşım sayesinde coverage yüzdeleri:
- Gerçek iş kurallarını yansıtır
- Otomatik üretilmiş veya pasif kodlardan etkilenmez

---

## ⚙ Kullanılan Teknolojiler

- ASP.NET Core
- Entity Framework Core
- SQLite (InMemory – testler için)
- xUnit
- GitHub Actions
- Codecov
- ReportGenerator
- RESTful API prensipleri

---

## 📈 Kalite Hedefleri

- Temiz ve okunabilir kod yapısı
- Yüksek test güvenilirliği
- Otomatik kalite kontrol süreçleri
- Şeffaf ve ölçülebilir coverage raporları
- Production seviyesinde mimari yaklaşım

---

## 👩‍💻 Geliştirici

**Zeynep Kaska**  
Bilgisayar Mühendisliği Yüksek Lisans Öğrencisi  
ASP.NET Core & Backend Development

---

## 📝 Notlar

Bu proje; akademik ve pratik yazılım mühendisliği çalışması kapsamında,  
**test disiplini**, **CI/CD süreçleri** ve **kod kalitesi ölçümleri** odağında geliştirilmiştir.
