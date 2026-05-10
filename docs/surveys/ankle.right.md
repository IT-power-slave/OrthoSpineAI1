# ankle.right — Pomiary kątowe stawu skokowo-goleniowego prawego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `ankle.right` |
| **Nazwa** | Pomiary kątowe stawu skokowo-goleniowego prawego |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie skokowo-goleniowym po stronie prawej. Obejmuje płaszczyznę strzałkową (zgięcie grzbietowe/podeszwowe) oraz czołową (inwersja/ewersja).

## Powiązanie z bazą danych

Klucz w bazie: `ankle` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("ankle"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 105 | Przygotowanie | Leżenie na plecach, stopa w pozycji neutralnej. Pow. C na bocznej powierzchni stopy/podudzia. Zerowanie. | 20°–45° |
| 106 | Zgięcie grzbietowe | Wykonaj zgięcie grzbietowe. Odczytaj kąt. | — |
| 107 | Przygotowanie do zgięcia podeszwowego | Pow. C na bocznej powierzchni. Zerowanie. | — |
| 108 | Zgięcie podeszwowe | Wykonaj zgięcie podeszwowe. Odczytaj kąt. | — |
| 109 | Przejście do płaszczyzny czołowej | Naciśnij NASTĘPNY. | — |

### Płaszczyzna czołowa (inwersja / ewersja)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 110 | Przygotowanie do inwersji | Stopa w pozycji neutralnej. Pow. C na powierzchni grzbietowej stopy. Zerowanie. | 35°–15° |
| 111 | Inwersja | Wykonaj inwersję. Odczytaj kąt. | — |
| 112 | Przygotowanie do ewersji | Pow. C na powierzchni grzbietowej stopy. Zerowanie. | — |
| 113 | Ewersja | Wykonaj ewersję. Odczytaj kąt. | — |

### ankle.summary

Ekran podsumowania wyników.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja lewostronna: [ankle.left.md](ankle.left.md)
