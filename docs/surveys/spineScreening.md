# spineScreening — Ocena postawy ciała (badanie przesiewowe)

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucze w bazie** | `spineScreening.1`, `spineScreening.2`, `spineScreening.1.summary` |
| **Nazwa** | Ocena postawy ciała — badanie przesiewowe |
| **Strona ciała** | Brak (badanie globalne) |

## Opis

Uproszczona, przesiewowa wersja oceny postawy ciała. Składa się z dwóch etapów:
1. **spineScreening.1** — Ocena płaszczyzny strzałkowej (NM, LL, KW, KZ)
2. **spineScreening.2** — Test Adamsa (pomiar asymetrii ATR bez kalibracji długości pleców)
3. **spineScreening.1.summary** — Podsumowanie

## Powiązanie z bazą danych

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("spineScreening"))
```

## Etapy badania

### spineScreening.1 — Ocena płaszczyzny strzałkowej

| Id | Nazwa etapu | Opis instrukcji |
|----|-------------|-----------------|
| 39 | Ocena nachylenia miednicy | Pow. A na kość krzyżową. Zmierz NM. |
| 40 | Ocena kąta lordozy lędźwiowej | Pow. A na L1–Th12. Zmierz LL. |
| 41 | Ocena kąta kifozy wstępującej | Pow. A na część szczytową kifozy. Zmierz KW. |
| 42 | Ocena kąta kifozy zstępującej | Pow. A na Th1–Th3. Zmierz KZ. |
| 43 | KP = KW + KZ | Informacja, przejdź dalej. |

### spineScreening.2 — Test Adamsa (bez kalibracji)

| Id | Nazwa etapu | Opis instrukcji |
|----|-------------|-----------------|
| 44 | Przygotowanie i C7 | Pozycja stojąca tyłem, oznacz C7, Th6, Th12, L3, S1. Pomiar na C7. |
| 45 | Pomiar Th6 | ATR (ciągły) na Th6. |
| 46 | Pomiar Th12 | ATR (ciągły) na Th12. |
| 47 | Pomiar L3 | ATR (ciągły) na L3. |
| 48 | Pomiar S1 | ATR (ciągły) na S1. |

### spineScreening.1.summary

Ekran podsumowania wyników.

## Uwagi

- Wariant skrócony badania `backbone` — brak dwuetapowej kalibracji długości pleców.
- Pomiary ATR w etapie 2 są ciągłe (`OrtContinousMeas = 1`).
- Brak podziału na stronę lewą / prawą.
