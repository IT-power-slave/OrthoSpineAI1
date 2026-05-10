# OrthoSpineAI

**Medical spine assessment desktop application** for conducting and interpreting orthopaedic examinations with BLE hardware integration.

Built with **.NET 10 · WPF · Clean Architecture · MVVM**

---

## Features

- Patient management (register, search, edit, delete)
- Survey/examination workflow guided by configurable `MedTestDefinition` entries
- Live BLE device capture (ORT-100 sensor) with real-time frame streaming
- **AWWS / PiLS algorithm** — automatic posture-group classification (scoliosis risk, sagittal curvature, pelvic tilt, leg statics, hypermobility)
- Diagnostic result history per patient
- Dashboard with today/month exam counts and recent results
- Local SQLite database with EF Core migrations and seed data

---

## Solution Structure

```
OrthoSpineAI/
├── src/
│   ├── OrthoSpineAI.Domain/          Pure business entities, interfaces, enums
│   ├── OrthoSpineAI.Application/     Services, DTOs, algorithm engine
│   ├── OrthoSpineAI.Infrastructure/  EF Core, repositories, BLE device driver
│   └── OrthoSpineAI.UI/              WPF views and ViewModels (MVVM)
└── tests/
    └── OrthoSpineAI.Tests/           xUnit unit tests (116 tests)
```

Dependency flow: `UI → Application → Domain ← Infrastructure`

---

## Getting Started

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0 or later |
| Windows | 10 / 11 (WPF) |
| Visual Studio | 2022+ or VS 2026 |

### Run

```powershell
git clone https://github.com/IT-power-slave/OrthoSpineAI1.git
cd OrthoSpineAI1
dotnet run --project src/OrthoSpineAI.UI
```

The database is created automatically at `%LocalApplicationData%\OrthoSpineAI\ortho.db` on first launch and seeded with default survey definitions and a demo user.

### Test

```powershell
dotnet test
```

---

## Architecture

See [`ARCHITECTURE_ANALYSIS.md`](ARCHITECTURE_ANALYSIS.md) for a detailed analysis of SOLID principles, MVVM quality, and improvement history.

### Key design decisions

| Decision | Rationale |
|----------|-----------|
| Clean Architecture | Protects domain from infrastructure/UI changes |
| Interface-based services (`IAuthService`, `IPatientService`, …) | Enables unit testing and DIP compliance |
| `IDialogService` abstraction | Removes WPF `MessageBox` from ViewModels |
| `AwwsEngine` injected via DI | Decouples algorithm from service constructor |
| `CommunityToolkit.Mvvm` source generators | Reduces boilerplate for `INotifyPropertyChanged` / `ICommand` |
| Singleton `DbContext` | Safe for single-user desktop app; avoids repeated connection overhead |
| Global `DispatcherUnhandledException` handler | Prevents silent crashes |

---

## CI

GitHub Actions runs on every push and pull request to `main`:

- `dotnet restore`
- `dotnet build --configuration Release`
- `dotnet test` with `coverlet` code-coverage collection (Cobertura XML artifact)

See [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

---

## Algorithm — AWWS / PiLS

The `AwwsEngine` in `OrthoSpineAI.Application.Algorithm` evaluates a set of posture parameters against a pipeline of `IPGLogic` modules:

| Module | Classifies |
|--------|-----------|
| `PGLogicAtr` | ATR / scoliosis risk (healthy / low / medium / high) |
| `PGLogicBeightonScaleNumeric` | Joint hypermobility by age and Beighton score |
| `PGLogicFLLD` | Pelvic inclination / leg-length discrepancy |
| `PGLogicPT` | Pelvic tilt / sagittal posture group |
| `PGLogicLLTHK` | Lumbar lordosis + thoracic kyphosis by age |
| `PGLogicLegsStatics` | Lower-limb static deformities |

Each module returns `IReadOnlyDictionary<AwwsGroup, bool>`. The engine aggregates results and maps them to a **PiLS variant (0–4)** and a **control key (1–6)**, producing a textual conclusion and physiotherapy recommendation.
