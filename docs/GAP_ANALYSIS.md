# OrthoSpineAI — Gap Analysis: Documentation vs. Implementation

> **Generated:** 2025  
> **Purpose:** Tracks all known missing or incomplete features compared to the official documentation set.  
> **Status legend:** 🔴 Critical · 🟡 Important · 🟠 Device · 🟢 Minor

---

## Summary Table

| # | Gap | Severity | Area | Status |
|---|-----|----------|------|--------|
| 1 | Side selection for bilateral surveys | 🔴 Critical | Survey flow | ✅ Fixed |
| 2 | `OrtNextStepButton` not honoured | 🔴 Critical | Stage execution | ✅ Fixed |
| 3 | `MEAS_NULL` preparation stages not skipped | 🔴 Critical | Stage execution | ✅ Fixed |
| 4 | Adams continuous recording (`OrtContinousMeas`) | 🔴 Critical | Hardware + persistence | ✅ Fixed |
| 5 | ATR_max extracted from continuous results | 🔴 Critical | AWWS algorithm input | ✅ Fixed |
| 6 | HS (Hump Score) parameter never populated | 🔴 Critical | AWWS algorithm input | ✅ Fixed |
| 7 | FLLD / LegsStatics AWWS parameter mapping incomplete | 🔴 Critical | AWWS algorithm input | ✅ Fixed |
| 8 | PT / LL / THK extraction from `MedTestResult` by `OrtMeas` | 🔴 Critical | AWWS algorithm input | ✅ Fixed |
| 9 | Sub-test hierarchy sequential navigation (backbone.1 → .2 → .summary) | 🟡 Important | Survey flow | ✅ Fixed |
| 10 | `.summary` definition stages displayed as measurement stages | 🟡 Important | Stage execution | ✅ Fixed |
| 11 | ISOM normative reference values not displayed during measurement | 🟡 Important | Survey UX | ✅ Fixed |
| 12 | `MedTest.Description` free-text notes field not exposed | 🟡 Important | Data model | ✅ Fixed |
| 13 | OrtState / vertebra highlight not rendered (Adams UX) | 🟡 Important | Adams UX | ✅ Fixed |
| 14 | Real BLE driver (`cdortometr.dll` P/Invoke wrapper) | 🟠 Device | Hardware | ❌ Simulated only |
| 15 | `OrtResetFlag` zeroing behaviour not simulated | 🟠 Device | Hardware | ✅ Fixed |
| 16 | Wrist survey key descriptions swapped in seeder | 🟢 Minor | Seed data | ✅ Fixed |
| 17 | `MedTestDefinitionKey` is a soft string FK, no real FK constraint | 🟢 Minor | Schema | ✅ Fixed |
| 18 | No `SortOrder` column on `MedTestStage` | 🟢 Minor | Schema | ✅ Fixed |
| 19 | Report / print / PDF pipeline not implemented | 🟢 Minor | Reporting | ✅ Fixed |

---

## Detailed Findings

---

### #1 🔴 Side Selection for Bilateral Surveys

**Documentation reference:** `Implementation_Guide.md` §5 — Side Handling

**What the docs require:**  
Shoulder, elbow, hip, knee, wrist, and ankle surveys share a **single DB key** (no `.left` / `.right` suffix). Before starting such a survey, the application must ask the clinician which side is being examined (`SIDE_LEFT` / `SIDE_RIGHT`). This side value must be:
- Stored on every `MedTestResult` row produced by that session.
- Used by the UI to label the active side throughout the survey.

**✅ Fixed:** A bilateral-survey detection set (`shoulder`, `elbow`, `hip`, `knee`, `wrist`, `ankle`) was added to `SurveySelectionViewModel`. When a bilateral survey is selected, `IsBilateralSurvey` becomes `true`, showing a Left/Right radio-button panel in the selection view. `CanStart()` now requires a side to be chosen for bilateral surveys before the start button is enabled. The chosen `MedTestSide` value (`SIDE_LEFT` / `SIDE_RIGHT` / `SIDE_NONE`) is passed through `SurveyStartRequested`, `ShellViewModel.NavigateToPreTest`, and `NavigateToSurveyRunAsync` into `SurveyRunViewModel`, which stores it in `_side` and forwards it to every `SaveMeasurementDto` produced during the session, replacing the previous hardcoded `SIDE_NONE`.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/SurveySelectionViewModel.cs` — bilateral detection, `SelectedSide`, `IsSideLeft`, `IsSideRight`, `SelectLeftCommand`, `SelectRightCommand`, updated `CanStart` and event signature
- `src/OrthoSpineAI.UI/Views/SurveySelectionView.xaml` — left/right radio button panel conditionally visible for bilateral surveys
- `src/OrthoSpineAI.UI/ViewModels/ShellViewModel.cs` — threaded `MedTestSide` through `NavigateToPreTest` and `NavigateToSurveyRunAsync`
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — accepts `side` parameter, stores `_side`, uses it in `SaveMeasurementDto`

---

### #2 🔴 `OrtNextStepButton` Stage Rule Not Honoured

**Documentation reference:** `Implementation_Guide.md` §6 — Stage Execution Rules, §4 — Stage Lifecycle

**What the docs require:**  
Each `MedTestStage` specifies exactly which device button advances it:

| `OrtNextStepButton` | Behaviour |
|---------------------|-----------|
| `BTN_SAMPLE` | Capture current sensor reading → save as `MedTestResult` → advance |
| `BTN_NEXT` | Advance without recording (instruction / preparation stages) |
| `BTN_RESET` | Zero the device reference → **stay** in the same stage (clinician repositions) |

**✅ Fixed:** `SurveyRunViewModel` now exposes `IsBtnSampleStage` (true only when `OrtNextStepButton == BTN_SAMPLE`) and branches on it throughout the stage lifecycle:
- `GoToStage()` pre-sets `HasCapturedValue = !IsBtnSampleStage`, so Next is immediately available for `BTN_NEXT` and `BTN_RESET` stages without any capture.
- `NextStepAsync()` calls `SaveMeasurementAsync` and logs to the history panel **only** for `BTN_SAMPLE` stages; `BTN_NEXT` and `BTN_RESET` stages advance without writing a `MedTestResult` row.
- `SurveyRunView.xaml` shows the measurement capture panel **only** for `BTN_SAMPLE` stages via a `DataTrigger` on `IsBtnSampleStage`.

This supersedes and generalises the `IsNullMeasStage` check introduced for gap #3.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs`
- `src/OrthoSpineAI.UI/Views/SurveyRunView.xaml`

---

### #3 🔴 `MEAS_NULL` Preparation Stages Not Handled

**Documentation reference:** `Implementation_Guide.md` §6; `Seed_Data_Stages.md`

**What the docs require:**  
Stages with `OrtMeas = MEAS_NULL` are purely informational: they display an instruction image (`TipControl`) and descriptive text. No measurement is taken and no `MedTestResult` row should be created.

**✅ Fixed:** `SurveyRunViewModel` now detects `MEAS_NULL` stages via the `IsNullMeasStage` computed property. For such stages:
- `HasCapturedValue` is pre-set to `true` on `GoToStage()` so Next is immediately available without a capture click.
- `NextStepAsync` skips the `SaveMeasurementAsync` call — no `MedTestResult` row is created.
- `SurveyRunView.xaml` hides the measurement capture panel via a `DataTrigger` on `IsNullMeasStage`.

---

### #4 🔴 Adams Test Continuous Recording (`OrtContinousMeas`) Missing

**Documentation reference:** `Implementation_Guide.md` §6, §7 — Continuous Results; `Hardware_Protocol.md`

**What the docs require:**  
When `MedTestStage.OrtContinousMeas = true` (Adams test stages AD1–AD5), **every** incoming Bluetooth telemetry frame must be saved as a `MedTestContinuousResult` row with all sensor fields:
`Status`, `Signal`, `Battery`, `Shake`, `Roll`, `RollOffset`, `Tilt`, `Way`, `Space`, `Force1`, `Force2`, `OrtMeas`, `MedTestId`.

**✅ Fixed:** `SurveyRunViewModel.OnFrameReceived()` now checks `CurrentStage?.OrtContinousMeas == true` on every incoming frame. When true, it constructs a `SaveContinuousFrameDto` from the incoming `DeviceFrame` fields (including `Status.RawValue`, `Signal`, `Battery`, `Shake`, `Roll`, `RollOffset`, `Tilt`, `Way`, `Space`, `Force1`, `Force2`) and fires `_medTestService.SaveContinuousFrameAsync(dto, _cts.Token)` asynchronously. The existing `SaveContinuousFrameAsync` in `MedTestService` and `AddContinuousResultAsync` in `MedTestRepository` persist all required fields to `MedTestContinuousResult`.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — wired continuous persistence in `OnFrameReceived`

---

### #5 🔴 ATR_max Not Extracted from Continuous Results

**Documentation reference:** `AWWS_Algorithm.md` §5 — PiLS Decision Logic; `Implementation_Guide.md` §8

**What the docs require:**  
```
ATR_max = max(|aMin|, |aMax|)
```
Computed from the `Roll` field of `MedTestContinuousResult` rows where `OrtMeas ∈ {AC7, AT6, AT12, AL3, ASIPS}` (Adams test measurements). This value drives the entire PiLS decision tree.

**✅ Fixed:** `MedTestService.FinishTestAsync()` now computes `ATR_max` exclusively from `MedTestContinuousResult` rows (loaded via the existing `Include(t => t.ContinuousResults)` in the repository). It filters continuous rows by the five Adams `OrtMeas` values (`MEAS_AC7`, `MEAS_AT6`, `MEAS_AT12`, `MEAS_AL3`, `MEAS_ASIPS`), takes `Math.Abs(Roll)` for each, and uses the maximum as `ATR_max`. No new repository method was required — `ContinuousResults` is already eagerly loaded. When no continuous frames exist, `ATR_max` falls back to 0.

**Files changed:**
- `src/OrthoSpineAI.Application/Services/MedTestService.cs` — replaced discrete-row ATR lookup with continuous-row max(|Roll|) computation

---

### #6 🔴 HS (Hump Score) Parameter Never Populated

**Documentation reference:** `AWWS_Algorithm.md` §2 — Input Parameters; `PGLogicAtr` conditions

**What the docs require:**  
`HS` (Hump Score) is a separate AWWS input representing back asymmetry measured with a point grid. It is distinct from `ATR`. The `PGLogicAtr` logic conditions on **both** `ATR` and `HS`:
- `IS_LowRiskGroup` requires `(ATR ≥ 3 AND ATR ≤ 4) OR (HS ≥ 4 AND HS ≤ 5)`
- `IS_HighRiskGroup` requires `ATR ≥ 7 OR HS ≥ 8`

**✅ Fixed:** `HS` is now a full first-class field throughout the stack:
- `MedTest.Hs` (int, default 0) added to the domain entity with an EF Core migration (`AddMedTestHs`).
- `CreateMedTestDto` and `MedTestDto` include `Hs`.
- `MedTestService.CreateAsync()` persists `dto.Hs` to the entity; `FinishTestAsync()` now sets `p[AwwsParams.HS] = test.Hs` (replacing the earlier `HS ≈ ATR` approximation).
- `PreTestViewModel` exposes `Hs` (int, 0–20) with a validation guard.
- `PreTestView.xaml` shows a labeled Hump Score slider (0–20) below the Beighton slider.
- `ShellViewModel` passes `hs: preTest.Hs` when constructing `SurveyRunViewModel`.
- `SurveyRunViewModel` accepts and forwards `hs` to `CreateMedTestDto`.

**Files changed:**
- `src/OrthoSpineAI.Domain/Entities/MedTest.cs`
- `src/OrthoSpineAI.Application/DTOs/MedTestDto.cs`
- `src/OrthoSpineAI.Application/Services/MedTestService.cs`
- `src/OrthoSpineAI.UI/ViewModels/PreTestViewModel.cs`
- `src/OrthoSpineAI.UI/Views/PreTestView.xaml`
- `src/OrthoSpineAI.UI/ViewModels/ShellViewModel.cs`
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs`
- `src/OrthoSpineAI.Infrastructure/Persistence/Migrations/` — new migration `AddMedTestHs`

---

### #7 🔴 FLLD / LegsStatics AWWS Parameter Mapping Incomplete

**Documentation reference:** `AWWS_Algorithm.md` §4 — PGLogicFLLD, PGLogicLegsStatics

**What the docs require:**

| AWWS Parameter | Source |
|----------------|--------|
| `FLLD_POSITIVE` | `MedTest.TestPP = true` |
| `FLLD_NEGATIVE` | `MedTest.TestPP = false` AND test is not neutral |
| `FLLD_NEUTRAL` | Neutral result (not stored currently) |
| `LEGSSTAT_DISTURBED` | `MedTest.KneeValgus OR MedTest.TarsalValgus` |
| `LEGSSTAT_CORRECT` | Neither KneeValgus nor TarsalValgus |

**Current implementation:**  
`MedTestService.FinishTestAsync()` maps `TestPP → FLLD_POSITIVE/NEGATIVE` correctly, but `KneeValgus` and `TarsalValgus` are **not** mapped to `LEGSSTAT_DISTURBED`/`LEGSSTAT_CORRECT` in the parameter dictionary, so `PGLogicLegsStatics` always receives no input.

**✅ Fixed:** `MedTestService.FinishTestAsync()` now correctly maps:
- `LEGSSTAT_DISTURBED` = `KneeValgus || TarsalValgus` (TestPP excluded per docs)
- `LEGSSTAT_CORRECT` = `!KneeValgus && !TarsalValgus`
- `FLLD_POSITIVE` = `TestPP`, `FLLD_NEGATIVE` = `!TestPP` (replaced incorrect NM-angle heuristic)

4 new unit tests added to `MedTestServiceTests.cs`.

---

### #8 🔴 PT / LL / THK Extraction from `MedTestResult` by `OrtMeas`

**Documentation reference:** `Implementation_Guide.md` §8 — AWWS Inputs table; `AWWS_Algorithm.md` §2

**What the docs require:**

| AWWS Parameter | `OrtMeas` value | Measurement |
|----------------|-----------------|-------------|
| `PT` | `MEAS_NM` (int 1) | Pelvic tilt |
| `LL` | `MEAS_LL` (int 2) | Lumbar lordosis |
| `THK` | `MEAS_KP` (int 5) | Thoracic kyphosis total |

**✅ Fixed:** Audit confirmed that `ORT100Measurement` enum integer values exactly match Appendix D of `Implementation_Guide.md` (MEAS_NULL=0, MEAS_NM=1, MEAS_LL=2, MEAS_KP=5, etc.). EF Core stores enums as int by convention. The seeder seeds stages using the same typed enum constants. `FinishTestAsync()` queries `MedTestResult` rows using the correct `ORT100Measurement` enum members (`MEAS_NM`, `MEAS_LL`, `MEAS_KP`) — no mismatch exists. No code change was required.

---

### #9 🟡 Sub-Test Hierarchy Not Navigated Sequentially

**Documentation reference:** `Implementation_Guide.md` §5 — Survey Keys Structure

**What the docs require:**  
The `backbone` survey is a group of definitions run in sequence:
```
backbone          (preparation)
  └─ backbone.1   (sagittal plane measurement)
  └─ backbone.2   (Adams test)
  └─ backbone.summary  (results display)
```
Similarly: `spineScreening.1 → spineScreening.2 → spineScreening.1.summary`

All sub-tests in a group share one `MedTest` record (same `MedTestId`). The app must auto-advance to the next definition when one finishes.

**✅ Fixed:** `ShellViewModel.NavigateToSurveyRunAsync` now calls `_surveyService.GetSurveyGroupAsync(rootKey)` before constructing `SurveyRunViewModel`, where `rootKey` is the first dot-separated segment of the chosen definition key (e.g. `backbone`). The full ordered group list is passed as a new `group` parameter to the view-model. `SurveyRunViewModel` stores the group in `_group` and tracks `_definitionIndex`. When `GoToStage()` exhausts a definition's stages, it calls `AdvanceToDefinition(_definitionIndex + 1)` instead of immediately finalising — swapping `Definition` to the next entry, resetting stage state, and continuing with its stages using the same `_medTestId`. When the last definition in the group is exhausted the normal `FinishSurvey()` / AWWS calculation path is invoked.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — added `_group`, `_definitionIndex`, settable `Definition`, `IsSummaryDefinition`, `AdvanceToDefinition()`, and group-aware `GoToStage()` branching
- `src/OrthoSpineAI.UI/ViewModels/ShellViewModel.cs` — loads group via `GetSurveyGroupAsync` and passes `group:` to `SurveyRunViewModel`

---

### #10 🟡 `.summary` Definition Stages Not Handled

**Documentation reference:** `Implementation_Guide.md` §5, §6; `Seed_Data_Stages.md`

**What the docs require:**  
`*.summary` definitions contain display-only stages (charts, result tables, `OrtMeas = MEAS_NULL`). No device interaction or measurement. The stage should display computed results and provide "Finish" / "New survey" actions.

**✅ Fixed:** `SurveyRunViewModel` exposes `IsSummaryDefinition` (true when `Definition.Key.EndsWith(".summary")`), notified on every definition switch. `.summary` definitions have `MEAS_NULL` / `BTN_NEXT` stages, so the existing `IsBtnSampleStage = false` logic already suppresses the capture panel and auto-enables Next. Additionally, `SurveyRunView.xaml` now shows a dedicated green "📊 Podsumowanie badania" information panel (bound with a `DataTrigger` on `IsSummaryDefinition`) that explains the summary context and prompts the clinician to press Finish. The measurement capture card remains hidden (BTN_SAMPLE is false), so no meaningless `MedTestResult` rows are created.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — added `IsSummaryDefinition` property, notified in `AdvanceToDefinition()`
- `src/OrthoSpineAI.UI/Views/SurveyRunView.xaml` — added summary-mode green banner panel in Grid.Row="2"

---

### #11 🟡 ISOM Normative Reference Values Not Displayed

**Documentation reference:** `Implementation_Guide.md` §4 — ISOM Reference Values, Appendix A

**What the docs require:**  
Each `MedTestStage` carries:
- `ValueISOM1` — upper normative reference (extension / away direction)
- `ValueISOM3` — lower normative reference (flexion / toward direction)

These must be shown during measurement so the clinician can compare the live reading to expected normal range.

**✅ Fixed:** `SurveyRunView.xaml` now displays a "Norma ISOM" reference panel inside the measurement capture card, directly below the live Roll angle and captured value. The panel is conditionally visible — it appears only when `HasIsomReference` is `true` (i.e. at least one of `ValueISOM1` / `ValueISOM3` is non-null). It shows both the upper (↑) and lower (↓) normative values in degrees. `SurveyRunViewModel` was extended with a `HasIsomReference` computed property that is notified on every `GoToStage()` call alongside `IsBtnSampleStage` and `IsNullMeasStage`. No DTO or service changes were required — `StageDto.ValueISOM1` and `ValueISOM3` were already populated.

**Files changed:**
- `src/OrthoSpineAI.UI/Views/SurveyRunView.xaml` — added ISOM reference panel in measurement capture card
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — added `HasIsomReference` computed property, notified in `GoToStage()`

---

### #12 🟡 `MedTest.Description` Not Exposed

**Documentation reference:** `Implementation_Guide.md` §2 — `MedTest` entity

**What the docs require:**  
`MedTest.Description` is a free-text field for clinician notes per examination session.

**✅ Fixed:** `Description` is now fully threaded from UI through to persistence:
- `PreTestViewModel` exposes a `Description` observable string property (empty by default).
- `PreTestView.xaml` shows a labelled multi-line `TextBox` ("Notatki kliniczne") below the clinical flags checkboxes.
- `SurveyRunViewModel` accepts `description` as a constructor parameter and passes it as `Description:` to `CreateMedTestDto`.
- `ShellViewModel.NavigateToSurveyRunAsync` forwards `preTest.Description`.
- `MedTestService.CreateAsync` already mapped `dto.Description` to `MedTest.Description` and the field was already present on `CreateMedTestDto` — no service or DTO changes were required.

**Files changed:**
- `src/OrthoSpineAI.UI/ViewModels/PreTestViewModel.cs` — added `Description` observable property
- `src/OrthoSpineAI.UI/Views/PreTestView.xaml` — added notes TextBox
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — added `description` parameter and forwards to `CreateMedTestDto`
- `src/OrthoSpineAI.UI/ViewModels/ShellViewModel.cs` — passes `preTest.Description`

---

### #13 🟡 OrtState / Vertebra Highlight Not Rendered

**Documentation reference:** `Implementation_Guide.md` §3 — `ORT100ControlState`; §6

**What the docs require:**  
Adams test stages carry `OrtState` indicating which spinal landmark is currently active:
`HIGHLIGHT_C7`, `HIGHLIGHT_TH6`, `HIGHLIGHT_TH12`, `HIGHLIGHT_L3`, `HIGHLIGHT_S1`

A spine diagram should visually highlight the active vertebra so the clinician knows where to place the device.

**✅ Fixed:** The right-side telemetry panel in `SurveyRunView.xaml` now contains a "Punkt pomiaru" (measurement point) spine-landmark list that visually highlights the active vertebra using `DataTrigger`-driven background/foreground swaps (blue fill, white text when active; grey fill otherwise). The panel is hidden entirely for stages where `OrtState == HIGHLIGHT_NONE`. `SurveyRunViewModel` was extended with `IsSpineDiagramVisible` and five boolean properties — `HighlightC7`, `HighlightTH6`, `HighlightTH12`, `HighlightL3`, `HighlightS1` — each true when the current stage's `OrtState` matches that landmark or is `HIGHLIGHT_ALL`. All six properties are notified on every `GoToStage()` call. No DTO or service changes were required — `StageDto.OrtState` was already populated.

**Files changed:**
- `src/OrthoSpineAI.UI/Views/SurveyRunView.xaml` — added spine-landmark highlight panel in right telemetry column
- `src/OrthoSpineAI.UI/ViewModels/SurveyRunViewModel.cs` — added `IsSpineDiagramVisible`, `HighlightC7/TH6/TH12/L3/S1` computed properties, notified in `GoToStage()`

---

### #14 🟠 Real BLE Driver (`cdortometr.dll`) Not Implemented

**Documentation reference:** `Hardware_Protocol.md` §1–§7

**What the docs require:**  
On Windows: discover ORT-100 device via registry `HKLM\SYSTEM\ControlSet001\Enum\BTHLE\...` (filter by `FriendlyName` containing `"ORT-100"`), then communicate via P/Invoke to `cdortometr.dll`:
- `CDClientOrtometr.SetMacAddress(mac)`
- `CDClientOrtometr.SendConfig(ref SOrtometrCfgFrame)`
- `CDClientOrtometr.ProcessFrames(...)` → raises `OrtoDataReceived` event with `SOrtometrDataFrame`

**Current implementation:**  
`SimulatedDeviceDriver` generates random Roll/Tilt/Way values. The `cdortometr.dll` P/Invoke wrapper and registry device discovery are not implemented.

**Files to create:**
- `src/OrthoSpineAI.Infrastructure/Device/NativeOrtometrDriver.cs` — registry scan + P/Invoke wrapper
- `src/OrthoSpineAI.Infrastructure/Device/SOrtometrDataFrame.cs` — binary frame struct
- `src/OrthoSpineAI.Infrastructure/Device/SOrtometrCfgFrame.cs` — config frame struct

---

### #15 🟠 `OrtResetFlag` Zeroing Not Simulated

**Documentation reference:** `Implementation_Guide.md` §3 — `ORT100ResetFlag`; §4 — Stage Lifecycle

**What the docs require:**  
On stage entry the device reference is zeroed according to `OrtResetFlag`:

| Flag | Effect |
|------|--------|
| `NONE` | No reset |
| `ZERO_ANGLE` | Zero angle to current physical position |
| `ZERO_ANGLE_DEF` | Zero angle to gravity reference |
| `ZERO_WAY` | Zero distance only |
| `ZERO_WAY_ANGLE` | Zero both (current position) |
| `ZERO_WAY_ANGLE_DEF` | Zero both (gravity default) |

**✅ Fixed:** `BleDeviceDriver.SendConfig()` now applies the zeroing flags decoded into `DeviceConfig` at the simulator level. The driver tracks two internal offsets — `_rollOffset` (angle) and `_wayOffset` (distance) — alongside the last raw generated values (`_rawRoll`, `_rawWay`). On every call to `SendConfig`:
- If `config.ZeroAngle || config.ZeroAngleDef` → `_rollOffset` is set to `_rawRoll`, zeroing the angle to the current simulated position.
- If `config.ZeroWay` → `_wayOffset` is set to `_rawWay`, zeroing the distance counter.

`BuildSimulatedFrame()` publishes `Roll = rawRoll − _rollOffset` and `Way = rawWay − _wayOffset`, so subsequent frames read relative to the zeroed reference — matching the physical device behaviour described in the docs. `DeviceConfig.FromResetFlag()` already decoded all six `ORT100ResetFlag` combinations correctly; no Domain-layer changes were required.

8 new unit tests added to `DeviceConfigTests.cs` verifying all flag combinations and text padding.

**Files changed:**
- `src/OrthoSpineAI.Infrastructure/Devices/BleDeviceDriver.cs` — added `_rawRoll`, `_rawWay`, `_rollOffset`, `_wayOffset` fields; implemented zeroing in `SendConfig()`; applied offsets in `BuildSimulatedFrame()`
- `tests/OrthoSpineAI.Tests/Domain/DeviceConfigTests.cs` — new test class (8 tests) covering all `ORT100ResetFlag` variants and text truncation/padding

---

### #16 🟢 Wrist Survey Key Descriptions Swapped

**Documentation reference:** `surveys/index.md` — known issue note; `Implementation_Guide.md` §5

**What the docs require:**  
Correct mapping:
- `wrist.left` → "Lewy nadgarstek" (Left wrist)
- `wrist.right` → "Prawy nadgarstek" (Right wrist)

The reference implementation has these **swapped**.

**✅ Fixed:** Audit of `DatabaseSeeder.SeedWristAsync()` confirmed the definitions are seeded in the correct order: `Def(db, "wrist.left", "Lewy nadgarstek")` followed by `Def(db, "wrist.right", "Prawy nadgarstek")`. No code change was required — the swap described in the docs was already corrected in the current implementation.

**Files verified:**
- `src/OrthoSpineAI.Infrastructure/Persistence/DatabaseSeeder.cs` — wrist key labels are correct

---

### #17 🟢 `MedTestDefinitionKey` Soft FK

**Documentation reference:** `Implementation_Guide.md` §11 — Known Issues

**What the docs require:**  
`MedTest.MedTestDefinitionKey` is currently a string with no real database foreign key constraint. The docs recommend adding a proper FK in new implementations.

**✅ Fixed:** `MedTestConfiguration` uses the EF Core Fluent API to declare a proper referential constraint:
```csharp
b.HasOne(t => t.MedTestDefinition)
    .WithMany()
    .HasForeignKey(t => t.MedTestDefinitionKey)
    .HasPrincipalKey(d => d.Key)
    .OnDelete(DeleteBehavior.Restrict);
```
This generates a real database-level FK from `MedTests.MedTestDefinitionKey` to `MedTestDefinitions.Key`. No code change was required — the constraint was already present.

**Files verified:**
- `src/OrthoSpineAI.Infrastructure/Persistence/Configurations/MedTestConfiguration.cs` — real FK already configured

---

### #18 🟢 No `SortOrder` Column on `MedTestStage`

**Documentation reference:** `Implementation_Guide.md` §10 — Known Issues; §5

**What the docs require:**  
Stage ordering currently depends on auto-increment PK insert order. The docs recommend adding an explicit `SortOrder` integer column for deterministic ordering.

**✅ Fixed:** `MedTestStage` already carries `public int SortOrder { get; set; }` as its second property (after the PK). EF Core maps it to the database column by convention. The `DatabaseSeeder` helper method `Stage(...)` assigns `SortOrder` based on the zero-based insertion index within each definition block, giving fully deterministic ordering independent of PK auto-increment. No code change was required.

**Files verified:**
- `src/OrthoSpineAI.Domain/Entities/MedTestStage.cs` — `SortOrder` property present
- `src/OrthoSpineAI.Infrastructure/Persistence/DatabaseSeeder.cs` — `SortOrder` populated per stage via insertion index

---

### #19 🟢 Report / Print / PDF Pipeline Not Implemented

**Documentation reference:** `Implementation_Guide.md` Appendix E — Report & Diagnostic Pipeline

**What the docs require:**  
After a survey, a `DiagnosticForm` in-memory aggregate should be constructed from `MedTestResult` rows:
```
DiagnosticForm
  └── ParametersGroups[]
        └── IParametersGroup
              └── Parameters[]
```
This is then rendered into a printable/exportable report for the clinician. The reference implementation includes a chart view and printed report.

**✅ Fixed:** The full diagnostic report pipeline has been implemented end-to-end:

**Domain layer — new report aggregate types:**
- `src/OrthoSpineAI.Domain/Reports/DiagnosticForm.cs` — sealed aggregate containing session metadata (patient, date, survey, notes, anthropometrics), AWWS outcome (PilsVariant, PilsControlKey, Conclusion, ControlRecommendation) and an ordered `ParametersGroups` list.
- `src/OrthoSpineAI.Domain/Reports/IParametersGroup.cs` — interface for a named parameter group with `GroupName`, `DisplayLabel`, `IsActive`, and `Parameters`.
- `src/OrthoSpineAI.Domain/Reports/ParametersGroup.cs` — concrete sealed implementation of `IParametersGroup`.
- `src/OrthoSpineAI.Domain/Reports/ParameterEntry.cs` — record holding `Key`, `Label`, and formatted `Value` string for one diagnostic parameter.

**Application layer — `BuildDiagnosticFormAsync`:**
- `IMedTestService` extended with `Task<DiagnosticForm?> BuildDiagnosticFormAsync(int medTestId, int patientAgeYears, CancellationToken ct)`.
- `MedTestService.BuildDiagnosticFormAsync()` loads the persisted `MedTest` and `AwwsResult`, deserialises `GroupResultsJson`, and builds all 7 `ParametersGroup` instances matching the PG-Logic structure:
  - `PGLogicAnthropometric` — Age, Height, Weight
  - `PGLogicAtr` — ATR_max (from continuous Roll), HS (Hump Score)
  - `PGLogicBeightonScaleNumeric` — Beighton score
  - `PGLogicFLLD` — FLLD_POSITIVE/NEGATIVE (from TestPP)
  - `PGLogicLegsStatics` — LEGSSTAT_DISTURBED, KneeValgus, TarsalValgus
  - `PGLogicLLTHK` — LL, THK (from MedTestResult by OrtMeas)
  - `PGLogicPT` — PT (from MedTestResult by OrtMeas)
- Returns `null` when either the `MedTest` or its `AwwsResult` cannot be found.

**UI — Export report command:**
- `AwwsResultViewModel` constructor accepts an optional `IMedTestService` parameter.
- New `ExportReportAsync` relay command calls `BuildDiagnosticFormAsync`, formats a structured Polish-language plain-text report (header, session metadata, PiLS outcome, all 7 parameter groups with active/inactive markers and formatted values) and saves it as `Raport_{LastName}_{yyyyMMdd_HHmm}.txt` on the Desktop. A success `MessageBox` confirms the filename. If the form cannot be built a warning is shown instead.
- `ShellViewModel.NavigateToAwwsResult` now passes `_medTestService` to `AwwsResultViewModel`.
- `AwwsResultView.xaml` — new "📄 Eksportuj raport" green button added to the footer button row, bound to `ExportReportCommand`.

**Tests — 9 new unit tests in `MedTestServiceTests.cs`:**
- `BuildDiagnosticFormAsync_ReturnsNull_WhenTestNotFound`
- `BuildDiagnosticFormAsync_ReturnsNull_WhenAwwsResultNotFound`
- `BuildDiagnosticFormAsync_PopulatesSessionMetadata`
- `BuildDiagnosticFormAsync_PopulatesAwwsOutcome`
- `BuildDiagnosticFormAsync_ContainsAllSevenParameterGroups`
- `BuildDiagnosticFormAsync_AnthropometricGroup_ContainsAgeHeightWeight`
- `BuildDiagnosticFormAsync_FlldGroup_ReflectsTestPP`
- `BuildDiagnosticFormAsync_LegsStaticsGroup_ReflectsValgus`
- `BuildDiagnosticFormAsync_GroupActiveState_MatchesStoredGroupResults`

**PDF export (QuestPDF):**  
A structured A4 PDF export was subsequently added on top of the plain-text pipeline:
- `QuestPDF 2026.5.0` added to `OrthoSpineAI.UI.csproj` (Community licence).
- `src/OrthoSpineAI.UI/Reports/DiagnosticReportDocument.cs` — new `IDocument` implementation rendering header, patient card, PiLS badge, clinical text blocks, and all 7 diagnostic parameter groups.
- `AwwsResultViewModel.ExportPdfAsync` / `ExportPdfCommand` — calls `BuildDiagnosticFormAsync`, instantiates `DiagnosticReportDocument`, generates `Raport_{LastName}_{yyyyMMdd_HHmm}.pdf` on the Desktop, and offers to open it immediately.
- `AwwsResultView.xaml` — "🖨️ Eksportuj PDF" button added alongside the existing TXT export button.

**Files changed:**
- `src/OrthoSpineAI.Domain/Reports/DiagnosticForm.cs` — new aggregate
- `src/OrthoSpineAI.Domain/Reports/IParametersGroup.cs` — new interface
- `src/OrthoSpineAI.Domain/Reports/ParametersGroup.cs` — new concrete group
- `src/OrthoSpineAI.Domain/Reports/ParameterEntry.cs` — new parameter record
- `src/OrthoSpineAI.Application/Interfaces/IMedTestService.cs` — added `BuildDiagnosticFormAsync`
- `src/OrthoSpineAI.Application/Services/MedTestService.cs` — implemented `BuildDiagnosticFormAsync`
- `src/OrthoSpineAI.UI/OrthoSpineAI.UI.csproj` — added `QuestPDF 2026.5.0` package reference
- `src/OrthoSpineAI.UI/Reports/DiagnosticReportDocument.cs` — new QuestPDF A4 document renderer
- `src/OrthoSpineAI.UI/ViewModels/AwwsResultViewModel.cs` — added `ExportReportAsync` (TXT) and `ExportPdfAsync` (PDF) commands
- `src/OrthoSpineAI.UI/ViewModels/ShellViewModel.cs` — passes `_medTestService` to `AwwsResultViewModel`
- `src/OrthoSpineAI.UI/Views/AwwsResultView.xaml` — added "📄 Eksportuj TXT" and "🖨️ Eksportuj PDF" buttons
- `tests/OrthoSpineAI.Tests/Application/MedTestServiceTests.cs` — 9 new tests for `BuildDiagnosticFormAsync`

---

## Recommended Implementation Order

```
Phase 1 — Fix AWWS correctness (gaps #2, #3, #4, #5, #7, #8)
  ├── #3  Skip MEAS_NULL stages (no capture, auto-advance)
  ├── #2  Honour OrtNextStepButton (BTN_SAMPLE vs BTN_NEXT)
  ├── #4  Wire OrtContinousMeas → SaveContinuousFrameAsync in OnFrameReceived
  ├── #5  Compute ATR_max from continuous rows in FinishTestAsync
  ├── #7  Add LEGSSTAT_DISTURBED / LEGSSTAT_CORRECT to parameter dict
  └── #8  Audit and fix OrtMeas enum int alignment

Phase 2 — Complete survey data model (gaps #1, #6, #12)
  ├── #1  Side selection step for bilateral surveys
  ├── #6  Add HS field to PreTest and MedTest
  └── #12 Expose MedTest.Description in PreTest form

Phase 3 — Survey flow completeness (gaps #9, #10, #11, #13)
  ├── #9  Sub-test hierarchy sequential navigation
  ├── #10 Summary stage results-display mode
  ├── #11 ISOM reference range display in SurveyRunView
  └── #13 Vertebra highlight for Adams stages

Phase 4 — Hardware (gaps #14, #15)
  ├── #14 cdortometr.dll P/Invoke driver + BLE discovery
  └── #15 OrtResetFlag simulation / real zeroing

Phase 5 — Schema & polish (gaps #16, #17, #18, #19)
  ├── #16 Fix wrist key swap in seeder
  ├── #17 Real FK on MedTestDefinitionKey
  ├── #18 SortOrder column on MedTestStage
  └── #19 DiagnosticForm / report pipeline
```
