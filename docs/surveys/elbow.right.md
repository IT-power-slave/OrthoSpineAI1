# elbow.right — Pomiary kątowe stawu łokciowego prawego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `elbow.right` |
| **Nazwa** | Pomiary kątowe stawu łokciowego prawego |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie łokciowym po stronie prawej. Obejmuje płaszczyznę strzałkową (wyprost/zgięcie) oraz ruchy obrotowe (pronacja/supinacja).

## Powiązanie z bazą danych

Klucz w bazie: `elbow` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("elbow"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 65 | Przygotowanie | Pozycja stojąca/siedząca, ramię wzdłuż tułowia. Pow. C na powierzchni bocznej ramienia. Zerowanie. | 0°–150° |
| 66 | Wyprost | Kończyna w pełnym wyproście. Odczytaj kąt wyprostu. | — |
| 67 | Przygotowanie do zgięcia | Pow. C na bocznej powierzchni ramienia. Zerowanie. | — |
| 68 | Zgięcie | Wykonaj zgięcie łokcia. Odczytaj kąt zgięcia. | — |
| 69 | Przejście do pronacji/supinacji | Naciśnij NASTĘPNY. | — |

### Płaszczyzna rotacji (pronacja / supinacja)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 70 | Przygotowanie do supinacji | Łokieć zgięty 90°, przedramię w pozycji pośredniej. Pow. C na grzbiecie ręki. Zerowanie. | 90°–90° |
| 71 | Supinacja | Wykonaj supinację. Odczytaj kąt. | — |
| 72 | Przygotowanie do pronacji | Łokieć 90°, przedramię pośrednie. Pow. C na grzbiecie ręki. Zerowanie. | — |
| 73 | Pronacja | Wykonaj pronację. Odczytaj kąt. | — |

### elbow.summary

Ekran podsumowania wyników.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja lewostronna: [elbow.left.md](elbow.left.md)
