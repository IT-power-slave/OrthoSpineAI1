# Database Schema Diagram

Schemat bazy danych aplikacji **OrthoSpine / ORT100** wygenerowany na podstawie analizy architektury EF6.

> Kolumny oznaczone `⚠` wskazują na zidentyfikowane problemy (patrz [EF6_Database_Architecture_Analysis.md](EF6_Database_Architecture_Analysis.md)).

---

## Diagram ERD (Entity Relationship Diagram)

```mermaid
erDiagram

    CLINICS {
        int     Id          PK
        string  Name
        string  Adress      "⚠ Typo: powinno być Address"
    }

    SYSTEM_USERS {
        int     Id          PK
        string  Login       "⚠ Brak indeksu unikalności"
        string  Passwd      "⚠ Plaintext — brak hashowania"
        int     ClinicId    FK
    }

    PATIENTS {
        int     Id          PK
        string  FirstName   "⚠ Brak [Required] / [MaxLength]"
        string  LastName    "⚠ Brak [Required] / [MaxLength]"
        string  PESEL       "⚠ Brak ograniczenia unikalności"
        string  DateOfBirth
        int     ClinicId    FK
    }

    MED_TESTS {
        int     Id              PK
        string  DateTime        "⚠ Powinno być DateTime, nie string"
        int     PatientId       FK
        int     SystemUserId    FK
    }

    MED_TEST_DEFINITIONS {
        int     Id      PK
        string  Key     "⚠ Brak indeksu unikalności"
        string  Name
    }

    MED_TEST_STAGES {
        int     Id                      PK
        string  Name
        float   ValueISOM1              "nullable"
        float   ValueISOM3              "nullable"
        int     MedTestDefinitionId     FK "⚠ nav. prop nie jest virtual"
    }

    MED_TEST_RESULTS {
        int     Id              PK
        int     MedTestPlane    "enum jako int"
        int     Measurement     "enum jako int"
        int     MedTestSide     "enum jako int"
        string  PhysicalUnit    "⚠ Brak [Required]"
        float   PhysicalValue   "⚠ Brak [Required]"
        int     MedTestId       FK
    }

    MED_TEST_CONTINUOUS_RESULTS {
        int     Id          PK
        float   Roll
        float   RollOffset
        float   Tilt
        float   Way
        float   Space
        float   Force1
        float   Force2
        int     MedTestId   FK
    }

    DIAGNOSTIC_FORMS {
        int     Id          PK
        string  FormName
    }

    CLINICS            ||--o{ SYSTEM_USERS              : "posiada"
    CLINICS            ||--o{ PATIENTS                  : "rejestruje"
    PATIENTS           ||--o{ MED_TESTS                 : "wykonuje"
    SYSTEM_USERS       ||--o{ MED_TESTS                 : "prowadzi"
    MED_TEST_DEFINITIONS ||--o{ MED_TEST_STAGES         : "składa się z"
    MED_TESTS          ||--o{ MED_TEST_RESULTS          : "generuje"
    MED_TESTS          ||--o{ MED_TEST_CONTINUOUS_RESULTS : "rejestruje"
```

> **Uwaga:** Relacja `MedTest → DiagnosticForm` **nie istnieje w bazie danych** — właściwość nawigacyjna jest oznaczona `[NotMapped]` (problem P5). Tabela `DIAGNOSTIC_FORMS` istnieje jako oddzielna encja bez FK do `MED_TESTS`.

---

## Legenda problemów

| Symbol | Znaczenie |
|--------|-----------|
| `⚠`   | Zidentyfikowany problem architektoniczny lub bezpieczeństwa |
| `PK`  | Klucz główny (Primary Key) |
| `FK`  | Klucz obcy (Foreign Key) |

---

## Podsumowanie relacji

| Relacja | Typ | Kaskada | Uwagi |
|---------|-----|---------|-------|
| `Clinic` → `Patient` | 1 : N | EF domyślna (cascade delete) | FK: `Patient.ClinicId` |
| `Clinic` → `SystemUser` | 1 : N | Nieznana | FK może być niezdefiniowany na encji |
| `Patient` → `MedTest` | 1 : N | EF domyślna | FK: `MedTest.PatientId` |
| `SystemUser` → `MedTest` | 1 : N | EF domyślna | FK: `MedTest.SystemUserId` |
| `MedTestDefinition` → `MedTestStage` | 1 : N | EF domyślna | FK: `MedTestStage.MedTestDefinitionId` |
| `MedTest` → `MedTestResult` | 1 : N | EF domyślna | FK: `MedTestResult.MedTestId` |
| `MedTest` → `MedTestContinuousResult` | 1 : N | EF domyślna | FK: `MedTestContinuousResult.MedTestId` |
| `MedTest` → `DiagnosticForm` | **Brak** | N/A | `[NotMapped]` — relacja nie jest utrwalana ⚠ |
