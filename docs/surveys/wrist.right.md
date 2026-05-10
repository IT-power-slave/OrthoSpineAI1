# wrist.right — Pomiary kątowe stawu promieniowo-nadgarstkowego lewego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `wrist.right` |
| **Nazwa** | Pomiary kątowe stawu promieniowo-nadgarstkowego lewego |
| **Strona ciała** | Prawa (`MedTestSide.SIDE_RIGHT`) |

> ⚠ **Uwaga:** Nazwa badania w słowniku `surveyKeys` zawiera niespójność — klucz `wrist.right` opisany jest jako „lewego", a `wrist.left` jako „prawego". Wymaga weryfikacji i korekty w `Survey.cs`.

## Opis

Pomiary kątowe zakresu ruchu w stawie promieniowo-nadgarstkowym po stronie prawej. Obejmuje dwie płaszczyzny: strzałkową (wyprost/zgięcie) i czołową (odchylenie promieniowe/łokciowe).

## Powiązanie z bazą danych

Klucz w bazie: `wrist` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("wrist"))
// + filtrowanie strony: MedTestSide.SIDE_RIGHT
```

## Etapy badania

### Płaszczyzna strzałkowa (wyprost / zgięcie)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 95 | Przygotowanie | Siedzenie, przedramię nawrócone na biurku, łokieć 90°. Pow. C na grzbiecie śródręcza. Zerowanie. | 50°–60° |
| 96 | Wyprost (zgięcie grzbietowe) | Wykonaj wyprost nadgarstka. Odczytaj kąt. | — |
| 97 | Przygotowanie do zgięcia | Pow. C na grzbietowej powierzchni śródręcza. Zerowanie. | — |
| 98 | Zgięcie dłoniowe | Wykonaj zgięcie nadgarstka. Odczytaj kąt. | — |
| 99 | Przejście do płaszczyzny czołowej | Naciśnij NASTĘPNY. | — |

### Płaszczyzna czołowa (odchylenie promieniowe / łokciowe)

| Id | Nazwa etapu | Opis instrukcji | ISOM ref. |
|----|-------------|-----------------|----------:|
| 100 | Przygotowanie | Siedzenie, ramię lekko odwiedzione, łokieć 90°, przedramię na biurku w pozycji pośredniej. Pow. C na grzbiecie ręki. Zerowanie. | 50°–60° |
| 101 | Odchylenie promieniowe | Wykonaj zgięcie promieniowe (odwiedzenie). Odczytaj kąt. | — |
| 102 | Przygotowanie do odchylenia łokciowego | Pow. C na grzbiecie ręki. Zerowanie. | — |
| 103 | Odchylenie łokciowe | Wykonaj zgięcie łokciowe (przywodzenie). Odczytaj kąt. | — |

### wrist.summary

Ekran podsumowania wyników.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.
- Wartości referencyjne ISOM: 50°/60° dla obu etapów przygotowawczych.

> ⚠ **Uwaga:** Nazwy badań w słowniku `Survey.surveyKeys` mają zamienione opisy dla `wrist.left` i `wrist.right`. Wymaga korekty w `Survey.cs`.

## Powiązane badanie

- Wersja lewostronna: [wrist.left.md](wrist.left.md)
