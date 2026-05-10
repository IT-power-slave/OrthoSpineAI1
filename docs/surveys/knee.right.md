# knee.right — Pomiary kątowe stawu kolanowego prawego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `knee.right` |
| **Nazwa** | Pomiary kątowe stawu kolanowego prawego |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie kolanowym po stronie prawej (wyprost/przeprost i zgięcie) w płaszczyźnie strzałkowej.

## Powiązanie z bazą danych

Klucz w bazie: `knee` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("knee"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 75 | Przygotowanie | Leżenie na plecach, kolano wyprostowane. Pow. C na powierzchni przedniej podudzia w połowie długości. Zerowanie. | 0°–130° |
| 76 | Wyprost / przeprost | Poleć wykonanie wyprostu (przeprostu). Odczytaj kąt. | — |
| 77 | Przygotowanie do zgięcia | Leżenie na brzuchu. Pow. C na tylnej powierzchni podudzia (1/3 dolna). Zerowanie. | — |
| 78 | Zgięcie | Wykonaj zgięcie kolana. Odczytaj kąt. | — |

### knee.summary

Ekran podsumowania wyników.

## Uwagi

- Tylko płaszczyzna strzałkowa — brak pomiarów rotacji w kolanie.
- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja lewostronna: [knee.left.md](knee.left.md)
