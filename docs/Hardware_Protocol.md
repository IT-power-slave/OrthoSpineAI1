# ORT100 Hardware Communication Protocol

> Krytyczny dokument dla każdej implementacji — bez znajomości protokołu BLE nie można obsługiwać urządzenia.

---

## Spis treści

1. [Architektura komunikacji](#1-architektura-komunikacji)
2. [Odkrywanie urządzenia BLE](#2-odkrywanie-urządzenia-ble)
3. [Ramka danych od urządzenia — SOrtometrDataFrame](#3-ramka-danych-od-urządzenia--sortometrdataframe)
4. [Ramka statusu — SOrtometrStatusFrame (dekodowanie pola `status`)](#4-ramka-statusu--sortometrestatusframe-dekodowanie-pola-status)
5. [Ramka konfiguracyjna do urządzenia — SOrtometrCfgFrame](#5-ramka-konfiguracyjna-do-urządzenia--sortometrcfgframe)
6. [Przyciski fizyczne urządzenia](#6-przyciski-fizyczne-urządzenia)
7. [Cykl życia sesji BLE](#7-cykl-życia-sesji-ble)
8. [Warstwa abstrakcji — interfejsy](#8-warstwa-abstrakcji--interfejsy)
9. [Mapowanie pól ramki na model danych](#9-mapowanie-pól-ramki-na-model-danych)

---

## 1. Architektura komunikacji

Komunikacja z urządzeniem ORT100 działa przez **BLE (Bluetooth Low Energy)** z użyciem natywnej biblioteki DLL (`cdortometr.dll`). W implementacji referencyjnej (.NET/WPF) architektura wygląda następująco:

```
Aplikacja
    │
    ├── DrvOrtometr (warstwa fasady)
    │       ├── Initialize(mac)   → ustawia adres MAC urządzenia
    │       ├── Start()           → ładuje DLL, startuje wątek odbioru
    │       ├── Stop()            → zamyka sesję BLE
    │       └── SendConfig(...)   → wysyła SOrtometrCfgFrame
    │
    └── CDClientOrtometr (wrapper P/Invoke → cdortometr.dll)
            ├── SetMacAddress(mac)
            ├── SendConfig(ref cfgFrame)
            └── ProcessFrames(...)  → dekoduje SOrtometrDataFrame → zdarzenie OrtoDataReceived
```

**Dla innych platform:**
- Na **Android/iOS** — użyj natywnego BLE API (Android: `BluetoothLeScanner`, iOS: `CoreBluetooth`). Musisz zaimplementować self-contained driver zamiast DLL.
- Na **Web (BLE API)** — dostępny tylko w Chrome/Edge; użyj `navigator.bluetooth`.
- Protokół binarny (ramki) jest identyczny niezależnie od platformy.

---

## 2. Odkrywanie urządzenia BLE

### Windows (implementacja referencyjna)

Urządzenie jest wyszukiwane w rejestrze systemowym:

```
HKLM\SYSTEM\ControlSet001\Enum\BTHLE\{device-keys}
```

Filtrowanie: urządzenie musi zawierać `"ORT-100"` w polu `FriendlyName` lub `DeviceDesc`.

Zwracane dane:
- `name` — pełna nazwa urządzenia (np. `"ORT-100 v1.2"`)
- `mac` — adres MAC w formacie `XXXXXXXXXXXX` (12 znaków hex bez separatorów, **małymi literami**)

```csharp
// Przykład: DevParam { name = "ORT-100 v1.2", mac = "e017731830d8" }
```

### Inne platformy

| Platforma | Sposób skanowania |
|-----------|------------------|
| Android | `BluetoothLeScanner.startScan()`, filtr po nazwie zawierającej `"ORT-100"` |
| iOS | `CBCentralManager.scanForPeripherals(withServices:)`, filtr po `advertisedName` |
| Web BLE | `navigator.bluetooth.requestDevice({ filters: [{ namePrefix: "ORT-100" }] })` |
| Linux | `bluez` / `bluetoothctl` |

**Po odnalezieniu urządzenia**: zapisz MAC i przekaż do inicjalizacji sterownika.

---

## 3. Ramka danych od urządzenia — SOrtometrDataFrame

Urządzenie wysyła ciągły strumień ramek binarnych przez BLE. Każda ramka ma stały rozmiar i układ pól (`Pack = 1`, `LayoutKind.Sequential`).

### Struktura ramki (C — binary layout)

```
Offset  Size  Type    Field           Unit    Description
------  ----  ------  --------------  ------  ----------------------------------
  0      2    Int16   header          —       Nagłówek: 0xAA01
  2      4    Int32   status          —       Status urządzenia (patrz §4)
  6      2    Int16   signal          dB      Siła sygnału Bluetooth
  8      4    float   f_battery       V       Stan naładowania akumulatora
 12      4    float   f_shake         g       Przyspieszenie działające na urządzenie
 16      4    float   f_roll          °       Kąt główny lewo-prawo
 20      4    float   f_roll_offset   °       Przesunięcie kąta (zapamiętane po CAL)
 24      4    float   f_tilt          °       Kąt pomocniczy przód-tył
 28      2    Int16   way             mm      Droga zmierzona przez rolkę
 30      2    UInt16  space           mm      Rozsunięcie nóg urządzenia
 32      4    float   f_force1        N       Siła czujnik nacisku 1
 36      4    float   f_force2        N       Siła czujnik nacisku 2
 40      2    UInt16  counter         —       Licznik wysłanych ramek
 42      2    UInt16  crc             —       Suma kontrolna CRC
TOTAL   44 bytes
```

> **Ważne:** Wszystkie liczby zmiennoprzecinkowe (`float`) są w formacie IEEE 754 little-endian.

### Identyfikacja bloku

```csharp
public enum Block { BLOCK_ID = 2018070501 }
```

### Mapowanie na MedTestContinuousResult

| Pole ramki | Pole MedTestContinuousResult |
|-----------|------------------------------|
| `status` (Int32) | `Status` (int) |
| `signal` (Int16) | `Signal` (int) |
| `f_battery` | `Battery` (double) |
| `f_shake` | `Shake` (double) |
| `f_roll` | `Roll` (double) |
| `f_roll_offset` | `RollOffset` (double) |
| `f_tilt` | `Tilt` (double) |
| `way` (Int16) | `Way` (int) |
| `space` (UInt16) | `Space` (int) |
| `f_force1` | `Force1` (double) |
| `f_force2` | `Force2` (double) |

---

## 4. Ramka statusu — SOrtometrStatusFrame (dekodowanie pola `status`)

Pole `status` (Int32) z ramki danych koduje wiele flag bitowych. Należy je zdekodować w aplikacji:

```
Bity   Maska       Pole                    Opis
-----  ----------  ----------------------  ------------------------------------
4:0    0x0000001F  mode                    Tryb pracy (ORT100Mode enum, 5 bitów)
5      0x00000020  b_force_det             Podłączony czujnik siły
6      0x00000040  b_batt_usb              Podłączone USB (ładowanie)
7      0x00000080  b_batt_low              Stan silnego rozładowania akumulatora
8      0x00000100  b_batt_charge           Trwa ładowanie
9      0x00000200  b_ble_conn_err          Wykryto problemy z komunikacją BLE
10     0x00000400  b_acc_err               Błąd czujnika przyspieszenia/żyroskop
11     0x00000800  b_way_err               Błąd czujnika drogi
12     0x00001000  b_space_err             Błąd czujnika rozsunięcia rolek
13     0x00002000  b_oled_err              Błąd komunikacji z wyświetlaczem OLED
14     0x00004000  b_next_btn              Wciśnięto NEXT (krótko)
15     0x00008000  b_sample_btn            Wciśnięto SAMPLE/POMIAR (krótko)
16     0x00010000  b_cal_btn               Wciśnięto CAL/ZEROWANIE (krótko)
17     0x00020000  b_power_btn             Wciśnięto POWER (krótko)
18     0x00040000  b_tmp_btn               Wciśnięto TMP (niepodłączony)
19     0x00080000  b_next_long_btn         Przytrzymano NEXT (długo)
20     0x00100000  b_sample_long_btn       Przytrzymano SAMPLE (długo)
21     0x00200000  b_cal_long_btn          Przytrzymano CAL (długo)
22     0x00800000  b_power_long_btn        Przytrzymano POWER (długo)
23     0x01000000  b_tmp_long_btn          Przytrzymano TMP (długo)
```

### Pseudo-kod dekodowania (language-agnostic)

```
function decodeStatus(status: int32):
    mode         = status & 0x1F             // pierwsze 5 bitów → ORT100Mode
    force_det    = (status >> 5)  & 1
    batt_usb     = (status >> 6)  & 1
    batt_low     = (status >> 7)  & 1
    batt_charge  = (status >> 8)  & 1
    ble_err      = (status >> 9)  & 1
    acc_err      = (status >> 10) & 1
    way_err      = (status >> 11) & 1
    space_err    = (status >> 12) & 1
    oled_err     = (status >> 13) & 1
    next_btn     = (status >> 14) & 1
    sample_btn   = (status >> 15) & 1
    cal_btn      = (status >> 16) & 1
    power_btn    = (status >> 17) & 1
    next_long    = (status >> 19) & 1
    sample_long  = (status >> 20) & 1
    cal_long     = (status >> 21) & 1
    power_long   = (status >> 22) & 1
```

---

## 5. Ramka konfiguracyjna do urządzenia — SOrtometrCfgFrame

Aplikacja wysyła konfigurację do urządzenia (tryb pracy + flagi zerowania + napisy na wyświetlaczu OLED).

### Struktura ramki konfiguracyjnej

```
Offset  Size  Type    Field           Description
------  ----  ------  -------------   ------------------------------------------
  0      4    UInt32  mode_set        Tryb pracy (ORT100Mode enum)
  4      1    bool    b_zero_angle    Zerowanie kąta do aktualnej pozycji
  5      1    bool    b_zero_angle_def  Zerowanie kąta do pozycji domyślnej (grawitacja)
  6      1    bool    b_zero_way      Zerowanie drogi
  7     10    byte[]  text_up         Napis górny na wyświetlaczu OLED (ASCII, 10 znaków, dopełniony spacjami)
 17     21    byte[]  text_dw         Napis dolny na wyświetlaczu OLED (ASCII, 21 znaków, dopełniony spacjami)
TOTAL  38 bytes
```

### Mapowanie ORT100ResetFlag → flagi konfiguracji

| ORT100ResetFlag | b_zero_angle | b_zero_angle_def | b_zero_way |
|-----------------|:---:|:---:|:---:|
| `NONE` | false | false | false |
| `ZERO_ANGLE` | **true** | false | false |
| `ZERO_ANGLE_DEF` | false | **true** | false |
| `ZERO_WAY` | false | false | **true** |
| `ZERO_WAY_ANGLE` | **true** | false | **true** |
| `ZERO_WAY_ANGLE_DEF` | false | **true** | **true** |

### Napisy OLED

- `text_up`: maks. **10 znaków** ASCII; jeśli krótszy — dopełnij spacjami do 10
- `text_dw`: maks. **21 znaków** ASCII; jeśli krótszy — dopełnij spacjami do 21
- Kodowanie: ASCII (nie UTF-8)

Przykładowa zawartość przy starcie aplikacji:
```
text_up = "Ort100    "   (10 znaków)
text_dw = "Sterowanie ortometrem"  (21 znaków)
```

---

## 6. Przyciski fizyczne urządzenia

Urządzenie ma 4 fizyczne przyciski. Każdy generuje zdarzenie (krótkie lub długie wciśnięcie) w polu `status` ramki.

| Przycisk | Funkcja aplikacyjna | ORT100Button | Flagi w `status` |
|---------|--------------------|--------------|--------------------|
| **NEXT** (Następny/Cofnij) | Przejście do następnego etapu bez pomiaru | `BTN_NEXT` | `b_next_btn` / `b_next_long_btn` |
| **SAMPLE** (Pomiar) | Zapisz aktualny odczyt jako wynik | `BTN_SAMPLE` | `b_sample_btn` / `b_sample_long_btn` |
| **CAL** (Kalibracja/Zerowanie) | Zeruje urządzenie do bieżącej pozycji | `BTN_RESET` | `b_cal_btn` / `b_cal_long_btn` |
| **POWER** (Włącz/Wyłącz) | Zasilanie (nie obsługiwane aplikacyjnie) | — | `b_power_btn` / `b_power_long_btn` |

> **Uwaga:** W `MedTestStage.OrtNextStepButton` definiujemy **który przycisk powinien zatwierdzić etap**. Aplikacja musi ignorować inne przyciski podczas aktywnego etapu lub obsługiwać je warunkowo.

### Logika reagowania na przycisk (pseudokod)

```
on frame received:
    decode statusFrame from dataFrame.status

    if statusFrame.b_sample_btn AND stage.OrtNextStepButton == BTN_SAMPLE:
        saveResult(currentFrame)
        advanceToNextStage()

    elif statusFrame.b_cal_btn AND stage.OrtNextStepButton == BTN_RESET:
        applyResetFlag(stage.OrtResetFlag)
        // NIE przechodzi do następnego etapu — czeka na BTN_SAMPLE

    elif statusFrame.b_next_btn AND stage.OrtNextStepButton == BTN_NEXT:
        advanceToNextStage()   // bez zapisu pomiaru
```

---

## 7. Cykl życia sesji BLE

```
STARTUP:
1. Wczytaj dostępne urządzenia BLE (skanowanie)
2. Wyświetl listę urządzeń "ORT-100" użytkownikowi
3. Użytkownik wybiera urządzenie → zapisz MAC
4. Initialize(mac) → zarejestruj MAC w sterowniku
5. Start() → załaduj DLL, wyślij konfigurację startową:
       SendConfig(MODE_SEQ_BT_UANGEL, "Ort100", "Sterowanie...", ZERO_WAY_ANGLE_DEF)
6. Uruchom wątek odbioru ramek

DURING SURVEY:
7. Na wejście do etapu:
       SendConfig(stage.OrtMode, stageNameShort, stageTip, stage.OrtResetFlag)
8. Odbieraj ramki → dekoduj status → wyświetlaj Roll lub Way live
9. Jeśli stage.OrtContinousMeas = true → zapisuj każdą ramkę do MedTestContinuousResult
10. Reaguj na przyciski (patrz §6)

SHUTDOWN:
11. Stop() → wyślij MODE_SEQ_END, zwolnij DLL:
        SendConfig(MODE_SEQ_END, "", "", ZERO_WAY_ANGLE_DEF)
12. Zamknij wątek odbioru
```

---

## 8. Warstwa abstrakcji — interfejsy

Dla implementacji cross-platform zaleca się utrzymanie tej samej warstwy abstrakcji:

```
IDeviceDriver
    ├── Initialize(mac: string) → string (błąd lub "")
    ├── Start() → bool
    ├── Stop() → bool
    ├── SendConfig(mode, textUp, textDw, resetFlag) → void
    └── event DataReceived(frame: DeviceDataFrame)

DeviceDataFrame
    ├── status: int32         // surowe pole status (dekoduj przez decodeStatus)
    ├── signal: int           // [dB]
    ├── battery: float        // [V]
    ├── shake: float          // [g]
    ├── roll: float           // [°]
    ├── rollOffset: float     // [°]
    ├── tilt: float           // [°]
    ├── way: int              // [mm]
    ├── space: int            // [mm]
    ├── force1: float         // [N]
    └── force2: float         // [N]

DeviceStatusFrame (zdekodowany status)
    ├── mode: ORT100Mode
    ├── buttons: { next, sample, cal, power, ... }
    └── errors: { bleErr, accErr, wayErr, ... }
```

---

## 9. Mapowanie pól ramki na model danych

### Zapis dyskretnego wyniku (MedTestResult) z ramki

Gdy użytkownik wciśnie `BTN_SAMPLE`:

```
MedTestResult {
    MedTestId      = currentMedTest.Id
    Plane          = currentStage.Plane
    OrtMeas        = currentStage.OrtMeas
    Side           = selectedSide   (SIDE_LEFT / SIDE_RIGHT / SIDE_NONE)
    PhysicalValue  = frame.f_roll   (dla pomiarów kątowych)
                   = frame.way      (dla pomiarów długości, np. FLLD)
    PhysicalUnit   = "°"            (dla kątów)
                   = "mm"           (dla drogi)
}
```

### Wyświetlanie live

| Tryb urządzenia (`OrtMode`) | Wyświetlana wartość | Jednostka |
|----------------------------|--------------------|-|
| `MODE_SEQ_BT_SANGEL` | `frame.f_roll` (ze znakiem) | ° |
| `MODE_SEQ_BT_UANGEL` | `abs(frame.f_roll)` | ° |
| `MODE_SEQ_BT_WAY` | `frame.way` | mm |
| `MODE_SEQ_BT_ADAMS` | `frame.f_roll` + ostrzeżenia `tilt`/`shake` | ° |
| `MODE_SEQ_LS*` | `frame.way` (kalibracja I — pozycja stojąca) | mm |
| `MODE_SEQ_LB*` | `frame.way` (kalibracja II — skłon) | mm |
| `MODE_SEQ_AD*` | `frame.f_roll` (ATR podczas testu Adamsa) | ° |

### Ostrzeżenia podczas testu Adamsa (`MODE_SEQ_BT_ADAMS`)

Aplikacja powinna wyświetlać ostrzeżenia gdy:
- `frame.f_tilt` przekracza bezpieczny próg (przechylenie urządzenia)
- `frame.f_shake` przekracza bezpieczny próg (zbyt szybki ruch)

Progi bezpieczeństwa muszą być zdefiniowane przez implementatora na podstawie kalibracji urządzenia.
