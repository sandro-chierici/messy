---
name: create-entity
description: Use when adding or changing an entity (table, model, DTOs, mapper, repository, service, controller) in src/data-service. Encodes the key rules (int PK + V7 GUID external key, OFBiz datamodel) and the Tenant-based layering checklist.
---

# Create Entity

Reference implementation: **Tenant** in `src/data-service`. Copy its layering and style.

## Rules

1. **PK** is `id`, `int` (`SERIAL`). Use `bigint` only for tables that will realistically exceed 2^31 rows.
2. **External key** is `<entity>_id`, `UUID NOT NULL UNIQUE`, a **V7 GUID** generated in the service with `SwissKnife.GenerateGuid()`. The client never supplies it.
3. **Internal relations use the int PK.** FK columns are named `<entity>_pk` (e.g. `tenant_pk`), so they never collide with a GUID column such as `tenants.tenant_id`.
4. **Externally only the GUID exists** (routes, DTOs, events, logs). DTOs never expose `id` or `*_pk`.
   - Write: resolve GUID to PK in SQL, e.g. `(SELECT id FROM tenants WHERE tenant_id = @TenantId::uuid)`.
   - Read: JOIN the parent and select its GUID.
5. **Datamodel comes from Apache OFBiz.** Name and shape entities and fields after the OFBiz data model (Party, Person, UserLogin, RoleType, SecurityGroup, ...). If the OFBiz entity is unknown to you, **ask the user for the datamodel; do not invent it.**
6. **OFBiz exceptions to rules 1-2:**
   - *Reference/type tables* (PartyType, RoleType, SecurityPermission, ...): int PK plus unique natural `code` (the OFBiz string id, e.g. `EMPLOYEE`). No GUID; the API exposes `code`.
   - *1:1 subtype tables* (Person): PK is also FK to the parent's `id`; identified externally by the parent's GUID.
   - *Link tables*: composite PK of the two int PKs (plus `from_date` when OFBiz has it), nullable `thru_date`, no GUID of their own.
7. **Tenant scoping:** entities that belong to a tenant carry `tenant_pk`; every query filters by tenant. A row of another tenant is reported as 404, never returned.
8. **Conventions:**
   - Columns snake_case, tables plural snake_case, C# entity singular PascalCase, one class per `.cs` file.
   - Standard columns: `created_utc_date`, `updated_utc_date`, `created_by`, optional `ext_props JSONB` (`Dictionary<string, object?>` in DTOs, converted with `SwissKnife.SerializeExtProps/DeserializeExtProps`).
   - Repositories and services return `OkOrError<T>`; never throw for expected failures, and never leave a `NotImplementedException` stub (implement the method or do not declare it).
   - Services publish `CreatedEvent` / `FailureEvent` through `IEventPublisher`, like `TenantService.CreateTenantAsync`.
   - Secrets (password hashes) never appear in a DTO, log or event.

## Checklist (paths under `src/data-service`)

1. **Model in OFBiz terms.** Note the OFBiz entity name as a SQL comment above the table.
2. **SQL** in `script/data-schema.sql`: table, PK, GUID unique, FKs, unique constraints (include `tenant_pk` where uniqueness is per tenant), and indexes on the GUID and on every `*_pk`.
3. **Model** `Domain/Repository/Models/<Entity>.cs`: `int Id`, `required Guid <Entity>Id`, `int <Parent>Pk` for FKs; XML docs on properties.
4. **DTOs** `Domain/IO/<Entity>/`: `<Entity>CreateDTO`, `<Entity>UpdateDTO`, `<Entity>ViewDTO`. FKs appear as parent GUID strings (`TenantId`), never as ints.
5. **Mapper** `Domain/Mapper/<Entity>/<Entity>Mapper.cs` (see `TenantMapper`); register as singleton in `Infrastructure/ApplicationServicesExtension.cs`.
6. **Repository**
   - Interface `Domain/Repository/I<Entity>Repository.cs`, extends `IRepository` (auto-registered by `DiscoverAndRegisterRepositories`).
   - Implementation `Infrastructure/Repository/<Entity>Repository.cs`: Dapper, `IDbConnectionFactory`, parameterised SQL only, one transaction for multi-table writes, `::uuid` casts for GUID parameters.
7. **Service**
   - Interface `Domain/Services/I<Entity>Service.cs` (extends `IBackendService`).
   - Implementation `Infrastructure/Services/<Entity>Service.cs`: generate the GUID, apply business rules, publish events. Register as scoped in `ApplicationServicesExtension`.
8. **Controller** `Api/V1/<Entity>/<Entity>Controller.cs`: route `/api/v1/...`, ids taken from the route as strings and validated with `InputValidator` (GUID format), responses wrapped in `ApiV1Response`, `Created` returns the new GUID.
9. **Requests** for each endpoint in `data-service.http`.
10. **Verify:** `dotnet build`, run the schema script, exercise the `.http` calls. Check that no response contains `id`, `*_pk` or secrets.

## Don't

- Don't expose an int PK or accept a client-supplied GUID.
- Don't relate entities by GUID in the database; FKs are always int PKs.
- Don't invent fields when an OFBiz definition exists (or ask for it when it is unknown).
- Don't build string SQL from input.
