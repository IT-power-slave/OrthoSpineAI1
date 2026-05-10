# Database Architecture Overview

This document provides a comprehensive analysis of the Entity Framework 6 (EF6) database architecture used in the **ORT100 / OrthoSpine** solution — a medical orthopaedic measurement and diagnostic desktop application built with WPF and .NET Framework 4.8.

The persistence layer is split across two projects:

| Project | Target Framework | Role |
|---|---|---|
| `OrthoSpine.Shared.Model` | .NET Framework 4.7.2 | Entity/domain model (plain classes + interfaces) |
| `ORT100.Model` | .NET Framework 4.8 | EF6 DbContext, migrations, services, seed data |

---

# Current State Analysis

## DbContext Design

A single `ORT100Context : DbContext` (in `ORT100.Model\Persistance\EntityFramework\ORT100Context.cs`) acts as the sole persistence gateway for the entire application.

**DbSets exposed:**

```csharp
public DbSet<Clinic>                   Clinics                    { get; set; }
public DbSet<Patient>                  Patients                   { get; set; }
public DbSet<SystemUser>               SystemUsers                { get; set; }
public DbSet<MedTest>                  MedTests                   { get; set; }
public DbSet<MedTestDefinition>        MedTestDefinitions         { get; set; }
public DbSet<MedTestStage>             MedTestStages              { get; set; }
public DbSet<MedTestResult>            MedTestResults             { get; set; }
public DbSet<MedTestContinuousResult>  MedTestContinuousResults   { get; set; }
public DbSet<DiagnosticForm>           DiagnosticForms            { get; set; }
```

**Constructor behaviour (critical issues):**

```csharp
public ORT100Context(string dbName, string instanceName) : base(String.Format(@"data source=" + instanceName + @";
    initial catalog={0};
    integrated security=true;
    AttachDBFilename=|DataDirectory|\{0}.mdf", dbName))
{
    CreateExceptionLogFile.Now("ORT100 ORT100Context");
    Database.SetInitializer(new MigrateDatabaseToLatestVersion<...>());
}
```

- The connection string is assembled inline inside the constructor from raw parameters — no connection string abstraction.
- `Database.SetInitializer(...)` is called **inside the constructor**, meaning every instantiation of the context re-registers the initialiser. This is both a performance overhead and a design smell.
- Logging calls (`CreateExceptionLogFile.Now(...)`) are embedded directly in the constructor.

**Fluent API (`OnModelCreating`):**

Only two mappings are declared:

```csharp
modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
modelBuilder.Entity<DiagnosticForm>().Ignore(df => df.ParametersGroups);
```

All other mappings rely entirely on EF6 conventions, with Data Annotations scattered across entities in the shared model project.

**`MedTestCreator` as a nested static class inside a `partial ORT100Context`:**

`MedTestCreator.cs` declares `public partial class ORT100Context` and nests `MedTestCreator` as a static class inside it. This merges seeding/data-creation logic directly into the context type.

---

## Entity Modeling

### `Patient`
- Navigation to `Clinic` (FK `ClinicId`) — `virtual` ✔
- Navigation to `MedTests` collection — `virtual` ✔
- **No Data Annotations** on any property (`FirstName`, `LastName`, `PESEL`, etc.) — no `[Required]`, no `[MaxLength]`, no unique index on `PESEL`.
- `PESEL` is the Polish national ID (unique per citizen) — stored as a plain `string` with no uniqueness constraint.

### `MedTest`
- `DateTime` property is typed as **`string`** instead of `System.DateTime` — temporal queries are impossible, sorting is lexicographic.
- `DiagnosticForm` navigation is marked `[NotMapped]` — the relationship to `DiagnosticForm` is **not persisted**.
- No FK column for `DiagnosticForm` on `MedTest`.
- `virtual` navigation properties to `Patient`, `SystemUser`, `MedTestResults`, `MedTestContinuousResults` — lazy loading enabled.

### `MedTestResult`
- Enum properties (`MedTestPlane`, `ORT100Measurement`, `MedTestSide`) stored as integers by convention — no explicit column type or enum-to-string conversion configured.
- No `[Required]` on `PhysicalUnit`/`PhysicalValue`.

### `MedTestContinuousResult`
- 12 flat scalar columns (`Roll`, `RollOffset`, `Tilt`, `Way`, `Space`, `Force1`, `Force2`, etc.) representing a single sensor frame. No value-object decomposition; the table will be very wide.
- No `[Required]` or range constraints.

### `MedTestStage`
- Navigation property `MedTestDefinition` is **not `virtual`** — lazy loading will not fire for this navigation.
- Has nullable doubles `ValueISOM1`/`ValueISOM3` which represent physical reference angles; semantics are only documented via Polish comments.

### `MedTestDefinition`
- `Key` property (a string business key) has no `[Index(IsUnique = true)]` or Fluent API uniqueness constraint.

### `DiagnosticForm`
- `ParametersGroups` is `List<IParametersGroup>` — EF6 cannot map interface collections. It is correctly ignored via Fluent API, but the property is `public` with a mutable getter, creating confusion between the domain object and the persisted entity.

### `SystemUser`
- `Passwd` property stores a password as a plain `string` — no indication of hashing or encryption.
- No `[Required]` on `Login`; no unique index.

### `Clinic`
- Property `Adress` is a **typo** (should be `Address`) — this is now a schema name that would require a migration to fix.
- `SystemUsers` collection — implies a `Clinic` owns `SystemUser` entities, but there is no FK on `SystemUser` for `ClinicId`; this relationship may be broken at the DB level.

---

## Relationships and Constraints

| Relationship | Type | Cascade | Notes |
|---|---|---|---|
| `Clinic` → `Patient` | 1 : N | EF default (cascade delete) | FK on `Patient.ClinicId` |
| `Clinic` → `SystemUser` | 1 : N | Unknown | No FK visible on `SystemUser` entity |
| `Patient` → `MedTest` | 1 : N | EF default | FK on `MedTest.PatientId` |
| `SystemUser` → `MedTest` | 1 : N | EF default | FK on `MedTest.SystemUserId` |
| `MedTestDefinition` → `MedTestStage` | 1 : N | EF default | FK on `MedTestStage.MedTestDefinitionId` |
| `MedTest` → `MedTestResult` | 1 : N | EF default | FK on `MedTestResult.MedTestId` |
| `MedTest` → `MedTestContinuousResult` | 1 : N | EF default | FK on `MedTestContinuousResult.MedTestId` |
| `MedTest` → `DiagnosticForm` | None | N/A | Explicitly `[NotMapped]` |

- No cascade rules are explicitly configured — all cascades follow EF6 convention defaults.
- No explicit `[Index]` attributes or Fluent API index definitions anywhere in the model.
- No concurrency tokens (`[Timestamp]` / `rowversion`) on any entity.

---

## Querying and Performance

- **Lazy loading is the default strategy** — all navigation properties marked `virtual` will trigger additional SQL queries on access. With collections like `MedTestResults` and `MedTestContinuousResults` (which can be large for continuous sensor data), this is a significant N+1 risk.
- `DiagnosticFormService` uses `DbSet<T>.Find()` (primary-key lookup, uses identity map — acceptable) and `FirstOrDefault` with a lambda predicate (translated to SQL — acceptable for simple cases).
- `LoadAllDiagnosticForms()` calls `.ToList()` with no filtering, ordering, or pagination — unbounded query.
- No use of `AsNoTracking()` for read-only queries anywhere in the service layer.
- No `Include()`/eager loading calls in the service layer — all related data relies on lazy loading.
- No async EF6 operations (`ToListAsync`, `FindAsync`, `SaveChangesAsync`) despite the application being a WPF app where UI thread responsiveness is important.
- `MedTestContinuousResult` can accumulate thousands of rows per test (sensor data at high frequency); no archiving or pagination strategy exists.

---

## Migration Strategy

- **`AutomaticMigrationsEnabled = true`** with **`AutomaticMigrationDataLossAllowed = true`** in `Configuration.cs`.
- This means EF6 can silently drop columns and tables when the model changes, **without any migration history or rollback capability**.
- One explicit migration exists: `202501_AddDiagnosticForms.cs` (adds the `DiagnosticForms` table). It follows the standard `Up()`/`Down()` pattern correctly.
- The migration filename uses a non-standard naming convention (`202501_AddDiagnosticForms` instead of the default timestamp prefix `YYYYMMDDHHMMSS_Name`).
- `Seed()` in `Configuration.cs` calls `context.CreateMedTestDefinitions()` directly — which calls `DbSet.Add()` unconditionally — **no `AddOrUpdate()` guards**. Running migrations more than once will insert duplicate seed records.

---

## Transaction and Concurrency Handling

- `SaveChanges()` is called once per operation in `DiagnosticFormService` — each service method is implicitly wrapped in a single EF6 transaction (acceptable for single-entity operations).
- No explicit `DbContextTransaction` (`context.Database.BeginTransaction()`) usage — multi-step operations (e.g., seed data creation writing multiple `MedTestDefinition` + `MedTestStage` records) run without an explicit transaction boundary.
- No optimistic concurrency (`[Timestamp]` or `[ConcurrencyCheck]`) on any entity.
- No pessimistic locking patterns.
- The `Seed()` method in `Configuration` has no transaction wrapper; a failure mid-seed leaves the database in a partially seeded, inconsistent state.

---

# Identified Problems

## P1 — `AutomaticMigrationDataLossAllowed = true` (Critical)
Setting `AutomaticMigrationDataLossAllowed = true` permits EF6 to automatically drop columns or tables when the C# model no longer references them. In a medical application storing patient examination data, **silent data loss is unacceptable**.

## P2 — `Database.SetInitializer` Called in the DbContext Constructor (High)
Calling `Database.SetInitializer(...)` inside the constructor means every `new ORT100Context(...)` re-registers the global initialiser. The initialiser should be set once at application startup, not per-instance.

## P3 — Seed Data Uses Unconditional `DbSet.Add()` (High)
`CreateMedTestDefinitions()` calls `Clinics.Add(...)`, `SystemUsers.Add(...)`, and hundreds of `MedTestStages.Add(...)` without checking for existing records. Re-running migrations (or reinitialising the database) will insert duplicate rows.

## P4 — `MedTest.DateTime` Is a `string` (High)
Storing date/time as a `string` prevents date range queries, sorting, and indexing on the temporal dimension. This is a fundamental type error.

## P5 — `MedTest.DiagnosticForm` Is `[NotMapped]` (High)
The link between a `MedTest` and its `DiagnosticForm` is not persisted. Reloading a `MedTest` from the database loses its form association. The relationship needs a proper FK column.

## P6 — `SystemUser.Passwd` Is Plaintext (High)
Storing passwords as plaintext strings is a critical security vulnerability. A hash (e.g., bcrypt, PBKDF2) should be stored instead.

## P7 — `MedTestCreator` Nested Inside `ORT100Context` (Medium)
Seeding and test-definition creation logic is embedded in a static class nested inside a `partial ORT100Context`. This violates the Single Responsibility Principle and makes the context class responsible for both persistence and business data initialisation.

## P8 — Connection String Assembled in Constructor (Medium)
The connection string is string-formatted inside the DbContext constructor. Any change to the server name or database name requires a code change and recompile. Connection strings should be externalised (app.config / settings).

## P9 — No `AsNoTracking()` on Read-Only Queries (Medium)
All queries track returned entities by default. Read-only queries (e.g., loading forms for display) incur unnecessary memory and CPU overhead from the EF6 change tracker.

## P10 — No Async EF6 Operations (Medium)
All database calls are synchronous. In a WPF application, long-running synchronous DB calls block the UI thread. EF6 supports `async`/`await` via `ToListAsync()`, `SaveChangesAsync()`, etc.

## P11 — No Indexes Defined (Medium)
No `[Index]` attributes or Fluent API `HasIndex()` calls exist. High-cardinality lookup columns (`Patient.PESEL`, `MedTestDefinition.Key`, `SystemUser.Login`) have no database indexes, leading to full table scans.

## P12 — `MedTestStage.MedTestDefinition` Not `virtual` (Medium)
The navigation property is not `virtual`, so EF6 lazy loading cannot proxy it. Accessing `stage.MedTestDefinition` after detachment will return `null` silently.

## P13 — Unbounded `LoadAllDiagnosticForms()` Query (Low–Medium)
`_context.DiagnosticForms.ToList()` loads all rows with no filter, ordering, or paging. As the table grows this will degrade.

## P14 — `Clinic.Adress` Typo (Low)
The property `Adress` (missing the second `d`) is now part of the DB schema. Renaming it requires a migration column rename. It is a minor but visible code-quality issue.

## P15 — `PESEL` Has No Unique Constraint (Low)
Poland's national ID (`PESEL`) is unique per citizen. Storing it without a uniqueness constraint allows accidental duplicate patient records.

## P16 — No Unit of Work Abstraction (Low)
`DiagnosticFormService` takes a concrete `ORT100Context` rather than an interface or IUnitOfWork. This tightly couples the service to EF6 and makes unit testing without a real database impossible.

## P17 — Manual "Test" Class Without a Test Framework (Low)
`DiagnosticFormServiceTests.cs` is a plain class with `Console.WriteLine` assertions and no test runner integration (NUnit, xUnit, MSTest). It will not be automatically discovered or executed.

## P18 — `DiagnosticForm.ParametersGroups` Leaks Domain Complexity Into the Entity (Low)
`DiagnosticForm` simultaneously acts as a JPA entity and a domain aggregate root (holding `List<IParametersGroup>`). This mixing of persistence and domain concerns complicates both layers.

---

# Recommended Refactoring Plan

## Short-Term Improvements

### ST-1: Disable `AutomaticMigrationDataLossAllowed`

```csharp
// Configuration.cs
public Configuration()
{
    AutomaticMigrationsEnabled = false;          // use explicit migrations only
    AutomaticMigrationDataLossAllowed = false;   // never allow silent data loss
}
```

Generate a full baseline explicit migration to replace the automatic history.

### ST-2: Move `Database.SetInitializer` to Application Startup

```csharp
// App.xaml.cs or MainWindow.xaml.cs — called once
Database.SetInitializer(
    new MigrateDatabaseToLatestVersion<ORT100Context, ORT100.Model.Migrations.Configuration>());
```

Remove it from the `ORT100Context` constructor entirely.

### ST-3: Fix Seed Data with `AddOrUpdate`

```csharp
// Configuration.cs Seed()
protected override void Seed(ORT100Context context)
{
    context.Clinics.AddOrUpdate(
        c => c.Name,
        new Clinic { Name = "Ośrodek Rehabilitacji Leczniczej 'Troniny'" }
    );
    context.SaveChanges();
    // etc.
}
```

### ST-4: Fix `MedTest.DateTime` Type

```csharp
// MedTest.cs
public DateTime ExaminationDate { get; set; }   // was: public string DateTime
```

Create an explicit migration for the column rename/type change.

### ST-5: Add `AsNoTracking()` to Read-Only Queries

```csharp
public List<DiagnosticForm> LoadAllDiagnosticForms()
{
    return _context.DiagnosticForms.AsNoTracking().ToList();
}

public DiagnosticForm LoadDiagnosticFormByName(string formName)
{
    return _context.DiagnosticForms
        .AsNoTracking()
        .FirstOrDefault(df => df.FormName == formName);
}
```

### ST-6: Add Missing Indexes via Fluent API

```csharp
// ORT100Context.OnModelCreating
modelBuilder.Entity<Patient>()
    .HasIndex(p => p.PESEL)
    .IsUnique()
    .HasName("IX_Patient_PESEL");

modelBuilder.Entity<SystemUser>()
    .HasIndex(u => u.Login)
    .IsUnique()
    .HasName("IX_SystemUser_Login");

modelBuilder.Entity<MedTestDefinition>()
    .HasIndex(d => d.Key)
    .IsUnique()
    .HasName("IX_MedTestDefinition_Key");
```

### ST-7: Hash Passwords

Replace the plaintext `Passwd` with a hashed representation. At minimum, use `System.Security.Cryptography.Rfc2898DeriveBytes` (PBKDF2):

```csharp
// Never store raw password; store only the hash
public string PasswordHash { get; set; }
```

---

## Medium-Term Refactoring

### MT-1: Introduce a DbContext Interface and Repository Abstraction

```csharp
public interface IOrt100UnitOfWork : IDisposable
{
    IRepository<Clinic>     Clinics     { get; }
    IRepository<Patient>    Patients    { get; }
    IRepository<MedTest>    MedTests    { get; }
    // ...
    int SaveChanges();
    Task<int> SaveChangesAsync();
}

public interface IRepository<T> where T : class
{
    T GetById(int id);
    IQueryable<T> Query();
    void Add(T entity);
    void Remove(T entity);
}
```

`DiagnosticFormService` (and all other services) should depend on `IOrt100UnitOfWork`, not on `ORT100Context` directly.

### MT-2: Separate Domain Model from EF Entities

The `DiagnosticForm` entity should not expose `List<IParametersGroup>` (domain concept). Split:

- **Persistence entity**: `DiagnosticFormEntity` — only mapped, scalar properties.
- **Domain object**: `DiagnosticForm` — contains `ParametersGroups`, business logic.

Use a mapper (AutoMapper or manual) to convert between the two.

### MT-3: Add Async Support

```csharp
// DiagnosticFormService
public async Task<DiagnosticForm> LoadDiagnosticFormByIdAsync(int id)
{
    return await _context.DiagnosticForms
        .AsNoTracking()
        .FirstOrDefaultAsync(df => df.DiagnosticFormId == id);
}

public async Task<int> SaveChangesAsync()
{
    return await _context.SaveChangesAsync();
}
```

### MT-4: Externalise the Connection String

```xml
<!-- app.config in ORT100.MainApp -->
<connectionStrings>
  <add name="ORT100Context"
       connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=ORT100Database;Integrated Security=True;AttachDBFilename=|DataDirectory|\ORT100Database.mdf"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

```csharp
// ORT100Context constructor
public ORT100Context() : base("name=ORT100Context") { }
```

### MT-5: Extract `MedTestCreator` Out of `ORT100Context`

Move `MedTestCreator` to a dedicated `SeedDataBuilder` class in a separate namespace (e.g., `ORT100.Model.Migrations.Seed`). It should not be a partial extension of `ORT100Context`.

### MT-6: Fix `MedTest` → `DiagnosticForm` Relationship

Add a nullable FK and proper navigation:

```csharp
// MedTest.cs
public int? DiagnosticFormId { get; set; }
public virtual DiagnosticForm DiagnosticForm { get; set; }
```

```csharp
// OnModelCreating
modelBuilder.Entity<MedTest>()
    .HasOptional(m => m.DiagnosticForm)
    .WithMany()
    .HasForeignKey(m => m.DiagnosticFormId)
    .WillCascadeOnDelete(false);
```

### MT-7: Make `MedTestStage.MedTestDefinition` Virtual

```csharp
public virtual MedTestDefinition MedTestDefinition { get; set; }
```

---

## Long-Term Architectural Improvements

### LT-1: Adopt a Proper Unit Test Framework

Replace `DiagnosticFormServiceTests.cs` with proper NUnit or xUnit tests using an in-memory SQLite provider or EF6 effort-based mocking (`Moq` + `DbSet` fakes).

### LT-2: Enable EF6 Query Logging in Development

```csharp
// ORT100Context constructor (debug builds only)
#if DEBUG
Database.Log = sql => System.Diagnostics.Debug.WriteLine(sql);
#endif
```

This surfaces N+1 queries and missing indexes during development.

### LT-3: Consider Bounded Contexts

The single `ORT100Context` owns every entity. As the system grows, consider splitting into:

- `ClinicalContext` — `Clinic`, `Patient`, `SystemUser`
- `ExaminationContext` — `MedTest`, `MedTestResult`, `MedTestContinuousResult`, `MedTestDefinition`, `MedTestStage`
- `DiagnosticContext` — `DiagnosticForm`

### LT-4: Add Soft Delete and Audit Fields

Medical records are sensitive; hard-deleting rows is dangerous. Introduce:

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? ModifiedAt { get; set; }
    bool IsDeleted { get; set; }
}
```

Override `SaveChanges` to set these fields automatically and apply a global query filter (`modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted)`).

### LT-5: Address `MedTestContinuousResult` Scalability

Continuous sensor data at high frequency should be considered for:
- Bulk insert via `SqlBulkCopy` or `EFCore.BulkExtensions` equivalent for EF6.
- Archiving old results to a separate table or compressed storage.
- Indexed `MedTestId` column (already present via FK convention, but verify).

---

# EF6 Best Practices Checklist

| # | Checklist Item | Current Status |
|---|---|---|
| 1 | `AutomaticMigrationsEnabled = false` | ❌ Enabled |
| 2 | `AutomaticMigrationDataLossAllowed = false` | ❌ Allowed |
| 3 | `Database.SetInitializer` called once at app startup | ❌ Called in constructor |
| 4 | Connection string externalised to config file | ❌ Hardcoded in constructor |
| 5 | Seed data uses `AddOrUpdate` guards | ❌ Unconditional `Add()` |
| 6 | Read-only queries use `AsNoTracking()` | ❌ None present |
| 7 | Async EF operations for UI responsiveness | ❌ All synchronous |
| 8 | Navigation properties that need lazy loading are `virtual` | ⚠️ `MedTestStage.MedTestDefinition` is not virtual |
| 9 | Unique indexes on natural keys (`PESEL`, `Login`, `Key`) | ❌ Missing |
| 10 | Date/time stored as `DateTime`, not `string` | ❌ `MedTest.DateTime` is `string` |
| 11 | Passwords hashed, never stored plaintext | ❌ `SystemUser.Passwd` is plaintext |
| 12 | FK for `MedTest → DiagnosticForm` persisted | ❌ `[NotMapped]` |
| 13 | Services depend on abstraction, not concrete `DbContext` | ❌ Direct `ORT100Context` dependency |
| 14 | Seeding logic separated from `DbContext` class | ❌ Embedded as nested static class |
| 15 | Domain model separated from persistence entity | ⚠️ `DiagnosticForm` mixes both |
| 16 | EF6 query logging enabled in DEBUG builds | ❌ Absent |
| 17 | Explicit transactions for multi-step seed/write operations | ❌ Absent |
| 18 | Concurrency tokens on long-lived entities | ❌ Absent |
| 19 | Proper unit tests with a test framework | ❌ Console-only manual tests |
| 20 | Cascade delete rules explicitly configured | ⚠️ Implicit EF6 defaults |

---

# Example Refactoring Snippets

## 1. Connection String — Before / After

**Before:**
```csharp
public ORT100Context(string dbName, string instanceName) : base(String.Format(
    @"data source=" + instanceName + @"; initial catalog={0}; ...", dbName))
{
    Database.SetInitializer(new MigrateDatabaseToLatestVersion<...>());
}
```

**After:**
```csharp
// app.config
// <add name="ORT100Context" connectionString="..." providerName="System.Data.SqlClient" />

// ORT100Context.cs
public ORT100Context() : base("name=ORT100Context")
{
    // Lazy loading is on by default; disable if using explicit/eager loading exclusively
    // this.Configuration.LazyLoadingEnabled = false;
}

// App.xaml.cs (once, at startup)
Database.SetInitializer(
    new MigrateDatabaseToLatestVersion<ORT100Context, Migrations.Configuration>());
```

---

## 2. Seed Data — Before / After

**Before:**
```csharp
protected override void Seed(ORT100Context context)
{
    context.CreateMedTestDefinitions(); // Calls Clinics.Add(...) unconditionally
}
```

**After:**
```csharp
protected override void Seed(ORT100Context context)
{
    context.Clinics.AddOrUpdate(
        c => c.Name,
        new Clinic { Name = "Ośrodek Rehabilitacji Leczniczej 'Troniny'" }
    );
    context.SaveChanges();

    context.SystemUsers.AddOrUpdate(
        u => u.Login,
        new SystemUser { Login = "admin", PasswordHash = PasswordHasher.Hash("changeme") }
    );
    context.SaveChanges();

    SeedMedTestDefinitions(context);
}
```

---

## 3. Read-Only Query — Before / After

**Before:**
```csharp
public List<DiagnosticForm> LoadAllDiagnosticForms()
{
    return _context.DiagnosticForms.ToList();
}
```

**After:**
```csharp
public List<DiagnosticForm> LoadAllDiagnosticForms()
{
    return _context.DiagnosticForms
        .AsNoTracking()
        .OrderBy(df => df.FormName)
        .ToList();
}
```

---

## 4. Async Service Method — Before / After

**Before:**
```csharp
public DiagnosticForm LoadDiagnosticFormById(int id)
{
    return _context.DiagnosticForms.Find(id);
}
```

**After:**
```csharp
public async Task<DiagnosticForm> LoadDiagnosticFormByIdAsync(int id)
{
    return await _context.DiagnosticForms
        .AsNoTracking()
        .FirstOrDefaultAsync(df => df.DiagnosticFormId == id);
}
```

---

## 5. Adding Fluent API Indexes — Before / After

**Before:**
```csharp
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
    modelBuilder.Entity<DiagnosticForm>().Ignore(df => df.ParametersGroups);
}
```

**After:**
```csharp
protected override void OnModelCreating(DbModelBuilder modelBuilder)
{
    modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

    modelBuilder.Entity<DiagnosticForm>()
        .Ignore(df => df.ParametersGroups);

    modelBuilder.Entity<Patient>()
        .Property(p => p.PESEL).HasMaxLength(11)
        .HasColumnAnnotation("Index",
            new IndexAnnotation(new IndexAttribute("IX_Patient_PESEL") { IsUnique = true }));

    modelBuilder.Entity<SystemUser>()
        .Property(u => u.Login).IsRequired().HasMaxLength(100)
        .HasColumnAnnotation("Index",
            new IndexAnnotation(new IndexAttribute("IX_SystemUser_Login") { IsUnique = true }));

    modelBuilder.Entity<MedTestDefinition>()
        .Property(d => d.Key).IsRequired().HasMaxLength(100)
        .HasColumnAnnotation("Index",
            new IndexAnnotation(new IndexAttribute("IX_MedTestDefinition_Key") { IsUnique = true }));
}
```

---

## 6. Repository Abstraction — Before / After

**Before:**
```csharp
public class DiagnosticFormService
{
    private readonly ORT100Context _context;

    public DiagnosticFormService(ORT100Context context) { _context = context; }
}
```

**After:**
```csharp
public interface IDiagnosticFormRepository
{
    DiagnosticForm GetById(int id);
    DiagnosticForm GetByName(string formName);
    IReadOnlyList<DiagnosticForm> GetAll();
    void Add(DiagnosticForm form);
    void Remove(DiagnosticForm form);
}

public class DiagnosticFormRepository : IDiagnosticFormRepository
{
    private readonly ORT100Context _context;
    public DiagnosticFormRepository(ORT100Context context) { _context = context; }

    public DiagnosticForm GetById(int id)
        => _context.DiagnosticForms.AsNoTracking().FirstOrDefault(df => df.DiagnosticFormId == id);

    public IReadOnlyList<DiagnosticForm> GetAll()
        => _context.DiagnosticForms.AsNoTracking().OrderBy(df => df.FormName).ToList();

    // ...
}

public class DiagnosticFormService
{
    private readonly IDiagnosticFormRepository _repository;
    private readonly IOrt100UnitOfWork _uow;

    public DiagnosticFormService(IDiagnosticFormRepository repository, IOrt100UnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }
}
```

---

*Document generated: 2025 | Solution: ORT100 / OrthoSpine | EF Version: Entity Framework 6.x | Target: .NET Framework 4.8*
