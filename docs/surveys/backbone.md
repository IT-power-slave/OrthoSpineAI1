# backbone — Ocena postawy ciała

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucze w bazie** | `backbone` (root), `backbone.1`, `backbone.2`, `backbone.summary` |
| **Nazwa** | Ocena postawy ciała |
| **Strona ciała** | Brak (badanie globalne) |

## Opis

Badanie oceniające ogólną postawę ciała pacjenta. Składa się z trzech etapów:
1. **backbone.1** — Ocena płaszczyzny strzałkowej (nachylenie miednicy, lordoza lędźwiowa, kifoza piersiowa)
2. **backbone.2** — Test Adamsa z pomiarem symetrii pleców orthometrem (kalibracja w pozycji stojącej + skłonie + pomiary ATR)
3. **backbone.summary** — Podsumowanie wyników

## Powiązanie z bazą danych

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("backbone"))
```

## Etapy badania

### backbone.1 — Ocena płaszczyzny strzałkowej

| Id | Nazwa etapu | Płaszczyzna | Opis instrukcji |
|----|-------------|-------------|-----------------|
| 1 | Ocena nachylenia miednicy | — | Przyłóż orthometr pow. A na kość krzyżową. Zmierz NM. |
| 2 | Ocena kąta lordozy lędźwiowej | — | Przyłóż orthometr pow. A na L1–Th12. Zmierz LL. |
| 3 | Ocena kąta kifozy piersiowej wstępującej | — | Przyłóż orthometr pionowo pow. A na część szczytową kifozy. Zmierz KW. |
| 4 | Ocena kąta kifozy piersiowej (zstępująca) | — | Przyłóż orthometr pow. A na Th1–Th3. Zmierz KZ. |
| 5 | Ocena kąta kifozy piersiowej (KP = KW + KZ) | — | Informacja: KP = KW + KZ. Przejdź dalej. |

### backbone.2 — Test Adamsa (kalibracja + pomiary ATR)

| Id | Nazwa etapu | Opis instrukcji |
|----|-------------|-----------------|
| 7 | Przygotowanie | Pozycja stojąca tyłem. Oznacz C7, Th6, Th12, L3, S1. |
| 8–12 | Kalibracja 1 — pozycja stojąca | Pomiar orthometrem pow. B od C7 kolejno do Th6, Th12, L3, S1. |
| 13 | Przejście do kalibracji 2 | Naciśnij NASTĘPNY. |
| 14 | Opis kalibracji 2 | Pozycja z głową pochyloną w przód. Orthometr na C7, przesuwaj w dół. |
| 15–18 | Kalibracja 2 — pozycja skłonu | Pomiar od Th6 kolejno do Th12, L3, S1. |
| 19 | Przejście do testu Adamsa | Naciśnij NASTĘPNY. |
| 20 | Przygotowanie i C7 — Test Adamsa | Pozycja jak przy kalibracji 2. Pomiar na C7 (ciągły). |
| 21 | Pomiar Th6 | Pomiar ATR (ciągły) na Th6. |
| 22 | Pomiar Th12 | Pomiar ATR (ciągły) na Th12. |
| 23 | Pomiar L3 | Pomiar ATR (ciągły) na L3. |
| 24 | Pomiar S1 | Pomiar ATR (ciągły) na S1. |

### backbone.summary

Ekran podsumowania wyników całego badania.

## Uwagi

- Brak podziału na stronę lewą / prawą.
- Etap `backbone.2` korzysta z pomiarów ciągłych (`OrtContinousMeas = 1`) — sensor rejestruje dane przez cały czas przesuwania orthometru.
