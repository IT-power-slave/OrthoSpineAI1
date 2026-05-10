# knee.left — Pomiary kątowe stawu kolanowego lewego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `knee.left` |
| **Nazwa** | Pomiary kątowe stawu kolanowego lewego |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie kolanowym po stronie lewej (wyprost/przeprost i zgięcie) w płaszczyźnie strzałkowej.

## Powiązanie z bazą danych

Klucz w bazie: `knee` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("knee"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [knee.right.md](knee.right.md) — ten sam rekord `MedTestDefinition` (`Key = knee`) jest używany dla obu stron.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja prawostronna: [knee.right.md](knee.right.md)
