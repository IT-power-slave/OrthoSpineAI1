# hip.right — Pomiary kątowe stawu biodrowego prawego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `hip.right` |
| **Nazwa** | Pomiary kątowe stawu biodrowego prawego |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie biodrowym po stronie prawej. Obejmuje trzy płaszczyzny: strzałkową (zgięcie/wyprost), czołową (odwodzenie/przywodzenie) i rotacyjną (rotacja zewnętrzna/wewnętrzna przy kolanie zgiętym 90°).

## Powiązanie z bazą danych

Klucz w bazie: `hip` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("hip"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 80 | Przygotowanie | Leżenie na plecach, kolano wyprostowane. Pow. C na udzie w połowie długości. Zerowanie. | 30°–120° |
| 81 | Wyprost | Kończyna w wyproście. Odczytaj kąt. | — |
| 82 | Przygotowanie do zgięcia | Pow. C na udzie w połowie długości. Zerowanie. | — |
| 83 | Zgięcie | Wykonaj zgięcie biodra. Odczytaj kąt. | — |
| 84 | Przejście do płaszczyzny czołowej | Naciśnij NASTĘPNY. | — |

### Płaszczyzna czołowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 85 | Przygotowanie do odwodzenia | Leżenie na boku, kolano wyprostowane. Pow. C na udzie w połowie. Zerowanie. | 45°–25° |
| 86 | Odwodzenie | Unieś kończynę bez ruchu miednicy. Odczytaj kąt odwodzenia. | — |
| 87 | Przygotowanie do przywodzenia | Leżenie na boku, kończyna przy brzegu leżanki. Pow. C na udzie. Zerowanie. | — |
| 88 | Przywodzenie | Przesuń nogę do przodu i w dół poza leżankę. Odczytaj kąt przywodzenia. | — |
| 89 | Przejście do płaszczyzny rotacji | Naciśnij NASTĘPNY. | — |

### Płaszczyzna rotacji (kolano zgięte 90°)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 90 | Przygotowanie | Siedzenie na leżance, podudzia swobodne. Pow. C na bocznej powierzchni podudzia (1/3 dolna). Zerowanie. | 45°–45° |
| 91 | Rotacja zewnętrzna | Ruch stopy do środka. Odczytaj kąt rot. zewnętrznej. | — |
| 92 | Przygotowanie do rot. wewnętrznej | Pow. C na bocznej powierzchni podudzia. Zerowanie. | — |
| 93 | Rotacja wewnętrzna | Ruch stopy na zewnątrz. Odczytaj kąt rot. wewnętrznej. | — |

### hip.summary

Ekran podsumowania wyników.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja lewostronna: [hip.left.md](hip.left.md)
