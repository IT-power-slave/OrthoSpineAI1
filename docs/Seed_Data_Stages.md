# ORT100 — Kompletne dane etapów badań (seed data)

> Źródło: live query do bazy `ORT100` na `(localdb)\MSSQLLocalDB`  
> Tabele: `MedTestDefinitions` JOIN `MedTestStages`  
> Format kolumn: `DefKey | StageId | StageName | Plane | OrtMeas | Button | Mode (int) | ResetFlag | ContinuousMeas | ISOM1 | ISOM3`

---

## Legenda

| Kolumna | Typ | Znaczenie |
|---------|-----|-----------|
| `DefKey` | string | `MedTestDefinition.Key` — klucz definicji badania |
| `StageId` | int | `MedTestStage.MedTestStageId` — PK, określa kolejność |
| `StageName` | string | Nazwa etapu (wyświetlana użytkownikowi) |
| `Plane` | enum | Płaszczyzna anatomiczna |
| `OrtMeas` | enum | Rodzaj pomiaru |
| `Button` | enum | Przycisk zatwierdzający etap |
| `Mode` | int | `ORT100Mode` jako int (tryb urządzenia) |
| `ResetFlag` | enum | Sposób zerowania urządzenia przy wejściu w etap |
| `Cont` | 0/1 | `OrtContinousMeas` — zapis ciągły do `MedTestContinuousResult` |
| `ISOM1` | double? | Wartość referencyjna — kierunek wyprostu |
| `ISOM3` | double? | Wartość referencyjna — kierunek zgięcia |

### Mapowanie OrtMode (int → enum)

| int | ORT100Mode |
|-----|-----------|
| 0 | `MODE_MANUAL` |
| 1 | `MODE_SEQ_A1` |
| 2 | `MODE_SEQ_A2` |
| 3 | `MODE_SEQ_A3` |
| 4 | `MODE_SEQ_A4` |
| 5 | `MODE_SEQ_LS1` (kalibracja I, C7 start) |
| 6 | `MODE_SEQ_LS2` |
| 7 | `MODE_SEQ_LS3` |
| 8 | `MODE_SEQ_LS4` |
| 9 | `MODE_SEQ_LS5` |
| 10 | `MODE_SEQ_LB1` (kalibracja II, bending) |
| 11 | `MODE_SEQ_LB2` |
| 12 | `MODE_SEQ_LB3` |
| 13 | `MODE_SEQ_LB4` |
| 14 | `MODE_SEQ_LB5` |
| 15 | `MODE_SEQ_AD1` (Adams test, C7) |
| 16 | `MODE_SEQ_AD2` |
| 17 | `MODE_SEQ_AD3` |
| 18 | `MODE_SEQ_AD4` |
| 19 | `MODE_SEQ_AD5` |

---

## backbone — Ocena postawy ciała (109 etapów łącznie)

### backbone (root — przygotowanie)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 1 | Przygotowanie do badania | SAGGITTAL | NULL | BTN_NEXT | 0 (MANUAL) | NONE | 0 | — | — |

### backbone.1 — Ocena płaszczyzny strzałkowej

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 2 | Ocena nachylenia miednicy | SAGGITTAL | NM | BTN_SAMPLE | 1 (A1) | ZERO_WAY_ANGLE_DEF | 0 | — | — |
| 3 | Ocena kąta lordozy lędźwiowej | SAGGITTAL | LL | BTN_SAMPLE | 2 (A2) | ZERO_ANGLE | 0 | — | — |
| 4 | Ocena kąta kifozy piersiowej wstępującej | SAGGITTAL | KW | BTN_SAMPLE | 3 (A3) | ZERO_ANGLE | 0 | — | — |
| 5 | Ocena kąta kifozy piersiowej (zstępująca) | SAGGITTAL | KZ | BTN_SAMPLE | 4 (A4) | ZERO_ANGLE | 0 | — | — |
| 6 | Ocena kąta kifozy piersiowej (KP=KW+KZ) | SAGGITTAL | KP | BTN_NEXT | 4 (A4) | NONE | 0 | — | — |

### backbone.2 — Test Adamsa

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 7 | Przygotowanie do badania | TRANSVERSE | NULL | BTN_NEXT | 5 (LS1) | ZERO_WAY_ANGLE_DEF | 0 | — | — |
| 8 | Kalibracja I — C7, pozycja stojąca | TRANSVERSE | PC7 | BTN_SAMPLE | 5 (LS1) | NONE | 0 | — | — |
| 9 | Kalibracja I — T6, pozycja stojąca | TRANSVERSE | PT6 | BTN_SAMPLE | 6 (LS2) | NONE | 0 | — | — |
| 10 | Kalibracja I — T12, pozycja stojąca | TRANSVERSE | PT12 | BTN_SAMPLE | 7 (LS3) | NONE | 0 | — | — |
| 11 | Kalibracja I — L3, pozycja stojąca | TRANSVERSE | PL3 | BTN_SAMPLE | 8 (LS4) | NONE | 0 | — | — |
| 12 | Kalibracja I — S1, pozycja stojąca | TRANSVERSE | PSIPS | BTN_SAMPLE | 9 (LS5) | NONE | 0 | — | — |
| 13 | Przejście do kalibracji II | TRANSVERSE | NULL | BTN_NEXT | 9 (LS5) | NONE | 0 | — | — |
| 14 | Kalibracja II — C7, skłon | TRANSVERSE | SC7 | BTN_SAMPLE | 10 (LB1) | ZERO_WAY_ANGLE_DEF | 0 | — | — |
| 15 | Kalibracja II — T6, skłon | TRANSVERSE | ST6 | BTN_SAMPLE | 11 (LB2) | NONE | 0 | — | — |
| 16 | Kalibracja II — T12, skłon | TRANSVERSE | ST12 | BTN_SAMPLE | 12 (LB3) | NONE | 0 | — | — |
| 17 | Kalibracja II — L3, skłon | TRANSVERSE | SL3 | BTN_SAMPLE | 13 (LB4) | NONE | 0 | — | — |
| 18 | Kalibracja II — S1, skłon | TRANSVERSE | SSIPS | BTN_SAMPLE | 14 (LB5) | NONE | 0 | — | — |
| 19 | Przejście do testu Adamsa | TRANSVERSE | NULL | BTN_NEXT | 14 (LB5) | NONE | 0 | — | — |
| 20 | Test Adamsa — C7 (start) | TRANSVERSE | AC7 | BTN_SAMPLE | 15 (AD1) | ZERO_WAY_ANGLE_DEF | 0 | — | — |
| 21 | Test Adamsa — T6 | TRANSVERSE | AT6 | BTN_SAMPLE | 16 (AD2) | NONE | **1** | — | — |
| 22 | Test Adamsa — T12 | TRANSVERSE | AT12 | BTN_SAMPLE | 17 (AD3) | NONE | **1** | — | — |
| 23 | Test Adamsa — L3 | TRANSVERSE | AL3 | BTN_SAMPLE | 18 (AD4) | NONE | **1** | — | — |
| 24 | Test Adamsa — S1 | TRANSVERSE | ASIPS | BTN_SAMPLE | 19 (AD5) | NONE | **1** | — | — |

### backbone.summary

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont |
|---------|-----------|-------|---------|--------|------|-----------|------|
| 25 | (ekran podsumowania) | SAGGITTAL | NULL | BTN_NEXT | 0 | NONE | 0 |

---

## spineFlexibility.1 — Ocena elastyczności kręgosłupa

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont |
|---------|-----------|-------|---------|--------|------|-----------|------|
| 26 | Przygotowanie | TRANSVERSE | NULL | BTN_NEXT | 5 (LS1) | ZERO_WAY_ANGLE_DEF | 0 |
| 27 | Pomiar I — C7, stojąca | TRANSVERSE | PC7 | BTN_SAMPLE | 5 (LS1) | NONE | 0 |
| 28 | Pomiar I — T6, stojąca | TRANSVERSE | PT6 | BTN_SAMPLE | 6 (LS2) | NONE | 0 |
| 29 | Pomiar I — T12, stojąca | TRANSVERSE | PT12 | BTN_SAMPLE | 7 (LS3) | NONE | 0 |
| 30 | Pomiar I — L3, stojąca | TRANSVERSE | PL3 | BTN_SAMPLE | 8 (LS4) | NONE | 0 |
| 31 | Pomiar I — S1, stojąca | TRANSVERSE | PSIPS | BTN_SAMPLE | 9 (LS5) | NONE | 0 |
| 32 | Przejście do pomiaru II | TRANSVERSE | NULL | BTN_NEXT | 9 (LS5) | NONE | 0 |
| 33 | Pomiar II — C7, skłon | TRANSVERSE | SC7 | BTN_SAMPLE | 10 (LB1) | ZERO_WAY_ANGLE_DEF | 0 |
| 34 | Pomiar II — T6, skłon | TRANSVERSE | ST6 | BTN_SAMPLE | 11 (LB2) | NONE | 0 |
| 35 | Pomiar II — T12, skłon | TRANSVERSE | ST12 | BTN_SAMPLE | 12 (LB3) | NONE | 0 |
| 36 | Pomiar II — L3, skłon | TRANSVERSE | SL3 | BTN_SAMPLE | 13 (LB4) | NONE | 0 |
| 37 | Pomiar II — S1, skłon | TRANSVERSE | SSIPS | BTN_SAMPLE | 14 (LB5) | NONE | 0 |

### spineFlexibility.1.summary

| StageId | Plane | OrtMeas | Button | Mode |
|---------|-------|---------|--------|------|
| 38 | SAGGITTAL | NULL | BTN_NEXT | 0 |

---

## spineScreening — Badanie przesiewowe

### spineScreening.1 — Płaszczyzna strzałkowa

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont |
|---------|-----------|-------|---------|--------|------|-----------|------|
| 39 | Ocena nachylenia miednicy | SAGGITTAL | NM | BTN_SAMPLE | 1 (A1) | ZERO_WAY_ANGLE_DEF | 0 |
| 40 | Ocena lordozy lędźwiowej | SAGGITTAL | LL | BTN_SAMPLE | 2 (A2) | ZERO_ANGLE | 0 |
| 41 | Ocena kifozy piersiowej wstępującej | SAGGITTAL | KW | BTN_SAMPLE | 3 (A3) | ZERO_ANGLE | 0 |
| 42 | Ocena kifozy zstępującej | SAGGITTAL | KZ | BTN_SAMPLE | 4 (A4) | ZERO_ANGLE | 0 |
| 43 | KP = KW + KZ | SAGGITTAL | KP | BTN_NEXT | 4 (A4) | NONE | 0 |

### spineScreening.2 — Test Adamsa (bez kalibracji)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont |
|---------|-----------|-------|---------|--------|------|-----------|------|
| 44 | Test Adamsa — C7 | TRANSVERSE | AC7 | BTN_SAMPLE | 15 (AD1) | ZERO_WAY_ANGLE_DEF | 0 |
| 45 | Test Adamsa — T6 | TRANSVERSE | AT6 | BTN_SAMPLE | 16 (AD2) | NONE | **1** |
| 46 | Test Adamsa — T12 | TRANSVERSE | AT12 | BTN_SAMPLE | 17 (AD3) | NONE | **1** |
| 47 | Test Adamsa — L3 | TRANSVERSE | AL3 | BTN_SAMPLE | 18 (AD4) | NONE | **1** |
| 48 | Test Adamsa — S1 | TRANSVERSE | ASIPS | BTN_SAMPLE | 19 (AD5) | NONE | **1** |

### spineScreening.1.summary

| StageId | Plane | Button | Mode |
|---------|-------|--------|------|
| 49 | SAGGITTAL | BTN_NEXT | 0 |

---

## shoulder — Staw barkowy (ISOM1=50°, ISOM3=170° dla sag.; rotacja: ISOM1=90°, ISOM3=80°)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 50 | Przygotowanie | SAGGITTAL | NULL | BTN_NEXT | 0 | ZERO_WAY_ANGLE_DEF | 0 | 50 | 170 |
| 51 | Wyprost | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 52 | Zgięcie — reset | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 53 | Zgięcie — pomiar | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 54 | Zgięcie — następny | SAGGITTAL | FLEXION | BTN_NEXT | 0 | NONE | 0 | — | — |
| 55 | Odwodzenie — reset | FRONTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 170 | 0 |
| 56 | Odwodzenie | FRONTAL | ABDUCTION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 57 | Przywodzenie — reset | FRONTAL | ABDUCTION | BTN_RESET | 0 | NONE | 0 | — | — |
| 58 | Przywodzenie | FRONTAL | ADDUCTION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 59 | Przywodzenie — następny | FRONTAL | FLEXION | BTN_NEXT | 0 | NONE | 0 | — | — |
| 60 | Rotacja — przygotowanie | ROTATION_90 | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 90 | 80 |
| 61 | Rotacja zewnętrzna | ROTATION_90 | EXT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 62 | Rotacja wewnętrzna — reset | ROTATION_90 | EXT_ROT | BTN_RESET | 0 | NONE | 0 | — | — |
| 63 | Rotacja wewnętrzna | ROTATION_90 | INT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### shoulder.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 64 | SAGGITTAL | BTN_NEXT |

---

## elbow — Staw łokciowy (ISOM1=0°, ISOM3=150°; rotacja: ISOM1=90°, ISOM3=80°)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 65 | Przygotowanie | SAGGITTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 0 | 150 |
| 66 | Wyprost | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 67 | Zgięcie — reset | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 68 | Zgięcie | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 69 | Supinacja/Pronacja — start | SAGGITTAL | INT_ROT | BTN_NEXT | 0 | NONE | 0 | — | — |
| 70 | Rotacja — przygotowanie | ROTATION_90 | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 90 | 80 |
| 71 | Supinacja (odwracanie) | ROTATION_90 | EXT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 72 | Pronacja — reset | ROTATION_90 | EXT_ROT | BTN_RESET | 0 | NONE | 0 | — | — |
| 73 | Pronacja (nawracanie) | ROTATION_90 | INT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### elbow.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 74 | SAGGITTAL | BTN_NEXT |

---

## knee — Staw kolanowy (ISOM1=0°, ISOM3=130°)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 75 | Przygotowanie | SAGGITTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 0 | 130 |
| 76 | Wyprost/Przeprost | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 77 | Zgięcie — reset | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 78 | Zgięcie | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### knee.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 79 | SAGGITTAL | BTN_NEXT |

---

## hip — Staw biodrowy

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 80 | Przygotowanie (sag.) | SAGGITTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 15 | 125 |
| 81 | Wyprost | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 82 | Zgięcie — reset | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 83 | Zgięcie | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 84 | Zgięcie — następny | SAGGITTAL | FLEXION | BTN_NEXT | 0 | NONE | 0 | — | — |
| 85 | Przygotowanie (czoł.) | FRONTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 45 | 25 |
| 86 | Odwodzenie | FRONTAL | ABDUCTION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 87 | Przywodzenie — reset | FRONTAL | ABDUCTION | BTN_RESET | 0 | NONE | 0 | — | — |
| 88 | Przywodzenie | FRONTAL | ADDUCTION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 89 | Przywodzenie — następny | FRONTAL | ADDUCTION | BTN_NEXT | 0 | NONE | 0 | — | — |
| 90 | Przygotowanie (rot.) | ROTATION_90 | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 45 | 45 |
| 91 | Rotacja zewnętrzna | ROTATION_90 | EXT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 92 | Rotacja wewnętrzna — reset | ROTATION_90 | EXT_ROT | BTN_RESET | 0 | NONE | 0 | — | — |
| 93 | Rotacja wewnętrzna | ROTATION_90 | INT_ROT | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### hip.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 94 | SAGGITTAL | BTN_NEXT |

---

## wrist — Staw promieniowo-nadgarstkowy (ISOM sag: 50°/60°; czoł: 50°/60°)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 95 | Przygotowanie (sag.) | SAGGITTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 50 | 60 |
| 96 | Wyprost | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 97 | Zgięcie — reset | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 98 | Zgięcie | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 99 | Następny | SAGGITTAL | FLEXION | BTN_NEXT | 0 | NONE | 0 | — | — |
| 100 | Przygotowanie (czoł.) | FRONTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 50 | 60 |
| 101 | Odwrócenie promieniowe | FRONTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 102 | Odwrócenie łokciowe — reset | FRONTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 103 | Odwrócenie łokciowe | FRONTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### wrist.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 104 | SAGGITTAL | BTN_NEXT |

---

## ankle — Staw skokowo-goleniowy (ISOM1=20°, ISOM3=45°)

| StageId | StageName | Plane | OrtMeas | Button | Mode | ResetFlag | Cont | ISOM1 | ISOM3 |
|---------|-----------|-------|---------|--------|------|-----------|------|-------|-------|
| 105 | Przygotowanie | SAGGITTAL | NULL | BTN_RESET | 0 | ZERO_WAY_ANGLE_DEF | 0 | 20 | 45 |
| 106 | Wyprost (grzbietowe) | SAGGITTAL | EXTENSION | BTN_SAMPLE | 0 | NONE | 0 | — | — |
| 107 | Zgięcie — reset (podeszwowe) | SAGGITTAL | EXTENSION | BTN_RESET | 0 | NONE | 0 | — | — |
| 108 | Zgięcie (podeszwowe) | SAGGITTAL | FLEXION | BTN_SAMPLE | 0 | NONE | 0 | — | — |

### ankle.summary

| StageId | Plane | Button |
|---------|-------|--------|
| 109 | SAGGITTAL | BTN_NEXT |

---

## Obserwacje i wzorce

### Wzorzec pomiaru stawu (joint measurement pattern)

Każdy staw 2-kierunkowy (flexion/extension) powtarza ten sam schemat 4 etapów:

```
Stage N+0: Przygotowanie  → BTN_RESET → ZERO_WAY_ANGLE_DEF → wyświetl ISOM1/ISOM3
Stage N+1: Pomiar DIR1    → BTN_SAMPLE (zapis)
Stage N+2: Reset do DIR2  → BTN_RESET (ponowne zerowanie)
Stage N+3: Pomiar DIR2    → BTN_SAMPLE (zapis)
```

Dla badań z 3 płaszczyznami (bark, biodro) wzorzec powtarza się 3 razy (sagittal, frontal, rotation).

### Pomiar ciągły (Adams test)

Etapy z `OrtContinousMeas = 1` (StageId 21–24, 45–48): każda ramka z urządzenia musi być zapisana do `MedTestContinuousResult`. Dotyczy **wyłącznie** landmark'ów T6, T12, L3, S1 — landmark C7 (start) ma `Cont = 0`.

### Etapy summary

Każda definicja `.summary` ma dokładnie **1 etap** z `OrtMeas = NULL`, `Button = BTN_NEXT`, `Mode = 0`. Jest to tylko ekran wyświetlający wyniki — nie wchodzi w interakcję z urządzeniem.
