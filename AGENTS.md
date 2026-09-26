# Engineering rules

Read README.md, IMPLEMENTATION-PLAN.md, DEPENDENCIES.md and affected source before editing. Work only in this repository; Express and Go siblings are read-only contract references.

## Source of truth

Trace controller DTO -> application use case -> persistence mapping -> EF configuration/migration. HTTP fixtures originate from actual reference registry and module source. Never infer compatibility from names. Document conflicting reference behavior and security corrections in docs/CONTRACTS.md.

## Types and architecture

- Nullable references and warnings as errors are mandatory. No dynamic, untyped application dictionaries, object payloads, generic repositories, entity serialization, blocking async or swallowed exceptions.
- Domain depends on no infrastructure/framework. Application uses Domain and explicit ports. Infrastructure implements ports using installed official libraries. Api owns transport/DI only.
- Install and pin supported Microsoft packages or official publisher SDKs; do not implement cryptography, JWT, database clients, SDKs, logging or serialization libraries from scratch. Record license/publisher/source before introducing packages.
- Required audit and mutation commit in the same PostgreSQL transaction. Authorization reads live database grants; never trust stale permission claims. Snapshots exclude credentials and bytes. Cancellation flows through I/O.
- Every action has a typed endpoint ID matching registry, OpenAPI and parity fixture. Preserve envelope, statuses, nullability and allowlists.

## Operations and verification

- Dynamic configuration means environment plus redeployment. Validate startup options. No migration/seed in API startup. Separate PostgreSQL database; EF is its migration owner.
- Preserve dirty changes. Never expose secrets in logs, fixtures or output. .env is ignored; .env.example contains placeholders only.
- Run focused meaningful tests, locked restore, Release build, formatter and vulnerability audit. Runtime checks require actual PostgreSQL/Redis/S3. Clearly distinguish compile, integration, Docker and production evidence.
- Update implementation status with outstanding gates. Do not claim completion while required behavior is absent. Do not commit/push/publish/deploy without explicit authorization.
