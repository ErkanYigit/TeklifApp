# TeklifApp (Frontend)

Bu klasör, projenin Angular 20 tabanlı frontend uygulamasıdır. Arka uç için `backend/Api` (ASP.NET Core) kullanılır.

## Gereksinimler

- Node.js 18+ (önerilen LTS)
- Angular CLI 20.x
- Arka uç (API) için .NET 8 SDK

## Hızlı Başlangıç

1) Bağımlılıkları yükleyin:

```bash
npm install
```

2) Geliştirme sunucusunu başlatın:

```bash
ng serve
```

3) Tarayıcıdan `http://localhost:4200/` adresine gidin.

## API Adresi (Environment)

Frontend, API adresini `src/environments/environment.ts` dosyasındaki `apiUrl` üzerinden kullanır. Gerekirse güncelleyin:

```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001' // veya backend'in çalıştığı adres
};
```

Not: Production derlemesi için `environment.prod.ts` dosyasını da aynı şekilde düzenleyin.

## Backend (Kısaca)

Arka ucu çalıştırmak için proje kökünde:

```bash
cd ..\backend\Api
dotnet restore
dotnet run
```

Varsayılan olarak `https://localhost:5001` (veya proje ayarlarınıza göre) dinliyor olacaktır. EF Core migrasyonlarını uygulamak isterseniz:

```bash
cd ..\Infrastructure
dotnet ef database update
```

> EF CLI yoksa: `dotnet tool install --global dotnet-ef`

## Geliştirme Komutları

- Yeni bileşen oluşturma:

```bash
ng generate component components/ornek
```

- Servis/guard/pipe vb. için yardım:

```bash
ng generate --help
```

## Build (Üretim)

```bash
ng build --configuration production
```

Çıktılar `dist/` klasörüne yazılır. Bu çıktı, herhangi bir statik sunucu ile servis edilebilir.

## Test

- Unit test:

```bash
ng test
```

- E2E test: Projede varsayılan E2E çerçevesi bulunmaz; tercih ettiğiniz aracı ekleyip yapılandırabilirsiniz (Cypress, Playwright, vb.).

## Sorun Giderme

- `node_modules` kaynaklı hatalarda:

```bash
rm -rf node_modules package-lock.json
npm install
```

- API bağlantı sorunlarında: `environment.ts` içindeki `apiUrl` ile backend adresinin eşleştiğinden emin olun.

## Lisans

Bu proje kurum içi kullanım içindir. Gerekli durumlarda lisans metni burada paylaşılabilir.
