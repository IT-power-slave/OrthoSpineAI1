# spineFlexibility — Ocena elastyczności kręgosłupa

[← Powrót do indeksu](index.md)

## Informacje ogólne

| Pole | Wartość |
|------|---------|
| **Klucze w bazie** | `spineFlexibility.1`, `spineFlexibility.1.summary` |
| **Nazwa** | Ocena elastyczności kręgosłupa |
| **Strona ciała** | Brak (badanie globalne) |

## Opis

Badanie oceniające zakres ruchomości i elastyczność kręgosłupa. Polega na dwukrotnym pomiarze długości pleców orthometrem — raz w pozycji stojącej, raz w pełnym skłonie w przód — i obliczeniu różnicy (wskaźnik elastyczności).

## Powiązanie z bazą danych

```csharp
ort100Context.MedTestDefinitions.Where(s => s.Key.Contains("spineFlexibility"))
```

## Etapy badania

### spineFlexibility.1 — Pomiar elastyczności

| Id | Nazwa etapu | Opis instrukcji |
|----|-------------|-----------------|
| 26 | Przygotowanie | Pozycja stojąca tyłem. Oznacz C7, Th6, Th12, L3, S1. |
| 27–31 | Etap 1 — pozycja stojąca | Pomiar pow. B od C7 kolejno do Th6, Th12, L3, S1. |
| 32 | Przejście do etapu 2 | Naciśnij NASTĘPNY. |
| 33 | Opis etapu 2 | Pozycja z głową pochyloną, orthometr na C7, przesuwaj w dół. |
| 34–37 | Etap 2 — pozycja skłonu | Pomiar od Th6 kolejno do Th12, L3, S1. |

### spineFlexibility.1.summary

Ekran podsumowania z obliczonym wskaźnikiem elastyczności.

## Uwagi

- Brak pomiarów ATR — badanie nie ocenia asymetrii, tylko ruchomość kręgosłupa.
- Brak podziału na stronę lewą / prawą.
- Struktura etapów identyczna jak `backbone.2` (kalibracja), lecz wyniki interpretowane jako wskaźnik elastyczności.
