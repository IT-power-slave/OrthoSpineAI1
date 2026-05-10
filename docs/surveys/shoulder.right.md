# shoulder.right — Pomiary kątowe w obręczy barkowej prawej

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `shoulder.right` |
| **Nazwa** | Pomiary kątowe w obręczy barkowej prawej |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie barkowym po stronie prawej. Obejmuje ruchy w trzech płaszczyznach: strzałkowej (zgięcie/wyprost), czołowej (odwodzenie/przywodzenie) i rotacyjnej (rotacja zewnętrzna/wewnętrzna).

## Powiązanie z bazą danych

Klucz w bazie: `shoulder` (wspólny dla obu stron — strona wybierana przez `MedTestSide.SIDE_RIGHT` w `SurveysDefinitions`).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("shoulder"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 50 | Przygotowanie | Pozycja stojąca. Pow. C wzdłuż ramienia. Zerowanie. | 50°–170° |
| 51 | Wyprost | Kończyna w wyproście. Odczytaj kąt wyprostu. | — |
| 52 | Przygotowanie do zgięcia | Kończyna wzdłuż tułowia. Pow. C wzdłuż tylnej strony ramienia. Zerowanie. | — |
| 53 | Zgięcie | Wykonaj zgięcie. Odczytaj kąt zgięcia. | — |
| 54 | Przejście do płaszczyzny czołowej | Naciśnij NASTĘPNY. | — |

### Płaszczyzna czołowa

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 55 | Przygotowanie do odwodzenia | Pozycja stojąca, rotacja zewnętrzna ramienia. Pow. C ~1,5 cm poniżej wyrostka barkowego. Zerowanie. | 170°–0° |
| 56 | Odwodzenie | Wykonaj odwodzenie. Odczytaj kąt. | — |
| 57 | Przygotowanie do przywodzenia | Ramię w rot. zewnętrznej wzdłuż tułowia. Pow. C ~1,5 cm poniżej wyrostka barkowego. Zerowanie. | — |
| 58 | Przywodzenie | Ruch przywodzenia do linii środkowej. Odczytaj kąt. | — |
| 59 | Przejście do płaszczyzny rotacji | Naciśnij NASTĘPNY. | — |

### Płaszczyzna rotacji (ramię w odwiedzeniu 90°)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 60 | Przygotowanie | Leżenie na brzuchu, ramię odwiedzione 90°, przedramię zwisające. Pow. C na wyrostku łokciowym. Zerowanie. | 90°–80° |
| 61 | Rotacja zewnętrzna | Wykonaj rotację zewnętrzną. Odczytaj kąt. | — |
| 62 | Przygotowanie do rot. wewnętrznej | Pozycja jak do rot. zewnętrznej. Pow. C na wyrostku łokciowym. Zerowanie. | — |
| 63 | Rotacja wewnętrzna | Wykonaj rotację wewnętrzną. Odczytaj kąt. | — |

### shoulder.summary

Ekran podsumowania wyników.

## Uwagi

- Kolumna **ISOM ref.** podaje wartości referencyjne (`ValueISOM1` / `ValueISOM3`) wpisane w bazie dla etapów przygotowawczych.
- `OrtContinousMeas = 0` — wszystkie pomiary są jednorazowe (nie ciągłe).

## Powiązane badanie

- Wersja lewostronna: [shoulder.left.md](shoulder.left.md)
