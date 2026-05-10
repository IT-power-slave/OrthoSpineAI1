# elbow.left — Pomiary kątowe stawu łokciowego lewego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `elbow.left` |
| **Nazwa** | Pomiary kątowe stawu łokciowego lewego |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie łokciowym po stronie lewej. Obejmuje płaszczyznę strzałkową (wyprost/zgięcie) oraz ruchy obrotowe (pronacja/supinacja).

## Powiązanie z bazą danych

Klucz w bazie: `elbow` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("elbow"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [elbow.right.md](elbow.right.md) — ten sam rekord `MedTestDefinition` (`Key = elbow`) jest używany dla obu stron.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja prawostronna: [elbow.right.md](elbow.right.md)
