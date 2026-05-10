# Indeks badań (Surveys)

Lista wszystkich zdefiniowanych badań w systemie OrthoSpine / ORT100.  
Klucze badań pochodzą ze słownika `Survey.surveyKeys` (`ORT100.Surveys\Survey.cs`).  
Logika ładowania badań realizowana jest przez `SurveysDefinitions.CreateTests()` (`ORT100.MainApp\SurveysDefinitions.cs`).

---

## Badania

| Klucz (`surveyKey`) | Klucze w bazie danych (`MedTestDefinition.Key`) | Nazwa badania | Dokumentacja |
|---------------------|-------------------------------------------------|---------------|--------------|
| `backbone` | `backbone`, `backbone.1`, `backbone.2`, `backbone.summary` | Ocena postawy ciała | [→ backbone.md](backbone.md) |
| `spineScreening` | `spineScreening.1`, `spineScreening.2`, `spineScreening.1.summary` | Ocena postawy ciała — badanie przesiewowe | [→ spineScreening.md](spineScreening.md) |
| `spineFlexibility` | `spineFlexibility.1`, `spineFlexibility.1.summary` | Ocena elastyczności kręgosłupa | [→ spineFlexibility.md](spineFlexibility.md) |
| `shoulder.right` | `shoulder`, `shoulder.summary` | Pomiary kątowe w obręczy barkowej prawej | [→ shoulder.right.md](shoulder.right.md) |
| `shoulder.left` | `shoulder`, `shoulder.summary` | Pomiary kątowe w obręczy barkowej lewej | [→ shoulder.left.md](shoulder.left.md) |
| `elbow.right` | `elbow`, `elbow.summary` | Pomiary kątowe stawu łokciowego prawego | [→ elbow.right.md](elbow.right.md) |
| `elbow.left` | `elbow`, `elbow.summary` | Pomiary kątowe stawu łokciowego lewego | [→ elbow.left.md](elbow.left.md) |
| `hip.right` | `hip`, `hip.summary` | Pomiary kątowe stawu biodrowego prawego | [→ hip.right.md](hip.right.md) |
| `hip.left` | `hip`, `hip.summary` | Pomiary kątowe stawu biodrowego lewego | [→ hip.left.md](hip.left.md) |
| `knee.right` | `knee`, `knee.summary` | Pomiary kątowe stawu kolanowego prawego | [→ knee.right.md](knee.right.md) |
| `knee.left` | `knee`, `knee.summary` | Pomiary kątowe stawu kolanowego lewego | [→ knee.left.md](knee.left.md) |
| `wrist.right` | `wrist`, `wrist.summary` | Pomiary kątowe stawu promieniowo-nadgarstkowego prawego ⚠ | [→ wrist.right.md](wrist.right.md) |
| `wrist.left` | `wrist`, `wrist.summary` | Pomiary kątowe stawu promieniowo-nadgarstkowego lewego ⚠ | [→ wrist.left.md](wrist.left.md) |
| `ankle.right` | `ankle`, `ankle.summary` | Pomiary kątowe stawu skokowo-goleniowego prawego | [→ ankle.right.md](ankle.right.md) |
| `ankle.left` | `ankle`, `ankle.summary` | Pomiary kątowe stawu skokowo-goleniowego lewego | [→ ankle.left.md](ankle.left.md) |

> ⚠ **Uwaga:** Nazwy `wrist.left` i `wrist.right` w słowniku `Survey.surveyKeys` (`Survey.cs`) mają zamienione opisy stron. Wymaga korekty w kodzie.

---

## Powiązane pliki źródłowe

| Plik | Rola |
|------|------|
| `ORT100.Surveys\Survey.cs` | Definicja klasy `Survey`, słownik kluczy `surveyKeys` |
| `ORT100.MainApp\SurveysDefinitions.cs` | Fabryka badań — `CreateTests()`, `CreateSurveysFromMedTestDefinition()` |
| `OrthoSpine.Shared.Model\MedTestDefinition.cs` | Encja definicji badania (baza danych) |
| `OrthoSpine.Shared.Model\MedTestStage.cs` | Encja etapu badania (baza danych) |
