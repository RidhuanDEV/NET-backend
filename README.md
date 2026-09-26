# Modular .NET Backend

Template modular monolith ASP.NET Core Controllers, .NET 10 LTS dan PostgreSQL tersendiri. Library dipasang dari penerbit resmi: Microsoft, Npgsql, AWS dan proyek OpenTelemetry. Lihat [DEPENDENCIES.md](DEPENDENCIES.md), [rencana implementasi](IMPLEMENTATION-PLAN.md) dan [kontrak](docs/CONTRACTS.md).

Repository: https://github.com/RidhuanDEV/NET-backend.git. Kode lokal tidak otomatis dipush atau dipublish.

## Prasyarat

- .NET SDK **10.0.401**, dipin melalui `global.json`; runtime **10.0.12**.
- PostgreSQL 18.3; Redis 8.6.1 hanya bila cache/limiter Redis dipilih.
- Docker Desktop/Engine dengan Compose untuk alur container, atau layanan PostgreSQL sendiri untuk alur manual.
- PowerShell 7 pada Windows; Python 3 hanya bila memakai loader `.env` lintas platform.

Install SDK melalui [installer/script resmi Microsoft](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script), bukan membuat runtime sendiri. Package NuGet dipulihkan dari `nuget.org`, versi terpusat dan dependency graph dikunci oleh `packages.lock.json`. Pada komputer dengan instalasi SDK user-local, `scripts/run.ps1` memilih `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`.

## Konfigurasi awal

Salin `.env.example` menjadi `.env`. Ganti seluruh `CHANGE_ME` dengan nilai baru; jangan memasukkan secrets nyata ke file contoh atau Git. Untuk JWT gunakan minimal 32 byte acak, misalnya hasil `RandomNumberGenerator.GetBytes(32)` yang dikonversi menjadi hex. Buat password PostgreSQL dan bootstrap yang berbeda. Bootstrap minimal 12 karakter.

Nama environment .NET memakai `__`: `Database__ConnectionString`, `Jwt__Secret`, `Rate__Store`, `Upload__Storage`, dan seterusnya. `.env` tidak dimuat otomatis oleh .NET. Compose membacanya; loader berikut memasukkannya ke environment proses tanpa mengevaluasi kode atau mencetak nilai.

## Menjalankan di Windows

```powershell
Copy-Item .env.example .env
# Edit .env dan ganti placeholders dahulu.
docker compose up -d postgres
pwsh -File scripts/run.ps1 restore --locked-mode
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.Migrator
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.Seeder
pwsh -File scripts/run.ps1
```

API: `http://localhost:5080`; spesifikasi: `/docs`. Untuk PostgreSQL sendiri, sesuaikan `Database__ConnectionString` dan lewati perintah Docker. Database ini harus berbeda dari database Prisma/Goose referensi.

## Menjalankan di Linux

```sh
cp .env.example .env
# Edit .env, lalu:
dotnet restore --locked-mode
python3 scripts/run.py run --project tools/ModularBackend.Migrator
python3 scripts/run.py run --project tools/ModularBackend.Seeder
python3 scripts/run.py
```

Native environment dan development user-secrets tetap tersedia tanpa loader. Penggantian konfigurasi memerlukan restart/redeploy. Tidak ada konfigurasi admin runtime.

## Compose lengkap

```powershell
docker compose up --build -d
docker compose run --rm --entrypoint dotnet app /app/seeder/ModularBackend.Seeder.dll
```

Migrator berjalan sekali sebelum app. API tidak melakukan migrate/seed saat startup. Data PostgreSQL dan file lokal memakai volume berbeda. Port HTTP default 5080, PostgreSQL 55432; sesuaikan `.env` atau salin `compose.override.yaml.example` menjadi `compose.override.yaml`.

Redis: ubah `Rate__Store=redis` dan/atau `Cache__Enabled=true`, lalu gunakan `COMPOSE_PROFILES=redis`. Dua replica atau lebih wajib `Rate__InstanceCount` sesuai jumlahnya dan limiter Redis. Limiter memory hanya berlaku pada satu instance.

S3 development: `Upload__Storage=s3`, endpoint manual `http://localhost:19000`, `COMPOSE_PROFILES=s3` (atau `redis,s3`), isi access key/secret dan bucket. Profile menggunakan image MinIO resmi yang dikunci digest untuk pengujian lokal. Community MinIO telah diarsipkan; gunakan layanan S3 yang masih dipelihara untuk production. Endpoint container default `http://minio:9000`; S3 eksternal memakai `S3_ENDPOINT_DOCKER`; untuk endpoint default AWS set nilai kosong dan pilih region, serta gunakan credential chain resmi AWS. Kredensial AWS default chain didukung bila access key/secret kosong.

## API dan akses

- Success: `{"success":true,"data":...}`; list users memiliki `meta` pagination.
- Failure: `{"success":false,"message":"...","errors":[]}`; delete 204 tanpa body.
- Auth: register, login, me. JWT HS256 berlaku 24 jam; signature, issuer, audience, expiry dan clock skew 30 detik diperiksa.
- Users memerlukan `manage_users`; roles `manage_roles`; permissions `manage_permissions`. Upload juga `manage_users`. Grant dan status akun selalu dibaca dari database.
- Upload field multipart `file`, PNG/JPEG/PDF dengan signature dan batas ukuran. Nama asli hanya metadata; objek memakai UUID. Tidak ada endpoint download publik.
- `/health` dan `/live` memeriksa HTTP; `/ready` memeriksa PostgreSQL serta Redis bila limiter memerlukannya. Redis untuk cache saja tidak menurunkan readiness.

Seed membuat admin/user dan tiga permission. Akun bootstrap dibuat bila `Bootstrap__Email`/`Bootstrap__Password` tersedia; seed ulang tidak mengganti password akun yang sudah ada.

## Endpoint policies

`ENDPOINT_POLICIES_JSON={"user.get":{"audit":"optional","cache":"off"}}` mengubah policy startup untuk ID yang dikenal. Unknown ID/property/enum ditolak; GET audit required ditolak karena jaminan producer belum disediakan. Required audit mutasi masuk transaksi yang sama; optional audit ditulis setelah commit dengan timeout. Tidak ada token/hash/password di snapshot.

Cache DTO bertipe default off; tanpa koneksi Redis ketika tidak dipakai. Versi database berubah atomik bersama mutasi, sehingga cache antar-instance tidak kembali stale setelah Redis outage; Redis generation juga diincrement setelah commit. Cache korup/down fallback ke DB. Set prefix unik untuk setiap deployment/database.

Forwarded client headers hanya boleh diaktifkan untuk IP proxy yang dikenal. Default tidak mempercayai forwarded headers; lihat konfigurasi proxy di [runbook](docs/OPERATIONS.md). Production wajib origins CORS eksplisit; credentialed CORS default off.

## Proyek baru

```powershell
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.Initializer -- ../MyBackend
# Tanpa prompt dan tanpa restore:
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.Initializer -- --yes --no-restore ../MyBackend
```

Wizard menggunakan mesin template resmi `dotnet new`, menolak target berisi file, menanyakan port/database/Redis/storage, membuat secrets baru di `.env`, dan menjalankan restore kecuali dimatikan. Setelah dibuat, konfigurasi database, migrasi dan seed dilakukan eksplisit.

Atau install template lokal dan gunakan CLI Microsoft:

```sh
dotnet pack templates/ModularBackend.Template.csproj -c Release -o artifacts/packages
dotnet new install artifacts/packages/RidhuanDEV.ModularBackend.Template.0.1.0.nupkg
dotnet new modular-net -n MyBackend -o ../MyBackend --port 5180
```

Paket NuGet disiapkan lokal, tidak dipublish otomatis. Periksa isi paket dan gate initializer sebelum distribusi.

## Verifikasi

```powershell
pwsh -File scripts/run.ps1 restore --locked-mode
pwsh -File scripts/run.ps1 build -c Release --no-restore -warnaserror
pwsh -File scripts/run.ps1 format --verify-no-changes --no-restore
pwsh -File scripts/run.ps1 test -c Release --no-restore
pwsh -File scripts/run.ps1 list package --vulnerable --include-transitive
```

Integration tests membuat database sementara dengan nama acak dan menghapusnya sesudah test; user PostgreSQL test harus boleh membuat database. Mereka memerlukan PostgreSQL nyata; Redis/S3 tests memerlukan environment sesuai `.env`. Tanpa layanan tes terkait berstatus inconclusive, bukan dianggap lulus. Contract/unit tests tidak membutuhkan layanan eksternal. Laporan bukti dan gate yang belum dijalankan ada di [ACCEPTANCE-REPORT.md](docs/ACCEPTANCE-REPORT.md).

## Arsitektur

`Api -> Infrastructure -> Application -> Domain`. DTO tidak menyerialisasi entity EF. Use case mengatur transaksi melalui persistence port khusus; EF Core menyediakan tracking/unit of work. Migrator, Seeder, UploadCleanup dan Initializer adalah executable terpisah. Timestamp disimpan `timestamptz` UTC dan dikirim RFC3339 UTC; penyajian zona IANA tidak menggeser data database.

Cleanup orphan default dry run:

```powershell
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.UploadCleanup
pwsh -File scripts/run.ps1 run --project tools/ModularBackend.UploadCleanup -- --apply
```

Lihat [OPERATIONS.md](docs/OPERATIONS.md) untuk release, rollback, backup/restore, retention dan telemetry. Hasil build/test lokal tidak membuktikan kapasitas/SLA atau kesiapan deployment production tertentu.
