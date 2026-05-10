# ankle.left — Pomiary kątowe stawu skokowo-goleniowego lewego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `ankle.left` |
| **Nazwa** | Pomiary kątowe stawu skokowo-goleniowego lewego |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie skokowo-goleniowym po stronie lewej. Obejmuje płaszczyznę strzałkową (zgięcie grzbietowe/podeszwowe) oraz czołową (inwersja/ewersja).

## Powiązanie z bazą danych

Klucz w bazie: `ankle` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("ankle"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [ankle.right.md](ankle.right.md) — ten sam rekord `MedTestDefinition` (`Key = ankle`) jest używany dla obu stron.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja prawostronna: [ankle.right.md](ankle.right.md)
