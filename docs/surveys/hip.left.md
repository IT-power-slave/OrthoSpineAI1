# hip.left — Pomiary kątowe stawu biodrowego lewego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `hip.left` |
| **Nazwa** | Pomiary kątowe stawu biodrowego lewego |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie biodrowym po stronie lewej. Obejmuje trzy płaszczyzny: strzałkową (zgięcie/wyprost), czołową (odwodzenie/przywodzenie) i rotacyjną.

## Powiązanie z bazą danych

Klucz w bazie: `hip` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("hip"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [hip.right.md](hip.right.md) — ten sam rekord `MedTestDefinition` (`Key = hip`) jest używany dla obu stron.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

## Powiązane badanie

- Wersja prawostronna: [hip.right.md](hip.right.md)
