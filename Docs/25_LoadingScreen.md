# LoadingScreen — Animierter Ladebildschirm

**Datei:** `LoadingScreen.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class LoadingScreen`

---

## Zweck

`LoadingScreen` zeigt beim Programmstart einen animierten Ladebildschirm mit Fortschrittsbalken und lädt dabei sequenziell alle notwendigen Daten und Systemkomponenten. Er ist die erste Klasse, die nach dem Programmstart aufgerufen wird.

---

## Konfiguration

| Feld | Wert | Beschreibung |
|---|---|---|
| `progressBarWidth` | `50` | Breite des Fortschrittsbalkens in Zeichen |
| `primaryColor` | `Cyan` | Hauptfarbe für den Ladebalken |
| `accentColor` | `Yellow` | Akzentfarbe für Prozessnamen |

---

## Methoden

### `Show()`

Hauptmethode — wird in `Program.Main()` als erstes aufgerufen.

**Ablauf:**

1. Löscht die Konsole und versteckt den Cursor
2. Zeigt das ASCII-Logo via `ZeigeLogo()`
3. Führt sequenziell alle Ladeprozesse aus:

| Schritt | Aktion |
|---|---|
| Initialisiere System | `InitialisiereSystem()` — Dateien und Einstellungen |
| Lade Benutzerdaten | `DataManager.LoadBenutzer()` |
| Lade Mitarbeiter | `DataManager.LoadMitarbeiter()` |
| Lade Inventar | `DataManager.LoadInventar()` |
| Initialisiere KI Engine 2.0 | `KIEngine.Initialisiere()` |
| Initialisiere Verschlüsselung | `LogManager.InitializeLog()` |
| Prüfe Systemintegrität | `PruefeSystem()` |

4. Jeder Schritt aktualisiert den Fortschrittsbalken
5. Fehler in einzelnen Schritten werden abgefangen und angezeigt (kein Programmabbruch)
6. Nach Abschluss: Cursor wieder sichtbar

---

### `ZeigeLogo()` *(private)*

Zeigt das große ASCII-Art-Logo der Anwendung mit Animationseffekt (Progressive Reveal).

---

### `ZeigeProgressBar(progress, prozessName)` *(private)*

Rendert den aktuellen Fortschrittsbalken an einer festen Cursor-Position:

```
[████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 40% | Lade Inventar...
```

- Gefüllte Zeichen: `█`
- Leere Zeichen: `░`
- Prozentzahl und aktueller Prozessname

---

### `InitialisiereSystem()` *(private)*

Initialisiert die grundlegende Systeminfrastruktur:
- `ConsoleHelper.SetupConsole()` — Konsolenfenster konfigurieren
- `FileManager.InitializeFiles()` — Datendateien anlegen
- `FileManager.FixFileAttributes()` — Datei-Attribute bereinigen

---

### `PruefeSystem()` *(private)*

Führt grundlegende Integritätsprüfungen durch:
- Prüft ob alle Datendateien les- und schreibbar sind
- Warnt bei fehlenden oder beschädigten Dateien
- Loggt den Systemstart

---

## Aufrufkette beim Programmstart

```
Program.Main()
  └── LoadingScreen.Show()
        ├── InitialisiereSystem()
        ├── DataManager.LoadBenutzer()
        ├── DataManager.LoadMitarbeiter()
        ├── DataManager.LoadInventar()
        ├── KIEngine.Initialisiere()
        ├── LogManager.InitializeLog()
        └── PruefeSystem()
  └── AuthManager.Anmeldung()
  └── ConsoleHelper.PrintWelcome()
  └── AppSetup.Build().Run()
```

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Laden aller Datenlisten |
| `KIEngine` | KI-Initialisierung |
| `LogManager` | Log-Initialisierung |
| `FileManager` | Datei-Setup |
| `ConsoleHelper` | Konsolen-Konfiguration |
