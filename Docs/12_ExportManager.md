# ExportManager — Datenexport

**Datei:** `Exportmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class ExportManager`

---

## Zweck

`ExportManager` ermöglicht den Export aller Anwendungsdaten in verschiedene Formate. Der Benutzer kann Inventar, Mitarbeiter und Benutzer separat oder als vollständigen Gesamt-Export ausgeben lassen. Das Zielverzeichnis ist frei wählbar.

---

## Konfiguration

| Feld | Wert | Beschreibung |
|---|---|---|
| `StandardExportVerzeichnis` | `<Arbeitsverzeichnis>/Exports` | Standardpfad für alle Exporte |

---

## Methoden

### `ZeigeExportMenu()`

Zeigt das interaktive Export-Hauptmenü:

| Auswahl | Aktion |
|---|---|
| `[1]` | Artikel exportieren |
| `[2]` | Mitarbeiter exportieren |
| `[3]` | Benutzer exportieren |
| `[4]` | Vollständiger Export (alle Daten) |
| `[0]` | Zurück zum Hauptmenü |

---

### `ExportiereArtikel(pfad)`

Exportiert alle Inventarartikel aus `DataManager.Inventar` in das angegebene Verzeichnis. Erzeugt eine strukturierte Datei mit Header, Spaltenüberschriften und allen Artikeldetails. Meldet Anzahl der exportierten Datensätze.

---

### `ExportiereMitarbeiter(pfad)`

Exportiert alle Mitarbeiter aus `DataManager.Mitarbeiter`. Format: Vorname, Nachname, Abteilung. Mit Header und Statistik-Fußzeile.

---

### `ExportiereBenutzer(pfad)`

Exportiert alle Benutzer-Accounts aus `DataManager.Benutzer`. Format: Benutzername, Berechtigung. Geeignet für Backup oder externe Verarbeitung.

---

### `ExportiereAlles(pfad)`

Führt alle drei Einzel-Exporte nacheinander durch. Erstellt für jeden Typ eine eigene Datei im selben Verzeichnis. Zeigt eine Zusammenfassung aller exportierten Dateien.

---

### `FragePfad() → string` *(private)*

Fragt den Benutzer nach einem Exportpfad. Wenn keine Eingabe erfolgt, wird `StandardExportVerzeichnis` verwendet. Erstellt das Verzeichnis automatisch, falls es nicht existiert.

---

### `ZeigeExportHeader()` *(private)*

Rendert den stilisierten Export-Header in der Konsole.

---

## Exportformate

Exporte werden als formatierte Textdateien (`.txt`) mit strukturiertem Header und Footer gespeichert. Die Struktur orientiert sich an den internen Datendateien (gleicher Aufbau mit `[DATEN]`-Marker und `;`-Trennzeichen).

> Hinweis: Das Menü erwähnt Excel und PDF als Formate — die aktuelle Implementierung erzeugt strukturierte Textdateien, die kompatibel zum internen Format sind.

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`, `Mitarbeiter`, `Benutzer` |
| `LogManager` | `LogExport()` |
| `ConsoleHelper` | Eingaben, Meldungen |
