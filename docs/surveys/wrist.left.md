# wrist.left — Pomiary kątowe stawu promieniowo-nadgarstkowego prawego

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucz** (`surveyKey`) | `wrist.left` |
| **Nazwa** | Pomiary kątowe stawu promieniowo-nadgarstkowego prawego |
| **Strona ciała** | Lewa (`MedTestSide.SIDE_LEFT`) |

> ⚠ **Uwaga:** Nazwa badania w słowniku `surveyKeys` zawiera niespójność — klucz `wrist.left` opisany jest jako „prawego", a `wrist.right` jako „lewego". Wymaga weryfikacji i korekty w `Survey.cs`.

## Opis

Pomiary kątowe zakresu ruchu w stawie promieniowo-nadgarstkowym po stronie lewej. Obejmuje dwie płaszczyzny: strzałkową (wyprost/zgięcie) i czołową (odchylenie promieniowe/łokciowe).

## Powiązanie z bazą danych

Klucz w bazie: `wrist` (wspólny dla obu stron).

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("wrist"))
// + filtrowanie strony: MedTestSide.SIDE_LEFT
```

## Etapy badania

Etapy identyczne jak w [wrist.right.md](wrist.right.md) — ten sam rekord `MedTestDefinition` (`Key = wrist`) jest używany dla obu stron.

## Uwagi

- `OrtContinousMeas = 0` — pomiary jednorazowe.

> ⚠ **Uwaga:** Nazwy badań w słowniku `Survey.surveyKeys` mają zamienione opisy dla `wrist.left` i `wrist.right`. Wymaga korekty w `Survey.cs`.

## Powiązane badanie

- Wersja prawostronna: [wrist.right.md](wrist.right.md)
