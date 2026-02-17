# HardwarePrintManager — Hardware-Ausgabe-Druck

**Datei:** `Hardwareprintermanager.cs`, `Hardwareprintmanagerextension.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static partial class HardwarePrintManager`

---

## Zweck

`HardwarePrintManager` verwaltet das Erstellen, Drucken und Archivieren von Hardware-Ausgabe-Belegen. Diese Belege dokumentieren die Ausgabe von Inventargegenständen an Mitarbeiter. Eine vollständige Druckhistorie wird als JSON gespeichert.

---

## Datenmodelle

### `DruckHistorieEintrag`

Repräsentiert einen vollständigen Druckvorgang:

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `Id` | `string` | Eindeutige Beleg-ID |
| `MitarbeiterKuerzel` | `string` | Kürzel des Empfängers |
| `MitarbeiterName` | `string` | Vollständiger Name |
| `Datum` | `DateTime` | Datum der Ausgabe |
| `GedrucktVon` | `string` | Ausführender Benutzer |
| `DruckerName` | `string` | Verwendeter Drucker |
| `Artikel` | `List<DruckArtikelPosition>` | Liste der ausgegebenen Artikel |
| `Status` | `string` | `Gedruckt` / `Vorschau` / `Storniert` |
| `Notizen` | `string` | Optionale Bemerkungen |

### `DruckArtikelPosition`

Einzelne Artikel-Position im Beleg:

| Eigenschaft | Typ | Beschreibung |
|---|---|---|
| `InvNmr` | `string` | Inventarnummer |
| `GeraeteName` | `string` | Gerätename |
| `SerienNummer` | `string` | Seriennummer |
| `Kategorie` | `string` | Kategorie |
| `Anzahl` | `int` | Ausgegebene Stückzahl |
| `Bemerkung` | `string` | Optionale Bemerkung |

---

## Methoden

### `DruckNeu()`

Startet den Workflow für einen neuen Druckbeleg:

1. Mitarbeiter auswählen (Kürzel oder Listenauswahl)
2. Artikel hinzufügen (Inventarnummer oder Suche)
   - Mehrere Artikel möglich
   - Anzahl und optionale Bemerkung pro Artikel
3. Vorschau des Belegs anzeigen
4. Drucker auswählen oder Vorschau-Modus
5. Drucken (via `System.Drawing.Printing`) oder als Vorschau speichern
6. Eintrag in `DruckHistorie.json`

---

### `ZeigeDruckHistorie()`

Zeigt alle gespeicherten Druckbelege in einer Tabelle mit ID, Datum, Mitarbeiter, Artikel-Anzahl und Status. Paginierung bei vielen Einträgen.

---

### `SucheDruckHistorie()`

Suche in der Druckhistorie nach:
- Mitarbeitername oder Kürzel
- Datum oder Datumsbereich
- Inventarnummer
- Beleg-ID

---

### `BearbeiteHistorieEintrag()`

Ermöglicht das nachträgliche Bearbeiten eines Historieneintrags:
- Status ändern (z.B. → Storniert)
- Notizen hinzufügen oder ändern

---

### `KonfiguriereDrucker()`

Öffnet das Drucker-Konfigurationsmenü:
- Standard-Drucker festlegen
- Papiergröße und Ausrichtung
- Test-Ausdruck

---

### `LadeHistorie() → List<DruckHistorieEintrag>` *(private)*

Lädt alle Einträge aus `DruckHistorie.json`. Gibt eine leere Liste zurück wenn die Datei nicht existiert.

---

### `SpeichereHistorie(eintraege)` *(private)*

Serialisiert die Liste und speichert sie als JSON in `DruckHistorie.json`.

---

## Persistenz

| Datei | Format | Inhalt |
|---|---|---|
| `DruckHistorie.json` | JSON (UTF-8) | Vollständige Druckhistorie |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | Inventar- und Mitarbeiterdaten |
| `AuthManager` | `AktuellerBenutzer` für `GedrucktVon` |
| `ConsoleHelper` | Menüs und Eingaben |
| `System.Drawing.Printing` | Tatsächlicher Druckvorgang |
| `System.Text.Json` | JSON-Serialisierung der Historie |
