# SchnellerfassungsManager — Schnellerfassung

**Datei:** `Schnellerfassungsmanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class SchnellerfassungsManager`

---

## Zweck

`SchnellerfassungsManager` bietet einen optimierten Schnellerfassungs-Workflow für neue Inventarartikel. Statt alle Felder einzeln einzugeben, können Artikel als kompakte Einzeileingabe, per CSV-Import oder über ein Template-System angelegt werden.

---

## Konfiguration

| Feld | Wert | Beschreibung |
|---|---|---|
| `TemplateVerzeichnis` | `<Arbeitsverzeichnis>/Templates` | Verzeichnis für gespeicherte Templates |
| `TemplateListeDatei` | `Templates/artikel_templates.txt` | Datei mit allen gespeicherten Templates |

---

## Methoden

### `ZeigeSchnellerfassungsMenu()`

Zeigt das Schnellerfassungs-Hauptmenü mit drei Modi:

| Option | Beschreibung |
|---|---|
| `[1]` ⚡ Ultra-Schnell | Einzeilen-Eingabe |
| `[2]` 📄 CSV-Import | Massenimport aus CSV |
| `[3]` 📋 Template | Gespeicherte Vorlage verwenden |
| `[0]` | Zurück |

---

### `UltraSchnellModus()`

Ermöglicht die Erfassung eines Artikels in einer einzigen Zeile:

**Format:** `InvNr;Gerätename;Mitarbeiter;Anzahl`

Beispiel: `INV042;Laptop Dell;Max Mustermann;5`

- Fehlende optionale Felder werden mit Standardwerten gefüllt
- KI leitet Kategorie und Hersteller automatisch ab
- Erfolgreiche Einträge werden sofort gespeichert
- Eingabe `0` oder `exit` beendet den Modus

---

### `CSVSchnellImport()`

Öffnet einen vereinfachten CSV-Import-Dialog im Schnellmodus:
- Dateipfad eingeben
- CSV wird direkt verarbeitet und alle gültigen Zeilen importiert
- Ergebnis-Zusammenfassung: importiert / übersprungen / fehlgeschlagen

---

### `TemplateAuswahlModus()`

Zeigt alle verfügbaren Templates an und lässt den Benutzer eines auswählen. Nach der Auswahl werden nur noch Pflichtfelder abgefragt (z.B. Anzahl, Mitarbeiter), der Rest wird aus dem Template übernommen.

---

### `TemplateErstellen()`

Führt durch einen geführten Workflow zum Erstellen eines neuen Templates:
1. Name des Templates eingeben
2. Alle Artikel-Stammdaten eingeben (Gerätename, Hersteller, Kategorie, Preis, Mindestbestand)
3. Template wird in `artikel_templates.txt` gespeichert

---

### `TemplateLaden(name) → SchnellerfassungsTemplate` *(private)*

Lädt ein gespeichertes Template aus der Template-Datei anhand des Namens.

---

### `ZeigeSchnellerfassungsHeader()` *(private)*

Rendert den animierten Header mit dem Blitz-Icon und Farbverlauf.

---

## Einzeilen-Format

```
INV-NR ; GERAETENAME ; MITARBEITER ; ANZAHL
```

Optionale Erweiterung:
```
INV-NR ; GERAETENAME ; MITARBEITER ; ANZAHL ; HERSTELLER ; KATEGORIE ; PREIS
```

---

## Template-Dateiformat

Templates werden zeilenweise in `artikel_templates.txt` gespeichert:

```
[TEMPLATE:Laptop-Standard]
GeraeteName=Laptop Dell XPS 13
Hersteller=Dell
Kategorie=IT
Preis=1299.99
Mindestbestand=2
```

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`-Liste, `SaveInvToFile()` |
| `KIEngine` | Kategorie- und Herstellervorschläge |
| `LogManager` | Logging aller Schnellerfassungs-Vorgänge |
| `ConsoleHelper` | Eingaben, Meldungen, Farben |
| `AuthManager` | `AktuellerBenutzer` für Tracking |
