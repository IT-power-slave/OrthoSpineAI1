# ORT100 — Dokumentacja: Indeks główny

> Kompletna dokumentacja aplikacji medycznej ORT100 / OrthoSpine.  
> Przeznaczona dla agentów i deweloperów budujących równoważne aplikacje na innych platformach (mobile, web, desktop) i w innych językach.

---

## Dokumenty

| Dokument | Zawartość | Status |
|----------|-----------|--------|
| [Implementation_Guide.md](Implementation_Guide.md) | Model domenowy, enumy, workflow badań, reguły etapów, storage, seeding, pipeline raportowania, API contract | ✅ Kompletny |
| [Hardware_Protocol.md](Hardware_Protocol.md) | Protokół BLE, ramki binarne, dekodowanie statusu, konfiguracja urządzenia, przyciski | ✅ Kompletny |
| [AWWS_Algorithm.md](AWWS_Algorithm.md) | Algorytm diagnostyczny AWWS/PiLS: parametry, logiki, drzewo decyzyjne, rekomendacje | ✅ Kompletny |
| [Seed_Data_Stages.md](Seed_Data_Stages.md) | Kompletna tabela wszystkich 109 etapów ze wszystkimi polami (źródło: live DB) | ✅ Kompletny |
| [Database_Schema_Diagram.md](Database_Schema_Diagram.md) | Diagram ERD (Mermaid), relacje, problemy w schemacie | ✅ Kompletny |
| [surveys/index.md](surveys/index.md) | Lista wszystkich badań, klucze DB, linki do dokumentacji badań | ✅ Kompletny |
| **surveys/\*.md** | Szczegóły etapów każdego badania (backbone, spineScreening, shoulder…) | ✅ 15 plików |

---

## Szybki start dla agenta implementującego nową aplikację

### Krok 1 — Zrozum domenę
Przeczytaj `Implementation_Guide.md` §1 (System Overview) i §2 (Domain Data Model).

### Krok 2 — Zaprojektuj bazę danych
Skorzystaj z `Database_Schema_Diagram.md` i §2 `Implementation_Guide.md`.  
Wdrażaj z poprawkami opisanymi w §11 (Known Issues).

### Krok 3 — Wgraj dane bazowe (seeding)
Wykonaj seeding zgodnie z §10 `Implementation_Guide.md` i danymi z `surveys/index.md` + plików `surveys/*.md`.

### Krok 4 — Zaimplementuj komunikację z urządzeniem
Przeczytaj `Hardware_Protocol.md` w całości.  
Zaimplementuj `IDeviceDriver` dla swojej platformy (BLE API natywne lub DLL wrapper).

### Krok 5 — Zbuduj workflow badań
Przeczytaj §5 i §6 `Implementation_Guide.md`.  
Użyj danych z `surveys/*.md` do zrozumienia sekwencji etapów dla każdego badania.

### Krok 6 — Zaimplementuj algorytm AWWS/PiLS
Przeczytaj `AWWS_Algorithm.md` w całości.  
Zaimplementuj wszystkie `PGLogic*` jako niezależne moduły agregowane przez silnik wnioskowania.

### Krok 7 — Zaimplementuj raportowanie
Patrz §Raportowanie poniżej.

---

## Mapa zależności dokumentów

```
Implementation_Guide.md
    ├── §2  Domain Model        ←── Database_Schema_Diagram.md
    ├── §3  Enumerations        ←── (wbudowane w dokument)
    ├── §4  Hardware Protocol   ←── Hardware_Protocol.md (szczegóły)
    ├── §5  Survey Workflow     ←── surveys/index.md
    │                                └── surveys/*.md (każde badanie)
    ├── §8  AWWS Algorithm      ←── AWWS_Algorithm.md (szczegóły)
    ├── App. C/D  OrtMode/OrtMeas mappings  ←── (wbudowane)
    ├── App. E  Report Pipeline ←── (wbudowane)
    └── App. F  API Contract    ←── (wbudowane)

Seed_Data_Stages.md
    └── 109 etapów z pełnymi polami ←── live DB query
```

---

## Czego NIE ma w dokumentacji (pozostałe luki)

### ⚠️ Bezpieczeństwo danych medycznych (RODO/HIPAA)

Brak dokumentacji wymagań prawnych dla przetwarzania danych pacjentów. Implementacje mobilne/webowe muszą spełniać lokalne przepisy.

### ⚠️ Progi ostrzeżeń w teście Adamsa (BLE)

Nie udokumentowano progów dla `Tilt` i `Shake` podczas testu Adamsa (kiedy urządzenie jest przechylone za bardzo lub przesuwa się za szybko). Wartości zdefiniowane są empirycznie przez producenta urządzenia.
