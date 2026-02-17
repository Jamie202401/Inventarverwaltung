# InventoryManager — Inventarverwaltung

**Datei:** `Inventorymanager.cs`  
**Namespace:** `Inventarverwaltung`  
**Typ:** `public static class InventoryManager`

---

## Zweck

`InventoryManager` stellt alle Kernoperationen für die Inventarverwaltung bereit: Anlegen neuer Artikel, Anzeigen der Inventarliste und Detailansicht einzelner Artikel. Alle Eingaben werden durch `IntelligentAssistant` mit KI-Vorschlägen unterstützt.

---

## Methoden

### `NeuenArtikelErstellen()`

Vollständig geführter Workflow zum Anlegen eines neuen Inventarartikels:

**Vorbedingung:** Mindestens ein Mitarbeiter muss vorhanden sein. Falls nicht, wird der Benutzer direkt zum Mitarbeiter-Anlegen weitergeleitet.

**Eingabefelder (in Reihenfolge):**

| Feld | Pflicht | KI-Unterstützung |
|---|---|---|
| Inventarnummer | Ja | KI schlägt nächste freie Nummer vor |
| Gerätename | Ja | KI analysiert ähnliche Geräte |
| Mitarbeiter | Ja | Auswahl aus bestehenden Mitarbeitern |
| Seriennummer | Nein | Freitext oder `N/A` |
| Preis | Ja | KI schlägt Durchschnittspreis vor |
| Anschaffungsdatum | Ja | Standard: heute |
| Hersteller | Ja | KI schlägt basierend auf Gerätename vor |
| Kategorie | Ja | KI schlägt passende Kategorie vor |
| Anzahl | Ja | Standardwert: 1 |
| Mindestbestand | Ja | KI-Empfehlung basierend auf Kategorie |

Nach erfolgreicher Eingabe:
- Artikel wird in `DataManager.Inventar` eingefügt
- `DataManager.SaveInvToFile()` wird aufgerufen
- `IntelligentAssistant.IniializeAI()` aktualisiert die KI
- Log-Eintrag via `LogManager.LogInventarHinzugefuegt()`

---

### `ZeigeInventar()`

Zeigt das gesamte Inventar in einer ausgerichteten Tabelle mit allen Kernfeldern:

```
Nr   InvNr      Gerät                Mitarbeiter          Anzahl   Status
──── ────────── ──────────────────── ──────────────────── ──────── ──────────
1    INV001     Laptop Dell XPS      Max Mustermann       5        🟢 GUT
2    INV002     Monitor HP           Anna Beispiel        0        🔴 LEER
```

- Bestandsstatus wird farbig hervorgehoben
- Am Ende: Gesamtanzahl Artikel und Statistik-Zusammenfassung
- Log-Eintrag via `LogManager.LogInventarAngezeigt()`

---

### `ZeigeArtikelDetail()`

Zeigt alle Details eines einzelnen Artikels in einer Detailkarte an:

- Benutzer gibt Inventarnummer oder Teilnamen ein
- Bei mehreren Treffern: Auswahlliste
- Detailkarte zeigt alle 12 Felder inkl. Tracking-Informationen
- Bestandsstatus mit Farbcodierung

---

## Validierungsregeln

| Feld | Regel |
|---|---|
| Inventarnummer | Pflichtfeld, keine Duplikate |
| Gerätename | Pflichtfeld, min. 2 Zeichen |
| Mitarbeiter | Muss aus bestehender Liste ausgewählt werden |
| Preis | Muss ≥ 0 sein, Dezimalzahl erlaubt |
| Datum | Format `dd.MM.yyyy`, Standard: heute |
| Anzahl | Muss ≥ 0 sein, Ganzzahl |
| Mindestbestand | Muss ≥ 0 sein, Ganzzahl |

---

## Abhängigkeiten

| Klasse | Verwendung |
|---|---|
| `DataManager` | `Inventar`-Liste, `SaveInvToFile()` |
| `IntelligentAssistant` | Vorschläge für alle Felder |
| `EmployeeManager` | Mitarbeiter anlegen wenn keiner vorhanden |
| `LogManager` | Logging aller Inventaroperationen |
| `ConsoleHelper` | Eingabe, Meldungen, Tabellen |
| `AuthManager` | `AktuellerBenutzer` für Tracking |
