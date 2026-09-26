# ASP.NET Core enterprise template

Target: modular-NET (user override of modular-dotnet). Reference repositories are read-only. User authorized commit and push on 26 September 2026. Package publication and production deployment are not authorized.

## Decisions

- Install Microsoft .NET 10 LTS SDK using the official dotnet-install.ps1, pin the resolved stable SDK and runtime. Install NuGet packages from nuget.org; use framework services for HTTP, validation, Identity password hashing, authorization, rate limiting and configuration. No replacement libraries written from scratch.
- Four projects: Domain, Application, Infrastructure, Api. EF Core owns a separate PostgreSQL database. Controllers bind typed DTOs; services coordinate business transactions through explicit ports. No generic repository, dynamic contracts or entity serialization.
- Reference HTTP contracts: Express endpoint-registry.ts, module schemas/services/mappers, Prisma schema; Go fixture is method/path/policy evidence only. Express user fields allowlist is id/email/roleId; Go accepts additional fields and does not project them. Follow Express projection, document this difference.
- Microsoft OpenAPI generates documents, /docs links specifications; no third-party interactive UI or CDN.
- Native environment configuration; explicit .env import for manual startup. Changes require redeploy.

## Stages and acceptance

1. Inspect reference contracts; install official SDK/packages; record publishers/licenses/versions; create solution, dependency direction, strict analyzer configuration and locked restore.
2. Implement typed contracts, endpoint metadata/registry startup checks, validation/error envelope and OpenAPI.
3. Implement EF schema/migrations, auth/live RBAC, CRUD, atomic required audit, optional audit, seed/migrator tools.
4. Implement Redis atomic limiter, typed cache/invalidation, local/AWS S3 uploads with compensation and orphan cleanup.
5. Add runtime configuration, readiness/CORS/request scopes/OTel, pinned Docker/Compose and release runbook.
6. Package initializer/custom dotnet new template with allowlisted contents, safe output checks and restore/build smoke.
7. Run locked restore, Release build, format, official MSTest tests, package vulnerability audit, fresh/upgrade PostgreSQL, Redis and storage/HTTP integration, initializer and Compose smoke. Record exact evidence; unexecuted gates stay pending.

## Status

Stages 1–6 implemented. Stage 7 local evidence: locked dependency restore, zero-warning Release build, formatting, 4 unit + 3 contract + 13 integration tests passed on Windows. Earlier Linux source and generated-template snapshots each passed 4 unit + 3 contract + 11 integration tests. Native initializer and generated Release build/unit/contract tests passed. Docker default HTTP smoke passed. Dependency audit reported no known vulnerabilities. See docs/ACCEPTANCE-REPORT.md for snapshot boundaries and external gates.

Origin is configured to https://github.com/RidhuanDEV/NET-backend.git on codex/bootstrap-template. Initial source delivery is authorized for commit and push. Remote CI results must be checked after delivery; production acceptance remains unexecuted.
