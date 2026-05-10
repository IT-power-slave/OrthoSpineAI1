# shoulder.left — Pomiary kątowe w obręczy barkowej lewej

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `shoulder.left` |
| **Nazwa** | Pomiary kątowe w obręczy barkowej lewej |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

## Opis

Pomiary kątowe zakresu ruchu w stawie barkowym po stronie lewej. Obejmuje ruchy w trzech płaszczyznach: strzałkowej (zgięcie/wyprost), czołowej (odwodzenie/przywodzenie) i rotacyjnej (rotacja zewnętrzna/wewnętrzna).

## Powiązanie z bazą danych

Klucz w bazie: `shoulder` (wspólny dla obu stron — strona wybierana przez `MedTestSide.SIDE_LEFT` w `SurveysDefinitions`).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("shoulder"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [shoulder.right.md](shoulder.right.md) — ten sam rekord `MedTestDefinition` (`Key = shoulder`) jest używany dla obu stron. Strona ciała jest przekazywana przez `MedTestSide` i wpływa na orientację wizualizacji (`FlipX` w `BondBendUserControl`).

## Uwagi

- `OrtContinousMeas = 0` — wszystkie pomiary jednorazowe.

## Powiązane badanie

- Wersja prawostronna: [shoulder.right.md](shoulder.right.md)
