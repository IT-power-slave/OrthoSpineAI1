# ORT100 — Implementation Guide for Medical Hardware-Integrated Applications

> **Audience:** Agents and developers building equivalent software for other platforms (mobile, web, cross-platform desktop) or languages. This document distils the authoritative architectural knowledge from the reference .NET/WPF implementation.

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Domain Data Model](#2-domain-data-model)
3. [Enumerations Reference](#3-enumerations-reference)
4. [Hardware (ORT100 Device) Protocol](#4-hardware-ort100-device-protocol)
5. [Survey Workflow Architecture](#5-survey-workflow-architecture)
6. [Stage Execution Rules](#6-stage-execution-rules)
7. [Result Storage Model](#7-result-storage-model)
8. [AWWS / PiLS Diagnostic Algorithm](#8-awws--pils-diagnostic-algorithm)
9. [User & Security Model](#9-user--security-model)
10. [Database Seeding Conventions](#10-database-seeding-conventions)
11. [Known Issues & Design Warnings](#11-known-issues--design-warnings)

---

## 1. System Overview

ORT100 is a clinical measurement application for orthopedic spine and joint assessment. It connects via **Bluetooth** to a physical device called the **orthometr (ORT100)** — an inclinometer/distance-measuring probe.

The application:
- Guides a clinician through a sequence of measurement **stages** for each body-part survey.
- Collects angle (°) and distance (mm) readings from the device per stage.
- Stores discrete results (`MedTestResult`) and streaming telemetry (`MedTestContinuousResult`).
- Runs the **AWWS/PiLS** posture-classification algorithm on collected data.

```
Clinician                  Application                   ORT100 Device
    │                           │                              │
    ├──select patient────────►  │                              │
    ├──choose survey─────────►  │                              │
    │                    load stages from DB                   │
    │                           ├──set OrtMode ──────────────► │
    │                           ├──set OrtResetFlag ─────────► │
    │                           │ ◄── streaming telemetry ───  │
    ├──press device button────► │                              │
    │                    save measurement                      │
    │                    advance to next stage                 │
    ├── ... repeat per stage ... ────────────────────────────  │
    │                    save MedTest + Results to DB          │
    │                    run AWWS/PiLS                         │
```

---

## 2. Domain Data Model

### Entity Relationships

```
Clinic ──< Patient ──< MedTest ──< MedTestResult
                                └──< MedTestContinuousResult
                                └──  DiagnosticForm  [NOT persisted, transient]

MedTestDefinition ──< MedTestStage
MedTest.MedTestDefinitionKey  ─── (soft FK to MedTestDefinition.Key)

MedTest >── SystemUser
```

### Entities

#### `Clinic`
| Column | Type | Notes |
|--------|------|-------|
| `ClinicId` | int PK | Seeded; value 1 = "Ośrodek Rehabilitacji Leczniczej 'Troniny'" |
| `Name` | string | |

#### `Patient`
| Column | Type | Notes |
|--------|------|-------|
| `PatientId` | int PK | |
| `FirstName` | string | |
| `LastName` | string | |
| `PESEL` | string | Polish national ID (11 digits) |
| `Sex` | enum `PatientSex` | |
| `BirthDate` | DateTime | |
| `AddressSt` | string | Street |
| `AddressCity` | string | |
| `ZipCode` | string | |
| `ClinicId` | int FK → Clinic | |

#### `SystemUser`
| Column | Type | Notes |
|--------|------|-------|
| `SystemUserId` | int PK | |
| `Login` | string | Unique per migration guard |
| `PasswordHash` | string | Bcrypt hash (see §9) |

#### `MedTest`  *(one exam session)*
| Column | Type | Notes |
|--------|------|-------|
| `MedTestId` | int PK | |
| `ExaminationDate` | DateTime | |
| `Description` | string | Free text notes |
| `MedTestDefinitionKey` | string | Soft FK to `MedTestDefinition.Key` |
| `Weight` | double | kg |
| `Growth` | double | cm (note: column named `Growth`, not `Height`) |
| `Beighton` | int | Score 0–9 (joint hypermobility) |
| `TestPP` | bool | Derbolowski palm-press test |
| `KneeValgus` | bool | |
| `TarsalValgus` | bool | |
| `GaitDisturbance` | bool | |
| `PatientId` | int FK → Patient | |
| `SystemUserId` | int FK → SystemUser | |
| `DiagnosticForm` | *transient* | **NOT mapped to DB** (`[NotMapped]`) |

> ⚠️ `DiagnosticForm` is computed in-memory from results; it must not be stored in the DB or serialised as a persistent field.

#### `MedTestResult`  *(one discrete measurement per stage)*
| Column | Type | Notes |
|--------|------|-------|
| `MedTestResultId` | int PK | |
| `Plane` | enum `MedTestPlane` | Body plane of movement |
| `OrtMeas` | enum `ORT100Measurement` | Which measurement was taken |
| `PhysicalValue` | double | Measured value |
| `PhysicalUnit` | string | e.g. `"°"` or `"mm"` |
| `Side` | enum `MedTestSide` | Left / Right / None |
| `MedTestId` | int FK → MedTest | |

#### `MedTestContinuousResult`  *(streaming telemetry — Adams test only)*
| Column | Type | Notes |
|--------|------|-------|
| `MedTestContinuousResultId` | int PK | |
| `Status` | int | Device status word (mode, buttons, sensor state) |
| `Signal` | int | Bluetooth signal [dB] |
| `Battery` | double | Battery voltage [V] |
| `Shake` | double | Acceleration on device [g] |
| `Roll` | double | Primary angle left/right [°] |
| `RollOffset` | double | Calibration offset on Roll [°] |
| `Tilt` | double | Auxiliary angle front/back [°] |
| `Way` | int | Distance measured by odometer wheel [mm] |
| `Space` | int | Leg spread of device [mm] |
| `Force1` | double | Pressure sensor 1 [N] |
| `Force2` | double | Pressure sensor 2 [N] |
| `OrtMeas` | enum | Which measurement this frame corresponds to |
| `MedTestId` | int FK → MedTest | |

#### `MedTestDefinition`  *(survey template)*
| Column | Type | Notes |
|--------|------|-------|
| `MedTestDefinitionId` | int PK | |
| `Key` | string | Unique text identifier (e.g. `"backbone.1"`) |
| `Name` | string | Human-readable Polish name |

#### `MedTestStage`  *(one step inside a survey template)*

See §5 for full field semantics. All fields described in `MedTestStage.cs`.

---

## 3. Enumerations Reference

### `ORT100Measurement` — What is being measured

| Value | Description (PL) | Notes |
|-------|-----------------|-------|
| `MEAS_NULL` | Undefined | Informational/transition stage, no measurement |
| `MEAS_NM` | Nachylenie miednicy | Pelvic tilt |
| `MEAS_LL` | Lordoza lędźwiowa | Lumbar lordosis |
| `MEAS_KW` | Kifoza wstępująca | Rising thoracic kyphosis |
| `MEAS_KZ` | Kifoza zstępująca | Descending thoracic kyphosis |
| `MEAS_KP` | Kifoza piersiowa | Total thoracic kyphosis (KW + KZ) |
| `MEAS_PC7`…`MEAS_PSIPS` | C7…S1 standing | Adams test calibration I: spine landmarks, standing |
| `MEAS_SC7`…`MEAS_SSIPS` | C7…S1 bending | Adams test calibration II: landmarks, forward bend |
| `MEAS_AC7`…`MEAS_ASIPS` | C7…S1 Adams | Adams test: ATR (Angle of Trunk Rotation) at each landmark |
| `MEAS_EXTENSION` | Wyprost | Extension |
| `MEAS_FLEXION` | Zgięcie | Flexion |
| `MEAS_ABDUCTION` | Odwodzenie | Abduction |
| `MEAS_ADDUCTION` | Przywodzenie | Adduction |
| `MEAS_INTERNAL_ROTATION` | Rotacja wewnętrzna | Internal rotation |
| `MEAS_EXTERNAL_ROTATION` | Rotacja zewnętrzna | External rotation |

### `ORT100Mode` — Device operating mode

| Value | Meaning |
|-------|---------|
| `MODE_MANUAL` | Free manual mode |
| `MODE_SEQ_A1`…`A4` | Angle measurement, sequential steps |
| `MODE_SEQ_LS1`…`LS5` | Distance measurement, standing (calibration I) |
| `MODE_SEQ_LB1`…`LB5` | Distance measurement, bending (calibration II) |
| `MODE_SEQ_AD1`…`AD5` | Adams test angle measurement (continuous) |
| `MODE_SEQ_BT_SANGEL` | Bluetooth: signed angle |
| `MODE_SEQ_BT_UANGEL` | Bluetooth: unsigned angle |
| `MODE_SEQ_BT_WAY` | Bluetooth: distance |
| `MODE_SEQ_BT_ADAMS` | Bluetooth: signed angle + distance + tilt/speed warnings |
| `MODE_SEQ_END` | End of sequence sentinel |

### `ORT100Button` — Which device button triggers stage advance

| Value | Meaning |
|-------|---------|
| `BTN_NEXT` | "NASTĘPNY" — advance without saving measurement |
| `BTN_SAMPLE` | "POMIAR" — save current reading and advance |
| `BTN_RESET` | "ZEROWANIE" — reset device reference and advance |

### `ORT100ResetFlag` — How device is zeroed on entering a stage

| Value | Meaning |
|-------|---------|
| `NONE` | No reset |
| `ZERO_ANGLE` | Zero angle to current position |
| `ZERO_ANGLE_DEF` | Zero angle to device default (gravity reference) |
| `ZERO_WAY` | Zero distance only |
| `ZERO_WAY_ANGLE` | Zero both, angle to current position |
| `ZERO_WAY_ANGLE_DEF` | Zero both, angle to gravity default |

### `MedTestPlane` — Anatomical plane

| Value | Description |
|-------|-------------|
| `SAGGITTAL_PLANE` | Płaszczyzna strzałkowa |
| `FRONTAL_PLANE` | Płaszczyzna czołowa |
| `TRANSVERSE_PLANE` | Płaszczyzna poprzeczna (Adams test) |
| `ROTATION_PLANE_0` | Rotation plane at 0° (adduction) |
| `ROTATION_PLANE_90` | Rotation plane at 90° (abduction) |

### `MedTestSide`

| Value | Meaning |
|-------|---------|
| `SIDE_NONE` | No side (bilateral / spine) |
| `SIDE_LEFT` | Left side |
| `SIDE_RIGHT` | Right side |

### `ORT100ControlState` — UI highlight state (Adams test vertebra selector)

`HIGHLIGHT_ALL`, `HIGHLIGHT_C7`, `HIGHLIGHT_TH6`, `HIGHLIGHT_TH12`, `HIGHLIGHT_L3`, `HIGHLIGHT_S1`, `HIGHLIGHT_NONE`

### `ORT100Control` — Which UI component to render

Each stage carries two control references:
- **`TipControl`** — illustration/instruction panel shown to the left (how to place device)
- **`MainSurveyControl`** — the live measurement display

Key values:
| Value | Role |
|-------|------|
| `NONE` | No UI component |
| `ORTHOMETRUSERCONTROL_EDGE_A/B/C` | Orthometr placement instruction (edge A, B, or C) |
| `BONDBENDUSERCONTROL_*` | Patient position/posture illustration |
| `MEASUREMENTSDISPLAYCONTROL` | Live numeric display of current measurement |
| `CHARTUSERCONTROL` | Results chart |
| `ANGLEUSERCONTROL` | Angle gauge display |
| `LONGNESSUSERCONTROL` | Distance/length display |

The `BONDBENDUSERCONTROL_*` naming encodes body segment and plane:
- `SKPS` = Staw Kolanowy Płaszczyzna Strzałkowa (Knee, Sagittal)
- `SSGPS` = Staw Skokowo-Goleniowy Płaszczyzna Strzałkowa (Ankle, Sagittal)
- `SSGRR` = Staw Skokowo-Goleniowy Ruchy Rotacyjne (Ankle, Rotation)
- `SPNPS*` = Staw Promieniowo-Nadgarstkowy Płaszczyzna Strzałkowa (Wrist, Sagittal)
- `SPNPC*` = Staw Promieniowo-Nadgarstkowy Płaszczyzna Czołowa (Wrist, Frontal)
- `TA*` = Test Adamsa (Adams test)
- Suffixes: `ps` = pozycja siedząca (sitting), `pst` = pozycja stojąca (standing)

---

## 4. Hardware (ORT100 Device) Protocol

### Physical Device

The orthometr ORT100 is a handheld spine/joint measurement device with:
- **3 edges (A, B, C)** — different contact surfaces for different body locations
- **Inclinometer** — measures angle (Roll, Tilt)
- **Odometer wheel** — measures distance traveled along the body (Way)
- **Pressure sensors** (Force1, Force2)
- **3 buttons**: NEXT (`BTN_NEXT`), SAMPLE (`BTN_SAMPLE`), RESET (`BTN_RESET`)
- **Bluetooth** connectivity

### Communication

The device streams telemetry frames via Bluetooth. Each frame contains all sensor values (see `MedTestContinuousResult` fields). The app receives frames continuously and:

1. **Displays** live values in the active `MainSurveyControl`.
2. **Warns** when `Tilt` or speed exceed safe limits (in Adams mode).
3. **Records** a single frame as `MedTestResult` when `BTN_SAMPLE` is pressed.
4. **Records all frames** as `MedTestContinuousResult` during continuous stages (Adams test).

### Stage Lifecycle

```
On stage entry:
  1. Send OrtMode to device  → configures what device measures/reports
  2. Apply OrtResetFlag      → zeros angle and/or distance reference
  3. Display TipControl      → show clinician how to position device
  4. Start streaming display → show MainSurveyControl with live values

While in stage:
  - Device streams telemetry frames continuously
  - If OrtContinousMeas = true: save every frame to MedTestContinuousResult
  - UI shows current Roll (angle) or Way (distance) live

On OrtNextStepButton press:
  - If BTN_SAMPLE: capture current frame → save as MedTestResult (PhysicalValue, OrtMeas, Plane, Side)
  - If BTN_RESET:  zero device per OrtResetFlag, stay in stage (clinician repositions)
  - If BTN_NEXT:   advance without recording
  - Advance to next stage
```

### ISOM Reference Values

Each stage can carry normative reference values for range of motion:
- `ValueISOM1` — reference for extension-direction movements (from body, left lean, external rotation)
- `ValueISOM2` — always 0 (neutral starting position)  
- `ValueISOM3` — reference for flexion-direction movements (to body, right lean, internal rotation)

These are seeded per stage and displayed as expected ranges during measurement.

---

## 5. Survey Workflow Architecture

### Key Principle: Data-Driven Surveys

Surveys are **not hardcoded**. They are built from `MedTestDefinition` + `MedTestStage` records in the database. Any implementation must:
1. Load definitions and stages from DB at startup (or cache them).
2. Present stages in DB order (ordered by `MedTestStageId`).
3. Use the stage fields to configure hardware and UI.

### Survey Keys Structure

Surveys follow a hierarchical key naming convention:

```
{body-part}              Root definition (preparation stage)
{body-part}.{n}          Sub-test definition (e.g. sagittal plane measurement)
{body-part}.{n}.summary  Summary/results display
{body-part}.summary      Final summary across all sub-tests
```

**All defined keys (as seeded):**

| Key | Survey Name |
|-----|-------------|
| `backbone` | Ocena postawy ciała |
| `backbone.1` | Ocena płaszczyzny strzałkowej |
| `backbone.2` | Pomiar symetrii pleców w Teście Adamsa |
| `backbone.summary` | Podsumowanie oceny postawy ciała |
| `spineFlexibility.1` | Badanie elastyczności kręgosłupa |
| `spineFlexibility.1.summary` | Podsumowanie elastyczności |
| `spineScreening.1` | Badanie przesiewowe — płaszczyzna strzałkowa |
| `spineScreening.2` | Badanie przesiewowe — test Adamsa |
| `spineScreening.1.summary` | Podsumowanie badania przesiewowego |
| `shoulder` | Pomiary obrączy barkowej (both sides share one key) |
| `shoulder.summary` | Podsumowanie barku |
| `elbow` | Pomiary stawu łokciowego |
| `elbow.summary` | Podsumowanie łokcia |
| `hip` | Pomiary stawu biodrowego |
| `hip.summary` | Podsumowanie biodra |
| `knee` | Pomiary stawu kolanowego |
| `knee.summary` | Podsumowanie kolana |
| `wrist` | Pomiary stawu promieniowo-nadgarstkowego |
| `wrist.summary` | Podsumowanie nadgarstka |
| `ankle` | Pomiary stawu skokowo-goleniowego |
| `ankle.summary` | Podsumowanie skoku |

### Side Handling

Bilateral surveys (shoulder, elbow, hip, knee, wrist, ankle) share a **single DB key** (no `.left` / `.right` suffix). The **side** (`MedTestSide`) is selected in application logic before the survey starts and is:
- Attached to each `MedTestResult` record.
- Used by the UI to label which side is being measured.
- **Not** encoded in `MedTestDefinition.Key`.

> ⚠️ **Bug in reference implementation:** In `Survey.surveyKeys`, `wrist.left` and `wrist.right` descriptions are swapped. Correct mapping: `wrist.left` → "Lewy nadgarstek", `wrist.right` → "Prawy nadgarstek".

### Survey Loading Pattern

```
1. Receive surveyKey (e.g. "backbone")
2. Query: SELECT * FROM MedTestDefinitions WHERE Key LIKE '{surveyKey}%' ORDER BY Key
3. For each definition: SELECT * FROM MedTestStages WHERE MedTestDefinitionId = ? ORDER BY MedTestStageId
4. Build in-memory list of Stage objects
5. Present stages sequentially
```

---

## 6. Stage Execution Rules

A `MedTestStage` record fully describes how to execute one step. An implementation must honour all fields:

| Field | Implementation Rule |
|-------|---------------------|
| `Name` | Display as step title |
| `Tip` | Display as instruction text for clinician |
| `TipControl` | Render the named illustration component |
| `MainSurveyControl` | Render the named live-measurement component |
| `Plane` | Tag results with anatomical plane |
| `OrtMeas` | Identifier for the measurement; tag result with this value |
| `OrtState` | Highlight specific vertebra in the spinal diagram (`ORT100ControlState`) |
| `OrtMode` | Send to device before stage starts |
| `OrtResetFlag` | Apply device reset before stage starts |
| `OrtNextStepButton` | Only this button press advances the stage |
| `OrtContinousMeas` | If `true`: store every telemetry frame as `MedTestContinuousResult` |
| `ValueISOM1` | Show as upper reference limit in UI |
| `ValueISOM3` | Show as lower reference limit in UI |

### Adams Test Specifics

The Adams test is the most complex flow:
1. **Calibration I** (standing): sweep device from C7 to S1, recording distances at 5 landmarks (`MEAS_PC7`…`MEAS_PSIPS`). Mode: `MODE_SEQ_LS1`…`LS5`.
2. **Calibration II** (forward bend): repeat sweep to calibrate path in bent position (`MEAS_SC7`…`MEAS_SSIPS`). Mode: `MODE_SEQ_LB1`…`LB5`.
3. **Test** (Adams): sweep again, device emits ATR angles at each landmark (`MEAS_AC7`…`MEAS_ASIPS`). Mode: `MODE_SEQ_AD1`…`AD5`. `OrtContinousMeas = true` — all frames saved.

The device uses the two calibration sweeps to map path-distance to a landmark, then automatically emits the angle at the correct point.

---

## 7. Result Storage Model

### Discrete Results (`MedTestResult`)

One row per measurement stage that used `BTN_SAMPLE`. Mandatory fields to populate:

```json
{
  "MedTestId": 42,
  "Plane": "SAGGITTAL_PLANE",
  "OrtMeas": "MEAS_FLEXION",
  "PhysicalValue": 85.5,
  "PhysicalUnit": "°",
  "Side": "SIDE_RIGHT"
}
```

### Continuous Results (`MedTestContinuousResult`)

One row per Bluetooth frame during Adams test stages. All sensor fields must be saved:

```json
{
  "MedTestId": 42,
  "OrtMeas": "MEAS_AC7",
  "Status": 0,
  "Signal": -65,
  "Battery": 3.85,
  "Shake": 0.02,
  "Roll": 3.5,
  "RollOffset": 0.0,
  "Tilt": 1.2,
  "Way": 0,
  "Space": 120,
  "Force1": 0.0,
  "Force2": 0.0
}
```

### MedTest Header

Before starting any survey, create a `MedTest` record with:
- `PatientId`, `SystemUserId`
- `ExaminationDate` = now
- `MedTestDefinitionKey` = root survey key (e.g. `"backbone"`)
- `Weight`, `Growth` (from patient or entered by clinician)
- Clinical flags: `Beighton`, `TestPP`, `KneeValgus`, `TarsalValgus`, `GaitDisturbance`

---

## 8. AWWS / PiLS Diagnostic Algorithm

See [`docs/AWWS_Algorithm.md`](AWWS_Algorithm.md) for full documentation.

### Summary

The algorithm uses collected survey results to classify a patient's posture pattern and generate a recommendation. Inputs:

| Parameter | Source |
|-----------|--------|
| ATR values (C7, T6, T12, L3, S1) | Adams test `MedTestContinuousResult` |
| Trunk symmetry (HS) | Derived from ATR data |
| FLLD | Spine length difference standing vs. bent |
| LL | Lumbar lordosis angle (`MEAS_LL`) |
| THK | Thoracic kyphosis angle (`MEAS_KP`) |
| PT | Pelvic tilt angle (`MEAS_NM`) |
| Beighton score | `MedTest.Beighton` |
| Derbolowski test | `MedTest.TestPP` |
| Age | Computed from `Patient.BirthDate` |
| Height, Weight | `MedTest.Growth`, `MedTest.Weight` |

### PiLS Decision Logic (simplified)

```
if ATR any region > threshold → classify as scoliosis risk
elif HS asymmetry → classify as functional asymmetry
elif FLLD > threshold → classify as stiffness pattern
else → evaluate LL, THK, PT for sagittal balance classification
```

The exact thresholds are defined in the `PGLogic*` classes. See `AWWS_Algorithm.md` §Decision Tree for details.

---

## 9. User & Security Model

### Authentication

- Single table `SystemUser` with `Login` + `PasswordHash`.
- **Password hashing**: Bcrypt via `OrthoSpine.Security.Core.PasswordHasher.Hash(plain)`.
- Default seeded account: login `admin`, password `changeme` — **must be changed in production**.

### Seeding Pattern (prevents duplicate users)

```
1. RemoveDuplicateUsers() — ensure at most one row per Login (call SaveChanges after)
2. PreDefineUsers()       — AddOrUpdate by Login, only if user does not exist
```

> ⚠️ Do NOT reset the password hash on every migration run. Check existence first.

### Session / Authorization

The reference implementation stores `SystemUserId` directly on `MedTest`. For web/mobile implementations, use standard JWT/OAuth2 with the user ID injected from the authenticated session.

---

## 10. Database Seeding Conventions

### Idempotency Guard

Every survey group is guarded by a sentinel key check before inserting:

```csharp
if (!context.MedTestDefinitions.Any(d => d.Key == "backbone.summary"))
{
    // Insert backbone group
}
```

**Rule:** The sentinel key is always the `.summary` definition of the group. Do not re-insert a group if its summary key already exists.

### Insertion Order

For each survey group:
1. Create root `MedTestDefinition` (e.g. `backbone`)
2. Add root stage (preparation)
3. Create sub-test definitions and their stages in measurement order
4. Call `Podsumowanie(...)` to create the `.summary` definition

### Stage Ordering

Stages are linked to definitions by FK (`MedTestDefinitionId`). Order within a definition is determined by `MedTestStageId` (auto-increment insert order). **There is no explicit sort column** — insertion order equals presentation order.

> ⚠️ In new implementations, add an explicit `SortOrder` integer column to avoid dependency on insert order.

### Required Seed Data

| Entity | Required records |
|--------|-----------------|
| `Clinic` | At least 1 (ClinicId = 1) |
| `SystemUser` | At least 1 admin account |
| `MedTestDefinition` | All 21 survey keys listed in §5 |
| `MedTestStage` | All stages for each definition |

---

## 11. Known Issues & Design Warnings

| Issue | Description | Recommendation |
|-------|-------------|----------------|
| `MedTest.Growth` naming | Column is named `Growth`, not `Height`. Confusing for API consumers. | Expose as `height_cm` in APIs |
| `DiagnosticForm` not persisted | `[NotMapped]` — computed each time from results. | Re-compute on load, or persist separately |
| No explicit stage sort order | Stage ordering depends on DB insert order (auto-increment PK). | Add `SortOrder` column |
| Wrist surveyKey swap | `wrist.left` / `wrist.right` descriptions are swapped in `Survey.surveyKeys`. | Fix in any re-implementation |
| `AutomaticMigrationDataLossAllowed = true` | EF6 migration config allows destructive schema changes. | Use versioned migrations only |
| Plaintext-style seeding | Earlier migrations may have used plaintext passwords. | Audit and re-hash on upgrade |
| `MedTestDefinitionKey` soft FK | `MedTest.MedTestDefinitionKey` is a string, not a real FK. | Add a proper FK in new schemas |
| No side column on `MedTest` | Side is stored per-result, not on the test header. | For queries, derive side from `MedTestResult.Side` |

---

## Appendix A: Survey Stage Fields Quick Reference

```
MedTestStage {
  MedTestStageId   : int        – PK, determines order
  Name             : string     – step title
  Tip              : string     – full instruction for clinician
  TipControl       : enum       – illustration panel to show
  MainSurveyControl: enum       – live measurement display
  BodyPlaneName    : string     – free-text plane label
  Plane            : enum       – anatomical plane (for result tagging)
  OrtMeas          : enum       – which measurement this stage captures
  OrtState         : enum       – vertebra highlight state
  OrtNextStepButton: enum       – device button that advances stage
  OrtMode          : enum       – device operating mode
  OrtResetFlag     : enum       – zeroing applied on stage entry
  OrtContinousMeas : bool       – true = stream all frames to ContinuousResult
  ValueISOM1       : double?    – normative reference, extension/away direction
  ValueISOM3       : double?    – normative reference, flexion/toward direction
  MedTestDefinitionId: int FK   – parent definition
}
```

## Appendix B: Survey Group → DB Keys Mapping

| Logical Survey | DB Key(s) |
|---------------|-----------|
| Backbone / Posture | `backbone`, `backbone.1`, `backbone.2`, `backbone.summary` |
| Spine Screening | `spineScreening.1`, `spineScreening.2`, `spineScreening.1.summary` |
| Spine Flexibility | `spineFlexibility.1`, `spineFlexibility.1.summary` |
| Shoulder | `shoulder`, `shoulder.summary` |
| Elbow | `elbow`, `elbow.summary` |
| Hip | `hip`, `hip.summary` |
| Knee | `knee`, `knee.summary` |
| Wrist | `wrist`, `wrist.summary` |
| Ankle | `ankle`, `ankle.summary` |

## Appendix C: OrtMode Integer → Enum Mapping

The `MedTestStage.OrtMode` column stores the mode as an integer. The full mapping:

| int | ORT100Mode enum | Context |
|-----|----------------|---------|
| 0 | `MODE_MANUAL` / `MODE_SEQ_BT_UANGEL` | General angle measurement (unsigned) |
| 1 | `MODE_SEQ_A1` | Sagittal — NM (pelvic tilt) |
| 2 | `MODE_SEQ_A2` | Sagittal — LL (lumbar lordosis) |
| 3 | `MODE_SEQ_A3` | Sagittal — KW (thoracic kyphosis ascending) |
| 4 | `MODE_SEQ_A4` | Sagittal — KZ/KP (thoracic kyphosis descending) |
| 5 | `MODE_SEQ_LS1` | Adams calibration I — standing at C7 |
| 6 | `MODE_SEQ_LS2` | Adams calibration I — standing at T6 |
| 7 | `MODE_SEQ_LS3` | Adams calibration I — standing at T12 |
| 8 | `MODE_SEQ_LS4` | Adams calibration I — standing at L3 |
| 9 | `MODE_SEQ_LS5` | Adams calibration I — standing at S1 |
| 10 | `MODE_SEQ_LB1` | Adams calibration II — forward bend at C7 |
| 11 | `MODE_SEQ_LB2` | Adams calibration II — forward bend at T6 |
| 12 | `MODE_SEQ_LB3` | Adams calibration II — forward bend at T12 |
| 13 | `MODE_SEQ_LB4` | Adams calibration II — forward bend at L3 |
| 14 | `MODE_SEQ_LB5` | Adams calibration II — forward bend at S1 |
| 15 | `MODE_SEQ_AD1` | Adams test — ATR measurement at C7 |
| 16 | `MODE_SEQ_AD2` | Adams test — ATR measurement at T6 |
| 17 | `MODE_SEQ_AD3` | Adams test — ATR measurement at T12 |
| 18 | `MODE_SEQ_AD4` | Adams test — ATR measurement at L3 |
| 19 | `MODE_SEQ_AD5` | Adams test — ATR measurement at S1 |

> Special modes sent programmatically (not stored in stages):  
> `MODE_SEQ_BT_UANGEL` — startup default (absolute angle display)  
> `MODE_SEQ_END` — shutdown signal to device

---

## Appendix D: OrtMeas Integer → Enum Mapping

| int | ORT100Measurement | Clinical meaning |
|-----|------------------|-----------------|
| 0 | `NONE` | No measurement (preparation/summary stages) |
| 1 | `NM` | Nachylenie Miednicy (Pelvic Tilt) |
| 2 | `LL` | Lordoza Lędźwiowa (Lumbar Lordosis) |
| 3 | `KW` | Kifoza Wstępująca (ascending kyphosis segment) |
| 4 | `KZ` | Kifoza Zstępująca (descending kyphosis segment) |
| 5 | `KP` | Kifoza Piersiowa total (KW+KZ) |
| 6 | `PC7` | Path calibration standing — C7 |
| 7 | `PT6` | Path calibration standing — T6 |
| 8 | `PT12` | Path calibration standing — T12 |
| 9 | `PL3` | Path calibration standing — L3 |
| 10 | `PSIPS` | Path calibration standing — S1/SIPS |
| 11 | `SC7` | Path calibration bent — C7 |
| 12 | `ST6` | Path calibration bent — T6 |
| 13 | `ST12` | Path calibration bent — T12 |
| 14 | `SL3` | Path calibration bent — L3 |
| 15 | `SSIPS` | Path calibration bent — S1/SIPS |
| 16 | `AC7` | Adams test ATR — C7 |
| 17 | `AT6` | Adams test ATR — T6 |
| 18 | `AT12` | Adams test ATR — T12 |
| 19 | `AL3` | Adams test ATR — L3 |
| 20 | `ASIPS` | Adams test ATR — S1/SIPS |
| 21 | `EXTENSION` | Extension (joint measurement) |
| 22 | `FLEXION` | Flexion (joint measurement) |
| 23 | `ABDUCTION` | Abduction (joint measurement) |
| 24 | `ADDUCTION` | Adduction (joint measurement) |
| 25 | `INT_ROT` | Internal rotation (joint measurement) |
| 26 | `EXT_ROT` | External rotation (joint measurement) |

---

## Appendix E: Report & Diagnostic Pipeline

The reference implementation generates reports from `MedTestResult` records after a survey. This section describes the model structure for reimplementation.

### DiagnosticForm (in-memory aggregate)

`DiagnosticForm` is **NOT persisted** (`[NotMapped]` on `MedTest.DiagnosticForm`). It is computed fresh each time results are displayed or exported.

```
DiagnosticForm
  FormName     : string
  DisplayName  : string
  Description  : string
  ParametersGroups : List<IParametersGroup>
      └── IParametersGroup
            GroupName    : string          (e.g. "sagittal_plane")
            DisplayName  : string
            Description  : string
            GroupType    : Type            (concrete class type)
            Parameters   : List<IParameter>
                └── IParameter
                      Name         : string
                      DisplayName  : string
                      Value        : object   (measured value)
                      Unit         : string
                      ReferenceMin : double?
                      ReferenceMax : double?
            CalculateResult(allParams) → Dictionary<GroupsEnum, bool>
```

### Report Generation Flow

```
1. Load MedTest + all MedTestResult rows
2. Build parameter dictionary:
       Dictionary<ParametersNamesEnum, object> params = {
           ATR  → max(|AC7|, |AT6|, |AT12|, |AL3|, |ASIPS|) from MedTestContinuousResult
           HS   → derived from ATR symmetry
           LL   → MedTestResult where OrtMeas = LL
           THK  → MedTestResult where OrtMeas = KP
           PT   → MedTestResult where OrtMeas = NM
           BEIGHTON → MedTest.Beighton
           FLLD_POSITIVE/NEGATIVE → derived from SC7-PC7 vs SL3-PL3 delta
           LEGSSTAT_* → derived from MedTest.KneeValgus + TarsalValgus
           AGE  → computed from Patient.BirthDate
           HEIGHT → MedTest.Growth
           WEIGHT → MedTest.Weight
       }
3. Create DiagnosticForm and add ParametersGroups
4. For each group: call group.CalculateResult(params) → Dictionary<GroupsEnum, bool>
5. Aggregate results across all groups
6. Run PiLS inference (see AWWS_Algorithm.md §5)
7. Generate awwsConclusion text + PiLS variant/control
8. Pass DiagnosticForm to IRaport.CreateRaport()
9. Render/export list of UI panels or document sections
```

### FLLD Derivation

FLLD (Functional Leg Length Discrepancy) is derived from calibration measurements:

```
delta_C7  = SC7  - PC7    (path difference C7: bent vs standing)
delta_L3  = SL3  - PL3    (path difference L3: bent vs standing)
delta_SIPS = SSIPS - PSIPS

if abs(delta_C7 - delta_L3) > threshold: FLLD_POSITIVE = true
else: FLLD_NEGATIVE = true
```

> Exact threshold is not hardcoded in the reference impl — it is determined by the clinical team. A starting value of **5 mm** is used in the reference UI.

### ATR_max Derivation

For PiLS inference:

```
atr_values = [AC7, AT6, AT12, AL3, ASIPS]  ← from MedTestResult or ContinuousResult
ATR_max = max( abs(v) for v in atr_values )
```

### IRaport Interface

```
IRaport {
  UserControls : List<UIComponent>   // platform-specific view panels
  CreateRaport() : void              // builds the UserControls list
}
```

For non-WPF implementations, replace `UserControls` with structured data (e.g. JSON sections, HTML blocks) and implement a platform-appropriate renderer.

---

## Appendix F: Minimal API Contract (for web/mobile)

A REST API serving this domain must provide at minimum:

```
GET  /surveys                          → list of survey definitions
GET  /surveys/{key}/stages             → ordered stages for a definition key
GET  /patients                         → patient list
POST /patients                         → create patient
GET  /patients/{id}/medtests           → test history
POST /patients/{id}/medtests           → start new MedTest
POST /medtests/{id}/results            → save discrete MedTestResult
POST /medtests/{id}/continuous-results → save streaming frame
GET  /medtests/{id}/diagnostic         → compute and return DiagnosticForm / AWWS
POST /auth/login                       → exchange Login+Password for JWT
```

Hardware communication (Bluetooth) must be implemented **on the client** (mobile app, desktop) — it cannot go through a server for real-time measurement. The server receives only the saved results.
